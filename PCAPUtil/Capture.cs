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
            capThread = new Thread(RecordCapture);
        }

        public void Run()
        {
            if(running)
            {

            }
            else
            {
                running = true;
                capThread.Start();
            }

        }

        private void RecordCapture()
        {
            UdpClient client = new UdpClient(sourcePort, AddressFamily.InterNetwork);
            client.JoinMulticastGroup(IPAddress.Parse(sourceIP), IPAddress.Parse(interfaceIP));
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse(sourceIP), sourcePort);
            while (running)
            {
                byte[] payload = client.Receive(ref endpoint);
            }
        }
    }
}
