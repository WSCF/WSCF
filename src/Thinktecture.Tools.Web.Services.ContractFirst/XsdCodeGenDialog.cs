using System;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace Thinktecture.Tools.Web.Services.ContractFirst
{
    public partial class XsdCodeGenDialog : Form
    {
        #region Constructors

        public XsdCodeGenDialog(string[] xsdfiles)
        {
            InitializeComponent();

			// Fill the file names text box.
        	tbFileNames.Text = string.Join(";", xsdfiles);
        }

        #endregion

        #region Event handlers

        private void cbMultipleFiles_CheckedChanged(object sender, EventArgs e)
        {
            // Disable the target file name text box if multiple files options is on.
            tbTargetFileName.Enabled = !cbMultipleFiles.Checked;
        }

        private void XsdCodeGenDialog_Load(object sender, EventArgs e)
        {
            LoadFormValues();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (tbTargetFileName.Text.Trim() == "" ||
                tbTargetFileName.Text.IndexOfAny(Path.GetInvalidFileNameChars()) > -1)
            {
                MessageBox.Show("Please enter a valid name for the tatget file name",
                                "Web Services Contract-First code generation", 
                                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            if (tbNamespace.Text.Trim() == "" || 
                !IsMatchingPattern(@"^(?:(?:((?![0-9_])[a-zA-Z0-9_]+)\.?)+)(?<!\.)$", tbNamespace.Text))
            {
                MessageBox.Show("Please enter a valid name for the namespace",
                                "Web Services Contract-First code generation",
                                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            
            DialogResult = DialogResult.OK;
            SaveFormValues();
            Close();
        }

        #endregion

        #region Properties

        public bool PublicProperties => cbProperties.Checked;

        public bool VirtualProperties => cbVirtualProperties.Checked;

        public bool Collections => cbCollections.Checked;

        public bool GenericLists => cbGenericList.Checked;

        public bool DataBinding => cbDataBinding.Checked;

        public bool OrderIdentifiers => cbOrderIds.Checked;

        public bool AdjustCasing => cbAdjustCasing.Checked;

        public bool OverwriteFiles => cbOverwrite.Checked;

        public bool GenerateMultipleFiles => cbMultipleFiles.Checked;

        public string Namespace
        {
            get => tbNamespace.Text;
            set => tbNamespace.Text = value;
        }

        public string TargetFileName
        {
            get => tbTargetFileName.Text;
            set => tbTargetFileName.Text = value;
        }

        #endregion

        bool IsMatchingPattern(string pattern, string value)
        {
            Regex regex = new Regex(pattern);
            Match match = regex.Match(value);
            return match.Success;
        }

        private void cbCollections_CheckedChanged(object sender, EventArgs e)
        {
            if (cbCollections.Checked && cbGenericList.Checked)
                cbGenericList.Checked = false;
        }

        private void cbGenericList_CheckedChanged(object sender, EventArgs e)
        {
            if (cbGenericList.Checked && cbCollections.Checked)
                cbCollections.Checked = false;
        }

        private void cbDataBinding_CheckedChanged(object sender, EventArgs e)
        {
            if (cbDataBinding.Checked)
            {
                cbProperties.Checked = true;
                cbProperties.Enabled = false;
            }
            else
            {
            	if (!cbVirtualProperties.Checked)
            	{
            		cbProperties.Enabled = true;
            	}
            }
        }

    	private void cbVirtualProperties_CheckedChanged(object sender, EventArgs e)
    	{
			if (cbVirtualProperties.Checked)
			{
				cbProperties.Checked = true;
				cbProperties.Enabled = false;
			}
			else
			{
				if (!cbDataBinding.Checked)
				{
					cbProperties.Enabled = true;
				}
			}
    	}

    	private void pbWscf_Click(object sender, EventArgs e)
        {
            Process.Start("http://www.thinktecture.com/");
        }

        /// <summary>
        /// Saves the form values
        /// </summary>
        private void SaveFormValues()
        {
            ConfigurationManager config = ConfigurationManager.GetConfigurationManager("WSCF05");
            if (cbSettings.Checked)
            {                
                config.Write("xsdProperties", cbProperties.Checked.ToString());
				config.Write("xsdVirtualProperties", cbVirtualProperties.Checked.ToString());
                config.Write("xsdCollections", cbCollections.Checked.ToString());
                config.Write("xsdGenericList", cbGenericList.Checked.ToString());
                config.Write("xsdDataBinding", cbDataBinding.Checked.ToString());
                config.Write("xsdOrderIdentifiers", cbOrderIds.Checked.ToString());
                config.Write("xsdAdjustCasing", cbAdjustCasing.Checked.ToString());
				config.Write("xsdMultipleFiles", cbMultipleFiles.Checked.ToString());
                config.Write("xsdOverwrite", cbOverwrite.Checked.ToString());
                config.Write("xsdRememberSettings", cbSettings.Checked.ToString());
				config.Write("xsdDestinationNamespace", tbNamespace.Text);
				config.Write("xsdDestinationFilename", tbTargetFileName.Text);                
            }
            else
            {
                config.Write("xsdRememberSettings", "false");
            }
            config.Persist();
        }

        /// <summary>
        /// Loads the values for the UI elements from the persisted storage.
        /// </summary>
        private void LoadFormValues()
        {
            ConfigurationManager config = ConfigurationManager.GetConfigurationManager("WSCF05");
            if ((cbSettings.Checked = config.ReadBoolean("xsdRememberSettings")))
            {
                cbProperties.Checked = config.ReadBoolean("xsdProperties");
            	cbVirtualProperties.Checked = config.ReadBoolean("xsdVirtualProperties");
                cbCollections.Checked = config.ReadBoolean("xsdCollections");
                cbGenericList.Checked = config.ReadBoolean("xsdGenericList");
                cbDataBinding.Checked = config.ReadBoolean("xsdDataBinding");
                cbOrderIds.Checked = config.ReadBoolean("xsdOrderIdentifiers");
                cbAdjustCasing.Checked = config.ReadBoolean("xsdAdjustCasing");
				cbMultipleFiles.Checked = config.ReadBoolean("xsdMultipleFiles");
                cbOverwrite.Checked = config.ReadBoolean("xsdOverwrite");
				tbNamespace.Text = config.Read("xsdDestinationNamespace");
				tbTargetFileName.Text = config.Read("xsdDestinationFilename");
            }
        }
    }
}