using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace HicapsConnectControl
{
    [ComVisible(false)]
    internal sealed partial class StatusForm : Form
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.I4)]
        static extern int SendMessage(IntPtr hWnd, [param: MarshalAs(UnmanagedType.U4)]uint Msg, [param: MarshalAs(UnmanagedType.U4)]uint wParam, [param: MarshalAs(UnmanagedType.I4)]int lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ReleaseCapture();

        const uint WM_NCLBUTTONDOWN = 0xA1; // 161
        const uint HTCAPTION = 2;
        int delegateCount = 0;
        public StatusForm(HicapsConnectControl myRef)
        {
            try
            {
                // Make the GUI ignore the DPI setting
                // ALso put this same code in the Designer.cs file for every font setting
                // This seems to be the only way to fix it for all dpi windows variations.  Basically tell it to not scale.

                Font = new Font(Font.Name, 8.25f * 96f / CreateGraphics().DpiX, Font.Style, Font.Unit, Font.GdiCharSet, Font.GdiVerticalFont);
              
                InitializeComponent();
                foreach (Control control in this.Controls) { FixFont(control); }            

                
                
                //  label2.Text = string.Format("{0:HH:mm}", DateTime.Now);
             
                myRef.StatusChanged += new HicapsConnectControl.StatusChangedEventHandler(myRef_StatusChanged);
                delegateCount++;
            }
            catch (Exception)
            { }
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


        public StatusForm()
        {
            try
            {
             InitializeComponent();
             label1.Visible = false;
           
            }
            catch (Exception)
            { }
        }
        void myRef_StatusChanged(string param)
        {
            try
            {
                SetText(param);
            }
            catch (Exception)
            { }
        }
        // This delegate enables asynchronous calls for setting
        // the text property on a TextBox control.
        delegate void SetTextCallback(string text);

        public void SetText(string text)
        {
            try
            {
                // InvokeRequired required compares the thread ID of the
                // calling thread to the thread ID of the creating thread.
                // If these threads are different, it returns true.
                if (this.label1.InvokeRequired)
                {
                    SetTextCallback d = new SetTextCallback(SetText);
                    this.Invoke(d, new object[] { text });
                }
                else
                {
                    try
                    {
                        this.Refresh();
                        text = (text ?? "").TrimEnd('\x0a').TrimEnd('\x0d').TrimEnd(' ');
                        if (this.label1.Text == text)
                        {

                        }
                        this.label1.Text = text;
                        this.label1.Refresh();
                        this.labelTime.Text = string.Format("{0:dd MMM yyyy}             {0:HH:mm:ss}", DateTime.Now);
                        this.labelTime.Refresh();
                        Application.DoEvents();
                    }
                    catch (Exception)
                    {

                    }
                }
            }
            catch (Exception)
            { }
        }

        private void StatusForm_Load(object sender, EventArgs e)
        {
            GraphicsPath path = new GraphicsPath();
            path.
                AddEllipse(0, 0, this.Width, this.Height);
            //
            try
            {
                Rectangle r = new Rectangle(0, 1, 293, 221);
                int d = 24;
                GraphicsPath gp = new GraphicsPath();
                gp.AddArc(r.X, r.Y, d, d, 180, 90);
                gp.AddArc(r.X + r.Width - d, r.Y, d, d, 270, 90);
                d=24;
                gp.AddArc(r.X + r.Width - d, r.Y + r.Height - d, d, d, 0, 90);
             //   d = 20;
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
        }

        private void StatusForm_Paint(object sender, PaintEventArgs e)
        {
            //Bitmap bmpSBBack;    //Create Bitmap Object

            //bmpSBBack = (Bitmap)this.BackgroundImage;    //Obtain Form Image

            ////Make All White Pixels Transparent
            //bmpSBBack.MakeTransparent(Color.White);
        }

        private void StatusForm_MouseDown(object sender, MouseEventArgs e)
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
            if (e.Clicks == 2) { this.Visible = false; return; }
            StatusForm_MouseDown(sender, e);
        }

        private void labelTime_MouseDown(object sender, MouseEventArgs e)
        {
            StatusForm_MouseDown(sender, e);
        }

        private void label1_Click(object sender, EventArgs e)
        {
            this.Visible = false;
        }



    }
}
