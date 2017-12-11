using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace HicapsConnectControl
{
    [ComVisible(false)]
    internal sealed partial class RefundPwd : Form
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.I4)]
        static extern int SendMessage(IntPtr hWnd, [param: MarshalAs(UnmanagedType.U4)]uint Msg, [param: MarshalAs(UnmanagedType.U4)]uint wParam, [param: MarshalAs(UnmanagedType.I4)]int lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ReleaseCapture();

        const uint WM_NCLBUTTONDOWN = 0xA1; // 161
        const uint HTCAPTION = 2;

        //static RefundPwd newMessageBox;
        //static string Button_id;
        internal string Passwd;

        public RefundPwd()
        {
            // Make the GUI ignore the DPI setting
            Font = new Font(Font.Name, 8.25f * 96f / CreateGraphics().DpiX, Font.Style, Font.Unit, Font.GdiCharSet, Font.GdiVerticalFont);

            InitializeComponent();

            foreach (Control control in this.Controls) { FixFont(control); }            


            tbPassword.Focus();
            tbPassword.Update();
        }

        private void FixFont(Control control)
        {
            var font = new Font(control.Font.Name, control.Font.Size * 96f / CreateGraphics().DpiX, control.Font.Style, control.Font.Unit, control.Font.GdiCharSet, control.Font.GdiVerticalFont);
            control.Font = font;
        }
        //Drop Shadow
        protected override CreateParams CreateParams
        {
            get
            {
                const int CS_DROPSHADOW = 0x20000;
                CreateParams param = base.CreateParams;
                param.ClassStyle += CS_DROPSHADOW;
                return param;
            }
        }
        public static string ShowBox(string txtMessage)
        {
            RefundPwd newMessageBox;
            newMessageBox = new RefundPwd();
            newMessageBox.label1.Text = txtMessage;
            newMessageBox.ShowDialog();
            return newMessageBox.Passwd;
        }

        public static string ShowBox(string txtMessage, string txtTitle)
        {
            RefundPwd newMessageBox;
            newMessageBox = new RefundPwd();
            newMessageBox.lblTitle.Text = txtTitle;
            newMessageBox.label1.Text = txtMessage;
            newMessageBox.tbPassword.Focus();
            //tbPassword.Update();
            newMessageBox.ShowDialog();
            return newMessageBox.Passwd;
        }

        private void RefundPwd_Load(object sender, EventArgs e)
        {
            GraphicsPath path = new GraphicsPath();
            path.
                AddEllipse(0, 0, this.Width, this.Height);
            //
            try
            {
                Rectangle r = new Rectangle(0, 0, 293, 222);
                int d = 20;
                GraphicsPath gp = new GraphicsPath();
                gp.AddArc(r.X, r.Y, d, d, 180, 90);
                gp.AddArc(r.X + r.Width - d, r.Y, d, d, 270, 90);
                gp.AddArc(r.X + r.Width - d, r.Y + r.Height - d, d, d, 0, 90);
                gp.AddArc(r.X, r.Y + r.Height - d, d, d, 90, 90);
                gp.AddLine(r.X, r.Y + r.Height - d, r.X, r.Y + d / 2);
                this.Region = new Region(gp);
            }
            catch (Exception)
            { }
            try
            {
                this.Refresh();
                this.label1.Refresh();
                this.labelTime.Refresh();
            }
            catch (Exception)
            { }

            this.tbPassword.Focus();
            //tbPassword.Update();
        }





        private void tbPassword_TextChanged(object sender, EventArgs e)
        {
            Passwd = tbPassword.Text;
        }

        private void btnOK_Click_1(object sender, EventArgs e)
        {
            SaveClose();
        }

        private bool ProcessPassword()
        {
            if (tbPassword.Text == "")
            {
                label1.Text = "Password must be entered";
                Refresh();
                return false;
            }
            else
            {
                string strDate = DateTime.Now.ToString("ddMMyy");
                int x = int.Parse(strDate);
                int y = int.Parse(tbPassword.Text);
                int z = 386745;
                Passwd = string.Format("{0:d6}", x + y + z);
                return true;
            }
        }

        private void btnCancel_Click_1(object sender, EventArgs e)
        {
            Abort();
        }

        private void Abort()
        {
            this.Passwd = null;
            this.Close();
        }



        private void tbPassword_MaskInputRejected(object sender, MaskInputRejectedEventArgs e)
        {

        }

        private void RefundPwd_Activated(object sender, EventArgs e)
        {
            this.tbPassword.Focus();
        }

        private void tbPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SaveClose();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                Abort();
            }
        }

        private void SaveClose()
        {
            if (ProcessPassword())
            {
                this.Close();
            }
        }

        private void RefundPwd_MouseDown(object sender, MouseEventArgs e)
        {
            // Releasing the mouse capture to allow the form to receive the order
            ReleaseCapture();
            // Ordering the form
            // Simulating left mouse button clicking on the title bar
            SendMessage(this.Handle, // Form handle
                WM_NCLBUTTONDOWN, // Simulating the click
                HTCAPTION, // Attaching it to the title bar
                0); // No further options required
            Application.DoEvents();
        }

        private void label1_MouseDown(object sender, MouseEventArgs e)
        {
            RefundPwd_MouseDown(sender, e);
        }

        private void labelTime_MouseDown(object sender, MouseEventArgs e)
        {
            RefundPwd_MouseDown(sender, e);
        }
    }
}