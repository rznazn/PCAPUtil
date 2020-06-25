using SharpPcap.LibPcap;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace PCAPUtil
{
    public partial class Form1 : Form
    {
        private bool running;//bool for handling background threads
        public delegate void CountUpdateDelegate();
        private CountUpdateDelegate updateDelegate;//invokable to update packet counts
        Thread countUpdateThread;//thread for updating packet counts

        JavaScriptSerializer mul = new JavaScriptSerializer();//serializer for saving and loading config
        public Form1()
        {
            InitializeComponent();
            SetDataGridView();
            updateDelegate = new CountUpdateDelegate(PCAPUtil.Capture.captures.ResetBindings);
        }

        /// <summary>
        /// hanlde run click. start packet count update thread and trigger Capture.Run()
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRun_Click(object sender, EventArgs e)
        {
            btnRun.Enabled = false;//disable button while starting to prevent wierd state from double clicking
            PCAPUtil.Capture.Run();//start Captures
            running = !running;//toggle running
            if ( running)//if running start update count thread and set led to green
            {
                countUpdateThread = new Thread(UpdateCounts);
                countUpdateThread.Start();
                ledRunning.SetLEDGradient(Color.LightGreen, Color.Green);
            }
            else if(!running)//if not running set led to gray and kill update thread
            {
                ledRunning.SetLEDGradient(Color.LightGray, Color.DarkGray);
                countUpdateThread.Abort();
                while (countUpdateThread.IsAlive) ;
                countUpdateThread = null;
            }
            //diable or enable parts of GUI appropriately
            dgvCaptures.Enabled = !running;
            btnLoad.Enabled = !running;
            btnRun.Enabled = true;
        }

        /// <summary>
        /// method called on count updater thread
        /// </summary>
        private void UpdateCounts()
        {
            while (running)
            {
                this.Invoke(updateDelegate);
                Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// build the data grid view appropiately
        /// </summary>
        private void SetDataGridView()
        {
            DataGridViewTextBoxColumn nameCol = new DataGridViewTextBoxColumn();
            nameCol.HeaderText = "name";
            nameCol.DataPropertyName = "name";
            dgvCaptures.Columns.Add(nameCol);
            String strHostName = Dns.GetHostName();
            IPHostEntry ipEntry = Dns.GetHostEntry(strHostName);
            IPAddress[] addr = ipEntry.AddressList;
            DataGridViewComboBoxColumn intIPCol = new DataGridViewComboBoxColumn();
            intIPCol.Items.Clear();
            foreach (IPAddress IPA in addr)
            {
                //Filter IPV6 
                if (IPA.AddressFamily.Equals(System.Net.Sockets.AddressFamily.InterNetwork))
                {
                    intIPCol.Items.Add(IPA.ToString());
                }
            }
            intIPCol.Items.Add(System.Net.IPAddress.Loopback.ToString());
            intIPCol.HeaderText = "interface IP";
            intIPCol.DataPropertyName = "interfaceIP";
            dgvCaptures.Columns.Add(intIPCol);
            DataGridViewTextBoxColumn sourceIPCol = new DataGridViewTextBoxColumn();
            sourceIPCol.HeaderText = "source ip";
            sourceIPCol.DataPropertyName = "sourceIP";
            dgvCaptures.Columns.Add(sourceIPCol);
            DataGridViewTextBoxColumn sourcePortCol = new DataGridViewTextBoxColumn();
            sourcePortCol.HeaderText = "source port";
            sourcePortCol.DataPropertyName = "sourcePort";
            sourcePortCol.Width = 50;
            dgvCaptures.Columns.Add(sourcePortCol);
            DataGridViewTextBoxColumn saveFileCol = new DataGridViewTextBoxColumn();
            saveFileCol.HeaderText = "save file";
            saveFileCol.DataPropertyName = "filepath";
            saveFileCol.Width = 300;
            dgvCaptures.Columns.Add(saveFileCol);
            DataGridViewTextBoxColumn fileLengthCol = new DataGridViewTextBoxColumn();
            fileLengthCol.HeaderText = "minutes per file";
            fileLengthCol.DataPropertyName = "minutes";
            fileLengthCol.Width = 50;
            dgvCaptures.Columns.Add(fileLengthCol);
            DataGridViewTextBoxColumn packetCountCol = new DataGridViewTextBoxColumn();
            packetCountCol.HeaderText = "packet count";
            packetCountCol.DataPropertyName = "packetCount";
            packetCountCol.ReadOnly = true;
            dgvCaptures.Columns.Add(packetCountCol);

            dgvCaptures.DataSource = PCAPUtil.Capture.captures;
        }

        /// <summary>
        /// write the configuration to a file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSave_Click(object sender, EventArgs e)
        {
            var dialog = new SaveFileDialog();
            dialog.Filter = "Text Files | *.txt";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                Stream filestream = dialog.OpenFile();
                StreamWriter author = new StreamWriter(filestream);
                author.Write(mul.Serialize(PCAPUtil.Capture.captures));
                author.Flush();
                author.Close();
            }
        }

        /// <summary>
        /// Load configuration from file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLoad_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Text Files | *.txt";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                PCAPUtil.Capture.captures.Clear();
                List<Capture> loaded = mul.Deserialize<List<Capture>>(File.ReadAllText(dialog.FileName));
                foreach(Capture cap in loaded)
                {
                    cap.packetCount = 0;
                    PCAPUtil.Capture.captures.Add(cap);
                }
                PCAPUtil.Capture.captures.ResetBindings();
            }
        }
    }
}
