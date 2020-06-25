using PacketDotNet;
using PcapDotNet.Core;
using SharpPcap;
using SharpPcap.LibPcap;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        private static int instanceCount = 1;
        private static DateTime epoch = new DateTime(1970, 1, 1);
        public static BindingList<Capture> captures = new BindingList<Capture>();
        private static List<string> usedInterfaces = new List<string>();
        private static Dictionary<string, LibPcapLiveDevice> capDevices = new Dictionary<string, LibPcapLiveDevice>();
        private static Dictionary<string, CaptureFileWriterDevice> authors = new Dictionary<string, CaptureFileWriterDevice>();
        private static List<Thread> threads = new List<Thread>();
        private static Thread capThread;

        private static bool running;
        public string name { get; set; }
        public string interfaceIP { get; set; }
        public string sourceIP { get; set; }
        public int sourcePort { get; set; }
        public string filepath { get; set; }
        public int minutes { get; set; }
        public int packetCount { get; set; }

        private DateTime lastFileCreateTime = DateTime.MinValue;
        private string lastFileCreated = "";

        private CaptureFileWriterDevice captureFileWriter = null;
        private LibPcapLiveDevice theDevice = null;

        public Capture()
        {
            running = false;
            name = "default" + instanceCount++;
            interfaceIP = "127.0.0.1";
            sourceIP = "239.1.1.1";
            sourcePort = 3001;
            filepath = "C:\\" + name + ".pcap";
            minutes = 60;
        }

        public static void Run()
        {
            if (running)
            {
                running = false;

                foreach (Thread t in threads)//stop all threads
                {
                    t.Abort();
                    while (t.IsAlive) ;
                }

                foreach (Capture cap in captures)
                {
                    cap.lastFileCreated = null;
                    cap.lastFileCreateTime = DateTime.MinValue;
                }

                foreach (KeyValuePair<string, LibPcapLiveDevice> pair in capDevices)//close all capture devices
                {
                    if (pair.Value != null)
                    {
                        pair.Value.StopCapture();
                        pair.Value.Close();
                        //capDevices[pair.Key] = null;
                    }
                }
                capDevices.Clear();

                foreach (KeyValuePair<string, CaptureFileWriterDevice> pair in authors)//close all file writers
                    if (pair.Value != null)
                    {
                        pair.Value.Close();
                        //authors[pair.Key] = null;
                    }
                authors.Clear();

                threads.Clear();//clear all threads
            }
            else
            {
                running = true;

                usedInterfaces.Clear();
                foreach (Capture cap in captures)
                {
                    if (!usedInterfaces.Contains(cap.interfaceIP)) usedInterfaces.Add(cap.interfaceIP);
                    cap.packetCount = 0;
                }
                foreach (string iip in usedInterfaces)
                {
                    Thread thread = new Thread(() => RecordCapture(iip));
                    threads.Add(thread);
                    thread.Start();
                }
            }
        }

        private static void RecordCapture(string intIP)
        {
            //UdpClient client = new UdpClient(sourcePort, AddressFamily.InterNetwork);
            //client.JoinMulticastGroup(IPAddress.Parse(sourceIP), IPAddress.Parse(interfaceIP));

            // Retrieve the device list

            // open the output file
            while (running)
            {
                LibPcapLiveDevice device;
                LibPcapLiveDeviceList devices = LibPcapLiveDeviceList.Instance;
                bool breakerBar = false;

                foreach (LibPcapLiveDevice dev in devices)
                {
                    if (breakerBar) break;
                    foreach (PcapAddress address in dev.Addresses)
                    {
                        if (address.Addr.ToString() == intIP)
                        {
                            try
                            {
                                device = dev;
                                device.OnPacketArrival +=
                                    new PacketArrivalEventHandler(device_OnPacketArrival);
                                device.Open(DeviceMode.Promiscuous, 50);
                                capDevices.Add(intIP, device);
                                device.Capture();
                            }
                            catch { }
                            breakerBar = true;
                            break;
                        }
                    }
                }
                Thread.Sleep(250);
            }
        }
        private static void device_OnPacketArrival(object sender, CaptureEventArgs e)
        {
            //var device = (ICaptureDevice)sender;

            // write the packet to the file
            var packet = PacketDotNet.Packet.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data);
            var ethernetPacket = (EthernetPacket)packet;

            var ip = packet.Extract<PacketDotNet.IPPacket>();
            var udp = packet.Extract<PacketDotNet.UdpPacket>();

            if (udp != null)
            {
                foreach (Capture cap in captures)
                {
                    if (udp.DestinationPort == cap.sourcePort && ip.DestinationAddress.ToString() == cap.sourceIP)
                    {
                        if (e.Packet.LinkLayerType == PacketDotNet.LinkLayers.Ethernet)
                        {
                            string datedFilePath = cap.filepath.Substring(0, cap.filepath.Length - 5) + string.Format("_{0}{1}{2}{3}{4}{5}", DateTime.UtcNow.Year, DateTime.UtcNow.Month.ToString("00"), DateTime.UtcNow.Day.ToString("00"), DateTime.UtcNow.Hour.ToString("00"), DateTime.UtcNow.Minute.ToString("00"), DateTime.UtcNow.Second.ToString("00")) + ".pcap";
                            if (!File.Exists(datedFilePath))
                            {//if file doens't exist and this is the first run create new file
                                if (cap.lastFileCreateTime == DateTime.MinValue)
                                {
                                    cap.lastFileCreateTime = DateTime.UtcNow;
                                    File.Create(datedFilePath).Close();
                                    cap.lastFileCreated = datedFilePath;
                                    if (authors.ContainsKey(cap.name))
                                    {
                                        if (authors[cap.name] != null)
                                        {
                                            authors[cap.name].Close();
                                            authors[cap.name] = null;
                                        }
                                    }
                                }//else file doesn't exist and this is not the first packet then wait one hour to start next file
                                else if (cap.lastFileCreateTime.AddMinutes(cap.minutes) < DateTime.UtcNow)
                                {
                                    cap.lastFileCreateTime = DateTime.UtcNow;
                                    File.Create(datedFilePath).Close();
                                    cap.lastFileCreated = datedFilePath;
                                    if (authors.ContainsKey(cap.name))
                                    {
                                        if (authors[cap.name] != null)
                                        {
                                            authors[cap.name].Close();
                                            authors[cap.name] = null;
                                        }
                                    }
                                }
                            }
                            if (authors.ContainsKey(cap.name))
                            {
                                if (authors[cap.name] == null)
                                {
                                    authors[cap.name] = new CaptureFileWriterDevice(capDevices[cap.interfaceIP], cap.lastFileCreated);
                                }
                            }
                            else
                            {
                                authors.Add(cap.name, new CaptureFileWriterDevice(capDevices[cap.interfaceIP], cap.lastFileCreated));
                            }
                            authors[cap.name].Write(e.Packet);
                            cap.packetCount++;
                        }
                    }
                }

            }
        }
    }
}
