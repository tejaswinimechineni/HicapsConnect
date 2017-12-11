using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using HicapsTools;

namespace HicapsConnectControl
{
    [ComVisible(false)]
    internal sealed partial class PanPrompt : Form
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.I4)]
        static extern int SendMessage(IntPtr hWnd, [param: MarshalAs(UnmanagedType.U4)]uint Msg, [param: MarshalAs(UnmanagedType.U4)]uint wParam, [param: MarshalAs(UnmanagedType.I4)]int lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ReleaseCapture();

        const uint WM_NCLBUTTONDOWN = 0xA1; // 161
        const uint HTCAPTION = 2;


        private HicapsConnectControl.SaleRequest mySaleRequest;
        private HicapsConnectControl.RefundRequest myRefundRequest;

        public PanPrompt()
        {
            // Make the GUI ignore the DPI setting
            Font = new Font(Font.Name, 8.25f * 96f / CreateGraphics().DpiX, Font.Style, Font.Unit, Font.GdiCharSet, Font.GdiVerticalFont);
            InitializeComponent();
            
            foreach(Control control in this.Controls){ FixFont(control);}            

        }
        private void FixFont(Control control)
        {
            var font = new Font(control.Font.Name, control.Font.Size * 96f / CreateGraphics().DpiX, control.Font.Style, control.Font.Unit, control.Font.GdiCharSet, control.Font.GdiVerticalFont);
            control.Font = font;
        }
        public PanPrompt(ref HicapsConnectControl.SaleRequest myRequest)
        {
            // TODO: Complete member initialization
            this.mySaleRequest = myRequest;
            InitializeComponent();
 
        }

        public PanPrompt(ref HicapsConnectControl.RefundRequest myRequest_2)
        {
            // TODO: Complete member initialization
            this.myRefundRequest = myRequest_2;
            InitializeComponent();
 
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
   
        private void PanPrompt_Load(object sender, EventArgs e)
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
                this.lbCCV.Refresh();
                this.labelTime.Refresh();
            }
            catch (Exception)
            { }

            this.mtb_PAN.Focus();
            // Populoate Combo Boxes
            PopulateYearCombo();
            PopulateSourceReasonCombo();
        }
        private void PopulateSourceReasonCombo()
        {
            PopulateReason();
            PopulateSource();
            if (!((mtb_CCV.Text ?? "").isNumeric()))
            {
                cbReason.Enabled = true;
                lbReason.Enabled = true;
                cbSource.Enabled = false;
                cbSource.Text = "";
                cbSource.SelectedIndex = -1;
                cbSource.Update();
                lbSource.Enabled = false;
            }
            else
            {
                cbReason.Text = "";
                cbReason.SelectedIndex = -1;
                cbReason.Update();

                cbReason.Enabled = false;
                lbReason.Enabled = false;
                cbSource.Enabled = true;
                lbSource.Enabled = true;

            }
        }

        private void PopulateSource()
        {
            if (cbReason.Items.Count != 4)
            {
                cbReason.Enabled = true;
                lbSource.Text = "Source";
                cbSource.Text = "";
                cbSource.Items.Clear();
                cbSource.Items.Add("1. Unknown");
                cbSource.Items.Add("2. Telephone");
                cbSource.Items.Add("3. Mail");
                cbSource.Items.Add("4. Internet");
            }
        }

        private void PopulateReason()
        {
            if (cbReason.Items.Count != 3)
            {
                cbReason.Enabled = true;
                lbReason.Text = "Reason";
                cbReason.Text = "";
                cbReason.Items.Clear();
                cbReason.Items.Add("1. No CCV on Card");
                cbReason.Items.Add("2. CCV Not Readable");
                cbReason.Items.Add("3. CCV Bypassed");
            }
        }
        private void PopulateYearCombo()
        {
            cbYear.Items.Clear();
            cbYear.AutoCompleteCustomSource.Clear();
            for (int i = 0; i < 8; i++)
            {
                cbYear.Items.Add(DateTime.Now.AddYears(i).Year.ToString());
                cbYear.AutoCompleteCustomSource.Add(DateTime.Now.AddYears(i).Year.ToString());
            }
        }





        private void btnOK_Click_1(object sender, EventArgs e)
        {
            SaveClose();
        }

        private bool ProcessPanFields()
        {
        
            if (mtb_PAN.Text.isNumeric())
            {
                if (!ValidatePanFields()) {return false ;}
                // Got Down here to now update request object.
                 if (myRefundRequest != null)
                 {
                     myRefundRequest.PrimaryAccountNumber = mtb_PAN.Text;
                     myRefundRequest.ExpiryDate = cbMonth.Text + cbYear.Text.Substring(2, 2);
                     myRefundRequest.CCV = mtb_CCV.Text;

                     if (mtb_CCV.Text.isNumeric())
                     {
                         if (!string.IsNullOrEmpty(cbSource.Text.Trim()))
                         {
                             myRefundRequest.CCVSource = int.Parse(cbSource.Text.Substring(0, 1));
                         }
                     }else
                     {
                         if (!string.IsNullOrEmpty(cbReason.Text.Trim()))
                         {
                             myRefundRequest.CCVReason = int.Parse(cbReason.Text.Substring(0, 1));
                         }
                     }
                 }

                 if (mySaleRequest != null)
                 {
                     mySaleRequest.PrimaryAccountNumber = mtb_PAN.Text;
                     mySaleRequest.ExpiryDate = cbMonth.Text + cbYear.Text.Substring(2, 2);
                     mySaleRequest.CCV = mtb_CCV.Text;
                     if (mtb_CCV.Text.isNumeric())
                     {
                         if (!string.IsNullOrEmpty(cbSource.Text.Trim()))
                         {
                             mySaleRequest.CCVSource = int.Parse(cbSource.Text.Substring(0, 1));
                         }
                     }
                     else
                     {
                          if (!string.IsNullOrEmpty(cbReason.Text.Trim()))
                         {
                         mySaleRequest.CCVReason = int.Parse(cbReason.Text.Substring(0, 1));
                              }
                     }
                 }
                 return true;
            }
            else
            {
                // Continue let terminal swipe
                return true;
            }
            return false;
        }

        private bool ValidatePanFields()
        {
           
            string message = validateEftposPAN(mtb_PAN.Text);
            if (!string.IsNullOrEmpty(message))
            {
                this.labelTime.Text = message;
                mtb_PAN.BackColor = Color.GreenYellow;
                lbNumber.ForeColor = Color.Red;
                return false;
            }
            // Okay so PAN is correct.  Check Expiry Date.
            mtb_PAN.BackColor = Color.White;

            message = validateExpiryDate(mtb_PAN.Text, cbMonth.Text, cbYear.Text);
            if (!string.IsNullOrEmpty(message))
            {
                this.labelTime.Text = message;
                this.lbExpiry.ForeColor = Color.Red;

                return false;
            }
            this.lbExpiry.ForeColor = Color.Black;
            // Okay so Pan and Expiry is correct.  Check CCV
            message = validateCCV(mtb_CCV.Text);
            if (!string.IsNullOrEmpty(message))
            {
                this.labelTime.Text = message;
                mtb_CCV.BackColor = Color.GreenYellow;
                this.lbCCV.ForeColor = Color.Red;

                return false;
            }
            mtb_CCV.BackColor = Color.White;
            this.lbCCV.ForeColor = Color.Black;
            // Okay Pan, Expiry and CCV are fine.  Check CCV Source/Reason
            message = validateCCVReason(mtb_CCV.Text, cbReason.Text);
            if (!string.IsNullOrEmpty(message))
            {
                this.labelTime.Text = message;
                //     cbSourceReason.BackColor = Color.LightSalmon;
                this.lbReason.ForeColor = Color.Red;
                return false;
            }
            string ccvSource = cbSource.Text;
            if (ccvSource.Length > 0)
            {
                ccvSource = ccvSource.Substring(0, 1);
            }
            message = validateCCVSource(mtb_PAN.Text, mtb_CCV.Text, ccvSource);
            if (!string.IsNullOrEmpty(message))
            {
                this.labelTime.Text = message;
                //cbSourceReason.BackColor = Color.LightSalmon;
                this.lbSource.ForeColor = Color.Red;
                return false;
            }

            //  cbSourceReason.BackColor = Color.White;
            this.lbReason.ForeColor = Color.Black;
            return true;
        }
             
        protected string validateEftposPAN(string PrimaryAccountNumber)
        {

            if (string.IsNullOrEmpty(PrimaryAccountNumber))
            {
                return "";
            }
            if (!PrimaryAccountNumber.isNumeric())
            {
                return "Card number must be numeric";
            }
            if (PrimaryAccountNumber.Length > 0 && PrimaryAccountNumber.Length < 13)
            {
                return "Card number is too short";
            }
            if (PrimaryAccountNumber.Length > 19)
            {
                return "Card number too long";
            }
            return "";
        }
        protected string validateExpiryDate(string PrimaryAccountNumber, string expiryMonth, string expiryYear)
        {
            if (string.IsNullOrEmpty(expiryMonth) || string.IsNullOrEmpty(expiryYear))
            {
                // No Expiry Date is fine..
                if (string.IsNullOrEmpty(PrimaryAccountNumber))
                {
                    return "";
                }
                else
                {
                    return "Expiry Date is missing";
                }
            }

            if (expiryMonth.Length != 2)
            {
                return "Invalid Expiry Month";
            }
            if (expiryYear.Length != 4)
            {
                return "Invalid Expiry Year";
            }
            string expiryDate = expiryMonth + expiryYear.Substring(2, 2);
            if (!(expiryDate.isNumeric()))
            {
                return "Invalid Expiry Date"; 
            }
            string month = expiryDate.Substring(0, 2);
            if ("01,02,03,04,05,06,07,08,09,10,11,12,".IndexOf(month + ",") < 0)
            {
                return "Invalid Expiry Month";
            }

            return "";
        }

        protected string validateCCV(string CCV)
        {
            if (string.IsNullOrEmpty(CCV))
            {
                return "";
            }
            if (!(CCV.isNumeric()))
            {
                return "CCV must be numeric";
            }
            if (CCV.Length > 6)
            {
                return "CCV is too long";
            }
            return "";
        }

        protected string validateCCVReason( string CCV, string CCVReason)
        {
            if (string.IsNullOrEmpty(CCVReason))
            {
                if (CCV.isNumeric())
                {
                    return "";
                }
                else
                {
                    return "Missing CCV Reason";
                }
            }
            return "";
        }
        protected string validateCCVSource(string PrimaryAccountNumber, string CCV, string CCVSource)
        {
            if (string.IsNullOrEmpty(PrimaryAccountNumber))
            {
                return "";
            }
            if (string.IsNullOrEmpty(CCV))
            {
                return "";
            }
            int iCCVSource;
            if (!int.TryParse(CCVSource,out iCCVSource))
            {
                return "Missing CCV Source";
            }
            if (iCCVSource > 4 || iCCVSource < 1)
            {
                return "Invalid CCV Source";
            }
            return "";
        }
        private void btnCancel_Click_1(object sender, EventArgs e)
        {
            Abort();
        }

        private void Abort()
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }


        private void PanPrompt_Activated(object sender, EventArgs e)
        {
            this.mtb_PAN.Focus();
        }


        private void SaveClose()
        {
            if (ProcessPanFields())
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void PanPrompt_MouseDown(object sender, MouseEventArgs e)
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
            PanPrompt_MouseDown(sender, e);
        }

        private void labelTime_MouseDown(object sender, MouseEventArgs e)
        {
            PanPrompt_MouseDown(sender, e);
        }

        private void mtb_CCV_KeyDown(object sender, KeyEventArgs e)
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

        private void mtb_CCV_Leave(object sender, EventArgs e)
        {
            PopulateSourceReasonCombo();
        }

        private void mtb_CCV_TextChanged(object sender, EventArgs e)
        {
            PopulateSourceReasonCombo();
        }

        private void cbSource_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                cbSource.SelectedIndex = -1;
            }
            if (e.KeyCode == Keys.Escape)
            {
                Abort();
            }
        }

        private void cbReason_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Abort();
            }
        }

        private void mtb_PAN_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Abort();
            }
        }

        private void cbMonth_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Abort();
            }
        }

        private void cbYear_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Abort();
            }
        }

   
    }
}