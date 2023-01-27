using System;
using System.Windows.Forms;

namespace OAuth2Client
{
    public partial class OAuth2ClientForm : Form
    {
        public string Code;
        public OAuth2ClientForm(string authorizationURL) 
        {
            InitializeComponent();
            this.authWindow.Navigate(authorizationURL);
        }

        private void authWindow_NavigationCompleted(object sender, Microsoft.Toolkit.Win32.UI.Controls.Interop.WinRT.WebViewControlNavigationCompletedEventArgs e)
        {
            if (!e.Uri.AbsoluteUri.Contains("?code=")) return;
            Code = e.Uri.AbsoluteUri.Split(new string[] { "?code=" }, StringSplitOptions.None)[1];
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
