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
        //static fields for management
        private static int instanceCount = 1;//used to increment names
        public static BindingList<Capture> captures = new BindingList<Capture>();//master list of all captures created
        private static List<string> usedInterfaces = new List<string>();//used to only create one thread per used interface
        private static Dictionary<string, LibPcapLiveDevice> capDevices = new Dictionary<string, LibPcapLiveDevice>();//map interface IP to capture device 
        private static Dictionary<string, CaptureFileWriterDevice> authors = new Dictionary<string, CaptureFileWriterDevice>();//map Capture.name to file writer
        private static List<Thread> threads = new List<Thread>(); //list of thread to aid thread clean up
        private static bool running;//used to toggle capturing on and off

        //instance unique properties
        public string name { get; set; }
        public string interfaceIP { get; set; }
        public string sourceIP { get; set; }
        public int sourcePort { get; set; }
        public string filepath { get; set; }
        public int minutes { get; set; }
        public int packetCount { get; set; }
        //instance unique properties not user defined
        private DateTime lastFileCreateTime = DateTime.MinValue;
        private string lastFileCreated = "";

        /// <summary>
        /// default constructor
        /// </summary>
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

        /// <summary>
        /// static function to start or stop capturing
        /// </summary>
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

                foreach (Capture cap in captures)//reset filepath for next capture start
                {
                    cap.lastFileCreated = null;
                    cap.lastFileCreateTime = DateTime.MinValue;
                }

                foreach (KeyValuePair<string, LibPcapLiveDevice> pair in capDevices)//close and clear all capture devices
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
                usedInterfaces.Clear();//clear then reset capture interfaces
                foreach (Capture cap in captures)
                {
                    if (!usedInterfaces.Contains(cap.interfaceIP)) usedInterfaces.Add(cap.interfaceIP);
                    cap.packetCount = 0;
                }
                foreach (string iip in usedInterfaces)//start a capture thread for all used interfaces
                {
                    Thread thread = new Thread(() => RecordCapture(iip));
                    threads.Add(thread);
                    thread.Start();
                }
            }
        }

        /// <summary>
        /// start capturing on a specific interface
        /// </summary>
        /// <param name="intIP"> the IP of the device intended for capture</param>
        private static void RecordCapture(string intIP)
        {
            while (running)//outer loop to attept to reconnect if device becomes unavailable ie USB NIC
            {
                LibPcapLiveDevice device;
                LibPcapLiveDeviceList devices = LibPcapLiveDeviceList.Instance;
                bool breakerBar = false;
                if (capDevices.ContainsKey(intIP))
                {
                    try  {capDevices[intIP].StopCapture(); } catch { }
                    try { capDevices[intIP].Close(); } catch { }
                    capDevices.Remove(intIP);
                }

                foreach (LibPcapLiveDevice dev in devices)//check all devices
                {
                    if (breakerBar) break;
                    foreach (PcapAddress address in dev.Addresses)//check all IPs on that device
                    {
                        if (address.Addr.ToString() == intIP)//if it matches the intended interface IP
                        {
                            try//set up capturing in try/catch so that an error kicks back to the while loop instead ocf killing thread
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
                Thread.Sleep(250);//no need to recheck for NIC reconnect every millisecond
            }
        }

        /// <summary>
        /// Handles incoming packets
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">e.packet is the collected packet</param>
        private static void device_OnPacketArrival(object sender, CaptureEventArgs e)
        {
            var packet = PacketDotNet.Packet.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data);
            var ethernetPacket = (EthernetPacket)packet;

            var ip = packet.Extract<PacketDotNet.IPPacket>();
            var udp = packet.Extract<PacketDotNet.UdpPacket>();

            if (udp != null)//at this time we are only looking for multicast/udp
            {
                foreach (Capture cap in captures)
                {
                    //check all Captrures if they match this packet
                    if (udp.DestinationPort == cap.sourcePort && ip.DestinationAddress.ToString() == cap.sourceIP)
                    {
                        //honestly this was from the example. not sure if it's needed here but it isn't hurting anything
                        if (e.Packet.LinkLayerType == PacketDotNet.LinkLayers.Ethernet)
                        {
                            string datedFilePath = cap.filepath.Substring(0, cap.filepath.Length - 5) + string.Format("_{0}{1}{2}{3}{4}{5}", DateTime.UtcNow.Year, DateTime.UtcNow.Month.ToString("00"), DateTime.UtcNow.Day.ToString("00"), DateTime.UtcNow.Hour.ToString("00"), DateTime.UtcNow.Minute.ToString("00"), DateTime.UtcNow.Second.ToString("00")) + ".pcap";

                            if (cap.lastFileCreateTime == DateTime.MinValue)//create first file for this run
                            {
                                cap.lastFileCreateTime = DateTime.UtcNow;
                                File.Create(datedFilePath).Close();
                                cap.lastFileCreated = datedFilePath;
                                if (authors.ContainsKey(cap.name))//if an author was already created clean it up. a new one will be created below
                                {
                                    if (authors[cap.name] != null)
                                    {
                                        authors[cap.name].Close();
                                        authors[cap.name] = null;
                                    }
                                    authors[cap.name] = new CaptureFileWriterDevice(capDevices[cap.interfaceIP], cap.lastFileCreated);
                                }
                                else
                                {
                                    authors.Add(cap.name, new CaptureFileWriterDevice(capDevices[cap.interfaceIP], cap.lastFileCreated));
                                }
                            }
                            else if (cap.lastFileCreateTime.AddMinutes(cap.minutes) < DateTime.UtcNow)//increment files at appropriate interval
                            {
                                cap.lastFileCreateTime = DateTime.UtcNow;
                                File.Create(datedFilePath).Close();
                                cap.lastFileCreated = datedFilePath;
                                if (authors.ContainsKey(cap.name))//if an author was already created clean it up. a new one will be created below
                                {
                                    if (authors[cap.name] != null)
                                    {
                                        authors[cap.name].Close();
                                        authors[cap.name] = null;
                                    }
                                    authors[cap.name] = new CaptureFileWriterDevice(capDevices[cap.interfaceIP], cap.lastFileCreated);
                                }
                                else
                                {
                                    authors.Add(cap.name, new CaptureFileWriterDevice(capDevices[cap.interfaceIP], cap.lastFileCreated));
                                }
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
