
namespace SpringCard.LibCs.Windows.Forms
{
    partial class ViewLoggerFilterForm
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
            this.clbSourceFilter = new System.Windows.Forms.CheckedListBox();
            this.clbInstanceFilter = new System.Windows.Forms.CheckedListBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lkClearSource = new System.Windows.Forms.LinkLabel();
            this.lbClearInstance = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // clbSourceFilter
            // 
            this.clbSourceFilter.CheckOnClick = true;
            this.clbSourceFilter.FormattingEnabled = true;
            this.clbSourceFilter.Location = new System.Drawing.Point(12, 25);
            this.clbSourceFilter.Name = "clbSourceFilter";
            this.clbSourceFilter.Size = new System.Drawing.Size(440, 157);
            this.clbSourceFilter.TabIndex = 1;
            // 
            // clbInstanceFilter
            // 
            this.clbInstanceFilter.CheckOnClick = true;
            this.clbInstanceFilter.FormattingEnabled = true;
            this.clbInstanceFilter.Location = new System.Drawing.Point(12, 224);
            this.clbInstanceFilter.Name = "clbInstanceFilter";
            this.clbInstanceFilter.Size = new System.Drawing.Size(440, 157);
            this.clbInstanceFilter.TabIndex = 2;
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(163, 406);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 3;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(244, 406);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(45, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Source:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(12, 208);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Instance:";
            // 
            // lkClearSource
            // 
            this.lkClearSource.AutoSize = true;
            this.lkClearSource.Location = new System.Drawing.Point(419, 9);
            this.lkClearSource.Name = "lkClearSource";
            this.lkClearSource.Size = new System.Drawing.Size(33, 13);
            this.lkClearSource.TabIndex = 7;
            this.lkClearSource.TabStop = true;
            this.lkClearSource.Text = "Clear";
            this.lkClearSource.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lkClearSource_LinkClicked);
            // 
            // lbClearInstance
            // 
            this.lbClearInstance.AutoSize = true;
            this.lbClearInstance.Location = new System.Drawing.Point(419, 208);
            this.lbClearInstance.Name = "lbClearInstance";
            this.lbClearInstance.Size = new System.Drawing.Size(33, 13);
            this.lbClearInstance.TabIndex = 8;
            this.lbClearInstance.TabStop = true;
            this.lbClearInstance.Text = "Clear";
            this.lbClearInstance.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lbClearInstance_LinkClicked);
            // 
            // ViewLogFilterForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(464, 441);
            this.ControlBox = false;
            this.Controls.Add(this.lbClearInstance);
            this.Controls.Add(this.lkClearSource);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.clbInstanceFilter);
            this.Controls.Add(this.clbSourceFilter);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ViewLogFilterForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Filters for Context and Instance";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckedListBox clbSourceFilter;
        private System.Windows.Forms.CheckedListBox clbInstanceFilter;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.LinkLabel lkClearSource;
        private System.Windows.Forms.LinkLabel lbClearInstance;
    }
}