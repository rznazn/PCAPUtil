using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PCAPUtil
{
    public partial class Form1 : Form
    {
        public BindingList<Capture> captures = new BindingList<Capture>();
        public Form1()
        {
            InitializeComponent();
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
            dgvCaptures.Columns.Add(saveFileCol);

            dgvCaptures.DataSource = captures;
        }

        private void btnRun_Click(object sender, EventArgs e)
        {

        }
    }
}
