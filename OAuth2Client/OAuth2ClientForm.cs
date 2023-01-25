using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OAuth2Client
{
    public partial class OAuth2ClientForm : Form
    {
        static readonly HttpClient client = new HttpClient();
        public string Code;

        private OAuth2ClientForm() { InitializeComponent(); }

        public OAuth2ClientForm(string authorizationURL) 
        {
            InitializeComponent();
            MessageBox.Show(authorizationURL);
            this.authWindow.Navigate(authorizationURL);
        }
    }
}
