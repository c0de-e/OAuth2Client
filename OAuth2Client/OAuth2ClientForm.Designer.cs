#pragma warning disable
namespace OAuth2Client
{
    partial class OAuth2ClientForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OAuth2ClientForm));
            this.authWindow = new Microsoft.Toolkit.Forms.UI.Controls.WebViewCompatible();
            this.SuspendLayout();
            // 
            // authWindow
            // 
            this.authWindow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.authWindow.Location = new System.Drawing.Point(0, 0);
            this.authWindow.Name = "authWindow";
            this.authWindow.Size = new System.Drawing.Size(800, 450);
            this.authWindow.TabIndex = 0;
            // 
            // OAuth2ClientForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.authWindow);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "OAuth2ClientForm";
            this.Text = "Authorization Client";
            this.ResumeLayout(false);

        }

        #endregion

        private Microsoft.Toolkit.Forms.UI.Controls.WebViewCompatible authWindow;
    }
}

