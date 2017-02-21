using System;
using System.Windows.Forms;
using Push.Foundation.Utilities.Security;

namespace Push.Foundation
{
    public partial class CryptoUi : Form
    {
        public CryptoUi()
        {
            InitializeComponent();
        }

        private void buttonExecute_Click(object sender, EventArgs e)
        {
            this.txtEncrypted.Text = 
                txtPlaintText.Text.ToSecureString().DpApiEncryptString();
            this.txtDecrypted.Text = 
                txtEncrypted.Text.DpApiDecryptString().ToInsecureString();
        }
    }
}
