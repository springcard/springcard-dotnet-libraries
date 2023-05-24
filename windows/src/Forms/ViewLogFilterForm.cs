using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpringCard.LibCs.Windows.Forms
{
    public partial class ViewLoggerFilterForm : Form
    {
        public ViewLoggerFilterForm()
        {
            InitializeComponent();
        }

        public void SetDataAndFilters(List<string> sources, List<string> instances, List<string> filterSource, List<string> filterInstance)
        {
            clbSourceFilter.Items.Clear();
            foreach (string source in sources)
            {
                CheckState state = CheckState.Unchecked;
                if (filterSource.Contains(source))
                    state = CheckState.Checked;
                clbSourceFilter.Items.Add(source, state);
            }
            clbInstanceFilter.Items.Clear();
            foreach (string instance in instances)
            {
                CheckState state = CheckState.Unchecked;
                if (filterInstance.Contains(instance))
                    state = CheckState.Checked;
                clbInstanceFilter.Items.Add(instance, state);
            }
        }

        public void GetFilters(List<string> filterSource, List<string> filterInstance)
        {
            filterSource.Clear();
            foreach (var item in clbSourceFilter.CheckedItems)
            {
                filterSource.Add(item.ToString());
            }
            filterInstance.Clear();
            foreach (var item in clbInstanceFilter.CheckedItems)
            {
                filterInstance.Add(item.ToString());
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void lkClearSource_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            for (int i = 0; i < clbSourceFilter.Items.Count; i++)
                clbSourceFilter.SetItemCheckState(i, CheckState.Unchecked);
        }

        private void lbClearInstance_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            for (int i = 0; i < clbInstanceFilter.Items.Count; i++)
                clbInstanceFilter.SetItemCheckState(i, CheckState.Unchecked);
        }
    }
}
