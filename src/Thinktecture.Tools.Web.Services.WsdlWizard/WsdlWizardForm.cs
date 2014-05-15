using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Text;
using System.Reflection;
using System.Net;
using Thinktecture.Tools.Web.Services.ServiceDescription;
using WizardControl;
using WRM.Windows.Forms;
using Message = Thinktecture.Tools.Web.Services.ServiceDescription.Message;
using Thinktecture.Tools.Web.Services.Wscf.Environment;

namespace Thinktecture.Tools.Web.Services.WsdlWizard
{
	/// <summary>
	/// Represents the user interface handler for WsdlWizardForm.
	/// </summary>
	public class WsdlWizardForm : Form
	{
		#region Private fields

		private int operationCount;
		private string schemaLocation = "";
		private string wsdlLocation = "";
		private string schemaNamespace = "";
		private bool openCodeGenDialog;
		private bool alreadyCancelled = false;
		private bool roundtripMode = false;
		
		private string currentFolder;
		private string wsdlFile = string.Empty;
		private string defaultPathForImports = string.Empty;
		private string projectRootDirectory = string.Empty;

		private ArrayList importedSchemaNamespaces = new ArrayList();

		private Wizard wsdlWizardCtrl;
		private WizardPage wizardPageBasicMetadata;
		private TextBox tbServiceName;
		private Label lblServiceName;
		private Label lblNamespace;
		private WizardPage wizardPageOperationsList;		
		private GroupBox groupBox1;
		private TextBox tbEdit;
		private ComboBox cbMEPs;
		private Panel panel1;
		private ToolTip toolTip1;
		private IContainer components;
		private TextBox tbNamespace;
		private LinkLabel llAddOperation;
		private EditableListView operationsListView;
		private InterfaceContract serviceInterfaceContract;
		private InterfaceContract importedContract = null;
		private SchemaElements messageSchemas = new SchemaElements();		
		private SchemaElements headerSchemas = new SchemaElements();
		private SchemaElements faultSchemas = new SchemaElements();	
		private LinkLabel llRemoveOperation;
		private Label lblServiceDoc;
		private TextBox tbServiceDoc;
		private WizardPage wizardPageMessageMapping;
		private WizardControl.WizardPage wizardPageAdditionalOptions;
		private System.Windows.Forms.CheckBox cbNeedsServiceElement;
		private System.Windows.Forms.CheckBox cbCodeGenDialog;
		private WRM.Windows.Forms.PropertyTree ptvServiceOperations;
		private WizardControl.WizardPage wizardPageSchemaImports;
		private System.Windows.Forms.GroupBox groupBox2;
		private Thinktecture.Tools.Web.Services.WsdlWizard.EditableListView importsListView;
		private System.Windows.Forms.LinkLabel llAddImport;
		private System.Windows.Forms.LinkLabel llRemoveImport;
		private System.Windows.Forms.OpenFileDialog openFileDialog1;
		private WizardControl.WizardPage wizardPageAlternativeXSDPaths;
		private Thinktecture.Tools.Web.Services.WsdlWizard.EditableListView xsdpathsListView;
		private System.Windows.Forms.ToolTip toolTipPath;
		private System.Windows.Forms.CheckBox cbInfer;
		private OperationsCollection inferOperations = new OperationsCollection(10);
        private CheckBox cbSoap12;
        private CheckBox cbSoap11;		
		private OperationsCollection oldOperations =  
			new OperationsCollection(); // Holds a list of old operations configured by the user.

		#endregion

		#region Public properties.

		/// <summary>
		/// Gets a value indicating whether the code generation dialog should be opened or not. 
		/// </summary>
		public bool OpenCodeGenDialog
		{
			get { return openCodeGenDialog; }
		}
		
		/// <summary>
		/// Gets or sets the location to create the WSDL file.
		/// </summary>
		public string WsdlLocation
		{
			get { return wsdlLocation; }
			set { wsdlLocation = value; }
		}

		/// <summary>
		/// Gets or sets the schema namespace.
		/// </summary>
		public string SchemaNamespace
		{
			get { return schemaNamespace; }
			set { schemaNamespace = value; }
		}

		/// <summary>
		/// Gets or sets the schema location.
		/// </summary>
		public string SchemaLocation
		{
			get { return schemaLocation; }
			set 
			{ 
				schemaLocation = value; 
				this.UpdateCurrentFolder(schemaLocation);
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the round tripping mode is on or off.
		/// </summary>
		public bool RoundtripMode
		{
			get { return roundtripMode; }
			set { roundtripMode = value; }
		}
		
		/// <summary>
		/// Gets or sets the default path for the imported XSD files.
		/// </summary>
		public string DefaultPathForImports
		{
			get { return defaultPathForImports; }
			set { defaultPathForImports = value; }
		}
		
		/// <summary>
		/// Gets or sets the root directory for the project.
		/// </summary>
		public string ProjectRootDirectory
		{
			get { return projectRootDirectory; }
			set { projectRootDirectory = value; }
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of WsdlWizardForm class with the specified values.
		/// </summary>
		/// <param name="xsdLocation">
		/// Location of the XSD file containing the message contract definitions for the WSDL.
		/// </param>
		public WsdlWizardForm(string xsdLocation)
		{
			//
			// Required for Windows Form Designer support.
			//
			InitializeComponent();
			
			schemaLocation = xsdLocation;
			this.UpdateCurrentFolder(schemaLocation);

			serviceInterfaceContract = new InterfaceContract();
			serviceInterfaceContract.SchemaNamespace = schemaLocation;
		}
		
		/// <summary>
		/// Initializes a new instance of WsdlWizardForm class with the specified values.
		/// </summary>
		/// <param name="wsdlLocation">Location of the WSDL file to modify.</param>
		/// <param name="roundTripping">
		/// Value indicating that the round tripping is required.
		/// </param>
		/// <remarks>
		/// The roundTripping parameter must be set to true in order to use the round tripping feature.
		/// </remarks>
		public WsdlWizardForm(string wsdlLocation, bool roundTripping)
		{
			//
			// Required for Windows Form Designer support.
			//
			InitializeComponent();
			
			// Import the interface contract from the selected WSDL file.
			try
			{										
				this.importedContract = 
					ServiceDescriptionEngine.GetInterfaceContract(wsdlLocation);	
				this.wsdlFile = wsdlLocation;
			}
			catch(WsdlModifiedException ex)
			{
				throw new WsdlFileLoadException(
					"Could not import the specified WSDL file for round-triping.\nThis file is not compatible for round-tripping.", ex);
			}
			catch(WsdlNotCompatibleForRoundTrippingException ex)
			{
				throw new WsdlFileLoadException(
					"Could not import the specified WSDL file for round-triping.\nThis file is not compatible for round-tripping.", ex);
			}
			catch
			{
				throw new WsdlFileLoadException(
					"System could not import the specified WSDL file for round triping.\nThis file is either modified or not a valid WSDL file created using WSCF.exe.");
			}				

			this.UpdateCurrentFolder(wsdlLocation);
			serviceInterfaceContract = new InterfaceContract();
			serviceInterfaceContract.SchemaNamespace = schemaLocation;
			this.roundtripMode = roundTripping;
		}

		#endregion

		#region Dispose

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}
		
		#endregion

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WsdlWizardForm));
			this.wsdlWizardCtrl = new WizardControl.Wizard();
			this.wizardPageBasicMetadata = new WizardControl.WizardPage();
			this.tbServiceDoc = new System.Windows.Forms.TextBox();
			this.lblServiceDoc = new System.Windows.Forms.Label();
			this.lblNamespace = new System.Windows.Forms.Label();
			this.tbNamespace = new System.Windows.Forms.TextBox();
			this.lblServiceName = new System.Windows.Forms.Label();
			this.tbServiceName = new System.Windows.Forms.TextBox();
			this.wizardPageSchemaImports = new WizardControl.WizardPage();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.llRemoveImport = new System.Windows.Forms.LinkLabel();
			this.llAddImport = new System.Windows.Forms.LinkLabel();
			this.importsListView = new Thinktecture.Tools.Web.Services.WsdlWizard.EditableListView();
			this.wizardPageOperationsList = new WizardControl.WizardPage();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.operationsListView = new Thinktecture.Tools.Web.Services.WsdlWizard.EditableListView();
			this.llAddOperation = new System.Windows.Forms.LinkLabel();
			this.llRemoveOperation = new System.Windows.Forms.LinkLabel();
			this.cbInfer = new System.Windows.Forms.CheckBox();
			this.panel1 = new System.Windows.Forms.Panel();
			this.tbEdit = new System.Windows.Forms.TextBox();
			this.cbMEPs = new System.Windows.Forms.ComboBox();
			this.wizardPageMessageMapping = new WizardControl.WizardPage();
			this.ptvServiceOperations = new WRM.Windows.Forms.PropertyTree();
			this.wizardPageAdditionalOptions = new WizardControl.WizardPage();
			this.cbSoap12 = new System.Windows.Forms.CheckBox();
			this.cbSoap11 = new System.Windows.Forms.CheckBox();
			this.cbNeedsServiceElement = new System.Windows.Forms.CheckBox();
			this.cbCodeGenDialog = new System.Windows.Forms.CheckBox();
			this.wizardPageAlternativeXSDPaths = new WizardControl.WizardPage();
			this.xsdpathsListView = new Thinktecture.Tools.Web.Services.WsdlWizard.EditableListView();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
			this.toolTipPath = new System.Windows.Forms.ToolTip(this.components);
			this.wsdlWizardCtrl.SuspendLayout();
			this.wizardPageBasicMetadata.SuspendLayout();
			this.wizardPageSchemaImports.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.wizardPageOperationsList.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.panel1.SuspendLayout();
			this.wizardPageMessageMapping.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ptvServiceOperations)).BeginInit();
			this.wizardPageAdditionalOptions.SuspendLayout();
			this.wizardPageAlternativeXSDPaths.SuspendLayout();
			this.SuspendLayout();
			// 
			// wsdlWizardCtrl
			// 
			this.wsdlWizardCtrl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.wsdlWizardCtrl.BannerBitmap = ((System.Drawing.Image)(resources.GetObject("wsdlWizardCtrl.BannerBitmap")));
			this.wsdlWizardCtrl.CloseForm = false;
			this.wsdlWizardCtrl.Controls.Add(this.wizardPageBasicMetadata);
			this.wsdlWizardCtrl.Controls.Add(this.wizardPageSchemaImports);
			this.wsdlWizardCtrl.Controls.Add(this.wizardPageOperationsList);
			this.wsdlWizardCtrl.Controls.Add(this.wizardPageMessageMapping);
			this.wsdlWizardCtrl.Controls.Add(this.wizardPageAdditionalOptions);
			this.wsdlWizardCtrl.Controls.Add(this.wizardPageAlternativeXSDPaths);
			this.wsdlWizardCtrl.Location = new System.Drawing.Point(0, 0);
			this.wsdlWizardCtrl.Name = "wsdlWizardCtrl";
			this.wsdlWizardCtrl.Size = new System.Drawing.Size(497, 360);
			this.wsdlWizardCtrl.TabIndex = 0;
			this.wsdlWizardCtrl.Title = "Generate WSDL";
			this.wsdlWizardCtrl.WelcomeBitmap = ((System.Drawing.Image)(resources.GetObject("wsdlWizardCtrl.WelcomeBitmap")));
			this.wsdlWizardCtrl.WelcomeText = resources.GetString("wsdlWizardCtrl.WelcomeText");
			this.wsdlWizardCtrl.Load += new System.EventHandler(this.wsdlWizardCtrl_Load);
			this.wsdlWizardCtrl.Cancelled += new WizardControl.Wizard.CancelledEventHandler(this.wsdlWizardCtrl_Cancelled);
			this.wsdlWizardCtrl.BeforeSummaryPageDisplayed += new WizardControl.Wizard.BeforeSummaryPageDisplayedEventHandler(this.wsdlWizardCtrl_BeforeSummaryPageDisplayed);
			this.wsdlWizardCtrl.BeforePageDisplayed += new WizardControl.Wizard.BeforePageDisplayedEventHandler(this.wsdlWizardCtrl_BeforePageDisplayed);
			this.wsdlWizardCtrl.Finished += new WizardControl.Wizard.FinishedEventHandler(this.wsdlWizardCtrl_Finished);
			this.wsdlWizardCtrl.AfterPageDisplayed += new WizardControl.Wizard.AfterPageDisplayedEventHandler(this.wsdlWizardCtrl_AfterPageDisplayed);
			this.wsdlWizardCtrl.ValidatePage += new WizardControl.Wizard.ValidatePageEventHandler(this.wsdlWizardCtrl_ValidatePage);
			// 
			// wizardPageBasicMetadata
			// 
			this.wizardPageBasicMetadata.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.wizardPageBasicMetadata.Controls.Add(this.tbServiceDoc);
			this.wizardPageBasicMetadata.Controls.Add(this.lblServiceDoc);
			this.wizardPageBasicMetadata.Controls.Add(this.lblNamespace);
			this.wizardPageBasicMetadata.Controls.Add(this.tbNamespace);
			this.wizardPageBasicMetadata.Controls.Add(this.lblServiceName);
			this.wizardPageBasicMetadata.Controls.Add(this.tbServiceName);
			this.wizardPageBasicMetadata.Description = "Please enter the name and the XML namespace of the Web Service.";
			this.wizardPageBasicMetadata.Heading = "Step 1: Specify your Web Service\'s basic settings";
			this.wizardPageBasicMetadata.Location = new System.Drawing.Point(21, 71);
			this.wizardPageBasicMetadata.Name = "wizardPageBasicMetadata";
			this.wizardPageBasicMetadata.Size = new System.Drawing.Size(456, 230);
			this.wizardPageBasicMetadata.TabIndex = 0;
			// 
			// tbServiceDoc
			// 
			this.tbServiceDoc.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.tbServiceDoc.Location = new System.Drawing.Point(120, 72);
			this.tbServiceDoc.Multiline = true;
			this.tbServiceDoc.Name = "tbServiceDoc";
			this.tbServiceDoc.Size = new System.Drawing.Size(312, 88);
			this.tbServiceDoc.TabIndex = 5;
			// 
			// lblServiceDoc
			// 
			this.lblServiceDoc.Location = new System.Drawing.Point(24, 72);
			this.lblServiceDoc.Name = "lblServiceDoc";
			this.lblServiceDoc.Size = new System.Drawing.Size(100, 23);
			this.lblServiceDoc.TabIndex = 4;
			this.lblServiceDoc.Text = "Documentation:";
			// 
			// lblNamespace
			// 
			this.lblNamespace.Location = new System.Drawing.Point(24, 40);
			this.lblNamespace.Name = "lblNamespace";
			this.lblNamespace.Size = new System.Drawing.Size(96, 23);
			this.lblNamespace.TabIndex = 3;
			this.lblNamespace.Text = "XML namespace:";
			// 
			// tbNamespace
			// 
			this.tbNamespace.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.tbNamespace.Location = new System.Drawing.Point(120, 40);
			this.tbNamespace.Name = "tbNamespace";
			this.tbNamespace.Size = new System.Drawing.Size(312, 20);
			this.tbNamespace.TabIndex = 2;
			// 
			// lblServiceName
			// 
			this.lblServiceName.Location = new System.Drawing.Point(24, 8);
			this.lblServiceName.Name = "lblServiceName";
			this.lblServiceName.Size = new System.Drawing.Size(88, 23);
			this.lblServiceName.TabIndex = 1;
			this.lblServiceName.Text = "Service name:";
			this.toolTip1.SetToolTip(this.lblServiceName, "\'Service name\' specifies the name of the Web Service binding to be generated. Thi" +
					"s will be the e.g. the class name for your Web Service proxy when generated from" +
					" the service description.");
			// 
			// tbServiceName
			// 
			this.tbServiceName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.tbServiceName.Location = new System.Drawing.Point(120, 6);
			this.tbServiceName.Name = "tbServiceName";
			this.tbServiceName.Size = new System.Drawing.Size(312, 20);
			this.tbServiceName.TabIndex = 0;
			this.toolTip1.SetToolTip(this.tbServiceName, "\'Service name\' specifies the name of the Web Service binding to be generated. Thi" +
					"s will be the e.g. the class name for your Web Service proxy when generated from" +
					" the service description.");
			// 
			// wizardPageSchemaImports
			// 
			this.wizardPageSchemaImports.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.wizardPageSchemaImports.Controls.Add(this.groupBox2);
			this.wizardPageSchemaImports.Cursor = System.Windows.Forms.Cursors.Default;
			this.wizardPageSchemaImports.Description = "Please add additional message schemas from XSD files as appropriate.";
			this.wizardPageSchemaImports.Heading = "Step 2: Specify additional message schemas";
			this.wizardPageSchemaImports.Location = new System.Drawing.Point(21, 71);
			this.wizardPageSchemaImports.Name = "wizardPageSchemaImports";
			this.wizardPageSchemaImports.Size = new System.Drawing.Size(456, 230);
			this.wizardPageSchemaImports.TabIndex = 2;
			// 
			// groupBox2
			// 
			this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox2.Controls.Add(this.llRemoveImport);
			this.groupBox2.Controls.Add(this.llAddImport);
			this.groupBox2.Controls.Add(this.importsListView);
			this.groupBox2.Location = new System.Drawing.Point(8, 3);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(440, 224);
			this.groupBox2.TabIndex = 2;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "XSD Imports:";
			// 
			// llRemoveImport
			// 
			this.llRemoveImport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.llRemoveImport.Location = new System.Drawing.Point(95, 205);
			this.llRemoveImport.Name = "llRemoveImport";
			this.llRemoveImport.Size = new System.Drawing.Size(100, 16);
			this.llRemoveImport.TabIndex = 4;
			this.llRemoveImport.TabStop = true;
			this.llRemoveImport.Text = "Remove import";
			this.llRemoveImport.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.llRemoveImport_LinkClicked);
			// 
			// llAddImport
			// 
			this.llAddImport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.llAddImport.Location = new System.Drawing.Point(8, 205);
			this.llAddImport.Name = "llAddImport";
			this.llAddImport.Size = new System.Drawing.Size(81, 15);
			this.llAddImport.TabIndex = 3;
			this.llAddImport.TabStop = true;
			this.llAddImport.Text = "Add import";
			this.llAddImport.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.llAddImport_LinkClicked);
			// 
			// importsListView
			// 
			this.importsListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.importsListView.DisabledColumns = ((System.Collections.ArrayList)(resources.GetObject("importsListView.DisabledColumns")));
			this.importsListView.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.importsListView.FullRowSelect = true;
			this.importsListView.GridLines = true;
			this.importsListView.Location = new System.Drawing.Point(8, 16);
			this.importsListView.Name = "importsListView";
			this.importsListView.Size = new System.Drawing.Size(424, 184);
			this.importsListView.TabIndex = 2;
			this.importsListView.UseCompatibleStateImageBehavior = false;
			this.importsListView.View = System.Windows.Forms.View.Details;
			this.importsListView.MouseMove += new System.Windows.Forms.MouseEventHandler(this.importsListView_MouseMove);
			// 
			// wizardPageOperationsList
			// 
			this.wizardPageOperationsList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.wizardPageOperationsList.Controls.Add(this.groupBox1);
			this.wizardPageOperationsList.Controls.Add(this.panel1);
			this.wizardPageOperationsList.Description = "Please add operations to the Web Service as needed.";
			this.wizardPageOperationsList.Heading = "Step 3: Specify settings for your Web Service\'s operations.";
			this.wizardPageOperationsList.Location = new System.Drawing.Point(21, 71);
			this.wizardPageOperationsList.Name = "wizardPageOperationsList";
			this.wizardPageOperationsList.Size = new System.Drawing.Size(456, 230);
			this.wizardPageOperationsList.TabIndex = 1;
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Controls.Add(this.operationsListView);
			this.groupBox1.Controls.Add(this.llAddOperation);
			this.groupBox1.Controls.Add(this.llRemoveOperation);
			this.groupBox1.Controls.Add(this.cbInfer);
			this.groupBox1.Location = new System.Drawing.Point(8, 3);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(440, 224);
			this.groupBox1.TabIndex = 4;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Operations:";
			// 
			// operationsListView
			// 
			this.operationsListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.operationsListView.DisabledColumns = ((System.Collections.ArrayList)(resources.GetObject("operationsListView.DisabledColumns")));
			this.operationsListView.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.operationsListView.FullRowSelect = true;
			this.operationsListView.GridLines = true;
			this.operationsListView.Location = new System.Drawing.Point(8, 16);
			this.operationsListView.Name = "operationsListView";
			this.operationsListView.Size = new System.Drawing.Size(424, 184);
			this.operationsListView.TabIndex = 0;
			this.operationsListView.UseCompatibleStateImageBehavior = false;
			this.operationsListView.View = System.Windows.Forms.View.Details;
			// 
			// llAddOperation
			// 
			this.llAddOperation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.llAddOperation.Location = new System.Drawing.Point(7, 205);
			this.llAddOperation.Name = "llAddOperation";
			this.llAddOperation.Size = new System.Drawing.Size(81, 15);
			this.llAddOperation.TabIndex = 1;
			this.llAddOperation.TabStop = true;
			this.llAddOperation.Text = "Add operation";
			this.llAddOperation.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.llAddItem_LinkClicked);
			// 
			// llRemoveOperation
			// 
			this.llRemoveOperation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.llRemoveOperation.Location = new System.Drawing.Point(95, 205);
			this.llRemoveOperation.Name = "llRemoveOperation";
			this.llRemoveOperation.Size = new System.Drawing.Size(100, 16);
			this.llRemoveOperation.TabIndex = 2;
			this.llRemoveOperation.TabStop = true;
			this.llRemoveOperation.Text = "Remove operation";
			this.llRemoveOperation.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.llRemoveItem_LinkClicked);
			// 
			// cbInfer
			// 
			this.cbInfer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cbInfer.Location = new System.Drawing.Point(336, 204);
			this.cbInfer.Name = "cbInfer";
			this.cbInfer.Size = new System.Drawing.Size(104, 16);
			this.cbInfer.TabIndex = 4;
			this.cbInfer.Text = "Infer Operations";
			this.cbInfer.CheckedChanged += new System.EventHandler(this.cbInfer_CheckedChanged);
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.tbEdit);
			this.panel1.Controls.Add(this.cbMEPs);
			this.panel1.Location = new System.Drawing.Point(24, 32);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(408, 192);
			this.panel1.TabIndex = 2;
			// 
			// tbEdit
			// 
			this.tbEdit.Location = new System.Drawing.Point(16, 16);
			this.tbEdit.Name = "tbEdit";
			this.tbEdit.Size = new System.Drawing.Size(100, 20);
			this.tbEdit.TabIndex = 3;
			this.tbEdit.Text = "textBox2";
			this.tbEdit.Visible = false;
			// 
			// cbMEPs
			// 
			this.cbMEPs.ItemHeight = 13;
			this.cbMEPs.Items.AddRange(new object[] {
            "Request/Response",
            "One-Way"});
			this.cbMEPs.Location = new System.Drawing.Point(272, 16);
			this.cbMEPs.Name = "cbMEPs";
			this.cbMEPs.Size = new System.Drawing.Size(121, 21);
			this.cbMEPs.TabIndex = 2;
			this.cbMEPs.Text = "comboBox1";
			this.cbMEPs.Visible = false;
			// 
			// wizardPageMessageMapping
			// 
			this.wizardPageMessageMapping.Controls.Add(this.ptvServiceOperations);
			this.wizardPageMessageMapping.Description = "Please enter all details for the service\'s operations and messages.";
			this.wizardPageMessageMapping.Heading = "Step 4: Specify the operation\'s message parameters";
			this.wizardPageMessageMapping.Location = new System.Drawing.Point(21, 71);
			this.wizardPageMessageMapping.Name = "wizardPageMessageMapping";
			this.wizardPageMessageMapping.Size = new System.Drawing.Size(456, 230);
			this.wizardPageMessageMapping.TabIndex = 3;
			// 
			// ptvServiceOperations
			// 
			this.ptvServiceOperations.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.ptvServiceOperations.ImageList = null;
			this.ptvServiceOperations.Indent = 19;
			this.ptvServiceOperations.Location = new System.Drawing.Point(0, -1);
			this.ptvServiceOperations.Name = "ptvServiceOperations";
			this.ptvServiceOperations.PaneHeaderVisible = false;
			this.ptvServiceOperations.SelectedImageIndex = -1;
			this.ptvServiceOperations.SelectedPaneNode = null;
			this.ptvServiceOperations.ShowLines = true;
			this.ptvServiceOperations.ShowPlusMinus = true;
			this.ptvServiceOperations.ShowRootLines = true;
			this.ptvServiceOperations.Size = new System.Drawing.Size(456, 232);
			this.ptvServiceOperations.SplitterColor = System.Drawing.SystemColors.AppWorkspace;
			this.ptvServiceOperations.SplitterLeft = 184;
			this.ptvServiceOperations.TabIndex = 1;
			// 
			// wizardPageAdditionalOptions
			// 
			this.wizardPageAdditionalOptions.Controls.Add(this.cbSoap12);
			this.wizardPageAdditionalOptions.Controls.Add(this.cbSoap11);
			this.wizardPageAdditionalOptions.Controls.Add(this.cbNeedsServiceElement);
			this.wizardPageAdditionalOptions.Controls.Add(this.cbCodeGenDialog);
			this.wizardPageAdditionalOptions.Description = "Please select any additional options to configure.";
			this.wizardPageAdditionalOptions.Heading = "Step 5: Additional options";
			this.wizardPageAdditionalOptions.Location = new System.Drawing.Point(21, 71);
			this.wizardPageAdditionalOptions.Name = "wizardPageAdditionalOptions";
			this.wizardPageAdditionalOptions.Size = new System.Drawing.Size(456, 230);
			this.wizardPageAdditionalOptions.TabIndex = 4;
			// 
			// cbSoap12
			// 
			this.cbSoap12.AutoSize = true;
			this.cbSoap12.Location = new System.Drawing.Point(24, 87);
			this.cbSoap12.Name = "cbSoap12";
			this.cbSoap12.Size = new System.Drawing.Size(144, 17);
			this.cbSoap12.TabIndex = 12;
			this.cbSoap12.Text = "Create SOAP 1.2 binding";
			this.cbSoap12.UseVisualStyleBackColor = true;
			// 
			// cbSoap11
			// 
			this.cbSoap11.AutoSize = true;
			this.cbSoap11.Checked = true;
			this.cbSoap11.CheckState = System.Windows.Forms.CheckState.Checked;
			this.cbSoap11.Enabled = false;
			this.cbSoap11.Location = new System.Drawing.Point(24, 51);
			this.cbSoap11.Name = "cbSoap11";
			this.cbSoap11.Size = new System.Drawing.Size(144, 17);
			this.cbSoap11.TabIndex = 11;
			this.cbSoap11.Text = "Create SOAP 1.1 binding";
			this.cbSoap11.UseVisualStyleBackColor = true;
			// 
			// cbNeedsServiceElement
			// 
			this.cbNeedsServiceElement.Location = new System.Drawing.Point(24, 8);
			this.cbNeedsServiceElement.Name = "cbNeedsServiceElement";
			this.cbNeedsServiceElement.Size = new System.Drawing.Size(192, 24);
			this.cbNeedsServiceElement.TabIndex = 10;
			this.cbNeedsServiceElement.Text = "Generate <service> element.";
			this.toolTip1.SetToolTip(this.cbNeedsServiceElement, "Enable this option if you want to have a <service> element generated for the WSDL" +
					" service description.");
			// 
			// cbCodeGenDialog
			// 
			this.cbCodeGenDialog.Location = new System.Drawing.Point(24, 169);
			this.cbCodeGenDialog.Name = "cbCodeGenDialog";
			this.cbCodeGenDialog.Size = new System.Drawing.Size(408, 24);
			this.cbCodeGenDialog.TabIndex = 6;
			this.cbCodeGenDialog.Text = "Open the code generation dialog after this wizard closes.";
			this.cbCodeGenDialog.CheckedChanged += new System.EventHandler(this.cbCodeGenDialog_CheckedChanged);
			// 
			// wizardPageAlternativeXSDPaths
			// 
			this.wizardPageAlternativeXSDPaths.Controls.Add(this.xsdpathsListView);
			this.wizardPageAlternativeXSDPaths.Description = "Please select the alternative XSD path for each XSD file imported.";
			this.wizardPageAlternativeXSDPaths.Heading = "Step 6: Alternative XSD Paths";
			this.wizardPageAlternativeXSDPaths.Location = new System.Drawing.Point(21, 71);
			this.wizardPageAlternativeXSDPaths.Name = "wizardPageAlternativeXSDPaths";
			this.wizardPageAlternativeXSDPaths.Size = new System.Drawing.Size(456, 230);
			this.wizardPageAlternativeXSDPaths.TabIndex = 5;
			// 
			// xsdpathsListView
			// 
			this.xsdpathsListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.xsdpathsListView.DisabledColumns = ((System.Collections.ArrayList)(resources.GetObject("xsdpathsListView.DisabledColumns")));
			this.xsdpathsListView.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.xsdpathsListView.FullRowSelect = true;
			this.xsdpathsListView.GridLines = true;
			this.xsdpathsListView.Location = new System.Drawing.Point(0, 0);
			this.xsdpathsListView.Name = "xsdpathsListView";
			this.xsdpathsListView.Size = new System.Drawing.Size(456, 224);
			this.xsdpathsListView.TabIndex = 3;
			this.xsdpathsListView.UseCompatibleStateImageBehavior = false;
			this.xsdpathsListView.View = System.Windows.Forms.View.Details;
			// 
			// openFileDialog1
			// 
			this.openFileDialog1.Filter = "XSD Files(*.xsd)|*.xsd";
			// 
			// toolTipPath
			// 
			this.toolTipPath.AutomaticDelay = 1000;
			// 
			// WsdlWizardForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(497, 360);
			this.Controls.Add(this.wsdlWizardCtrl);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.HelpButton = true;
			this.Name = "WsdlWizardForm";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Create a WSDL Interface Description";
			this.Load += new System.EventHandler(this.WSDLWizard_Load);
			this.Closing += new System.ComponentModel.CancelEventHandler(this.WsdlWizardForm_Closing);
			this.wsdlWizardCtrl.ResumeLayout(false);
			this.wizardPageBasicMetadata.ResumeLayout(false);
			this.wizardPageBasicMetadata.PerformLayout();
			this.wizardPageSchemaImports.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.wizardPageOperationsList.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.wizardPageMessageMapping.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ptvServiceOperations)).EndInit();
			this.wizardPageAdditionalOptions.ResumeLayout(false);
			this.wizardPageAdditionalOptions.PerformLayout();
			this.wizardPageAlternativeXSDPaths.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		#region Event handlers
		/// <summary>
		///	Adds an operation to the operationsListView.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">An instance of <see cref="LinkLabelLinkClickedEventArgs"/> class with Event data.</param>
		private void llAddItem_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			ListViewItem listViewItem1 =
				new ListViewItem(new string[] {
					"Operation" + operationCount,
					"Request/Response",
					"",
					""},
					-1);

			operationsListView.Items.AddRange(
				new ListViewItem[] {listViewItem1});
			
			operationCount++;
		}

		/// <summary>
		/// Removes an operation from the operationsListView.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">An instance of <see cref="LinkLabelLinkClickedEventArgs"/> class with Event data.</param>
		/// <remarks>
		/// This method will remove the first item of the selected items collection. If no item is selected then
		/// it removes the last item available in the list.
		/// </remarks>
		private void llRemoveItem_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			if(operationsListView.Items.Count > 0)
			{
				// Remove the first item of the selected items.
				if(operationsListView.SelectedItems.Count > 0)
				{
                    while (operationsListView.SelectedItems.Count != 0)
                    {
                        ListViewItem lvi = operationsListView.SelectedItems[0];
                        operationsListView.Items.Remove(lvi);
                    }					
				}
				else
				{
					// If anything is not selected, remove the last item on the list.
					operationsListView.Items.RemoveAt(operationsListView.Items.Count - 1);
					operationCount--;
				}
			}
		}

		/// <summary>
		/// Performs the necessary initialization when the wizard control loads.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">An instance of <see cref="EventArgs"/> class with event data.</param>
 		private void wsdlWizardCtrl_Load(object sender, EventArgs e)
		{
			// Add the columns to the operationsListView.
 			operationsListView.Columns.Add("Operation Name", 210, HorizontalAlignment.Left);
 			operationsListView.Columns.Add("Message Exchange Pattern", -2, HorizontalAlignment.Left);
			
			// Add the columns to the importsListView.
			importsListView.Columns.Add("File Name", 210, HorizontalAlignment.Left);
			importsListView.Columns.Add("Location", -1, HorizontalAlignment.Left);
			
			// Set the non editable columns. 
			importsListView.DisabledColumns.Add("File Name");
			importsListView.DisabledColumns.Add("Location");

			// Add the columns to the xsdpathsListView.
			xsdpathsListView.Columns.Add("File Name", 210, HorizontalAlignment.Left);
			xsdpathsListView.Columns.Add("Alternative Path", -2, HorizontalAlignment.Left);
			
			// Set the non editable columns. 
			xsdpathsListView.DisabledColumns.Add("File Name");

			// Load the default XSD to the schema imports list.
			string fname = schemaLocation.Substring(schemaLocation.LastIndexOf("\\") + 1);
			ListViewItem defaultLocation = new ListViewItem();
			defaultLocation.Text = fname;
			defaultLocation.SubItems.Add(schemaLocation);
			importsListView.Items.Add(defaultLocation);

			// Skip the wizard welcome page if the round tripping is on.
			if(RoundtripMode == true)
			{
				wsdlWizardCtrl.AdvancePage();
				tbNamespace.Enabled = false;
			}

			// Focus the first tbServiceName text box.
			tbServiceName.Focus();

            // Assign the cancel button.            
            this.CancelButton = wsdlWizardCtrl.CancelButton;
		}

		/// <summary>
		/// Asks the confirmation question and quits the wizard upon the click of the "Cancel" button.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">An instance of <see cref="EventArgs"/> class with the event data.</param>
		private void wsdlWizardCtrl_Cancelled(object sender, EventArgs e)
		{
            /*
			if(MessageBox.Show("Do you really want to quit?", 
				"WSDL Wizard", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{
				alreadyCancelled = true;
				this.Close();
			}
             */
		}		

		/// <summary>
		/// Performs the actions, when the wizard's "Finish" button is clicked.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">An instance of <see cref="EventArgs"/> class with the event data.</param>
		/// <remarks>
		/// This method finally wraps up the necessary details and calls the 
		/// <see cref="ServiceDescriptionEngine"/> class to generate the WSDL file.
		/// </remarks>
		private void wsdlWizardCtrl_Finished(object sender, EventArgs e)
		{
			try
			{
				// Set additional options for WSDL generation.
				serviceInterfaceContract.NeedsServiceElement = 
					cbNeedsServiceElement.Checked;

                if (cbSoap11.Checked)
                {
                    serviceInterfaceContract.Bindings |= InterfaceContract.SoapBindings.Soap11;
                }

                if (cbSoap12.Checked)
                {
                    serviceInterfaceContract.Bindings |= InterfaceContract.SoapBindings.Soap12;
                }

				// Use alternative location for imports by default.
				serviceInterfaceContract.UseAlternateLocationForImports = true;

				// Call the GenerateWSDL method according to the round tripping mode.
				if(this.roundtripMode)
				{
					wsdlLocation = ServiceDescriptionEngine.GenerateWsdl(serviceInterfaceContract, 
						wsdlLocation, GetXmlCommentForWSDL(), this.wsdlFile);
				}
				else
				{
					wsdlLocation = ServiceDescriptionEngine.GenerateWsdl(serviceInterfaceContract, 
						wsdlLocation, GetXmlCommentForWSDL());
				}

				this.DialogResult = DialogResult.OK;
				alreadyCancelled = true;
				this.Close();
			}
			catch(WsdlGenerationException ex)
			{
				MessageBox.Show("An error occured while generating WSDL: " + ex.Message, "WSDL Wizard", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			catch(Exception generalException)
			{
				MessageBox.Show("An error occured while generating WSDL: " + generalException.Message, "WSDL Wizard", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		/// <summary>
		/// Performs the actions prior to displaying each page on the wizard control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">An instance of <see cref="WizardPageDisplayedEventArgs"/> class with event data.</param>
		private void wsdlWizardCtrl_BeforePageDisplayed(object sender, WizardPageDisplayedEventArgs e)
		{            
            // Assign the basic meta data values to the instance of InterfaceContract class.
            if (e.Page == wizardPageSchemaImports)
            {
                // 07-23-2005
                // BDS: Added this gate to validate the namespace uri.				
                Uri uri;
                try
                {
                    if (tbNamespace.Text.IndexOf(":") > -1 || Path.IsPathRooted(tbNamespace.Text))
                    {
                        uri = new Uri(tbNamespace.Text);
                    }
                    else
                    {
                        uri = new Uri("anyuri:" + tbNamespace.Text);
                    }
                }
                catch
                {
                    MessageBox.Show(this,
                        "Invalid uri for the namespace. Enter a valid uri and try again.",
                        "WSDL Wizard", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    e.Cancel = true;
                    return;
                }

                serviceInterfaceContract.ServiceName = tbServiceName.Text;
                serviceInterfaceContract.ServiceNamespace = tbNamespace.Text;
                serviceInterfaceContract.SchemaNamespace = schemaNamespace;
                serviceInterfaceContract.ServiceDocumentation = tbServiceDoc.Text;

                return;
            }

            if (e.Page == wizardPageOperationsList)
            {
                // Clear the existing items.
                this.messageSchemas.Clear();
                this.headerSchemas.Clear();
				faultSchemas.Clear();
                this.importedSchemaNamespaces.Clear();
                this.serviceInterfaceContract.Imports.Clear();

                // Add the selected schemas to the ServiceInterfaceContract.Imports collection.
                foreach (ListViewItem importItem in importsListView.Items)
                {
                    string schemaNamespace = string.Empty;
                    string importLocation = importItem.SubItems[1].Text;
                    ArrayList result;
                    try
                    {
                        // Read the content of the imported files and add them to the local arrays for UI.
                         result = ServiceDescriptionEngine.GetSchemasFromXsd(importLocation, out schemaNamespace);
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show(this, ex.Message,
                       "WSDL Wizard", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        e.Cancel = true;
                        return;
                    }

                    // Check whether the schema has a valid namespace.
                    //if (schemaNamespace == null)
                    //{
                    //    MessageBox.Show(this, "Could not import the file: " +
                    //        importLocation + ". The schema definition does not belong to a valid namespace.",
                    //        "WSDL Wizard", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //    wsdlWizardCtrl.JumpToPage(2);
                    //    return;
                    //}

                    //// Check whether the Namespace already exists in the list. 
                    //if (this.importedSchemaNamespaces.IndexOf(schemaNamespace.ToLower()) > -1)
                    //{
                    //    MessageBox.Show(this, "Could not import the file: " +
                    //        importLocation + ". The Target Namespace already imported.",
                    //        "WSDL Wizard", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //    wsdlWizardCtrl.JumpToPage(2);
                    //    return;
                    //}
                    //else
                    //{  
                        if(schemaNamespace != null)
                            this.importedSchemaNamespaces.Add(schemaNamespace.ToLower());
                    //}

                    messageSchemas.AddRange((SchemaElements)result[1]);
                    headerSchemas.AddRange((SchemaElements)result[1]);
					faultSchemas.AddRange((SchemaElements)result[1]);

                    SchemaImport si = new SchemaImport(importItem.SubItems[1].Text, schemaNamespace, importItem.SubItems[0].Text);

                    // Check whether the schema is in the current directory.
                    if (Directory.GetFiles(this.wsdlLocation, si.SchemaName).Length > 0)
                    {
                        si.AlternateLocation = si.SchemaName;
                    }
                    else if (si.SchemaLocation.ToLower().StartsWith(this.projectRootDirectory.ToLower()))
                    {
                        string schemaDirectory = si.SchemaLocation.Substring(
                            0, si.SchemaLocation.LastIndexOf('\\'));
                        string currentDirectory = wsdlLocation;
                        // Remove the project root before passing them to the relative path finder.
                        schemaDirectory = schemaDirectory.Substring(this.projectRootDirectory.Length);
                        currentDirectory = currentDirectory.Substring(this.projectRootDirectory.Length);

                        si.AlternateLocation = IOPathHelper.GetRelativePath(schemaDirectory, currentDirectory);
                        if (si.AlternateLocation.EndsWith("/"))
                        {
                            si.AlternateLocation = si.AlternateLocation + si.SchemaName;
                        }
                        else
                        {
                            si.AlternateLocation = si.AlternateLocation + "/" + si.SchemaName;
                        }
                    }
                    else
                    {
                        si.AlternateLocation = si.SchemaLocation;
                    }
                    serviceInterfaceContract.Imports.Add(si);
                }

                // Import the embedded types.
                ImportEmbeddedTypes();

                // Check for the messages count found in the imported files and alert the user if no messages 
                // are found.
                if (messageSchemas.Count < 1)
                {
                    MessageBox.Show("There are no elements in this XSD to use as operation messages.",
                        "WSDL Wizard", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    e.Cancel = true;
                    return;
                }

                return;
            }

            if (e.Page == wizardPageMessageMapping)
            {
                try
                {
                    // Setup the dynamic UI controls for the operation - message mapping UI.
                    SetupOperationsMessagesPanes();

                    if (ptvServiceOperations.PaneNodes.Count > 0)
                    {
                        ptvServiceOperations.SelectedPaneNode = ptvServiceOperations.PaneNodes[0];
                        ptvServiceOperations.PaneNodes[0].Expanded = true;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                return;
            }

            if (e.Page == wizardPageAdditionalOptions)
            {
                cbCodeGenDialog.Checked = this.roundtripMode;
                return;
            }

            if (e.Page == wizardPageAlternativeXSDPaths)
            {
                xsdpathsListView.Items.Clear();
                if (serviceInterfaceContract.Imports.Count > 0)
                {
                    foreach (SchemaImport import in serviceInterfaceContract.Imports)
                    {
                        ListViewItem li = new ListViewItem(import.SchemaName);
                        li.SubItems.Add(import.AlternateLocation);
                        xsdpathsListView.Items.Add(li);
                    }
                }
                else
                {
                    wsdlWizardCtrl.AdvancePage();
                }
                return;
            }                        
		}

		/// <summary>
		/// Handles the SelectedIndexChanged event of the dynamically created combo box.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">An instance of <see cref="EventArgs"/> class with event data.</param>
		/// <remarks>This method changes the selected operation's in/out message parameters to the 
		/// newly selected values.</remarks>
		private void DynamicComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			ComboBox currentCombo = sender as ComboBox;
			
			// Check whether the dynamic combo belongs to an inbound message or an outbound message.
			if(currentCombo.Name.StartsWith("inMessageParamsCombo"))
			{
				string tail = currentCombo.Name.Substring(20);
				int index = int.Parse(tail);
				
				// Select the corresponding operation for the selected message.
				Operation currentOperation = serviceInterfaceContract.OperationsCollection[index];

				// Change the input message of the operation.
				currentOperation.Input.Element = messageSchemas[currentCombo.SelectedIndex];
			}
			else if(currentCombo.Name.StartsWith("outMessageParamsCombo"))
			{
				string tail = currentCombo.Name.Substring(21);
				int index = int.Parse(tail);

				// Select the corresponding operation for the selected message.
				Operation currentOperation = serviceInterfaceContract.OperationsCollection[index];

				// Change the input message of the operation.
				currentOperation.Output.Element = messageSchemas[currentCombo.SelectedIndex];
			}

			// this is a hack to 'convince' the combo box that the Text property has changed - don't ask! ...
			((ComboBox)sender).Text = "42";
		}

		/// <summary>
		/// Performs the actions on loading the WSDL wizard.
		/// </summary>
		/// <param name="sender">Source of the event.</param>
		/// <param name="e">An instance of <see cref="EventArgs"/> class with the event data.</param>
		/// <remarks>If the round tripping mode is on, this method imports the data from the existing 
		/// WSDL file to the UI.</remarks>
		private void WSDLWizard_Load(object sender, EventArgs e)
		{
			ptvServiceOperations.PaneHeaderVisible = true;

			// If the round tripping mode is on then import the data from the existing WSDL to the UI.
			if(this.roundtripMode)
			{
				if(importedContract.IsHttpBinding)
				{
					if(MessageBox.Show(this, 
						"This WSDL contains an HTTP binding. It will be converted to a SOAP binding.\nAre you sure you want to perform this action?", 
						"WSDL Wizard", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) == 
						DialogResult.Cancel)
					{
						this.DialogResult = DialogResult.Cancel;
						this.Close();
					}
				}
				ImportBasicServiceMetaData();
				if(!ImportSchemaImports())
				{
					this.DialogResult = DialogResult.Retry;
					this.Close();
					return;
				}
				ImportOperations();
				ImportAdditionalOptions();
				ImportEmbeddedTypes();
				
			}
		}

		/// <summary>
		/// Performs the actions before the summery page is loaded.
		/// </summary>
		/// <param name="sender">Source of the event.</param>
		/// <param name="e">An instance of <see cref="WizardPageSummaryEventArgs"/> with event data.</param>
		private void wsdlWizardCtrl_BeforeSummaryPageDisplayed(object sender, WizardPageSummaryEventArgs e)
		{
			openCodeGenDialog = cbCodeGenDialog.Checked;

			// Assign the modified alternative Xsd paths.
			foreach(ListViewItem li in xsdpathsListView.Items)
			{
				serviceInterfaceContract.Imports[li.Index].AlternateLocation =
					li.SubItems[1].Text;
			}
		}

		/// <summary>
		/// Performs the validation before navigating to another page on the wizard control.
		/// </summary>
		/// <param name="sender">Source of the event.</param>
		/// <param name="e">An instance of <see cref="WizardPageSummaryEventArgs"/> with event data.</param>
		private void wsdlWizardCtrl_ValidatePage(object sender, WizardPageValidateEventArgs e)
		{
			// Validate the basic metadata.
			if(e.Page == wizardPageBasicMetadata)
			{
				if(tbServiceName.Text.Length == 0 || tbNamespace.Text.Length == 0)
				{
					MessageBox.Show("Please enter valid values.", "WSDL Wizard", MessageBoxButtons.OK,
						MessageBoxIcon.Exclamation);
					e.NextPage = 1;
				}
			}

			// Validate the operations list.
			if(e.Page == wizardPageOperationsList)
			{
				if(operationsListView.Items.Count == 0)
				{
					MessageBox.Show("Please specify any operations.", "WSDL Wizard", MessageBoxButtons.OK,
						MessageBoxIcon.Exclamation);
					e.NextPage = 3;
				}
			}

			// Validate the imported schemas list.
			if(e.Page == wizardPageSchemaImports)
			{
				if(messageSchemas.Count == 0 && importsListView.Items.Count == 0)
				{
					MessageBox.Show("Please add at least one XSD file.", "WSDL Wizard", MessageBoxButtons.OK,
						MessageBoxIcon.Exclamation);
					e.NextPage = 2;
				}
			}

			// Validate the message mapping page.
			if (e.Page == wizardPageMessageMapping)
			{
				bool namelessFaultsFound = serviceInterfaceContract.OperationsCollection.OfType<Operation>()
					.SelectMany(operation => operation.Faults)
					.Where(fault => string.IsNullOrEmpty(fault.Name))
					.Any();

				if (namelessFaultsFound)
				{
					MessageBox.Show("Please provide a name for all fault messages.", "WSDL Wizard", MessageBoxButtons.OK,
						MessageBoxIcon.Exclamation);
					e.NextPage = 4;
					return;
				}

				bool duplicateFaultForOperationFound = false;
				foreach (Operation operation in serviceInterfaceContract.OperationsCollection)
				{
					duplicateFaultForOperationFound |= operation.Faults
						.GroupBy(fault => fault.Name)
						.Where(grouping => grouping.Count() > 1)
						.Any();
				}

				if (duplicateFaultForOperationFound)
				{
					MessageBox.Show("Please ensure that all fault messages for an operation have been given a unique name.", "WSDL Wizard", MessageBoxButtons.OK,
						MessageBoxIcon.Exclamation);
					e.NextPage = 4;
				}
			}
		}

		/// <summary>
		/// Performs the actions after displaying a wizard page.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">An instance of <see cref="WizardPageEventArgs"/> with event data.</param>
		private void wsdlWizardCtrl_AfterPageDisplayed(object sender, WizardPageEventArgs e)
		{
			if(e.Page == wizardPageBasicMetadata)
			{
				tbServiceName.Focus();
				return;
			}

			if(e.Page == wizardPageAdditionalOptions)
			{
				if(ptvServiceOperations.PaneNodes.Count > 0)
					ptvServiceOperations.SelectedPaneNode = ptvServiceOperations.PaneNodes[0];
			}
		}

		/// <summary>
		/// Performs the actions upon closing the main wizard form.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">An instance of <see cref="CancelEventArgs"/> with event data.</param>
		/// <remarks>This method shows the confirmation dialog box , if the form is closed using the "Close" button.</remarks>
		private void WsdlWizardForm_Closing(object sender, CancelEventArgs e)
		{
			if(!alreadyCancelled)
			{
                if (MessageBox.Show("Do you really want to quit?",
                    "WSDL Wizard", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    e.Cancel = true;
                }
			}
		}

		/// <summary>
		/// Performs the actions after clicking the "llAddImport" link label.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">An instance of <see cref="LinkLabelLinkClickedEventArgs"/> class with event data.</param>
		/// <remarks>This allows the user to select the XSD files using a file open dialog box and add them 
		/// to the importsListView control.</remarks>
		private void llAddImport_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			// Set the default location and the filter for the .xsd files.
			openFileDialog1.InitialDirectory = this.currentFolder;
			openFileDialog1.Filter = "XSD Files(*.xsd)|*.xsd";

			if(openFileDialog1.ShowDialog() == DialogResult.OK)
			{
				// Check whether the current item exists on the importsListView and add it to the list if it's not.
				string fname = string.Empty;
				fname = openFileDialog1.FileName.Substring(openFileDialog1.FileName.LastIndexOf("\\") + 1);
				ListViewItem li = new ListViewItem(fname);
				li.SubItems.Add(openFileDialog1.FileName);
				
				// Change the location of the XSD if it already exists on the list.
				foreach(ListViewItem eli in importsListView.Items)
				{
					if(eli.Text.ToLower() == li.Text.ToLower())
					{
						eli.SubItems[1].Text = openFileDialog1.FileName;
						return;
					}
				}
				
				importsListView.Items.Add(li);
			}
		}

		/// <summary>
		/// Performs the actions after clicking the llRemoveImport link label.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">An instance of <see cref="LinkLabelLinkClickedEventArgs"/> with event data.</param>
		/// <remarks>This action removes the items from the importsListView control.</remarks>
		private void llRemoveImport_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			// Delete the selected items.
			while(importsListView.SelectedItems.Count != 0)
			{
				importsListView.SelectedItems[0].Remove();
			}
		}
		
		/// <summary>
		/// Displays a tool tip about the list item underneath the mouse pointer.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">An instance of <see cref="MouseEventArgs"/> class with event data.</param>
		private void importsListView_MouseMove(object sender,
			System.Windows.Forms.MouseEventArgs e)
		{
			ListViewItem li = importsListView.GetItemAt(e.X,e.Y);

			if(li == null)
			{
				toolTipPath.Active = false;
			}
			else
			{
				toolTipPath.Active = true;
				toolTipPath.SetToolTip(importsListView,li.SubItems[1].Text);
			}
		}
		
		#endregion

		#region Private methods.

		private void addFaultMessage_Clicked(object sender, EventArgs e)
		{
			ptvServiceOperations.SuspendLayout();

			PaneNode operationNode = ptvServiceOperations.SelectedPaneNode;

			Operation selectedOperation = serviceInterfaceContract.OperationsCollection
				.Cast<Operation>()
				.FirstOrDefault(operation => operation.Name == (string)operationNode.PropertyPane.Tag);
			if (selectedOperation == null) return;

			string name = "Fault" + (selectedOperation.Faults.Count() + 1);
			Message faultMessage = new Message {Name = name, Element = new SchemaElement()};
			ptvServiceOperations.SelectedPaneNode = AddFaultPropertyPane(operationNode, selectedOperation, faultMessage);

			ptvServiceOperations.ResumeLayout(true);
		}

		private PaneNode AddFaultPropertyPane(PaneNode operationNode, Operation selectedOperation, Message faultMessage)
		{
			selectedOperation.Faults.Add(faultMessage);

			string guid = Guid.NewGuid().ToString();

			PropertyPane propPaneFaultMessage = new PropertyPane();
			propPaneFaultMessage.Name = "propPaneFault" + guid;
			propPaneFaultMessage.Text = "Fault " + faultMessage.Name;
			propPaneFaultMessage.Tag = selectedOperation.Name + "Fault";

			// Setup dynamic GUI controls for the pane - FaultMessage	
			Label faultMessageParameterLabel = new Label();
			faultMessageParameterLabel.Location = new Point(6, 5);
			faultMessageParameterLabel.Name = "faultMessageParameterLabel" + guid;
			faultMessageParameterLabel.Size = new Size(55, 33);
			faultMessageParameterLabel.Text = "Message body:";
			faultMessageParameterLabel.TabIndex = 0;

			ComboBox faultMessageParamsCombo = new ComboBox();
			faultMessageParamsCombo.DropDownStyle = ComboBoxStyle.DropDownList;
			faultMessageParamsCombo.Location = new Point(65, 5);
			faultMessageParamsCombo.Name = "faultMessageParamsCombo" + guid;
			faultMessageParamsCombo.Size = new Size(80, 21);
			faultMessageParamsCombo.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
			faultMessageParamsCombo.Tag = selectedOperation;
			faultMessageParamsCombo.TabIndex = 1;

			Label faultMessageNameLabel = new Label();
			faultMessageNameLabel.Location = new Point(6, 48);
			faultMessageNameLabel.Name = "faultMessageNameLabel" + guid;
			faultMessageNameLabel.Text = "Message name:";
			faultMessageNameLabel.AutoSize = true;

			TextBox faultMessageNameTextBox = new TextBox();
			faultMessageNameTextBox.Location = new Point(8, 69);
			faultMessageNameTextBox.Name = "faultMessageNameTextBox" + guid;
			faultMessageNameTextBox.Size = new Size(142, 20);
			faultMessageNameTextBox.Text = faultMessage.Name;
			faultMessageNameTextBox.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
			faultMessageNameTextBox.DataBindings.Add("Text", faultMessage, "Name");
			faultMessageNameTextBox.TextChanged += (o, s) => propPaneFaultMessage.Text = "Fault " + faultMessageNameTextBox.Text;
			faultMessageNameTextBox.TabIndex = 2;

			foreach (SchemaElement schemaElement in faultSchemas)
			{
				faultMessageParamsCombo.Items.Add(schemaElement);
			}

			if (faultMessageParamsCombo.Items.Count > 0)
			{
				faultMessageParamsCombo.SelectedIndex = 0;
			}

			faultMessageParamsCombo.DisplayMember = "ElementName";
			if (!string.IsNullOrEmpty(faultMessage.Element.ElementName))
			{
				faultMessageParamsCombo.SelectedItem = faultMessage.Element;		
			}

			faultMessageParamsCombo.SelectedIndexChanged += (o, s) =>
			{
				faultMessage.Element = (SchemaElement)faultMessageParamsCombo.SelectedItem;
			};

			LinkLabel faultRemoveLink = new LinkLabel();
			faultRemoveLink.Location = new Point(6, 100);
			faultRemoveLink.Text = "Remove Fault Message";
			faultRemoveLink.AutoSize = true;
			faultRemoveLink.TabIndex = 3;

			Label faultDocLabel = new Label();
			faultDocLabel.Location = new Point(6, 125);
			faultDocLabel.Name = "faultDocLabel" + guid;
			faultDocLabel.Size = new Size(88, 23);
			faultDocLabel.Text = "Documentation:";
			faultDocLabel.TabIndex = 4;

			TextBox faultDocTextBox = new TextBox();
			faultDocTextBox.Location = new Point(8, 144);
			faultDocTextBox.Text = faultMessage.Documentation;
			faultDocTextBox.Multiline = true;
			faultDocTextBox.Name = "faultDocTextBox" + guid;
			faultDocTextBox.Size = new Size(142, 0);
			faultDocTextBox.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;
			faultDocTextBox.DataBindings.Add("Text", faultMessage, "Documentation");
			faultDocTextBox.TabIndex = 5;

			propPaneFaultMessage.Controls.Add(faultMessageParameterLabel);
			propPaneFaultMessage.Controls.Add(faultMessageParamsCombo);
			propPaneFaultMessage.Controls.Add(faultMessageNameLabel);
			propPaneFaultMessage.Controls.Add(faultMessageNameTextBox);
			propPaneFaultMessage.Controls.Add(faultRemoveLink);
			propPaneFaultMessage.Controls.Add(faultDocTextBox);
			propPaneFaultMessage.Controls.Add(faultDocLabel);

			operationNode.Expanded = true;
			PaneNode faultPaneNode = operationNode.PropertyPane.PaneNodes.Add(propPaneFaultMessage);

			faultRemoveLink.Click += (o, s) =>
			{
				selectedOperation.Faults.Remove(faultMessage);
				operationNode.PaneNodes.Remove(faultPaneNode);
			};

			return faultPaneNode;
		}

		/// <summary>
		/// Prepares the dynamic controls for the operations-message parameters mapping UI.
		/// </summary>
        private void SetupOperationsMessagesPanes()
        {
            int i = 0;
            ((ISupportInitialize)ptvServiceOperations).BeginInit();
            ptvServiceOperations.SuspendLayout();

            // Clear the existing items.						
            serviceInterfaceContract.OperationsCollection.Clear();
            ptvServiceOperations.PaneNodes.Clear();

            foreach (ListViewItem lvi in operationsListView.Items)
            {
                bool found = false;
                // Check whether the operation already has a property pane or not.
                foreach (PaneNode proPane in ptvServiceOperations.PaneNodes)
                {
                    if (lvi.Text == (string)proPane.PropertyPane.Tag)
                    {
                        found = true;
                        break;
                    }
                }

                if (found)
                {
                    continue;
                }

                Operation operation = new Operation();
                operation.Name = lvi.Text;
                string mepValue = lvi.SubItems[1].Text;
                if (mepValue == "One-Way")
                    operation.Mep = Mep.OneWay;
                else
                    operation.Mep = Mep.RequestResponse;
                operation.Documentation = "";

                string opName = operation.Name;
                string opNamePrefix = opName.Substring(0, 1);
                opNamePrefix = opNamePrefix.ToLower(CultureInfo.CurrentCulture);
                opName = opName.Substring(1, opName.Length - 1);
                opName = opNamePrefix + opName;

                Message inMessage = new Message();
                inMessage.Name = opName + "In";
                inMessage.Documentation = "";
                inMessage.Element = messageSchemas[0];
                operation.MessagesCollection.Add(inMessage);

                MessageHeader inHeader = new MessageHeader();
                inHeader.Name = inMessage.Name + "Header";
                inHeader.Message = inMessage.Name + "Header";

                serviceInterfaceContract.OperationsCollection.Add(operation);

                PropertyPane propPaneOp = new PropertyPane();
                propPaneOp.Name = "propPaneOp" + i;
                propPaneOp.Text = "Operation " + operation.Name;
                propPaneOp.Tag = operation.Name;

				// Setup dynamic GUI controls for the pane - Operation
				LinkLabel addFaultMessage = new LinkLabel();
            	addFaultMessage.Text = "Add Fault Message";
				addFaultMessage.Location = new Point(6, 5);
            	addFaultMessage.Click += addFaultMessage_Clicked;
				propPaneOp.Controls.Add(addFaultMessage);

            	ptvServiceOperations.PaneNodes.Add(propPaneOp);

            	PropertyPane propPaneInMsg = new PropertyPane();
                propPaneInMsg.Name = "propPaneInMsg" + i;
                propPaneInMsg.Text = "Message " + inMessage.Name;
                propPaneOp.PaneNodes.Add(propPaneInMsg);
                propPaneOp.PaneNode.Expanded = true;

                PropertyPane propPaneOutMsg = null;
                Message outMessage = null;
                MessageHeader outHeader = null;

                if (operation.Mep == Mep.RequestResponse)
                {
                    outMessage = new Message();
                    outMessage.Name = opName + "Out";
                    outMessage.Documentation = "";
                    outMessage.Element = messageSchemas[0];
                    operation.MessagesCollection.Add(outMessage);

                    outHeader = new MessageHeader();
                    outHeader.Name = outMessage.Name + "Header";
                    outHeader.Message = outMessage.Name + "Header";

                    propPaneOutMsg = new PropertyPane();
                    propPaneOutMsg.Name = "propPaneOutMsg" + i;
                    propPaneOutMsg.Text = "Message " + outMessage.Name;
                    propPaneOp.PaneNodes.Add(propPaneOutMsg);
                    propPaneOp.PaneNode.Expanded = true;
                }

                // Setup dynamic GUI controls for the pane - Operation
                TextBox opDocTextBox = new TextBox();
                opDocTextBox.Location = new Point(8, 148);
                opDocTextBox.Multiline = true;
                opDocTextBox.Name = "outDocTextBox" + i;
                opDocTextBox.Size = new Size(135, 0);
                opDocTextBox.DataBindings.Add("Text", operation, "Documentation");

                opDocTextBox.Anchor = AnchorStyles.Left | AnchorStyles.Top |
                    AnchorStyles.Right | AnchorStyles.Bottom;

                Label opDocLabel = new Label();
                opDocLabel.Location = new Point(8, 128);
                opDocLabel.Name = "outDocLabel" + i;
                opDocLabel.Size = new Size(88, 23);
                opDocLabel.Text = "Documentation:";

                propPaneOp.Controls.Add(opDocTextBox);
                propPaneOp.Controls.Add(opDocLabel);

                // Setup dynamic GUI controls for the pane - InMessage									
                TextBox inDocTextBox = new TextBox();
                inDocTextBox.Location = new Point(8, 168);
                inDocTextBox.Multiline = true;
                inDocTextBox.Name = "inDocTextBox" + i;
                inDocTextBox.Size = new Size(135, 0);
                inDocTextBox.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;

                Label inMessageParameterLabel = new Label();
                inMessageParameterLabel.Location = new Point(6, 5);
                inMessageParameterLabel.Name = "inMessageParameterLabel" + i;
                inMessageParameterLabel.Size = new Size(55, 33);
                inMessageParameterLabel.Text = "Message body:";

                CheckBox inMessageNameCheckBox = new CheckBox();
                inMessageNameCheckBox.Location = new Point(9, 44);
                inMessageNameCheckBox.Name = "inMessageNameCheckBox" + i;
                inMessageNameCheckBox.Text = "Message name (optional)";
                inMessageNameCheckBox.Size = new Size(220, 25);

                TextBox inMessageNameTextBox = new TextBox();
                inMessageNameTextBox.Location = new Point(8, 69);
                inMessageNameTextBox.Name = "inMessageNameTextBox" + i;
                inMessageNameTextBox.Size = new Size(142, 20);
                inMessageNameTextBox.Enabled = false;
                inMessageNameTextBox.Text = inMessage.Name;
                inMessageNameTextBox.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;

                ComboBox inMessageParamsCombo = new ComboBox();
                inMessageParamsCombo.DropDownStyle = ComboBoxStyle.DropDownList;
                inMessageParamsCombo.Location = new Point(65, 5);
                inMessageParamsCombo.Name = "inMessageParamsCombo" + i;
                inMessageParamsCombo.Size = new Size(80, 21);
                inMessageParamsCombo.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;

                // Fill the combo box with the Message values.
                foreach (SchemaElement element in this.messageSchemas)
                {
                    inMessageParamsCombo.Items.Add(element.ElementName);
                }
                inMessageParamsCombo.SelectedIndex = 0;

                operation.Input = inMessage;

                // Import the parameters from WSDL file for round tripping.
                string originalOperationName = "";
                if (lvi.Tag != null)
                {
                    originalOperationName = lvi.Tag.ToString();
                }

                ImportMessageParameter(operation, false, inMessageParamsCombo,
                    inMessageNameCheckBox, inMessageNameTextBox, propPaneInMsg,
                    opDocTextBox, inDocTextBox, originalOperationName);

                // Attach the dynamic combo box event handler.
                inMessageParamsCombo.SelectedIndexChanged +=
                    new EventHandler(DynamicComboBox_SelectedIndexChanged);

                CheckBox inMessageHeaderCheckBox = new CheckBox();
                inMessageHeaderCheckBox.Location = new Point(9, 95);
                inMessageHeaderCheckBox.Name = "inMessageHeaderCheckBox" + i;
                inMessageHeaderCheckBox.Text = "Message headers (optional)";
                inMessageHeaderCheckBox.Size = new Size(181, 25);

                ComboBox inMessageHeaderCombo = new ComboBox();
                inMessageHeaderCombo.DropDownStyle = ComboBoxStyle.DropDownList;
                inMessageHeaderCombo.Location = new Point(8, 120);
                inMessageHeaderCombo.Enabled = false;
                inMessageHeaderCombo.Name = "inMessageHeaderCombo" + i;
                inMessageHeaderCombo.Size = new Size(100, 21);
                inMessageHeaderCombo.Items.Add("<New...>");
                inMessageHeaderCombo.SelectedIndex = 0;
                inMessageHeaderCombo.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;

                // Import headers from WSDL file for round tripping.
                MessagesCollection addedInHeaderMsgs = ImportMessageHeaders(operation, inMessage,
                    inMessageHeaderCombo, inMessageHeaderCheckBox, false);

                // Attach the dynamic combo box event handler.
                inMessageHeaderCombo.SelectedIndexChanged +=
                    new EventHandler(DynamicComboBox_SelectedIndexChanged);

                LinkLabel inRemoveHeaderLink = new LinkLabel();
                inRemoveHeaderLink.Text = "Remove";
                inRemoveHeaderLink.Location = new Point(193, 95);
                inRemoveHeaderLink.Size = new Size(64, 25);
                inRemoveHeaderLink.TextAlign = ContentAlignment.MiddleRight;
                inRemoveHeaderLink.Visible = (addedInHeaderMsgs != null);
                // inRemoveHeaderLink.Anchor = AnchorStyles.Right;

                // Initialize the dynamic control controllers.
                MessagePaneLabelController mplcIn =
                    new MessagePaneLabelController(inMessageNameTextBox, propPaneInMsg);
                MessageNameTextBoxController mntbcIn =
                    new MessageNameTextBoxController(inMessageNameTextBox, inMessageNameCheckBox,
                        inMessage, propPaneInMsg);
                InHeaderComboBoxController hcbcIn =
                    new InHeaderComboBoxController(inMessageHeaderCombo, inMessageHeaderCheckBox,
                        inMessageNameTextBox, operation, inMessage, inHeader, this.headerSchemas,
                        inRemoveHeaderLink, addedInHeaderMsgs);
                OperationDocumentationTextBoxController odtbc =
                    new OperationDocumentationTextBoxController(opDocTextBox, operation);
                MessageDocumentationTextBoxController mdtbc =
                    new MessageDocumentationTextBoxController(inDocTextBox, inMessage);


                Label inDocLabel = new Label();
                inDocLabel.Location = new Point(8, 149);
                inDocLabel.Name = "inDocLabel" + i;
                inDocLabel.Size = new Size(88, 23);
                inDocLabel.Text = "Documentation:";

                // Finally add the controls to the container.
                propPaneInMsg.Controls.Add(inDocTextBox);
                propPaneInMsg.Controls.Add(inDocLabel);
                propPaneInMsg.Controls.Add(inMessageParameterLabel);
                propPaneInMsg.Controls.Add(inMessageNameTextBox);
                propPaneInMsg.Controls.Add(inMessageParameterLabel);
                propPaneInMsg.Controls.Add(inMessageParamsCombo);
                propPaneInMsg.Controls.Add(inMessageNameCheckBox);
                propPaneInMsg.Controls.Add(inMessageHeaderCombo);
                propPaneInMsg.Controls.Add(inMessageHeaderCheckBox);
                propPaneInMsg.Controls.Add(inRemoveHeaderLink);

                // Setup dynamic GUI controls for the pane - OutMessage
                if (operation.Mep == Mep.RequestResponse)
                {
                    TextBox outDocTextBox = new TextBox();
                    outDocTextBox.Location = new Point(8, 165);
                    outDocTextBox.Multiline = true;
                    outDocTextBox.Name = "outDocTextBox" + i;
                    outDocTextBox.Size = new Size(135, 0);
                    outDocTextBox.DataBindings.Add("Text", outMessage, "Documentation");
                    outDocTextBox.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right |
                        AnchorStyles.Bottom;

                    Label outMessageParameterLabel = new Label();
                    outMessageParameterLabel.Location = new Point(6, 5);
                    outMessageParameterLabel.Name = "outMessageParameterLabel" + i;
                    outMessageParameterLabel.Size = new Size(55, 33);
                    outMessageParameterLabel.Text = "Message body:";

                    CheckBox outMessageNameCheckBox = new CheckBox();
                    outMessageNameCheckBox.Location = new Point(9, 44);
                    outMessageNameCheckBox.Name = "outMessageNameCheckBox" + i;
                    outMessageNameCheckBox.Text = "Message name (optional)";
                    outMessageNameCheckBox.Size = new Size(220, 25);

                    TextBox outMessageNameTextBox = new TextBox();
                    outMessageNameTextBox.Location = new Point(8, 69);
                    outMessageNameTextBox.Name = "outMessageNameTextBox" + i;
                    outMessageNameTextBox.Size = new Size(142, 20);
                    outMessageNameTextBox.Enabled = false;
                    outMessageNameTextBox.Text = outMessage.Name;
                    outMessageNameTextBox.Anchor = AnchorStyles.Left | AnchorStyles.Top |
                        AnchorStyles.Right;

                    ComboBox outMessageParamsCombo = new ComboBox();
                    outMessageParamsCombo.DropDownStyle = ComboBoxStyle.DropDownList;
                    outMessageParamsCombo.Location = new Point(65, 5);
                    outMessageParamsCombo.Name = "outMessageParamsCombo" + i;
                    outMessageParamsCombo.Size = new Size(80, 21);
                    outMessageParamsCombo.Anchor = AnchorStyles.Left | AnchorStyles.Top |
                        AnchorStyles.Right;

                    // Fill the combo box with the Message values.
                    foreach (SchemaElement element in this.messageSchemas)
                    {
                        outMessageParamsCombo.Items.Add(element.ElementName);
                    }
                    outMessageParamsCombo.SelectedIndex = 0;
                    operation.Output = outMessage;

                    // Import the parameters from an existing WSDL file for round tripping.
                    ImportMessageParameter(operation, true, outMessageParamsCombo, outMessageNameCheckBox,
                        outMessageNameTextBox, propPaneOutMsg, opDocTextBox, outDocTextBox, originalOperationName);

                    // Attach the dynamic combo box event handler.
                    outMessageParamsCombo.SelectedIndexChanged +=
                        new EventHandler(DynamicComboBox_SelectedIndexChanged);

                    CheckBox outMessageHeaderCheckBox = new CheckBox();
                    outMessageHeaderCheckBox.Location = new Point(9, 95);
                    outMessageHeaderCheckBox.Name = "outMessageHeaderCheckBox" + i;
                    outMessageHeaderCheckBox.Text = "Message headers (optional)";
                    outMessageHeaderCheckBox.Size = new Size(181, 25);

                    ComboBox outMessageHeaderCombo = new ComboBox();
                    outMessageHeaderCombo.DropDownStyle = ComboBoxStyle.DropDownList;
                    outMessageHeaderCombo.Location = new Point(8, 120);
                    outMessageHeaderCombo.Enabled = false;
                    outMessageHeaderCombo.Name = "outMessageHeaderCombo" + i;
                    outMessageHeaderCombo.Size = new Size(100, 21);
                    outMessageHeaderCombo.Items.Add("<New...>");
                    outMessageHeaderCombo.SelectedIndex = 0;
                    outMessageHeaderCombo.Anchor = AnchorStyles.Left | AnchorStyles.Top |
                        AnchorStyles.Right;

                    // Import headers from an existing WSDL file for round tripping.
                    MessagesCollection addedOutHeaderMsgs = ImportMessageHeaders(operation, outMessage,
                        outMessageHeaderCombo, outMessageHeaderCheckBox, true);

                    // Attach the dynamic combo box event handler.
                    outMessageHeaderCombo.SelectedIndexChanged +=
                        new EventHandler(DynamicComboBox_SelectedIndexChanged);

                    LinkLabel outRemoveHeaderLink = new LinkLabel();
                    outRemoveHeaderLink.Text = "Remove";
                    outRemoveHeaderLink.Location = new Point(193, 95);
                    outRemoveHeaderLink.Size = new Size(64, 25);
                    outRemoveHeaderLink.TextAlign = ContentAlignment.MiddleRight;
                    outRemoveHeaderLink.Visible = (addedOutHeaderMsgs != null);
                    // outRemoveHeaderLink.Anchor = AnchorStyles.Right;

                    // Initialize the dynamic control controllers.
                    MessagePaneLabelController mplcOut =
                        new MessagePaneLabelController(outMessageNameTextBox, propPaneOutMsg);
                    MessageNameTextBoxController mntbcOut =
                        new MessageNameTextBoxController(outMessageNameTextBox, outMessageNameCheckBox,
                            outMessage, propPaneOutMsg);
                    OutHeaderComboBoxController hcbcOut =
                        new OutHeaderComboBoxController(outMessageHeaderCombo, outMessageHeaderCheckBox,
                            outMessageNameTextBox, operation, outMessage, outHeader, headerSchemas,
                            outRemoveHeaderLink, addedOutHeaderMsgs);
                    MessageDocumentationTextBoxController outdtbc =
                        new MessageDocumentationTextBoxController(outDocTextBox, outMessage);


                    Label outDocLabel = new Label();
                    outDocLabel.Location = new Point(8, 149);
                    outDocLabel.Name = "outDocLabel" + i;
                    outDocLabel.Size = new Size(88, 23);
                    outDocLabel.Text = "Documentation:";

                    // Finally add the generated controls to the container.
                    propPaneOutMsg.Controls.Add(outDocTextBox);
                    propPaneOutMsg.Controls.Add(outDocLabel);
                    propPaneOutMsg.Controls.Add(outMessageParameterLabel);
                    propPaneOutMsg.Controls.Add(outMessageNameTextBox);
                    propPaneOutMsg.Controls.Add(outMessageParameterLabel);
                    propPaneOutMsg.Controls.Add(outMessageParamsCombo);
                    propPaneOutMsg.Controls.Add(outMessageNameCheckBox);
                    propPaneOutMsg.Controls.Add(outMessageHeaderCombo);
                    propPaneOutMsg.Controls.Add(outMessageHeaderCheckBox);
                    propPaneOutMsg.Controls.Add(outRemoveHeaderLink);

                }

				ImportFaultMessages(propPaneOp.PaneNode, operation, originalOperationName);

                i++;
            }

            oldOperations = (OperationsCollection)serviceInterfaceContract.OperationsCollection.Clone();

            ((ISupportInitialize)ptvServiceOperations).EndInit();
            ptvServiceOperations.ResumeLayout(false);
        }

		/// <summary>
		/// Reads the XML comment from the resource file. Wizard places this XML comment on the top of the 
		/// WSDL file. XML comment is stored in the resource file with "RT_HEADER" key. Once the XML comment is
		/// read, the procedure will replace the substring - @VERSION@ in it with application's current version
		/// number. 
		/// </summary>
		/// <returns>The formatted XML comment with the application's current version number.</returns>
		private string GetXmlCommentForWSDL()
		{
			// Read the current application version.
			Version ver = Assembly.GetExecutingAssembly().GetName().Version;
			string version = ver.Major.ToString() + "." + ver.Minor.ToString() + "." + 
				ver.Build.ToString() + "." + ver.Revision.ToString();

			// Get the XML comment and replace the @VERSION@ with the current version.
			string comment = ResourceHelper.GetString(
				"Thinktecture.Tools.Web.Services.WsdlWizard.Resources.AssemblyResources", 
				System.Reflection.Assembly.GetExecutingAssembly(), 
				"RT_HEADER");
			comment = comment.Replace("@VERSION@", version);

			return comment;
		}
		
		/// <summary>
		/// Updates the current folder, every time the schemaLocation is set.
		/// </summary>
		private void UpdateCurrentFolder(string location)
		{
			int lastBackSlash = location.LastIndexOf("\\");
			if(lastBackSlash > 0)
			{
				this.currentFolder = location.Substring(0, lastBackSlash + 1);
			}
			else
			{
				this.currentFolder = location;
			}
		}
		
		/// <summary>
		/// Imports the basic metadata to UI controls from an existing WSDL <see cref="InterfaceContract"/>.
		/// </summary>
		private void ImportBasicServiceMetaData()
		{
			if(importedContract != null)
			{
				tbServiceName.Text = importedContract.ServiceName;
				tbNamespace.Text = importedContract.ServiceNamespace;
				tbServiceDoc.Text = importedContract.ServiceDocumentation;
			}
		}

		/// <summary>
		/// Imports the imported schema files to UI from an existing WSDL <see cref="InterfaceContract"/>.
		/// </summary>
		private bool ImportSchemaImports()
		{
			importsListView.Items.Clear();

			foreach(SchemaImport import in importedContract.Imports)
			{
				string fname = string.Empty;
				ListViewItem importItem = new ListViewItem();
				
				// Check whether the file is a reference to an URI.
				if(import.SchemaLocation.ToLower().StartsWith("http://"))
				{
					// Obtain the file from the web and save it in the project folder. Then map that 
					// file name in the WSDL.
					WebRequest req = WebRequest.Create(import.SchemaLocation);
					WebResponse result = null;
					try
					{
						result = req.GetResponse();
					}
					catch
					{
						MessageBox.Show("Could not import the XSD file(s) from the specified URI.",
							"WSDL Wizard", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);						
						return false;
					}

					fname = import.SchemaLocation.Substring(
						import.SchemaLocation.LastIndexOf("/") + 1);

					importItem.Text = fname;
					importItem.SubItems.Add(import.SchemaLocation);
				}
				else if(import.SchemaLocation.LastIndexOf('\\') > -1)
				{
					fname = import.SchemaLocation.Substring(
						import.SchemaLocation.LastIndexOf("\\") + 1);
					
					importItem.Text = fname;
					importItem.SubItems.Add(import.SchemaLocation);
				}
				else
				{
					string fqName = "";
					fqName = import.SchemaLocation;

					if(fqName.StartsWith("../"))
					{
						fqName = IOPathHelper.GetAbsolutePath(fqName, this.wsdlLocation, this.projectRootDirectory);
					}
					else
					{
						fqName = IOPathHelper.GetAbsolutePath(fqName, this.wsdlLocation, this.projectRootDirectory);						
					}				
					
					fname = fqName.Substring(fqName.LastIndexOf("\\") + 1);
									
					if(File.Exists(fqName))
					{
						importItem.Text = fname;
						importItem.SubItems.Add(fqName);				
					}
					else
					{
						MessageBox.Show("Could not locate the XSD file(s) imported to this WSDL",
							"WSDL Wizard", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);						
						return false;
					}
				}

				importsListView.Items.Add(importItem);
			}

			return true;
		}

		/// <summary>
		/// Imports the service operations to UI from an existing WSDL <see cref="InterfaceContract"/>.
		/// </summary>
		private void ImportOperations()
		{
			if(importedContract != null)
			{
				operationsListView.Items.Clear();

				foreach(Operation op in importedContract.OperationsCollection)
				{
					string mep = "Request/Response";

					if(op.Mep == Mep.OneWay)
						mep = "One-Way";

					ListViewItem listViewItem1 =
						new ListViewItem(new string[] {op.Name, mep, "", ""}, -1);
					listViewItem1.Tag = op.Name;
					operationsListView.Items.AddRange(
						new ListViewItem[] {listViewItem1});
				
					// Check whether the Op name is using the default naming pattern (i.e. OperationX) and
					// increment the counter.
					// TODO: We can use a regular expression here to check this.
					if(op.Name.ToLower().StartsWith("operation"))
					{
						string tail = op.Name.Substring(9);
						try
						{
							int.Parse(tail);
							operationCount++;
						}
						catch(FormatException)
						{
						}
					}
				}
			}
		}

		private void ImportFaultMessages(PaneNode operationNode, Operation operation, string originalOperationName)
		{
			if(oldOperations.Count > 0)
			{
				ImportFaultMessages(operationNode, operation, originalOperationName, oldOperations);
				return;
			}

			if(importedContract != null)
			{
				ImportFaultMessages(operationNode, operation, originalOperationName, importedContract.OperationsCollection);
				return;
			}
			
			if(inferOperations.Count > 0)
			{
				ImportFaultMessages(operationNode, operation, originalOperationName, inferOperations);
				return;
			}
		}

		private void ImportFaultMessages(PaneNode operationNode, Operation operation, string originalOperationName, OperationsCollection operations)
		{
			foreach (Operation op in operations)
			{
				if (op.Name == operation.Name || (originalOperationName != string.Empty && op.Name == originalOperationName))
				{
					foreach (Message faultMessage in op.Faults)
					{
						AddFaultPropertyPane(operationNode, operation, faultMessage);
					}
				}
			}			
		}
		
		/// <summary>
		/// Imports the message parameters for a specified operation from an existing WSDL. 
		/// <see cref="InterfaceContract"/>. 
		/// </summary>
		/// <param name="op">The <see cref="Operation"/> to import the message parameters in to.</param>
		/// <param name="output">Indicates whether the current message is input or output.</param>
		/// <param name="parametersCombo">Reference to the combo box which contains the message parameters.</param>
		/// <param name="chkName">Reference to the check box, which indicates whether the customized message name is selected or not.</param>
		/// <param name="tbName">Reference to the text box which contains the customized message name. </param>
		/// <param name="messageLabel">Reference to the Property Pane which contains the message name.</param>
		/// <param name="opDocumentation">Reference to the text box which contains the <see cref="Operation"/>'s documentation.</param>
		/// <param name="msgDocumentation">Reference to the text box which contains the <see cref="Message"/>'s documentation.</param>
		/// <param name="originalOperationName">String which contains the original name of the operation. User can always change the operation name after infering operations or while round-tripping. This parameter contains the name of the operation before any of those alternations.</param>
		private void ImportMessageParameter(Operation op, bool output, ComboBox parametersCombo, 
			CheckBox chkName, TextBox tbName, PropertyPane messageLabel, TextBox opDocumentation, 
			TextBox msgDocumentation, string originalOperationName)
		{
			if(oldOperations.Count > 0)
			{
				if(ImportMessageParameter(oldOperations, op, output, 
					parametersCombo, chkName, tbName, messageLabel, 
					opDocumentation, msgDocumentation, originalOperationName))
				{
					return;
				}
			}

			if(importedContract != null)
			{
				if(ImportMessageParameter(importedContract.OperationsCollection, op, output, 
					parametersCombo, chkName, tbName, messageLabel, opDocumentation, msgDocumentation, 
					originalOperationName))
				{
					return;
				}
			}
			
			if(inferOperations.Count > 0)
			{
				if(ImportMessageParameter(inferOperations, op, output, 
					parametersCombo, chkName, tbName, messageLabel, 
					opDocumentation, msgDocumentation, originalOperationName))
				{
					return;
				}
			}
			
		}

		/// <summary>
		/// Imports the message parameters for a specified operation from an existing WSDL. 
		/// </summary>
		/// <param name="operations">Collection of operations to import the message parameters from.</param>
		/// <param name="op">The <see cref="Operation"/> to import the message parameters in to.</param>
		/// <param name="output">Indicates whether the current message is input or output.</param>
		/// <param name="parametersCombo">Reference to the combo box which contains the message parameters.</param>
		/// <param name="chkName">Reference to the check box, which indicates whether the customized message name is selected or not.</param>
		/// <param name="tbName">Reference to the text box which contains the customized message name. </param>
		/// <param name="messageLabel">Reference to the Property Pane which contains the message name.</param>
		/// <param name="opDocumentation">Reference to the text box which contains the <see cref="Operation"/>'s documentation.</param>
		/// <param name="msgDocumentation">Reference to the text box which contains the <see cref="Message"/>'s documentation.</param>
		/// <returns>A boolean indicating whether any message parameter is imported or not.</returns>
		/// <param name="originalOperationName">String which contains the original name of the operation. User can always change the operation name after infering operations or while round-tripping. This parameter contains the name of the operation before any of those alternations.</param>		
		private bool ImportMessageParameter(OperationsCollection operations, Operation op, bool output, 
			ComboBox parametersCombo, CheckBox chkName, TextBox tbName, PropertyPane messageLabel, 
			TextBox opDocumentation, TextBox msgDocumentation, string originalOperationName)
		{
			foreach(Operation importedOp in operations)
			{
				if(importedOp.Name == op.Name || (originalOperationName != "" && importedOp.Name == originalOperationName))
				{
					// Identify the message from the message collection.
					Message importedMsg = null;
					Message msg = null;
					if(!output)
					{
						importedMsg = importedOp.Input;
						msg = op.Input;
					}
					else
					{
						importedMsg = importedOp.Output;
						msg = op.Output;
					}
					
					if(importedMsg != null)
					{
						// Set the new message parameters.
						int index = 0;
						foreach(SchemaElement element in messageSchemas)
						{
							if(importedMsg.Element == element)
							{
								msg.Element = element;
								parametersCombo.SelectedIndex = index;
								break;
							}
							index++;
						}
					
						// Enable the custom name text box and put the custom message name in.
						
						if(importedOp.Name == op.Name && msg.Name != importedMsg.Name)
						{
							tbName.Enabled = true;
							tbName.Text = importedMsg.Name;
							msg.Name = importedMsg.Name;
							messageLabel.Text = importedMsg.Name;
							chkName.Checked = true;
						}

						// Set the Message documentation.
						msg.Documentation = importedMsg.Documentation;
						msgDocumentation.Text = importedMsg.Documentation;
					}

					// Set the operation documentation.
					op.Documentation = importedOp.Documentation;
					opDocumentation.Text = importedOp.Documentation;

					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Imports the message headers for a specified message from an existing WSDL. 
		/// <see cref="InterfaceContract"/>.
		/// </summary>		
		/// <param name="op">Reference to the <see cref="Operation"/>, <see cref="Message"/> msg belongs to.</param>
		/// <param name="msg">Reference to the <see cref="Message"/>, headers belong to.</param>
		/// <param name="headersCombo">Reference to the combo box which contains the headers.</param>
		/// <param name="hasHeaders">Reference to the check box which indicates whether the custom headers are added or not.</param>
		/// <param name="outMessage">Indicates whether the current message is input or output.</param>
		/// <returns>An instance of <see cref="MessagesCollection"/> class with the added header messages.</returns>
		private MessagesCollection ImportMessageHeaders(Operation op, Message msg, 
			ComboBox headersCombo, CheckBox hasHeaders, bool outMessage)
		{
			// Read the message headers from the old operations collection if available.
			MessagesCollection addedHeaderMessages = null;
			if(oldOperations.Count > 0)
			{
				addedHeaderMessages = 
					ImportMessageHeaders(oldOperations, op, msg, headersCombo, 
					hasHeaders, outMessage);
				if(addedHeaderMessages != null)
				{
					return addedHeaderMessages;
				}
			}	
			else
			{
				// Read the message headers from the imported contract.
				if(importedContract != null)
				{
					addedHeaderMessages = 
						ImportMessageHeaders(importedContract.OperationsCollection, op, msg, headersCombo, 
						hasHeaders, outMessage);
					if(addedHeaderMessages != null)
					{
						return addedHeaderMessages;
					}
				}
			}
			return addedHeaderMessages;
		}

		/// <summary>
		/// Imports the message headers for a specified message from an existing WSDL. 
		/// <see cref="InterfaceContract"/>.
		/// </summary>
		/// <param name="ops">Reference to the <see cref="OperationsCollection"/> to search for the <see cref="Operation"/>, which <see cref="Message"/> msg belongs to.</param>
		/// <param name="op">Reference to the <see cref="Operation"/>, <see cref="Message"/> msg belongs to.</param>
		/// <param name="msg">Reference to the <see cref="Message"/>, headers belong to.</param>
		/// <param name="headersCombo">Reference to the combo box which contains the headers.</param>
		/// <param name="hasHeaders">Reference to the check box which indicates whether the custom headers are added or not.</param>
		/// <param name="outMessage">Indicates whether the current message is input or output.</param>
		/// <returns>An instance of <see cref="MessagesCollection"/> class with the added header messages.</returns>
		private MessagesCollection ImportMessageHeaders(OperationsCollection ops, Operation op, 
			Message msg, ComboBox headersCombo, CheckBox hasHeaders, bool outMessage)
		{
			MessagesCollection addedHeaderMessages = null;
			foreach(Operation importedOp in ops)
			{
				if(importedOp.Name == op.Name)
				{
					Message importedMsg = null;
					if(!outMessage)
					{
						importedMsg = importedOp.Input;
					}
					else
					{
						importedMsg = importedOp.Output;
					}
				
					// Check for the headers.
					if(importedMsg != null && importedMsg.HeadersCollection.Count > 0)
					{
						addedHeaderMessages = new MessagesCollection();
						hasHeaders.Checked = true;   // Enable headers check box.
						headersCombo.Enabled = true; // Enable the headers combo box.
						
						// Add the headers to current message's headers collection.
						foreach(MessageHeader header in importedMsg.HeadersCollection)
						{
							// Find and add the header message to the operation.
							foreach(Message headerMsg in importedOp.MessagesCollection)
							{
								if(headerMsg.Name == header.Message)
								{
									msg.HeadersCollection.Add(header);

									op.MessagesCollection.Add(headerMsg);
									addedHeaderMessages.Add(headerMsg);

									// Finally add the header details to the combo box.
									headersCombo.Items.Insert(headersCombo.Items.Count - 1, 
										headerMsg.Element.ElementName);
								}
							}														
						}

						break;
					}
				}
			}

			return addedHeaderMessages;
		}

		/// <summary>
		/// Imports the additional options from an existing WSDL <see cref="InterfaceContract"/>.
		/// </summary>
		private void ImportAdditionalOptions()
		{
			if(importedContract != null)
			{
				this.cbNeedsServiceElement.Checked = importedContract.NeedsServiceElement;
                cbSoap12.Checked = (importedContract.Bindings & InterfaceContract.SoapBindings.Soap12) == InterfaceContract.SoapBindings.Soap12;
			}
		}
		
		/// <summary>
		/// Imports the embedded types in the WSDL.
		/// </summary>
		private void ImportEmbeddedTypes()
		{
			if(importedContract != null)
			{
				this.messageSchemas.AddRange(importedContract.Types);
				this.headerSchemas.AddRange(importedContract.Types);
				faultSchemas.AddRange(importedContract.Types);
			}
		}

		/// <summary>
		/// Infers the operations according to the message name patterns in the message contracts.
		/// </summary>
		private void InferOperations()
		{
			// Define a hash table with list of request/response patterns.
			Dictionary<string, string> patterns = new Dictionary<string, string>();
			patterns.Add("", "Response");
			patterns.Add("Request", "Response");
			patterns.Add("RequestMessage", "ResponseMessage");
			
			SchemaElements usedElements = new SchemaElements();
			
			// Infer two-way operations.
			foreach(SchemaElement melement in messageSchemas)
			{
				foreach(string requestPattern in patterns.Keys)
				{
					string operationName = "";
					if(melement.ElementName.EndsWith(requestPattern))
					{
						string responsePattern = patterns[requestPattern];
						
						// Create the response element.
						SchemaElement responseElement = new SchemaElement();
						if(requestPattern == "")
						{
							operationName = melement.ElementName;
							responseElement.ElementName = melement.ElementName + responsePattern;
						}
						else
						{
							if(melement.ElementName == requestPattern)
							{
								operationName = melement.ElementName;
								responseElement.ElementName = responsePattern;	
							}
							else
							{
								operationName = 
									melement.ElementName.Substring(0, 
									melement.ElementName.LastIndexOf(requestPattern));
								responseElement.ElementName = 
									melement.ElementName.Substring(0, 
									melement.ElementName.LastIndexOf(requestPattern)) + responsePattern;
							}
						}
						responseElement.ElementNamespace = melement.ElementNamespace;

						if(messageSchemas.Contains(responseElement))
						{
							// Check whether the operation exists in the imported operations list.
							bool exists = false;
							if(importedContract != null)
							{
								foreach(Operation impOp in importedContract.OperationsCollection)
								{
									if(impOp.Input.Element == melement && 
										(impOp.Output != null && impOp.Output.Element == responseElement))
									{
										exists = true;
										break;
									}
								}
							}
							
							if(exists)
							{
								break;
							}

							// Add the operation to the list.
							Operation op = new Operation();
							op.Name = operationName;
							op.Documentation = "";
							op.Mep = Mep.RequestResponse;
							
							string opName = op.Name;
							string opNamePrefix = opName.Substring(0, 1);
							opNamePrefix = opNamePrefix.ToLower(CultureInfo.CurrentCulture);
							opName = op.Name.Substring(1, op.Name.Length - 1);
							opName = opNamePrefix + opName;

							// Add the input message.
							Message input = new Message();
							input.Name = opName + "In";
							input.Element = melement;
							input.Documentation = "";
							op.MessagesCollection.Add(input);
							op.Input = input;
					
							// Add the output message.
							Message output = new Message();
							output.Name = opName + "Out";
							output.Element = responseElement;
							output.Documentation = "";
							op.MessagesCollection.Add(output);
							op.Output = output;
					
							// Add the operation to the inferred operations collection.
							inferOperations.Add(op);

							// Add the operation to the list view.
							ListViewItem listViewItem1 =
								new ListViewItem(new string[] {
								op.Name,
								"Request/Response",
								"",
								""},
								-1);

							listViewItem1.Tag = op.Name;
							operationsListView.Items.AddRange(
								new ListViewItem[] {listViewItem1});
							
							usedElements.Add(melement);
							usedElements.Add(responseElement);
							// Exit this loop.
							break;
						}						
					}
				}
			}

			// Infer one-way operations.
			foreach(SchemaElement melement in messageSchemas)
			{
				foreach(string requestPattern in patterns.Keys)
				{
					string operationName = "";
					if(melement.ElementName.EndsWith(requestPattern))
					{					
						if(!usedElements.Contains(melement))
						{
							// Check whether the operation exists in the imported operations list.
							bool exists = false;
							if(importedContract != null)
							{
								foreach(Operation impOp in importedContract.OperationsCollection)
								{
									if(impOp.Input.Element == melement  || 
										(impOp.Output != null && impOp.Output.Element == melement))
									{
										exists = true;
										break;
									}
								}
							}
							
							if(exists)
							{
								break;
							}
							
							// Compose the operation name.
							if(requestPattern == "")
							{
								operationName = melement.ElementName;						
							}
							else
							{
								if(melement.ElementName == requestPattern)
								{
									operationName = melement.ElementName;	
								}
								else
								{
									operationName = 
										melement.ElementName.Substring(0, 
										melement.ElementName.LastIndexOf(requestPattern));									
								}
							}

							// Add the operation to the list.
							Operation op = new Operation();
							op.Name = operationName;
							op.Documentation = "";
							op.Mep = Mep.OneWay;
					
							string opName = op.Name;
							string opNamePrefix = opName.Substring(0, 1);
							opNamePrefix = opNamePrefix.ToLower(CultureInfo.CurrentCulture);
							opName = op.Name.Substring(1, op.Name.Length - 1);
							opName = opNamePrefix + opName;

							// Add the input message.
							Message input = new Message();
							input.Name = opName + "In";
							input.Element = melement;
							input.Documentation = "";
							op.MessagesCollection.Add(input);
							op.Input = input;
					
							// Add the operation to the inferred operations collection.
							inferOperations.Add(op);

							// Add the operation to the list view.
							ListViewItem listViewItem1 =
								new ListViewItem(new string[] {
									op.Name,
									"One-Way",
									"",
									""},
								-1);

							listViewItem1.Tag = op.Name;
							operationsListView.Items.AddRange(
								new ListViewItem[] {listViewItem1});					
						}						
						// Exit this loop.
						break;
					}
				}
			}
		}
		
		/// <summary>
		/// Removes the inferred operations.
		/// </summary>
		private void RemoveInferedOperations()
		{
			int index = 0;
			while(index != operationsListView.Items.Count)
			{
				bool removed = false;
				ListViewItem li = operationsListView.Items[index];
				foreach(Operation op in inferOperations)
				{
					if(op.Name == li.Text)
					{
						li.Remove();
						removed = true;
						break;
					}
				}
				
				if(!removed)
				{
					index++;
				}
			}
			inferOperations.Clear();
		}

		#endregion

		/// <summary>
		/// CheckedChanged event handler for the cbInfer checkbox control.
		/// </summary>
		/// <param name="sender">Source of the event.</param>
		/// <param name="e">An instance of <see cref="EventArgs"/> class with event data.</param>
		/// <remarks>This checkbox specifies whether to infer the operations or not.</remarks>
		private void cbInfer_CheckedChanged(object sender, System.EventArgs e)
		{
			if(cbInfer.Checked)
			{
				InferOperations();
			}
			else
			{
				RemoveInferedOperations();
			}
		}

		/// <summary>
		/// CheckedChanged event handler for the cbCodeGenDialog checkbox control.
		/// </summary>
		/// <param name="sender">Source of the event.</param>
		/// <param name="e">An instance of <see cref="EventArgs"/> class with event data.</param>
		/// <remarks>This checkbox specifies whether the code generation dialog must start right after the wizard or not. 
		/// If the user is in the round-tripping mode this checkbox is selected by default and it verifies if the user tries to uncheck it.</remarks>
		private void cbCodeGenDialog_CheckedChanged(object sender, System.EventArgs e)
		{
			if(!cbCodeGenDialog.Checked && this.roundtripMode)
			{
				if(MessageBox.Show(this, 
					"You need to regenerate the code to reflect the changes which have been made in the contract. Are you sure you do not want to start code generation immediately after the wizard?", 
					"WZDL Wizard", 
					MessageBoxButtons.YesNo, 
					MessageBoxIcon.Question) == DialogResult.No)
				{
					cbCodeGenDialog.Checked = true;
				}
			}
		}			
			    
        protected override void WndProc(ref System.Windows.Forms.Message m)
        {            
            const int WM_PAINT = 0xf;            

            switch (m.Msg)
            {
                case WM_PAINT:
                    importsListView.Columns[1].Width = -2;
                    operationsListView.Columns[1].Width = -2;
                    xsdpathsListView.Columns[1].Width = -2;
                    break;
            }     

            base.WndProc(ref m);
        }
        
		#region MesssagePaneLabelController

		/// <summary>
		/// Represents a custom controller for the dynamically created PropertyPane and Textbox controls.
		/// </summary>
		private class MessagePaneLabelController
		{
			#region Private fields

			private TextBox currentTextBox;
			private PropertyPane currentLabel;

			#endregion

			#region Constructors

			/// <summary>
			/// Initializes a new instance of the MessagePaneLabelController with the specified values.
			/// </summary>
			/// <param name="messageTextBox">Reference to the Textbox to control.</param>
			/// <param name="messageLabel">Reference to the PropertyPane to control.</param>
			public MessagePaneLabelController(TextBox messageTextBox,
				PropertyPane messageLabel)
			{
				currentTextBox = messageTextBox;
				currentLabel = messageLabel;

				// Attach the event handler for the current text box's lost focus event.
				currentTextBox.LostFocus += new EventHandler(TextBox_FocusOver);
			}
			
			#endregion

			#region Event handlers

			/// <summary>
			/// Performs the actions when the controlled text box changes the focus.
			/// </summary>
			/// <param name="sender">The source of the event.</param>
			/// <param name="e">An instance of <see cref="EventArgs"/> class with event data.</param>
			private void TextBox_FocusOver(object sender, EventArgs e)
			{
				currentLabel.Text = currentTextBox.Text;
				PropertyPane propPane = (PropertyPane)currentTextBox.Parent;
				PropertyTree propTree = ((PropertyTree)currentLabel.Parent);
				propTree.SelectedPaneNode = propPane.PaneNode;
			}

			#endregion
		}
			
		#endregion

		#region MessageNameTextBoxController

		/// <summary>
		/// Represents a custom controller for dynamically created control for the custom message name. 
		/// </summary>
		private class MessageNameTextBoxController
		{
			#region Private fields

			private TextBox currentTextBox;
			private CheckBox currentCheckBox;
			private PropertyPane currentMessageLabel;
			private string initialName;
			private Message message;

			#endregion

			#region Constructors

			/// <summary>
			/// Initializes a new instance of MessageNameTextBoxController class with the specified values.
			/// </summary>
			/// <param name="messageNameTextBox">Reference to the text box which contains the message name.</param>
			/// <param name="messageNameCheckBox">Reference to the check box which indicates whether the customized message name is used or not.</param>
			/// <param name="message">Reference to the current <see cref="Message"/>.</param>
			/// <param name="messageLabel">Reference to the tree view node which displays the message name.</param>
			public MessageNameTextBoxController(TextBox messageNameTextBox, 
				CheckBox messageNameCheckBox, Message message, 
				PropertyPane messageLabel)
			{
				currentTextBox = messageNameTextBox;
				currentCheckBox = messageNameCheckBox;
				currentMessageLabel = messageLabel;
				initialName = currentTextBox.Text;
				this.message = message;

				// Attach the event handlers for the controls.
				currentCheckBox.CheckedChanged += new EventHandler(CheckBox_CheckedChanged);
				currentTextBox.TextChanged += new EventHandler(TextBox_TextChanged);
			}

			#endregion
	
			#region Event handlers

			/// <summary>
			/// Performs the actions when the check box status is changed.
			/// </summary>
			/// <param name="sender">The source of the event.</param>
			/// <param name="e">An instance of <see cref="EventArgs"/> class with event data.</param>
			/// <remarks>If the check box is unchecked this method will reset the message name to the initial name.</remarks>
			private void CheckBox_CheckedChanged(object sender, EventArgs e)
			{
				if(currentTextBox.Enabled == false)
				{
					currentTextBox.Enabled = true;
				}
				else
				{
					// Reset the message name to the initial value.
					currentTextBox.Enabled = false;
					currentTextBox.Text = initialName;
					message.Name = initialName;
					currentMessageLabel.Text = initialName;
				}
			}

			/// <summary>
			/// Performs the actions when the content of the text box changes.
			/// </summary>
			/// <param name="sender">The source of the event.</param>
			/// <param name="e">An instance of <see cref="EventArgs"/> class with event data.</param>
			/// <remarks>This method updates the current message's message name when updating the text box.</remarks>
			private void TextBox_TextChanged(object sender, EventArgs e)
			{
				message.Name = currentTextBox.Text;
			}

			#endregion

		}

		#endregion

		#region InHeaderComboBoxController

		/// <summary>
		/// Represents a custom controller for dynamically created controls 
		/// to hold header messages for input message.
		/// </summary>
		private class InHeaderComboBoxController
		{
			#region Private fields

			private bool showDialog;
			private ComboBox currentComboBox;
			private CheckBox currentCheckBox;
			private TextBox currentMessageNameTextBox;
			private Operation currentOperation;
			private Message currentMessage;
			private MessageHeader currentHeader;
			private Message headerMessage;
			private SchemaElements headerElements;
			private MessagesCollection headerMessages;
			private LinkLabel currentRemoveButton;
						
			#endregion

			#region Constructors

			/// <summary>
			/// Initializes a new instance of InHeaderComboBoxController class with specified values.
			/// </summary>
			/// <param name="headerComboBox">Reference to the combo box which contains the headers.</param>
			/// <param name="headerCheckBox">Reference to the check box which indicates whether to use the message header(s) or not.</param>
			/// <param name="messageNameTextBox">Reference to the text box which contains the customized message name.</param>
			/// <param name="operation">Reference to the instance of <see cref="Operation"/> class, <see cref="MessageHeader"/> belongs to.</param>
			/// <param name="message">Reference to the instance of <see cref="Message"/> class, <see cref="MessageHeader"/> belongs to.</param>
			/// <param name="header">Reference to the current <see cref="MessageHeader"/> of the message.</param>
			/// <param name="headerElements">
			/// Reference to an instance of <see cref="SchemaElements"/> class with schema elements for headers.
			/// </param>
			/// <param name="removeButton">Reference to the link label control used to remove headers.</param>
			/// <param name="addedMessages">Reference to an instance of <see cref="MessagesCollection"/> class with <see cref="Message"/> objects added for the header.</param>
			public InHeaderComboBoxController(ComboBox headerComboBox,
				CheckBox headerCheckBox,
				TextBox messageNameTextBox,
			    Operation operation,
			    Message message,
			    MessageHeader header,
				SchemaElements headerElements,
				LinkLabel removeButton,
				MessagesCollection addedMessages)
			{
				currentComboBox = headerComboBox;
				currentCheckBox = headerCheckBox;
				currentMessageNameTextBox = messageNameTextBox;
				currentOperation = operation;
				currentMessage = message;
				currentHeader = header;
				this.headerElements = headerElements;
				this.headerMessages = new MessagesCollection();
				if(addedMessages != null)
				{
					foreach(Message impHeader in addedMessages)
					{
						this.headerMessages.Add(impHeader);
					}
				}

				this.currentRemoveButton = removeButton;
				this.showDialog = true;

				// Attach the event handlers.
				currentCheckBox.CheckedChanged += new EventHandler(CheckBox_CheckedChanged);
				currentComboBox.SelectedIndexChanged +=
					new EventHandler(ComboBox_SelectedIndexChanged);
				currentMessageNameTextBox.LostFocus += new EventHandler(TextBox_FocusOver);
				currentRemoveButton.Click += new EventHandler(this.RemoveLink_Click);
			}
			
			#endregion

			#region Event handlers

			/// <summary>
			/// Performs the actions when textbox focus changes.
			/// </summary>
			/// <param name="sender">The event source.</param>
			/// <param name="e">An instance of <see cref="EventArgs"/> class with event data.</param>
			/// <remarks>When the content of the text box changes, this procedure updates the name of the 
			/// <see cref="MessageHeader"/> accordingly.</remarks>
			private void TextBox_FocusOver(object sender, EventArgs e)
			{
				if (headerMessage != null) headerMessage.Name = currentMessageNameTextBox.Text + "Header";
				if (currentHeader != null)
				{
					currentHeader.Message = currentMessageNameTextBox.Text + "Header";
				}
			}

			/// <summary>
			/// Performs the actions when the status of the checkbox control changes.
			/// </summary>
			/// <param name="sender">The event source.</param>
			/// <param name="e">An instance of <see cref="EventArgs"/> class with event data.</param>
			/// <remarks>If the check box is checked, this method will enable the combo box which displays the 
			/// headers. Then it calls the <see cref="AddNewHeader"/> method to add a new header.
			/// If the check box is unchecked this method will perform the following tasks.
			/// 1. Removes the header messages from the <see cref="Operation"/>.
			/// 2. Clears the HeadersCollection of the current <see cref="Message"/>.
			/// 3. Removes the items from the combo box which displays the headers.
			/// 4. Disables the combo box.
			/// </remarks>
			private void CheckBox_CheckedChanged(object sender, EventArgs e)
			{	
				// Code for multiple headers support.
				if(currentCheckBox.Checked)
				{
					currentComboBox.Enabled = true;
					this.AddNewHeader();
				}
				else
				{
					// Remove the Header messages from the operation.
					foreach(Message msg in this.headerMessages)
					{
						this.currentOperation.MessagesCollection.Remove(msg);
					}

					// Clear the headers for the current message.
					this.currentMessage.HeadersCollection.Clear();

					// Clear the headerMessages collection.
					this.headerMessages.Clear();

					// Remove the items on the combo box.
					while(this.currentComboBox.Items.Count != 1)
					{
						this.currentComboBox.Items.RemoveAt(0);
					}
					
					// Finally disable the combo box.
					showDialog = false;
					currentComboBox.SelectedIndex = currentComboBox.Items.Count - 1;
					currentComboBox.Enabled = false;
					currentRemoveButton.Visible = false;
				}
			}

			/// <summary>
			/// Performs the actions when the selected index of the header messages combo box changes.
			/// </summary>
			/// <param name="sender">The event source.</param>
			/// <param name="e">An instance of <see cref="EventArgs"/> class with event data.</param>
			/// <remarks>If the selected item is the last item of the list, the method will call the 
			/// <see cref="AddNewHeader"/> method. Otherwise it shows the remove button to remove the 
			/// selected header.</remarks>
			private void ComboBox_SelectedIndexChanged(object sender, EventArgs e)
			{
				if(currentComboBox.SelectedIndex == currentComboBox.Items.Count - 1)
				{
					currentRemoveButton.Visible = false;
					if(showDialog) AddNewHeader();
					showDialog = true;
				}
				else
				{
					currentRemoveButton.Visible = true;
				}
			}

			/// <summary>
			/// Performs the actions when the remove link label is clicked.
			/// </summary>
			/// <param name="sender">The event source.</param>
			/// <param name="e">An instance of <see cref="EventArgs"/> class with event data.</param>
			/// <remarks>This method removes the <see cref="MessageHeader"/> from the <see cref="Message"/> 
			/// and <see cref="Message"/> from the <see cref="Operation"/>. </remarks>
			private void RemoveLink_Click(object sender, EventArgs e)
			{
				// Select the message from the added header messages collection.
				Message msg = headerMessages[currentComboBox.SelectedIndex];
				// Remove the message from the Operation.
				currentOperation.MessagesCollection.Remove(msg);
				// Remove the MessageHeader from the currentMessage.
				currentMessage.HeadersCollection.RemoveAt(currentComboBox.SelectedIndex);
				// Remove the message from the header messages collection.
				headerMessages.RemoveAt(currentComboBox.SelectedIndex );
				// Remove the header from the combo box.
				currentComboBox.Items.RemoveAt(currentComboBox.SelectedIndex);

				// Change the current index of the combo box.				
				SelectTheLatestHeader();
			}
			
			#endregion

			#region Private methods

			/// <summary>
			/// Prompts a dialog box to select the new header. Once it is selected this method adds it to 
			/// <see cref="Message.HeadersCollection"/>. Also this method creates a new <see cref="Message"/>
			/// for the header and puts it to the <see cref="Operation.MessagesCollection"/>.
			/// </summary>
			private void AddNewHeader()
			{
				SchemaElement newHeaderMessage = new SchemaElement();
				
				// Display the dialog box.
				SelectHeaderDialog dialog = new SelectHeaderDialog(this.headerElements, ref newHeaderMessage);
				dialog.ShowDialog();

				if(newHeaderMessage.ElementName != null)
				{
					// First check whether the header already exists. And remove it if it does.
					foreach(Message msg in this.headerMessages)
					{
						if(newHeaderMessage.ElementName == msg.Element.ElementName &&
							newHeaderMessage.ElementNamespace == msg.Element.ElementNamespace)
						{				
							// Select the last item before returning.
							SelectTheLatestHeader();
							return; // Exit without adding.
						}
					}

					string headerName = "";
					int index = 1;
					while(headerName == "")
					{
						headerName = currentMessage.Name + "Header" + index.ToString();
						// Check whether we already have a header message with the same name.
						foreach(MessageHeader  hdr in currentMessage.HeadersCollection)
						{
							if(hdr.Name == headerName)
							{
								headerName = "";
								index++;
								break;
							}
						}
					}

					// Create the header message.
					headerMessage = new Message();
					headerMessage.Name = headerName;
					headerMessage.Documentation = "";
					headerMessage.Element.ElementName = newHeaderMessage.ElementName;
					headerMessage.Element.ElementNamespace = newHeaderMessage.ElementNamespace;
					
					// Create the header.
					MessageHeader newHeader = new MessageHeader();
					newHeader.Message = headerMessage.Name;
					newHeader.Name = headerMessage.Name;

					// Add header to the current message.
					currentMessage.HeadersCollection.Add(newHeader);

					// Add the new header message to the messages collection.
					currentOperation.MessagesCollection.Add(headerMessage);
					
					// Added the newly created header message to the header messages collection.
					// Reason: In case we need to remove the header messages we need to distinguish
					// them from the existing messages.
					this.headerMessages.Add(headerMessage);

					// Add the newly added header info to the headers combo box.
					currentComboBox.Items.Insert
						(currentComboBox.Items.Count - 1, headerMessage.Element.ElementName);

					// Set the last selected header message as the default header.
					currentComboBox.SelectedIndex = currentComboBox.Items.Count - 2;
				}
			}

			/// <summary>
			/// Sets the last item on the headers combo box as the default/selected item.
			/// </summary>
			private void SelectTheLatestHeader()
			{
				if(currentComboBox.Items.Count >= 2)
				{
					currentComboBox.SelectedIndex = currentComboBox.Items.Count - 2;
				}
				else
				{
					showDialog = false;
					currentComboBox.SelectedIndex = currentComboBox.Items.Count - 1;
				}
			}

			#endregion
		}
		
		#endregion
		
		#region OutHeaderComboBoxController

		/// <summary>
		/// Represents a custom controller for dynamically created controls 
		/// to hold header messages for output message.
		/// </summary>
		private class OutHeaderComboBoxController
		{
			#region Private fields

			private bool showDialog;
			private ComboBox currentComboBox;
			private CheckBox currentCheckBox;
			private TextBox currentMessageNameTextBox;
			private Operation currentOperation;
			private Message currentMessage;
			private MessageHeader currentHeader;
			private Message headerMessage;
			private SchemaElements headerElements;
			private MessagesCollection headerMessages;
			private LinkLabel currentRemoveButton;			
			
			#endregion

			#region Constructors

			/// <summary>
			/// Initializes a new instance of OutHeaderComboBoxController class with specified values.
			/// </summary>
			/// <param name="headerComboBox">Reference to the combo box which contains the headers.</param>
			/// <param name="headerCheckBox">Reference to the check box which indicates whether to use the message header(s) or not.</param>
			/// <param name="messageNameTextBox">Reference to the text box which contains the customized message name.</param>
			/// <param name="operation">Reference to the instance of <see cref="Operation"/> class, <see cref="MessageHeader"/> belongs to.</param>
			/// <param name="message">Reference to the instance of <see cref="Message"/> class, <see cref="MessageHeader"/> belongs to.</param>
			/// <param name="header">Reference to the current <see cref="MessageHeader"/> of the message.</param>
			/// <param name="headerElements">Reference to an instance of <see cref="SchemaElements"/> class with schema elements for headers.</param>
			/// <param name="removeButton">Reference to the link label control used to remove headers.</param>
			/// <param name="addedMessages">Reference to an instance of <see cref="MessagesCollection"/> class with <see cref="Message"/> objects added for the header.</param>
			public OutHeaderComboBoxController(ComboBox headerComboBox,
				CheckBox headerCheckBox,
				TextBox messageNameTextBox,
			    Operation operation,
			    Message message,
			    MessageHeader header,
				SchemaElements headerElements,
				LinkLabel removeButton,
				MessagesCollection addedMessages)
			{
				currentComboBox = headerComboBox;
				currentCheckBox = headerCheckBox;
				currentMessageNameTextBox = messageNameTextBox;
				currentOperation = operation;
				currentMessage = message;
				currentHeader = header;
				this.headerElements = headerElements;
				this.headerMessages = new MessagesCollection();
				if(addedMessages != null)
				{
					foreach(Message impHeader in addedMessages)
					{
						this.headerMessages.Add(impHeader);
					}
				}
				
				this.currentRemoveButton = removeButton;
				this.showDialog = true;

				// Attach the event handlers.
				currentCheckBox.CheckedChanged += new EventHandler(CheckBox_CheckedChanged);
				currentComboBox.SelectedIndexChanged +=
					new EventHandler(ComboBox_SelectedIndexChanged);
				currentMessageNameTextBox.LostFocus += new EventHandler(TextBox_FocusOver);
				currentRemoveButton.Click += new EventHandler(this.RemoveLink_Click);
			}
			
			#endregion

			#region Event handlers

			/// <summary>
			/// Performs the actions when text box focus changes.
			/// </summary>
			/// <param name="sender">The event source.</param>
			/// <param name="e">An instance of <see cref="EventArgs"/> class with event data.</param>
			/// <remarks>When the content of the text box changes, this procedure adjusts the name of the 
			/// header message accordingly.</remarks>
			private void TextBox_FocusOver(object sender, EventArgs e)
			{
				if (headerMessage != null) headerMessage.Name = currentMessageNameTextBox.Text + "Header";
				if (currentHeader != null)
				{
					currentHeader.Message = currentMessageNameTextBox.Text + "Header";
				}
			}

			/// <summary>
			/// Performs the actions when the status of the check box control changes.
			/// </summary>
			/// <param name="sender">The event source.</param>
			/// <param name="e">An instance of <see cref="EventArgs"/> class with event data.</param>
			/// <remarks>If the check box is checked, this method will enable the combo box which displays the 
			/// headers. Then it calls the <see cref="AddNewHeader"/> method to add a new header.
			/// If the check box is unchecked this method will perform the following tasks.
			/// 1. Removes the header messages from the <see cref="Operation"/>.
			/// 2. Clears the HeadersCollection of the current <see cref="Message"/>.
			/// 3. Removes the items from the combo box which displays the headers.
			/// 4. Disables the combo box.
			/// </remarks>
			private void CheckBox_CheckedChanged(object sender, EventArgs e)
			{
				if(currentCheckBox.Checked)
				{
					currentComboBox.Enabled = true;
					this.AddNewHeader();
				}
				else
				{
					// Remove the Header messages from the operation
					foreach(Message msg in this.headerMessages)
					{
						this.currentOperation.MessagesCollection.Remove(msg);
					}

					// Clear the headers for the current message.
					this.currentMessage.HeadersCollection.Clear();
					
					// Clear the header messages collection.
					this.headerMessages.Clear();

					// Remove the items on the combo box.
					while(this.currentComboBox.Items.Count != 1)
					{
						this.currentComboBox.Items.RemoveAt(0);
					}

					// Finally disable the combo box.
					showDialog = false;
					currentComboBox.SelectedIndex = currentComboBox.Items.Count - 1;
					currentComboBox.Enabled = false;
					currentRemoveButton.Visible = false;
				}
			}

			/// <summary>
			/// Performs the actions when the selected index of the header messages combo box changes.
			/// </summary>
			/// <param name="sender">The event source.</param>
			/// <param name="e">An instance of <see cref="EventArgs"/> class with event data.</param>
			/// <remarks>If the selected item is the last item of the list, the method will call the 
			/// <see cref="AddNewHeader"/> method. Otherwise it shows the remove button to remove the 
			/// selected header.</remarks>
			private void ComboBox_SelectedIndexChanged(object sender, EventArgs e)
			{
				if(currentComboBox.SelectedIndex == currentComboBox.Items.Count - 1)
				{
					currentRemoveButton.Visible = false;
					if(showDialog) AddNewHeader();
					showDialog = true;
				}
				else
				{
					currentRemoveButton.Visible = true;
				}
			}

			/// <summary>
			/// Performs the actions when the remove link label is clicked.
			/// </summary>
			/// <param name="sender">The event source.</param>
			/// <param name="e">An instance of <see cref="EventArgs"/> class with event data.</param>
			/// <remarks>This method removes the <see cref="MessageHeader"/> from the <see cref="Message"/> 
			/// and <see cref="Message"/> from the <see cref="Operation"/>. </remarks>
			private void RemoveLink_Click(object sender, EventArgs e)
			{
				// Remove the header message.
				Message msg = headerMessages[currentComboBox.SelectedIndex];
				currentOperation.MessagesCollection.Remove(msg);
				currentMessage.HeadersCollection.RemoveAt(currentComboBox.SelectedIndex);
				headerMessages.RemoveAt(currentComboBox.SelectedIndex);			
			
				currentComboBox.Items.RemoveAt(currentComboBox.SelectedIndex);
				// Change the current index of the combo box.
				SelectTheLatestHeader();
			}
			
			#endregion

			#region Private methods

			/// <summary>
			/// Prompts a dialog box to select the new header. Once it is selected this method adds it to 
			/// <see cref="Message.HeadersCollection"/>. Also this method creates a new <see cref="Message"/>
			/// for the header and puts it to the <see cref="Operation.MessagesCollection"/>.
			/// </summary>
			private void AddNewHeader()
			{
				SchemaElement newHeaderMessage = new SchemaElement();
				SelectHeaderDialog dialog = new SelectHeaderDialog(this.headerElements, ref newHeaderMessage);
				dialog.ShowDialog();

				if(newHeaderMessage.ElementName != null)
				{
					// First check whether the header already exists. And remove it if it does.
					foreach(Message msg in this.headerMessages)
					{
						if(newHeaderMessage.ElementName == msg.Element.ElementName &&
							newHeaderMessage.ElementNamespace == msg.Element.ElementNamespace)
						{	
							// Set the last item before returning.
							SelectTheLatestHeader();

							return; // Exit without adding.
						}
					}

					string headerName = "";
					int index = 1;
					while(headerName == "")
					{
						headerName = currentMessage.Name + "Header" + index.ToString();
						foreach(MessageHeader  hdr in currentMessage.HeadersCollection)
						{
							if(hdr.Name == headerName)
							{
								headerName = "";
								index++;
								break;
							}
						}
					}
		
					// Create the header message.
					headerMessage = new Message();
					headerMessage.Name = headerName; // currentMessage.Name + "Header" + headerNo;
					headerMessage.Documentation = "";
					headerMessage.Element.ElementName = newHeaderMessage.ElementName;
					headerMessage.Element.ElementNamespace = newHeaderMessage.ElementNamespace;
					
					// Create the header.
					MessageHeader newHeader = new MessageHeader();
					newHeader.Message = headerMessage.Name;
					newHeader.Name = headerMessage.Name;

					// Add header to the current message.
					currentMessage.HeadersCollection.Add(newHeader);

					// Add the new header message to the messages collection.
					currentOperation.MessagesCollection.Add(headerMessage);
					
					// Added the newly created header message to the header messages collection.
					// Reason: In case we need to remove the header messages we need to distinguish
					// them from the existing messages.
					this.headerMessages.Add(headerMessage);

					// Add the newly added header info to the headers combo box.
					currentComboBox.Items.Insert(currentComboBox.Items.Count - 1, 
						headerMessage.Element.ElementName);

					// Set the last selected header message as the default header.
					currentComboBox.SelectedIndex = currentComboBox.Items.Count - 2;
				}
			}

			/// <summary>
			/// Sets the last item on the headers combo box as the default/selected item.
			/// </summary>
			private void SelectTheLatestHeader()
			{
				if(currentComboBox.Items.Count >= 2)
				{
					currentComboBox.SelectedIndex = currentComboBox.Items.Count - 2;
				}
				else
				{
					showDialog = false;
					currentComboBox.SelectedIndex = currentComboBox.Items.Count - 1;
				}
			}

			#endregion

		}
		
		#endregion

		#region MessageDocumentationTextBoxController

		/// <summary>
		/// Represents a custom controller to handle the dynamically created text box which contains the <see cref="Message"/> documentation.
		/// </summary>
		private class MessageDocumentationTextBoxController
		{
			#region Private fields

			private TextBox currentTextBox;
			private Message message;
			
			#endregion

			#region Constructors

			/// <summary>
			/// Initializes a new instance of MessageDocumentationTextBoxController class with specified values.
			/// </summary>
			/// <param name="documentationTextBox">Reference to the text box control which contains the documentation.</param>
			/// <param name="message">Reference to an instance of <see cref="Message"/> class which belongs the documentation.</param>
			public MessageDocumentationTextBoxController(TextBox documentationTextBox, Message message)
			{
				this.currentTextBox = documentationTextBox;
				this.message = message;

				// Attach the event handlers.
				currentTextBox.TextChanged += new EventHandler(TextBox_TextChanged);
			}
			
			#endregion

			#region Event handlers

			/// <summary>
			/// Performs the actions when the text box content changes.
			/// </summary>
			/// <param name="sender">The event source.</param>
			/// <param name="e">An instance of <see cref="EventArgs"/> class with event data.</param>
			/// <remarks>This method updates the <see cref="Message"/> object's documentation property.</remarks>
			private void TextBox_TextChanged(object sender, EventArgs e)
			{
				this.message.Documentation = currentTextBox.Text;
			}
			
			#endregion
		}
		
		#endregion

		#region OperationDocumentationTextBoxController

		/// <summary>
		///  Represents a custom controller to handle the dynamically created text box which contains the <see cref="Operation"/> documentation.
		/// </summary>
		private class OperationDocumentationTextBoxController
		{
			#region Private fields

			private TextBox currentTextBox;
			private Operation operation;
			
			#endregion

			#region Constructors

			/// <summary>
			/// Initializes a new instance of OperationDocumentationTextBoxController class with specified values.
			/// </summary>
			/// <param name="documentationTextBox">Reference to the text box control which contains the documentation.</param>
			/// <param name="operation">Reference to an instance of <see cref="Operation"/> class which belongs the documentation.</param>
			public OperationDocumentationTextBoxController(TextBox documentationTextBox, Operation operation)
			{
				this.currentTextBox = documentationTextBox;
				this.operation = operation;

				// Attach the event handlers.
				currentTextBox.TextChanged += new EventHandler(TextBox_TextChanged);
			}
			
			#endregion

			#region Event handlers

			/// <summary>
			/// Performs the actions when the text box content changes.
			/// </summary>
			/// <param name="sender">The event source.</param>
			/// <param name="e">An instance of <see cref="EventArgs"/> class with event data.</param>
			/// <remarks>This method updates the <see cref="Operation"/> object's documentation property.</remarks>
			private void TextBox_TextChanged(object sender, EventArgs e)
			{
				this.operation.Documentation = currentTextBox.Text;
			}

			#endregion

		}

		#endregion
	}
}
