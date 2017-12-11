namespace HicapsConnectControl
{
    partial class PanPrompt
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
            this.btnCancel = new System.Windows.Forms.Button();
            this.mtb_CCV = new System.Windows.Forms.MaskedTextBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.lblTitle = new System.Windows.Forms.Label();
            this.labelTime = new System.Windows.Forms.Label();
            this.mtb_PAN = new System.Windows.Forms.MaskedTextBox();
            this.cbMonth = new System.Windows.Forms.ComboBox();
            this.cbYear = new System.Windows.Forms.ComboBox();
            this.lbExpiry = new System.Windows.Forms.Label();
            this.lbNumber = new System.Windows.Forms.Label();
            this.lbCCV = new System.Windows.Forms.Label();
            this.lbReason = new System.Windows.Forms.Label();
            this.cbReason = new System.Windows.Forms.ComboBox();
            this.cbSource = new System.Windows.Forms.ComboBox();
            this.lbSource = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Location = new System.Drawing.Point(26, 148);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(53, 22);
            this.btnCancel.TabIndex = 6;
            this.btnCancel.Text = "&Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click_1);
            // 
            // mtb_CCV
            // 
            this.mtb_CCV.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.mtb_CCV.HidePromptOnLeave = true;
            this.mtb_CCV.Location = new System.Drawing.Point(68, 72);
            this.mtb_CCV.Mask = "000000";
            this.mtb_CCV.Name = "mtb_CCV";
            this.mtb_CCV.Size = new System.Drawing.Size(46, 22);
            this.mtb_CCV.TabIndex = 1;
            this.mtb_CCV.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.mtb_CCV.TextChanged += new System.EventHandler(this.mtb_CCV_TextChanged);
            this.mtb_CCV.KeyDown += new System.Windows.Forms.KeyEventHandler(this.mtb_CCV_KeyDown);
            this.mtb_CCV.Leave += new System.EventHandler(this.mtb_CCV_Leave);
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnOK.Location = new System.Drawing.Point(216, 148);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(53, 22);
            this.btnOK.TabIndex = 7;
            this.btnOK.Text = "&OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click_1);
            // 
            // lblTitle
            // 
            this.lblTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTitle.AutoSize = true;
            this.lblTitle.BackColor = System.Drawing.Color.Transparent;
            this.lblTitle.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(89, 6);
            this.lblTitle.Margin = new System.Windows.Forms.Padding(3);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(0, 13);
            this.lblTitle.TabIndex = 9;
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.TopCenter;
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
            this.labelTime.TabIndex = 10;
            this.labelTime.Text = "Enter the Credit Card Information";
            this.labelTime.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.labelTime.MouseDown += new System.Windows.Forms.MouseEventHandler(this.labelTime_MouseDown);
            // 
            // mtb_PAN
            // 
            this.mtb_PAN.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.mtb_PAN.HidePromptOnLeave = true;
            this.mtb_PAN.Location = new System.Drawing.Point(68, 48);
            this.mtb_PAN.Mask = "0000 0000 0000 0000";
            this.mtb_PAN.Name = "mtb_PAN";
            this.mtb_PAN.Size = new System.Drawing.Size(205, 22);
            this.mtb_PAN.TabIndex = 0;
            this.mtb_PAN.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.mtb_PAN.TextMaskFormat = System.Windows.Forms.MaskFormat.ExcludePromptAndLiterals;
            this.mtb_PAN.KeyDown += new System.Windows.Forms.KeyEventHandler(this.mtb_PAN_KeyDown);
            // 
            // cbMonth
            // 
            this.cbMonth.AutoCompleteCustomSource.AddRange(new string[] {
            "01",
            "02",
            "03",
            "04",
            "05",
            "06",
            "07",
            "08",
            "09",
            "10",
            "11",
            "12"});
            this.cbMonth.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cbMonth.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cbMonth.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbMonth.FormattingEnabled = true;
            this.cbMonth.Items.AddRange(new object[] {
            "01",
            "02",
            "03",
            "04",
            "05",
            "06",
            "07",
            "08",
            "09",
            "10",
            "11",
            "12"});
            this.cbMonth.Location = new System.Drawing.Point(68, 121);
            this.cbMonth.Name = "cbMonth";
            this.cbMonth.Size = new System.Drawing.Size(46, 21);
            this.cbMonth.Sorted = true;
            this.cbMonth.TabIndex = 4;
            this.cbMonth.KeyDown += new System.Windows.Forms.KeyEventHandler(this.cbMonth_KeyDown);
            // 
            // cbYear
            // 
            this.cbYear.AutoCompleteCustomSource.AddRange(new string[] {
            "01",
            "02",
            "03",
            "04",
            "05",
            "06",
            "07",
            "08",
            "09",
            "10",
            "11",
            "12"});
            this.cbYear.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cbYear.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cbYear.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbYear.FormattingEnabled = true;
            this.cbYear.Items.AddRange(new object[] {
            "01",
            "02",
            "03",
            "04",
            "05",
            "06",
            "07",
            "08",
            "09",
            "10",
            "11",
            "12"});
            this.cbYear.Location = new System.Drawing.Point(123, 121);
            this.cbYear.Name = "cbYear";
            this.cbYear.Size = new System.Drawing.Size(62, 21);
            this.cbYear.TabIndex = 5;
            this.cbYear.KeyDown += new System.Windows.Forms.KeyEventHandler(this.cbYear_KeyDown);
            // 
            // lbExpiry
            // 
            this.lbExpiry.BackColor = System.Drawing.Color.Transparent;
            this.lbExpiry.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbExpiry.Location = new System.Drawing.Point(22, 125);
            this.lbExpiry.Name = "lbExpiry";
            this.lbExpiry.Size = new System.Drawing.Size(35, 13);
            this.lbExpiry.TabIndex = 15;
            this.lbExpiry.Text = "Expiry";
            // 
            // lbNumber
            // 
            this.lbNumber.BackColor = System.Drawing.Color.Transparent;
            this.lbNumber.Location = new System.Drawing.Point(22, 53);
            this.lbNumber.Name = "lbNumber";
            this.lbNumber.Size = new System.Drawing.Size(44, 13);
            this.lbNumber.TabIndex = 16;
            this.lbNumber.Text = "Number";
            // 
            // lbCCV
            // 
            this.lbCCV.AutoSize = true;
            this.lbCCV.BackColor = System.Drawing.Color.Transparent;
            this.lbCCV.Location = new System.Drawing.Point(22, 77);
            this.lbCCV.Name = "lbCCV";
            this.lbCCV.Size = new System.Drawing.Size(28, 13);
            this.lbCCV.TabIndex = 17;
            this.lbCCV.Text = "CCV";
            // 
            // lbReason
            // 
            this.lbReason.BackColor = System.Drawing.Color.Transparent;
            this.lbReason.Location = new System.Drawing.Point(120, 77);
            this.lbReason.Name = "lbReason";
            this.lbReason.Size = new System.Drawing.Size(44, 13);
            this.lbReason.TabIndex = 18;
            this.lbReason.Text = "Reason";
            // 
            // cbReason
            // 
            this.cbReason.AutoCompleteCustomSource.AddRange(new string[] {
            "01",
            "02",
            "03",
            "04",
            "05",
            "06",
            "07",
            "08",
            "09",
            "10",
            "11",
            "12"});
            this.cbReason.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbReason.DropDownWidth = 110;
            this.cbReason.FormattingEnabled = true;
            this.cbReason.Items.AddRange(new object[] {
            "01",
            "02",
            "03",
            "04",
            "05",
            "06",
            "07",
            "08",
            "09",
            "10",
            "11",
            "12"});
            this.cbReason.Location = new System.Drawing.Point(169, 72);
            this.cbReason.Name = "cbReason";
            this.cbReason.Size = new System.Drawing.Size(103, 21);
            this.cbReason.TabIndex = 2;
            this.cbReason.KeyDown += new System.Windows.Forms.KeyEventHandler(this.cbReason_KeyDown);
            // 
            // cbSource
            // 
            this.cbSource.AutoCompleteCustomSource.AddRange(new string[] {
            "01",
            "02",
            "03",
            "04",
            "05",
            "06",
            "07",
            "08",
            "09",
            "10",
            "11",
            "12"});
            this.cbSource.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbSource.DropDownWidth = 110;
            this.cbSource.FormattingEnabled = true;
            this.cbSource.Items.AddRange(new object[] {
            "01",
            "02",
            "03",
            "04",
            "05",
            "06",
            "07",
            "08",
            "09",
            "10",
            "11",
            "12"});
            this.cbSource.Location = new System.Drawing.Point(68, 97);
            this.cbSource.Name = "cbSource";
            this.cbSource.Size = new System.Drawing.Size(206, 21);
            this.cbSource.TabIndex = 3;
            this.cbSource.KeyDown += new System.Windows.Forms.KeyEventHandler(this.cbSource_KeyDown);
            // 
            // lbSource
            // 
            this.lbSource.BackColor = System.Drawing.Color.Transparent;
            this.lbSource.Location = new System.Drawing.Point(22, 102);
            this.lbSource.Name = "lbSource";
            this.lbSource.Size = new System.Drawing.Size(41, 13);
            this.lbSource.TabIndex = 20;
            this.lbSource.Text = "Source";
            // 
            // PanPrompt
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.BackgroundImage = global::HicapsConnectControl.Properties.Resources.Terminal_back2;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.ClientSize = new System.Drawing.Size(295, 225);
            this.ControlBox = false;
            this.Controls.Add(this.cbSource);
            this.Controls.Add(this.lbSource);
            this.Controls.Add(this.cbReason);
            this.Controls.Add(this.lbReason);
            this.Controls.Add(this.lbCCV);
            this.Controls.Add(this.lbNumber);
            this.Controls.Add(this.lbExpiry);
            this.Controls.Add(this.cbYear);
            this.Controls.Add(this.cbMonth);
            this.Controls.Add(this.mtb_CCV);
            this.Controls.Add(this.mtb_PAN);
            this.Controls.Add(this.labelTime);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.lblTitle);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "PanPrompt";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "PanPrompt";
            this.TopMost = true;
            this.Activated += new System.EventHandler(this.PanPrompt_Activated);
            this.Load += new System.EventHandler(this.PanPrompt_Load);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PanPrompt_MouseDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.MaskedTextBox mtb_CCV;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label labelTime;
        private System.Windows.Forms.MaskedTextBox mtb_PAN;
        private System.Windows.Forms.ComboBox cbMonth;
        private System.Windows.Forms.ComboBox cbYear;
        private System.Windows.Forms.Label lbExpiry;
        private System.Windows.Forms.Label lbNumber;
        private System.Windows.Forms.Label lbCCV;
        private System.Windows.Forms.Label lbReason;
        private System.Windows.Forms.ComboBox cbReason;
        private System.Windows.Forms.ComboBox cbSource;
        private System.Windows.Forms.Label lbSource;
    }
}