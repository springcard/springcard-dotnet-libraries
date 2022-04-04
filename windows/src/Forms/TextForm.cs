using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpringCard.LibCs.Windows.Forms
{
    public partial class TextForm : Form
    {
        void Prepare(string Title, string Subtitle, string Text)
        {
            lbTitle.Text = Title;
            lbSubTitle.Text = Subtitle;
            eText.Text = Text;

            Size size = TextRenderer.MeasureText(eText.Text, eText.Font, new Size(eText.Width, eText.Height), TextFormatFlags.WordBreak);
        }

        public TextForm(string Title, string Subtitle, string Text)
        {
            InitializeComponent();
            Prepare(Title, Subtitle, Text);
        }

        public TextForm(string Title, string Subtitle, List<string> Text)
        {
            InitializeComponent();
            Prepare(Title, Subtitle, string.Join("\n", Text));
        }

        public TextForm(string Title, string Subtitle, string[] Text)
        {
            InitializeComponent();
            Prepare(Title, Subtitle, string.Join("\n", Text));
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
