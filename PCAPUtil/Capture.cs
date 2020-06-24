using PacketDotNet;
using PcapDotNet.Core;
using SharpPcap;
using SharpPcap.LibPcap;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PCAPUtil
{
    public class Capture
    {
        public bool running;
        public string name { get; set; }
        public string interfaceIP { get; set; }
        public string sourceIP { get; set; }
        public int sourcePort { get; set; }
        public string filepath { get; set; }

        public int packetCount { get; private set; }

        private static int instanceCount = 1;
        private static DateTime epoch = new DateTime(1970, 1, 1);
        private DateTime lastFileCreateTime = DateTime.MinValue;
        private string lastFileCreated = "";
        private Thread capThread;
        private bool launched;


        private CaptureFileWriterDevice captureFileWriter = null;
        private LibPcapLiveDevice theDevice = null;

        public Capture()
        {
            running = false;
            name = "default" + instanceCount++;
            interfaceIP = "127.0.0.1";
            sourceIP = "127.0.0.1";
            sourcePort = 8888;
            filepath = "c:\\file.pcap";
        }

        public void Run()
        {
            if (running)
            {
                running = false;
                launched = false;

                theDevice.StopCapture();
                theDevice.Close();
                capThread.Abort();
                while (capThread.IsAlive) ;
                capThread = null;
            }
            else
            {
                running = true;
                if (capThread == null)
                {
                    capThread = new Thread(RecordCapture);
                }
                else
                {
                    capThread.Abort();
                    while (capThread.IsAlive) ;
                    capThread = null;
                    capThread = new Thread(RecordCapture);
                }
                capThread.Start();
            }
        }

        private void RecordCapture()
        {
            packetCount = 0;
            //UdpClient client = new UdpClient(sourcePort, AddressFamily.InterNetwork);
            //client.JoinMulticastGroup(IPAddress.Parse(sourceIP), IPAddress.Parse(interfaceIP));

            // Retrieve the device list

            // open the output file
            LibPcapLiveDeviceList devices = LibPcapLiveDeviceList.Instance;
            bool breakerBar = false;
            
                foreach (LibPcapLiveDevice dev in devices)
                {
                    if (breakerBar) break;
                    foreach (PcapAddress address in dev.Addresses)
                    {
                        if (address.Addr.ToString() == interfaceIP)
                        {
                            theDevice = dev;
                            theDevice.OnPacketArrival +=
                                new PacketArrivalEventHandler(device_OnPacketArrival);
                            theDevice.Open(DeviceMode.Promiscuous, 50);
                            theDevice.Capture();
                            launched = true;
                            breakerBar = true;
                            break;
                        }
                    }
                }
        }
        private void device_OnPacketArrival(object sender, CaptureEventArgs e)
        {
            //var device = (ICaptureDevice)sender;

            // write the packet to the file
            var packet = PacketDotNet.Packet.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data);
            var ethernetPacket = (EthernetPacket)packet;

            var ip = packet.Extract<PacketDotNet.IPPacket>();
            var udp = packet.Extract<PacketDotNet.UdpPacket>();

            if (udp != null)
            {
                if (udp.DestinationPort == sourcePort)
                {
                    if (e.Packet.LinkLayerType == PacketDotNet.LinkLayers.Ethernet)
                    {
                        string datedFilePath = filepath.Substring(0, filepath.Length - 5) + string.Format("_{0}{1}{2}{3}{4}", DateTime.UtcNow.Year, DateTime.UtcNow.Month.ToString("00"), DateTime.UtcNow.Day.ToString("00"), DateTime.UtcNow.Hour.ToString("00"), DateTime.UtcNow.Minute.ToString("00")) + "00.pcap";
                        if (!File.Exists(datedFilePath))
                        {//if file doens't exist and this is the first run create new file
                            if (lastFileCreateTime == DateTime.MinValue)
                            {
                                lastFileCreateTime = DateTime.UtcNow;
                                File.Create(datedFilePath).Close();
                                lastFileCreated = datedFilePath;
                                captureFileWriter = null;
                            }//else file doesn't exist and this is not the first packet then wait one hour to start next file
                            else if (lastFileCreateTime < DateTime.UtcNow.AddHours(-1))
                            {
                                lastFileCreateTime = DateTime.UtcNow;
                                File.Create(datedFilePath).Close();
                                lastFileCreated = datedFilePath;
                                captureFileWriter = null;
                            }
                        }
                        //if file does exist and this is first run
                        else if (File.Exists(datedFilePath) && lastFileCreateTime == DateTime.MinValue)
                        {
                            lastFileCreateTime = DateTime.UtcNow;
                            //add seconds to file name
                            datedFilePath = filepath.Substring(0, filepath.Length - 5) + string.Format("_{0}{1}{2}{3}{4}{5}", DateTime.UtcNow.Year, DateTime.UtcNow.Month.ToString("00"), DateTime.UtcNow.Day.ToString("00"), DateTime.UtcNow.Hour.ToString("00"), DateTime.UtcNow.Minute.ToString("00"), DateTime.UtcNow.Second.ToString("00")) + ".pcap";
                            File.Create(datedFilePath).Close();
                            lastFileCreated = datedFilePath;
                            captureFileWriter = null;
                        }
                        if (captureFileWriter == null)
                        {
                            captureFileWriter = new CaptureFileWriterDevice(theDevice, lastFileCreated);
                        }
                        captureFileWriter.Write(e.Packet);
                        packetCount++;

                    }
                }
            }
        }
        //public void WritePCap(
        //    string filename, TimeSpan dt, IPAddress srcIp, IPAddress dstIp,
        //  UInt16 srcPort, UInt16 dstPort, byte[] data)
        //{
        //    IPv4Packet ip = new IPv4Packet(srcIp, dstIp);
        //    ip.TimeToLive = 70;

        //    UdpPacket payload = new UdpPacket(srcPort, dstPort)
        //    {
        //        SourcePort = srcPort,
        //        DestinationPort = dstPort,
        //        PayloadData = data,
        //        ParentPacket = ip
        //    };

        //    ip.PayloadPacket = payload;

        //    payload.UpdateCalculatedValues();
        //    ip.UpdateCalculatedValues();

        //    byte[] ipData = ip.Bytes;

        //    CaptureFileWriterDevice storeDevice = new CaptureFileWriterDevice(
        //         PacketDotNet.LinkLayers.Ethernet, null, filename, FileMode.OpenOrCreate);
        //    PcapHeader hdr = new PcapHeader(
        //           (uint)dt.TotalSeconds, (uint)((dt.TotalMilliseconds - dt.TotalSeconds * 1000)*1000),
        //           (uint)ipData.Length, (uint)ipData.Length);
        //    storeDevice.Write(ipData, hdr);
        //}
    }
}
