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
        private Thread capThread;


        private  CaptureFileWriterDevice captureFileWriter;
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
                StopCapture();

                //    while (capThread.IsAlive) ;
                //    capThread.Abort();
                //    capThread = null;
            }
            else
            {
                running = true;
                RecordCapture();
            }
                //    if (capThread == null)
                //    {
                //        capThread = new Thread(RecordCapture);
                //    }
                //    else
                //    {
                //        capThread.Abort();
                //        capThread = null;
                //        capThread = new Thread(RecordCapture);
                //    }
                //    capThread.Start();
            }

        private void StopCapture()
        {
            theDevice.Close();
        }

        private void RecordCapture()
        {
            //packetCount = 0;
            UdpClient client = new UdpClient(sourcePort, AddressFamily.InterNetwork);
            client.JoinMulticastGroup(IPAddress.Parse(sourceIP), IPAddress.Parse(interfaceIP));
            //IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse(sourceIP), sourcePort);
            //; while (running)
            //{
            //    if (client.Available > 0)
            //    {
            //        byte[] payload = client.Receive(ref endpoint);
            //        packetCount++;
            //        TimeSpan unixTime = DateTime.UtcNow - epoch;
            //        string datedFilePath = filepath.Substring(0, filepath.Length - 5) + string.Format("_{0}{1}{2}{3}", DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, DateTime.UtcNow.Hour) + ".pcap";
            //        WritePCap(datedFilePath, unixTime, IPAddress.Parse(interfaceIP), IPAddress.Parse(sourceIP), (UInt16)sourcePort, (UInt16)sourcePort, payload);

            //    }
            //}
            //client.Close();
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
                        theDevice.Filter = "udp and port " + sourcePort;
                        theDevice.Capture();
                        breakerBar = true;
                        break;
                    }
                }

            }
            while (running) ;
        }
        private  void device_OnPacketArrival(object sender, CaptureEventArgs e)
        {
            //var device = (ICaptureDevice)sender;

            // write the packet to the file
                var packet = PacketDotNet.Packet.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data);
                var ethernetPacket = (EthernetPacket)packet;


            if (e.Packet.LinkLayerType == PacketDotNet.LinkLayers.Ethernet)
            {
                string datedFilePath = filepath.Substring(0, filepath.Length - 5) + string.Format("_{0}{1}{2}{3}", DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, DateTime.UtcNow.Hour) + ".pcap";
                if (!File.Exists(datedFilePath)) File.Create(datedFilePath);
                CaptureFileWriterDevice author = new CaptureFileWriterDevice(theDevice, datedFilePath);
                captureFileWriter.Write(e.Packet);
                author.Close();
                packetCount++;

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
