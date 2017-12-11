namespace HicapsConnectControl
{
    sealed partial class StatusForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.labelTime = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Font = new System.Drawing.Font("Courier New", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.MaximumSize = new System.Drawing.Size(295, 225);
            this.label1.MinimumSize = new System.Drawing.Size(295, 225);
            this.label1.Name = "label1";
            this.label1.Padding = new System.Windows.Forms.Padding(25, 50, 5, 5);
            this.label1.Size = new System.Drawing.Size(295, 225);
            this.label1.TabIndex = 2;
            this.label1.Text = "Waiting";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            this.label1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.label1_MouseDown);
            // 
            // labelTime
            // 
            this.labelTime.BackColor = System.Drawing.Color.Transparent;
            this.labelTime.Dock = System.Windows.Forms.DockStyle.Top;
            this.labelTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTime.ForeColor = System.Drawing.Color.White;
            this.labelTime.Location = new System.Drawing.Point(0, 0);
            this.labelTime.MinimumSize = new System.Drawing.Size(0, 50);
            this.labelTime.Name = "labelTime";
            this.labelTime.Padding = new System.Windows.Forms.Padding(0, 18, 0, 0);
            this.labelTime.Size = new System.Drawing.Size(295, 50);
            this.labelTime.TabIndex = 3;
            this.labelTime.Text = "Waiting";
            this.labelTime.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.labelTime.MouseDown += new System.Windows.Forms.MouseEventHandler(this.labelTime_MouseDown);
            // 
            // StatusForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.White;
            this.BackgroundImage = global::HicapsConnectControl.Properties.Resources.Terminal_back2;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.ClientSize = new System.Drawing.Size(295, 225);
            this.Controls.Add(this.labelTime);
            this.Controls.Add(this.label1);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.Name = "StatusForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "HicapsConnect Transaction:";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.StatusForm_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.StatusForm_Paint);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.StatusForm_MouseDown);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelTime;


    }
}