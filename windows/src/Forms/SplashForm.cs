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
using System.Drawing;
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

            Forms.ApplyStyle(form, style);

            Color backColor;
            Color textColor;
            Color accentColor;

            switch (style)
            {
                case FormStyle.ModernMarroon:
                    backColor = Forms.MarroonColor;
                    accentColor = Forms.DarkMarroonColor;
                    textColor = Forms.WhiteColor;
                    break;

                case FormStyle.ModernRed:
                    backColor = Forms.RedColor;
                    accentColor = Forms.DarkRedColor;
                    textColor = Forms.WhiteColor;
                    break;

                case FormStyle.ModernWhite:
                    backColor = Forms.WhiteColor;
                    accentColor = Forms.DarkWhiteColor;
                    textColor = Forms.BlackColor;
                    break;

                case FormStyle.ModernBlack:
                    backColor = Forms.BlackColor;
                    accentColor = Forms.GreyColor;
                    textColor = Forms.WhiteColor;
                    break;

                case FormStyle.ModernAccent1Red:
                case FormStyle.ModernAccent1:
                    backColor = Forms.Accent1Color;
                    accentColor = Forms.GreyColor;
                    textColor = Forms.WhiteColor;
                    break;

                case FormStyle.ModernAccent2Red:
                case FormStyle.ModernAccent2:
                    backColor = Forms.Accent2Color;
                    accentColor = Forms.GreyColor;
                    textColor = Forms.WhiteColor;
                    break;

                case FormStyle.Classical:
                case FormStyle.Default:
                case FormStyle.Modern:
                default:
                    backColor = Forms.ClassicColor;
                    accentColor = Forms.DarkClassicColor;
                    textColor = Forms.BlackColor;
                    break;
            }

            form.BackColor = backColor;

            form.lbProduct.BackColor = backColor;
            form.lbDisclaimer1.BackColor = backColor;
            form.lbDisclaimer2.BackColor = backColor;
            form.lbDisclaimer3.BackColor = backColor;

            form.pBottom.BackColor = accentColor;
            form.lbCopyright.BackColor = accentColor;
            form.lbVersion.BackColor = accentColor;

            form.lbProduct.ForeColor = textColor;
            form.lbDisclaimer1.ForeColor = textColor;
            form.lbDisclaimer2.ForeColor = textColor;
            form.lbDisclaimer3.ForeColor = textColor;
            form.lbCopyright.ForeColor = textColor;
            form.lbVersion.ForeColor = textColor;

            form.lbPrivate.BackColor = textColor;
            form.lbPrivate.ForeColor = accentColor;



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
