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
        public BindingList<Capture> captures = new BindingList<Capture>();
        private bool running;
        public delegate void CountUpdateDelegate();
        private CountUpdateDelegate updateDelegate;
        Thread countUpdateThread;

        JavaScriptSerializer mul = new JavaScriptSerializer();
        public Form1()
        {
            InitializeComponent();
            SetDataGridView();
            updateDelegate = new CountUpdateDelegate(captures.ResetBindings);
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            dgvCaptures.Enabled = running;
            running = !running;
            if (countUpdateThread == null)
            {
                countUpdateThread = new Thread(UpdateCounts);
                countUpdateThread.Start();
            }
            foreach (Capture cap in captures)
            {
                cap.Run();
            }

        }

        private void UpdateCounts()
        {
            while (true)
            {
                this.Invoke(updateDelegate);
                Thread.Sleep(1000);
            }
        }

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
            dgvCaptures.Columns.Add(sourcePortCol);
            DataGridViewTextBoxColumn saveFileCol = new DataGridViewTextBoxColumn();
            saveFileCol.HeaderText = "save file";
            saveFileCol.DataPropertyName = "filepath";
            saveFileCol.Width = 300;
            dgvCaptures.Columns.Add(saveFileCol);

            dgvCaptures.DataSource = captures;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            var dialog = new SaveFileDialog();
            dialog.Filter = "Text Files | *.txt";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                Stream filestream = dialog.OpenFile();
                StreamWriter author = new StreamWriter(filestream);
                author.Write(mul.Serialize(captures));
                author.Flush();
                author.Close();
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Text Files | *.txt";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                List<Capture> loaded = mul.Deserialize<List<Capture>>(File.ReadAllText(dialog.FileName));
                foreach(Capture cap in loaded)
                {
                    captures.Add(cap);
                }
                captures.ResetBindings();
            }
        }
    }
}
