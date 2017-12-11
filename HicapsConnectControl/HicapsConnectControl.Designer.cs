namespace HicapsConnectControl
{
    partial class HicapsConnectControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing && (components != null))
        //    {
        //        components.Dispose();
        //    }
        //    base.Dispose(disposing);
        //}

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HicapsConnectControl));
            this.HicapsConnectVersion = new System.Windows.Forms.Label();
            this.HicapsConnectTitle = new System.Windows.Forms.Label();
            this.pbLogo = new System.Windows.Forms.PictureBox();
            this.lStatus = new System.Windows.Forms.Label();
            this.lCopyright = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pbLogo)).BeginInit();
            //this.SuspendLayout();
            // 
            // HicapsConnectVersion
            // 
            this.HicapsConnectVersion.AutoSize = true;
            this.HicapsConnectVersion.Location = new System.Drawing.Point(2, 32);
            this.HicapsConnectVersion.Name = "HicapsConnectVersion";
            this.HicapsConnectVersion.Size = new System.Drawing.Size(40, 13);
            this.HicapsConnectVersion.TabIndex = 5;
            this.HicapsConnectVersion.Text = "0.0.0.1";
            // 
            // HicapsConnectTitle
            // 
            this.HicapsConnectTitle.AutoSize = true;
            this.HicapsConnectTitle.Location = new System.Drawing.Point(2, 12);
            this.HicapsConnectTitle.Name = "HicapsConnectTitle";
            this.HicapsConnectTitle.Size = new System.Drawing.Size(89, 13);
            this.HicapsConnectTitle.TabIndex = 3;
            this.HicapsConnectTitle.Text = "HICAPS Connect";
            // 
            // pbLogo
            // 
            this.pbLogo.BackColor = System.Drawing.Color.Transparent;
            this.pbLogo.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pbLogo.BackgroundImage")));
            this.pbLogo.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.pbLogo.Location = new System.Drawing.Point(113, 10);
            this.pbLogo.Name = "pbLogo";
            this.pbLogo.Size = new System.Drawing.Size(60, 115);
            this.pbLogo.TabIndex = 4;
            this.pbLogo.TabStop = false;
            // 
            // lStatus
            // 
            this.lStatus.AutoSize = true;
            this.lStatus.Location = new System.Drawing.Point(2, 52);
            this.lStatus.MaximumSize = new System.Drawing.Size(105, 90);
            this.lStatus.MinimumSize = new System.Drawing.Size(105, 90);
            this.lStatus.Name = "lStatus";
            this.lStatus.Size = new System.Drawing.Size(105, 90);
            this.lStatus.TabIndex = 6;
            this.lStatus.Text = "Waiting";
            // 
            // lCopyright
            // 
            this.lCopyright.Location = new System.Drawing.Point(2, 148);
            this.lCopyright.Name = "lCopyright";
            this.lCopyright.Size = new System.Drawing.Size(181, 13);
            this.lCopyright.TabIndex = 7;
            this.lCopyright.Text = " ";
          

        }

        #endregion

        private System.Windows.Forms.Label HicapsConnectVersion;
        private System.Windows.Forms.PictureBox pbLogo;
        private System.Windows.Forms.Label HicapsConnectTitle;
        private System.Windows.Forms.Label lStatus;
        private System.Windows.Forms.Label lCopyright;
    }
}
