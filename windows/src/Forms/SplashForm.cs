/*
 * Created by SharpDevelop.
 * User: johann
 * Date: 02/03/2012
 * Time: 17:56
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;

namespace SpringCard.LibCs.Windows.Forms
{
    /// <summary>
    /// Description of SplashForm.
    /// </summary>
    public partial class SplashForm : Form
    {
        private static Logger logger = new Logger(typeof(SplashForm).FullName);
        private FormStyle style = FormStyle.Default;

        public SplashForm()
        {
            InitializeComponent();

            lbPrivate.Visible = AppUtils.IsSpringCardPrivate();

            Assembly programAssembly = Assembly.GetEntryAssembly();

            try
            {
                FileVersionInfo i = FileVersionInfo.GetVersionInfo(programAssembly.Location);
                string s = i.ProductName;
                string[] e = i.ProductVersion.Split('.');
                if (e.Length < 4)
                {
                    lbVersion.Text = "v." + i.ProductVersion;
                }
                else
                {
                    s += string.Format(" {0}.{1}", e[0], e[1]);
                    lbVersion.Text = string.Format("{2}.{3}", e[0], e[1], e[2], e[3]);
                }
                lbProduct.Text = s;
                lbCopyright.Text = i.LegalCopyright;
            }
            catch (Exception e)
            {
                logger.debug(e.Message);
            }

            lbDisclaimer2.Text = T._("This application is a free, unsupported software.");
            lbDisclaimer3.Text = T._("See the LICENSE.txt file or the \"About\" box for details.");
        }

        protected override CreateParams CreateParams
        {
            get
            {
                const int CS_DROPSHADOW = 0x20000;
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= CS_DROPSHADOW;
                return cp;
            }
        }

        void SplashFormClose(object sender, EventArgs e)
        {
            Close();
        }

        /**
		 * \brief Display the splash form
         */
        public static void DoShowDialog(Form parent)
        {
            DoShowDialog(parent, FormStyle.Default);
        }

        /**
		 * \brief Display the about form as a modal dialog box, specifying the style
         */
        public static void DoShowDialog(Form parent, FormStyle style)
        {
            SplashForm form;
            form = new SplashForm();
            form.style = style;

            form.imgSplashLight.Visible = false;
            form.imgSplashRed.Visible = false;
            form.imgSplashMarroon.Visible = false;

            switch (style)
            {
                case FormStyle.ModernMarroon:
                    form.lbProduct.BackColor = Forms.MarroonColor;
                    form.lbDisclaimer1.BackColor = Forms.MarroonColor;
                    form.lbDisclaimer2.BackColor = Forms.MarroonColor;
                    form.lbDisclaimer3.BackColor = Forms.MarroonColor;
                    form.lbCopyright.BackColor = Forms.DarkMarroonColor;
                    form.lbVersion.BackColor = Forms.DarkMarroonColor;
                    form.lbProduct.ForeColor = Forms.WhiteColor;
                    form.lbDisclaimer1.ForeColor = Forms.WhiteColor;
                    form.lbDisclaimer2.ForeColor = Forms.WhiteColor;
                    form.lbDisclaimer3.ForeColor = Forms.WhiteColor;
                    form.lbCopyright.ForeColor = Forms.WhiteColor;
                    form.lbVersion.ForeColor = Forms.WhiteColor;
                    form.imgSplashMarroon.Visible = true;
                    break;

                case FormStyle.ModernRed:
                    form.lbProduct.BackColor = Forms.RedColor;
                    form.lbDisclaimer1.BackColor = Forms.RedColor;
                    form.lbDisclaimer2.BackColor = Forms.RedColor;
                    form.lbDisclaimer3.BackColor = Forms.RedColor;
                    form.lbCopyright.BackColor = Forms.DarkRedColor;
                    form.lbVersion.BackColor = Forms.DarkRedColor;
                    form.lbProduct.ForeColor = Forms.WhiteColor;
                    form.lbDisclaimer1.ForeColor = Forms.WhiteColor;
                    form.lbDisclaimer2.ForeColor = Forms.WhiteColor;
                    form.lbDisclaimer3.ForeColor = Forms.WhiteColor;
                    form.lbCopyright.ForeColor = Forms.WhiteColor;
                    form.lbVersion.ForeColor = Forms.WhiteColor;
                    form.imgSplashRed.Visible = true;
                    break;

                case FormStyle.ModernWhite:
                    form.BackColor = Forms.WhiteColor;
                    form.lbProduct.BackColor = Forms.WhiteColor;
                    form.lbDisclaimer1.BackColor = Forms.WhiteColor;
                    form.lbDisclaimer2.BackColor = Forms.WhiteColor;
                    form.lbDisclaimer3.BackColor = Forms.WhiteColor;
                    form.lbCopyright.BackColor = Forms.DarkWhiteColor;
                    form.lbVersion.BackColor = Forms.DarkWhiteColor;
                    form.lbProduct.ForeColor = Forms.BlackColor;
                    form.lbDisclaimer1.ForeColor = Forms.BlackColor;
                    form.lbDisclaimer2.ForeColor = Forms.BlackColor;
                    form.lbDisclaimer3.ForeColor = Forms.BlackColor;
                    form.lbCopyright.ForeColor = Forms.BlackColor;
                    form.lbVersion.ForeColor = Forms.BlackColor;
                    form.pBottomWhite.BackColor = Forms.DarkWhiteColor;
                    form.pBottomWhite.Visible = true;
                    form.imgSplashWhite.Visible = true;
                    break;

                case FormStyle.Classical:
                case FormStyle.Default:
                case FormStyle.Modern:
                default:
                    form.lbProduct.BackColor = Forms.ClassicColor;
                    form.lbDisclaimer1.BackColor = Forms.ClassicColor;
                    form.lbDisclaimer2.BackColor = Forms.ClassicColor;
                    form.lbDisclaimer3.BackColor = Forms.ClassicColor;
                    form.lbCopyright.BackColor = Forms.DarkClassicColor;
                    form.lbVersion.BackColor = Forms.DarkClassicColor;
                    form.lbProduct.ForeColor = Forms.BlackColor;
                    form.lbDisclaimer1.ForeColor = Forms.BlackColor;
                    form.lbDisclaimer2.ForeColor = Forms.BlackColor;
                    form.lbDisclaimer3.ForeColor = Forms.BlackColor;
                    form.lbCopyright.ForeColor = Forms.BlackColor;
                    form.lbVersion.ForeColor = Forms.BlackColor;
                    form.imgSplashLight.Visible = true;
                    break;
            }
            if (parent != null)
            {
                form.StartPosition = FormStartPosition.CenterParent;
                form.ShowDialog(parent);
            }
            else
            {
                form.StartPosition = FormStartPosition.CenterScreen;
                form.ShowDialog();
            }
        }

        private void SplashForm_FormClosed(object sender, FormClosedEventArgs e)
        {

        }
    }
}
