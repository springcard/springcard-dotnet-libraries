using SpringCard.LibCs.Windows;
using SpringCard.LibCs.Windows.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Test
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnAboutClassic_Click(object sender, EventArgs e)
        {
            AboutForm.DoShowDialog(this, FormStyle.Classical);
        }

        private void btnAboutModern_Click(object sender, EventArgs e)
        {
            AboutForm.DoShowDialog(this, FormStyle.Modern);
        }

        private void btnAboutRed_Click(object sender, EventArgs e)
        {
            AboutForm.DoShowDialog(this, FormStyle.ModernRed);
        }

        private void btnAboutMarroon_Click(object sender, EventArgs e)
        {
            AboutForm.DoShowDialog(this, FormStyle.ModernMarroon);
        }

        private void btnAboutWhite_Click(object sender, EventArgs e)
        {
            AboutForm.DoShowDialog(this, FormStyle.ModernWhite);
        }

        private void btnAboutBlack_Click(object sender, EventArgs e)
        {
            AboutForm.DoShowDialog(this, FormStyle.ModernBlack);
        }

        private void btnSplashClassic_Click(object sender, EventArgs e)
        {
            SplashForm.DoShowDialog(this, FormStyle.Classical);
        }

        private void btnSplashModern_Click(object sender, EventArgs e)
        {
            SplashForm.DoShowDialog(this, FormStyle.Modern);
        }

        private void btnSplashRed_Click(object sender, EventArgs e)
        {
            SplashForm.DoShowDialog(this, FormStyle.ModernRed);
        }

        private void btnSplashMarroon_Click(object sender, EventArgs e)
        {
            SplashForm.DoShowDialog(this, FormStyle.ModernMarroon);
        }

        private void btnSplashWhite_Click(object sender, EventArgs e)
        {
            SplashForm.DoShowDialog(this, FormStyle.ModernWhite);
        }

        private void btnSplashBlack_Click(object sender, EventArgs e)
        {
            SplashForm.DoShowDialog(this, FormStyle.ModernBlack);
        }


        private void btnInputDialog_Click(object sender, EventArgs e)
        {
            if (InputDialogForm.Display("Entrez un texte", "S'il vous plait", "(Valeur par défaut)", out string Result, this))
            {
                MessageBox.Show(this, Result);
            }
        }

        private void btnTestUsb_Click(object sender, EventArgs e)
        {
            Console.WriteLine("TEST USB");

            List<USB.DeviceInfo> devices = USB.EnumDevices_Serial();

            foreach (USB.DeviceInfo device in devices)
            {
                Console.WriteLine(device.Mode);
                Console.WriteLine(device.FriendlyName);
                Console.WriteLine(device.Service);
                Console.WriteLine(device.SerialNumber);
                Console.WriteLine(device.Description);
                Console.WriteLine("{0:X04} {1:X04}", device.wVendorId, device.wProductId);
                string commPortName = SERIAL.GetDeviceCommPortName(device);
                Console.WriteLine(commPortName);

                Console.WriteLine();
            }
        }

        private void btnPrivate_Click(object sender, EventArgs e)
        {
            if (AppUtils.IsSpringCardPrivate())
            {
                MessageBox.Show(this, "Private!");
            }
            else
            {
                MessageBox.Show(this, "Not private!");
            }
        }

        private void btnLogViewer_Click(object sender, EventArgs e)
        {
            ViewLoggerForm f = new ViewLoggerForm();
            f.Show(this);
        }

        private void btnLogCatcher_Click(object sender, EventArgs e)
        {
            ViewLoggerForm f = new ViewLoggerForm();
            f.BeginLive(true);
            f.Show(this);
        }

        private void btnAboutAccent1Red_Click(object sender, EventArgs e)
        {
            AboutForm.DoShowDialog(this, FormStyle.ModernAccent1Red);
        }

        private void btnSplashAccent1Red_Click(object sender, EventArgs e)
        {
            SplashForm.DoShowDialog(this, FormStyle.ModernAccent1Red);
        }

        private void btnAboutAccent1_Click(object sender, EventArgs e)
        {
            AboutForm.DoShowDialog(this, FormStyle.ModernAccent1);
        }

        private void btnSplashAccent1_Click(object sender, EventArgs e)
        {
            SplashForm.DoShowDialog(this, FormStyle.ModernAccent1);
        }

        private void btnAboutAccent2Red_Click(object sender, EventArgs e)
        {
            AboutForm.DoShowDialog(this, FormStyle.ModernAccent2Red);
        }

        private void btnSplashAccent2Red_Click(object sender, EventArgs e)
        {
            SplashForm.DoShowDialog(this, FormStyle.ModernAccent2Red);
        }

        private void btnAboutAccent2_Click(object sender, EventArgs e)
        {
            AboutForm.DoShowDialog(this, FormStyle.ModernAccent2);
        }

        private void btnSplashAccent2_Click(object sender, EventArgs e)
        {
            SplashForm.DoShowDialog(this, FormStyle.ModernAccent2);
        }
    }
}
