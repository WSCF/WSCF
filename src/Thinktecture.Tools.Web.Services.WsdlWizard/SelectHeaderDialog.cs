using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using Thinktecture.Tools.Web.Services.ServiceDescription;

namespace Thinktecture.Tools.Web.Services.WsdlWizard
{
	#region SelectHeaderDialog class

	/// <summary>
	/// Represents the user interface handler for SelectHeaderDialog form.
	/// </summary>
	public class SelectHeaderDialog : System.Windows.Forms.Form
	{
		#region Private fields

		/// <summary>
		/// This field is used to indicate whether the header selection is cancelled or not.
		/// </summary>
		private bool closingByForce;

		private SchemaElements headerSchemas;
		private SchemaElement selectedHeader;
		private System.Windows.Forms.ComboBox cbHeaderMessage;
		private System.Windows.Forms.Button btnOK;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Label label1;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		
		#endregion
		
		#region Constructors
		
		/// <summary>
		/// Initializes a new instance of SelectHeaderDialog class.
		/// </summary>
		/// <param name="headerSchemas">Reference to an instance of <see cref="SchemaElements"/> class.</param>
		/// <param name="selectedHeader">Reference to an instance of <see cref="SchemaElement"/> class.</param>
		/// <remarks>The headerSchemas parameter contains the header schemas available for the wizard.
		/// This collection is used to fill the cbHeaderMessage combo box later. selectedHeader parameter is
		/// used to return the selected header value from the dialog box to the caller.
		/// </remarks>
		public SelectHeaderDialog(SchemaElements headerSchemas, 
			ref SchemaElement selectedHeader)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			
			this.headerSchemas = headerSchemas;
			this.selectedHeader = selectedHeader;
			
			closingByForce = true;
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
            this.cbHeaderMessage = new System.Windows.Forms.ComboBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // cbHeaderMessage
            // 
            this.cbHeaderMessage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbHeaderMessage.Location = new System.Drawing.Point(65, 13);
            this.cbHeaderMessage.Name = "cbHeaderMessage";
            this.cbHeaderMessage.Size = new System.Drawing.Size(192, 21);
            this.cbHeaderMessage.TabIndex = 0;
            this.cbHeaderMessage.SelectedIndexChanged += new System.EventHandler(this.cbHeaderMessage_SelectedIndexChanged);
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(93, 48);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "O&K";
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(181, 48);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "&Cancel";
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(8, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(48, 23);
            this.label1.TabIndex = 3;
            this.label1.Text = "Header:";
            // 
            // SelectHeaderDialog
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(266, 80);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.cbHeaderMessage);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SelectHeaderDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Select New Message Header";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.SelectHeaderDialog_Closing);
            this.Load += new System.EventHandler(this.SelectHeaderDialog_Load);
            this.ResumeLayout(false);

		}
		#endregion
		
		#region Event handlers
		
		/// <summary>
		/// Performs the actions before loading the dialog box.
		/// </summary>
		/// <param name="sender">The event source.</param>
		/// <param name="e">An instance of <see cref="EventArgs"/> class with event data.</param>
		/// <remarks>This method fills the cbHeaderMessage combo box with available header schemas.
		/// Also this method sets the first item on the cbHeaderMessage combo box as the selected 
		/// header.</remarks>
		private void SelectHeaderDialog_Load(object sender, System.EventArgs e)
		{
			cbHeaderMessage.DataSource = this.headerSchemas;
			cbHeaderMessage.DisplayMember = "ElementName";

			// Initialize the selected header item to the first item in the combo box.
			this.selectedHeader.ElementName = ((SchemaElement)cbHeaderMessage.Items[0]).ElementName;
			this.selectedHeader.ElementNamespace = 
				((SchemaElement)cbHeaderMessage.Items[0]).ElementNamespace;
		}
		
		/// <summary>
		/// Performs the actions when the selected index changes on the cbHeaderMessage combo box.
		/// </summary>
		/// <param name="sender">The event source.</param>
		/// <param name="e">An instance of <see cref="EventArgs"/> class with event data.</param>
		/// <remarks>This method updates the selected header to the newly selected item on the cbHeaderMessage combo box.</remarks>
		private void cbHeaderMessage_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			this.selectedHeader.ElementName = ((SchemaElement)cbHeaderMessage.SelectedItem).ElementName;
			this.selectedHeader.ElementNamespace = 
				((SchemaElement)cbHeaderMessage.SelectedItem).ElementNamespace;
		}

		/// <summary>
		/// Performs the actions when the OK button is clicked.
		/// </summary>
		/// <param name="sender">The event source.</param>
		/// <param name="e">An instance of <see cref="EventArgs"/> class with event data.</param>
		/// <remarks>This method indicates that the header selection operation is 
		/// OK and closes the form.</remarks>
		private void btnOK_Click(object sender, System.EventArgs e)
		{
			this.closingByForce = false;
			this.Close();
		}

		/// <summary>
		/// Performs the actions when the Cancel button is clicked.
		/// </summary>
		/// <param name="sender">The event source.</param>
		/// <param name="e">An instance of <see cref="EventArgs"/> class with event data.</param>
		/// <remarks>This method indicates that the header selection operation is 
		/// canceled and closes the form.</remarks>
		private void btnCancel_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		/// <summary>
		/// Performs the actions when the dialog box is closing.
		/// </summary>
		/// <param name="sender">The event source.</param>
		/// <param name="e">An instance of <see cref="CancelEventArgs"/> class with event data.</param>
		/// <remarks>This method checks whether the header selection is cancelled or not. If it's cancelled then
		/// this method will set the returned <see cref="SchemaElement"/>'s ElementName and ElementNamespace to
		/// null.</remarks>
		private void SelectHeaderDialog_Closing(object sender, CancelEventArgs e)
		{
			if(this.closingByForce)
			{
				this.selectedHeader.ElementName = null;
				this.selectedHeader.ElementNamespace = null;
			}
		}

		#endregion
	}

	#endregion
}
