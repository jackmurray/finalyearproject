using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LibConfig;

namespace SpeakerController
{
    public partial class frmPairedDevices : Form
    {
        public frmPairedDevices()
        {
            InitializeComponent();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            TrustedKeys.Remove(lstKeys.SelectedItem as string);
            lstKeys.Items.Remove(lstKeys.SelectedItem);
        }

        private void frmPairedDevices_Load(object sender, EventArgs e)
        {
            var keys = TrustedKeys.GetAllKeys();
            foreach (string s in keys)
                lstKeys.Items.Add(s);
        }
    }
}
