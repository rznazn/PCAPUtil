using System;
using System.Collections.Generic;
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
        private Thread capThread;

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
            if(running)
            {
                running = false;
                while (capThread.IsAlive) ;
                capThread.Abort();
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
                    capThread = null;
                    capThread = new Thread(RecordCapture);
                }
                capThread.Start();
            }

        }

        private void RecordCapture()
        {
            packetCount = 0;
            UdpClient client = new UdpClient(sourcePort, AddressFamily.InterNetwork);
            client.JoinMulticastGroup(IPAddress.Parse(sourceIP), IPAddress.Parse(interfaceIP));
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse(sourceIP), sourcePort);
            while (running)
            {
                if (client.Available > 0)
                {
                    byte[] payload = client.Receive(ref endpoint);
                    packetCount++;
                }
            }
            client.Close();
        }
    }
}
