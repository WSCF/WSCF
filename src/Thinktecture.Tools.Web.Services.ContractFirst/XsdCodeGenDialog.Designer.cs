namespace Thinktecture.Tools.Web.Services.ContractFirst
{
    partial class XsdCodeGenDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(XsdCodeGenDialog));
			this.panel1 = new System.Windows.Forms.Panel();
			this.pbWscf = new System.Windows.Forms.PictureBox();
			this.pbWizard = new System.Windows.Forms.PictureBox();
			this.groupBox5 = new System.Windows.Forms.GroupBox();
			this.tbFileNames = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.cbDataBinding = new System.Windows.Forms.CheckBox();
			this.cbOrderIds = new System.Windows.Forms.CheckBox();
			this.cbAdjustCasing = new System.Windows.Forms.CheckBox();
			this.groupBox4 = new System.Windows.Forms.GroupBox();
			this.cbGenericList = new System.Windows.Forms.CheckBox();
			this.cbCollections = new System.Windows.Forms.CheckBox();
			this.cbProperties = new System.Windows.Forms.CheckBox();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.cbOverwrite = new System.Windows.Forms.CheckBox();
			this.cbMultipleFiles = new System.Windows.Forms.CheckBox();
			this.tbTargetFileName = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.tbNamespace = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.bnCancel = new System.Windows.Forms.Button();
			this.bnGenerate = new System.Windows.Forms.Button();
			this.cbSettings = new System.Windows.Forms.CheckBox();
			this.cbVirtualProperties = new System.Windows.Forms.CheckBox();
			this.panel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pbWscf)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pbWizard)).BeginInit();
			this.groupBox5.SuspendLayout();
			this.groupBox4.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// panel1
			// 
			this.panel1.BackColor = System.Drawing.Color.White;
			this.panel1.Controls.Add(this.pbWscf);
			this.panel1.Controls.Add(this.pbWizard);
			this.panel1.Location = new System.Drawing.Point(-3, 1);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(609, 40);
			this.panel1.TabIndex = 0;
			// 
			// pbWscf
			// 
			this.pbWscf.Image = ((System.Drawing.Image)(resources.GetObject("pbWscf.Image")));
			this.pbWscf.Location = new System.Drawing.Point(446, 3);
			this.pbWscf.Name = "pbWscf";
			this.pbWscf.Size = new System.Drawing.Size(104, 32);
			this.pbWscf.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.pbWscf.TabIndex = 11;
			this.pbWscf.TabStop = false;
			this.pbWscf.Click += new System.EventHandler(this.pbWscf_Click);
			// 
			// pbWizard
			// 
			this.pbWizard.Image = ((System.Drawing.Image)(resources.GetObject("pbWizard.Image")));
			this.pbWizard.Location = new System.Drawing.Point(10, 3);
			this.pbWizard.Name = "pbWizard";
			this.pbWizard.Size = new System.Drawing.Size(40, 32);
			this.pbWizard.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.pbWizard.TabIndex = 10;
			this.pbWizard.TabStop = false;
			// 
			// groupBox5
			// 
			this.groupBox5.Controls.Add(this.tbFileNames);
			this.groupBox5.Controls.Add(this.label4);
			this.groupBox5.Location = new System.Drawing.Point(8, 47);
			this.groupBox5.Name = "groupBox5";
			this.groupBox5.Size = new System.Drawing.Size(538, 49);
			this.groupBox5.TabIndex = 0;
			this.groupBox5.TabStop = false;
			this.groupBox5.Text = "XSD information  ";
			// 
			// tbFileNames
			// 
			this.tbFileNames.Location = new System.Drawing.Point(75, 18);
			this.tbFileNames.Name = "tbFileNames";
			this.tbFileNames.ReadOnly = true;
			this.tbFileNames.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.tbFileNames.Size = new System.Drawing.Size(453, 20);
			this.tbFileNames.TabIndex = 1;
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(8, 21);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(87, 20);
			this.label4.TabIndex = 0;
			this.label4.Text = "XSD file(s):";
			// 
			// cbDataBinding
			// 
			this.cbDataBinding.AutoSize = true;
			this.cbDataBinding.Location = new System.Drawing.Point(165, 27);
			this.cbDataBinding.Name = "cbDataBinding";
			this.cbDataBinding.Size = new System.Drawing.Size(86, 17);
			this.cbDataBinding.TabIndex = 2;
			this.cbDataBinding.Text = "Data binding";
			this.cbDataBinding.UseVisualStyleBackColor = true;
			this.cbDataBinding.CheckedChanged += new System.EventHandler(this.cbDataBinding_CheckedChanged);
			// 
			// cbOrderIds
			// 
			this.cbOrderIds.AutoSize = true;
			this.cbOrderIds.Location = new System.Drawing.Point(301, 62);
			this.cbOrderIds.Name = "cbOrderIds";
			this.cbOrderIds.Size = new System.Drawing.Size(99, 17);
			this.cbOrderIds.TabIndex = 6;
			this.cbOrderIds.Text = "Order identifiers";
			this.cbOrderIds.UseVisualStyleBackColor = true;
			// 
			// cbAdjustCasing
			// 
			this.cbAdjustCasing.Location = new System.Drawing.Point(165, 58);
			this.cbAdjustCasing.Name = "cbAdjustCasing";
			this.cbAdjustCasing.Size = new System.Drawing.Size(102, 24);
			this.cbAdjustCasing.TabIndex = 3;
			this.cbAdjustCasing.Text = "Adjust casing";
			// 
			// groupBox4
			// 
			this.groupBox4.Controls.Add(this.cbVirtualProperties);
			this.groupBox4.Controls.Add(this.cbGenericList);
			this.groupBox4.Controls.Add(this.cbDataBinding);
			this.groupBox4.Controls.Add(this.cbOrderIds);
			this.groupBox4.Controls.Add(this.cbCollections);
			this.groupBox4.Controls.Add(this.cbProperties);
			this.groupBox4.Controls.Add(this.cbAdjustCasing);
			this.groupBox4.Location = new System.Drawing.Point(8, 102);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.Size = new System.Drawing.Size(538, 94);
			this.groupBox4.TabIndex = 1;
			this.groupBox4.TabStop = false;
			this.groupBox4.Text = "Code generation options  ";
			// 
			// cbGenericList
			// 
			this.cbGenericList.AutoSize = true;
			this.cbGenericList.Location = new System.Drawing.Point(426, 28);
			this.cbGenericList.Name = "cbGenericList";
			this.cbGenericList.Size = new System.Drawing.Size(61, 17);
			this.cbGenericList.TabIndex = 5;
			this.cbGenericList.Text = "List<T>";
			this.cbGenericList.UseVisualStyleBackColor = true;
			this.cbGenericList.CheckedChanged += new System.EventHandler(this.cbGenericList_CheckedChanged);
			// 
			// cbCollections
			// 
			this.cbCollections.Location = new System.Drawing.Point(301, 24);
			this.cbCollections.Name = "cbCollections";
			this.cbCollections.Size = new System.Drawing.Size(80, 24);
			this.cbCollections.TabIndex = 4;
			this.cbCollections.Text = "Collections";
			this.cbCollections.CheckedChanged += new System.EventHandler(this.cbCollections_CheckedChanged);
			// 
			// cbProperties
			// 
			this.cbProperties.Location = new System.Drawing.Point(16, 24);
			this.cbProperties.Name = "cbProperties";
			this.cbProperties.Size = new System.Drawing.Size(115, 23);
			this.cbProperties.TabIndex = 0;
			this.cbProperties.Text = "Public properties";
			// 
			// groupBox2
			// 
			this.groupBox2.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.groupBox2.Controls.Add(this.cbOverwrite);
			this.groupBox2.Controls.Add(this.cbMultipleFiles);
			this.groupBox2.Controls.Add(this.tbTargetFileName);
			this.groupBox2.Controls.Add(this.label2);
			this.groupBox2.Controls.Add(this.tbNamespace);
			this.groupBox2.Controls.Add(this.label1);
			this.groupBox2.Location = new System.Drawing.Point(10, 203);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(538, 87);
			this.groupBox2.TabIndex = 2;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Files and namespaces  ";
			// 
			// cbOverwrite
			// 
			this.cbOverwrite.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.cbOverwrite.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.cbOverwrite.Location = new System.Drawing.Point(382, 48);
			this.cbOverwrite.Name = "cbOverwrite";
			this.cbOverwrite.Size = new System.Drawing.Size(144, 24);
			this.cbOverwrite.TabIndex = 5;
			this.cbOverwrite.Text = "Overwrite existing files";
			// 
			// cbMultipleFiles
			// 
			this.cbMultipleFiles.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.cbMultipleFiles.Location = new System.Drawing.Point(382, 21);
			this.cbMultipleFiles.Margin = new System.Windows.Forms.Padding(0);
			this.cbMultipleFiles.Name = "cbMultipleFiles";
			this.cbMultipleFiles.Size = new System.Drawing.Size(144, 24);
			this.cbMultipleFiles.TabIndex = 4;
			this.cbMultipleFiles.Text = "Separate files";
			this.cbMultipleFiles.CheckedChanged += new System.EventHandler(this.cbMultipleFiles_CheckedChanged);
			// 
			// tbTargetFileName
			// 
			this.tbTargetFileName.Location = new System.Drawing.Point(104, 50);
			this.tbTargetFileName.Name = "tbTargetFileName";
			this.tbTargetFileName.Size = new System.Drawing.Size(250, 20);
			this.tbTargetFileName.TabIndex = 3;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(8, 25);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(88, 16);
			this.label2.TabIndex = 0;
			this.label2.Text = "Namespace:";
			// 
			// tbNamespace
			// 
			this.tbNamespace.Location = new System.Drawing.Point(104, 23);
			this.tbNamespace.Name = "tbNamespace";
			this.tbNamespace.Size = new System.Drawing.Size(250, 20);
			this.tbNamespace.TabIndex = 1;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(8, 52);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(104, 16);
			this.label1.TabIndex = 2;
			this.label1.Text = "Target file name:";
			// 
			// bnCancel
			// 
			this.bnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.bnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.bnCancel.Location = new System.Drawing.Point(471, 297);
			this.bnCancel.Name = "bnCancel";
			this.bnCancel.Size = new System.Drawing.Size(77, 28);
			this.bnCancel.TabIndex = 5;
			this.bnCancel.Text = "Cancel";
			this.bnCancel.Click += new System.EventHandler(this.bnCancel_Click);
			// 
			// bnGenerate
			// 
			this.bnGenerate.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.bnGenerate.Location = new System.Drawing.Point(383, 297);
			this.bnGenerate.Name = "bnGenerate";
			this.bnGenerate.Size = new System.Drawing.Size(77, 28);
			this.bnGenerate.TabIndex = 4;
			this.bnGenerate.Text = "Generate";
			this.bnGenerate.Click += new System.EventHandler(this.button1_Click);
			// 
			// cbSettings
			// 
			this.cbSettings.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.cbSettings.Location = new System.Drawing.Point(10, 301);
			this.cbSettings.Name = "cbSettings";
			this.cbSettings.Size = new System.Drawing.Size(128, 23);
			this.cbSettings.TabIndex = 3;
			this.cbSettings.Text = "Remember settings";
			// 
			// cbVirtualProperties
			// 
			this.cbVirtualProperties.Location = new System.Drawing.Point(16, 59);
			this.cbVirtualProperties.Name = "cbVirtualProperties";
			this.cbVirtualProperties.Size = new System.Drawing.Size(115, 23);
			this.cbVirtualProperties.TabIndex = 1;
			this.cbVirtualProperties.Text = "Virtual properties";
			this.cbVirtualProperties.CheckedChanged += new System.EventHandler(this.cbVirtualProperties_CheckedChanged);
			// 
			// XsdCodeGenDialog
			// 
			this.AcceptButton = this.bnGenerate;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.bnCancel;
			this.ClientSize = new System.Drawing.Size(554, 332);
			this.Controls.Add(this.cbSettings);
			this.Controls.Add(this.bnCancel);
			this.Controls.Add(this.bnGenerate);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox4);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.groupBox5);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "XsdCodeGenDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "WSCF.blue Data Contract Code Generation Options 1.0";
			this.Load += new System.EventHandler(this.XsdCodeGenDialog_Load);
			this.panel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.pbWscf)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pbWizard)).EndInit();
			this.groupBox5.ResumeLayout(false);
			this.groupBox5.PerformLayout();
			this.groupBox4.ResumeLayout(false);
			this.groupBox4.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.PictureBox pbWscf;
        private System.Windows.Forms.PictureBox pbWizard;
        private System.Windows.Forms.GroupBox groupBox5;
		private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox cbDataBinding;
        private System.Windows.Forms.CheckBox cbOrderIds;
        private System.Windows.Forms.CheckBox cbAdjustCasing;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.CheckBox cbGenericList;
        private System.Windows.Forms.CheckBox cbCollections;
		private System.Windows.Forms.CheckBox cbProperties;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox tbTargetFileName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbNamespace;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox cbMultipleFiles;
        private System.Windows.Forms.CheckBox cbOverwrite;
        private System.Windows.Forms.Button bnCancel;
        private System.Windows.Forms.Button bnGenerate;
        private System.Windows.Forms.TextBox tbFileNames;
        private System.Windows.Forms.CheckBox cbSettings;
		private System.Windows.Forms.CheckBox cbVirtualProperties;
    }
}