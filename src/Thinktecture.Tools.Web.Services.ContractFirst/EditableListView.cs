using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections;

namespace Thinktecture.Tools.Web.Services.ContractFirst
{
	#region EditableListView class

	/// <summary>
	/// Represents the definition of EditableListView class.
	/// </summary>
	/// <remarks>This class inherits <see cref="ListView"/> class.</remarks>
	public class EditableListView : ListView 
	{
		
		#region Private fields

		private bool doubleClicked;
		private int x;
		private int y;
		private int subItemSelected;
		private string subItemText ;
		private ListViewItem li;
		private ListViewItem liPrevious;
		private TextBox  editBox = new TextBox();
		private ComboBox cmbBox = new ComboBox();
		private ArrayList disabledColumns;
		
		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of EditableListView class.
		/// </summary>
		public EditableListView()
		{
			cmbBox.Items.Add("One-Way");
			cmbBox.Items.Add("Request/Response");
			cmbBox.Size  = new Size(0,0);
			cmbBox.Location = new Point(0,0);
			this.Controls.AddRange(new Control[] {this.cmbBox});			
			cmbBox.SelectedIndexChanged += new EventHandler(this.CmbSelected);
			cmbBox.LostFocus += new EventHandler(this.CmbFocusOver);
			cmbBox.KeyPress += new KeyPressEventHandler(this.CmbKeyPress);
			cmbBox.Font = new Font("Microsoft Sans Serif", 8.5F, FontStyle.Regular, GraphicsUnit.Point, ((Byte)(0)));
			cmbBox.BackColor = Color.White; 
			cmbBox.DropDownStyle = ComboBoxStyle.DropDownList;
			cmbBox.Enabled = true;
			cmbBox.Hide();

			this.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, ((Byte)(0)));
			this.FullRowSelect = true;
			this.Name = "listView1";
			this.Size = new Size(0,0);
			this.TabIndex = 0;
			this.View = View.Details;
			this.MouseDown += new MouseEventHandler(this.MouseDownHandler);
			this.DoubleClick += new EventHandler(this.DoubleClickHandler);
			this.GridLines = true ;

			editBox.Size  = new Size(0,0);
			editBox.Location = new Point(0,0);
			this.Controls.AddRange(new Control[] {this.editBox});			
			editBox.KeyPress += new KeyPressEventHandler(this.EditOver);
			editBox.LostFocus += new EventHandler(this.FocusOver);
			editBox.Font = new Font("Microsoft Sans Serif", 8.5F, FontStyle.Regular, GraphicsUnit.Point, ((Byte)(0)));
			editBox.BorderStyle = BorderStyle.Fixed3D;
			editBox.Hide();
			editBox.Text = "";

			this.disabledColumns = new ArrayList();
		}
		
		#endregion

		#region Properties
		
		/// <summary>
		/// Gets or sets the an instance of ArrayList class containing the the list of 
		/// column names to be disabled.
		/// </summary>
		public ArrayList DisabledColumns
		{
			get { return this.disabledColumns; }
			set { this.disabledColumns = value; }
		}
		
		#endregion

		#region Event handlers

		/// <summary>
		/// Performs the actions after the key is pressed.
		/// </summary>
		/// <param name="sender">The event source.</param>
		/// <param name="e">An instance of <see cref="KeyPressEventArgs"/> class with event data.</param>
		private void CmbKeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == 13 || e.KeyChar == 27)
			{
				cmbBox.Hide();
			}
			doubleClicked = false;
		}

		/// <summary>
		/// Performs the actions after the selected index is changed on the combo box.
		/// </summary>
		/// <param name="sender">The event source.</param>
		/// <param name="e">An instance of <see cref="EventArgs"/> class with event data.</param>
		private void CmbSelected(object sender, EventArgs e)
		{
			int sel = cmbBox.SelectedIndex;
			if (sel >= 0)
			{
				string itemSel = cmbBox.Items[sel].ToString();
				li.SubItems[subItemSelected].Text = itemSel;
			}
			doubleClicked = true;
		}

		/// <summary>
		/// Performs the actions when the focus changes on the combo box.
		/// </summary>
		/// <param name="sender">The event source.</param>
		/// <param name="e">An instance of <see cref="EventArgs"/> class with event data.</param>
		private void CmbFocusOver(object sender, EventArgs e)
		{
			if(li == null) li =liPrevious;
			li.SubItems[subItemSelected].Text = cmbBox.Text;
			cmbBox.Hide();
			doubleClicked = false;
		}
	
		/// <summary>
		/// Performs the actions when a key is pressed on the text box.
		/// </summary>
		/// <param name="sender">The event source.</param>
		/// <param name="e">An instance of <see cref="KeyPressEventArgs"/> with event data.</param>
		private void EditOver(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == 13) 
			{
				li.SubItems[subItemSelected].Text = editBox.Text;
				editBox.Hide();
			}

			if (e.KeyChar == 27) 
				editBox.Hide();

			doubleClicked = true;
		}

		/// <summary>
		/// Performs the actions when the text box changes the focus.
		/// </summary>
		/// <param name="sender">The event source.</param>
		/// <param name="e">An instance of <see cref="EventArgs"/> class with event data.</param>
		private void FocusOver(object sender, EventArgs e)
		{
			li = liPrevious;
			li.SubItems[subItemSelected].Text = editBox.Text;
			editBox.Hide();
			doubleClicked = false;
		}

		/// <summary>
		/// Performs the actions when double clicked on the control.
		/// </summary>
		/// <param name="sender">The event source.</param>
		/// <param name="e">An instance of <see cref="EventArgs"/> class with event data.</param>
		private void DoubleClickHandler(object sender, EventArgs e)
		{
			// Check the subitem clicked
			int nStart = x ;
			int spos = 0 ; 
			int epos = this.Columns[0].Width ;

			for(int i=0; i < this.Columns.Count ; i++)
			{
				if (nStart > spos && nStart < epos) 
				{
					subItemSelected = i ;
					break; 
				}
				
				spos = epos ; 
				epos += this.Columns[i].Width;
			}

			subItemText = li.SubItems[subItemSelected].Text;

			string colName = this.Columns[subItemSelected].Text;

            /*
			// Display the edit control only if the clicked column is not in the 
			// 'Disabled columns' list.
			if(colName == CodeConstants.MEP && !this.disabledColumns.Contains(colName)) 
			{
				cmbBox.Size  = new Size(epos - spos , li.Bounds.Bottom-li.Bounds.Top - 24);
				cmbBox.Location = new Point(spos + 2, li.Bounds.Y - 1);
				cmbBox.Show() ;
				cmbBox.Text = subItemText;
				cmbBox.SelectAll() ;
				cmbBox.Focus();
			}

			if(colName != CodeConstants.MEP && !this.disabledColumns.Contains(colName))
			{
				editBox.Size  = new Size(epos - spos, li.Bounds.Bottom-li.Bounds.Top -22);
				editBox.Location = new Point(spos + 1, li.Bounds.Y - 1);
				editBox.Show() ;
				editBox.Text = subItemText;
				editBox.SelectAll() ;
				editBox.Focus();
			}
            */
			doubleClicked = true;
		}

		/// <summary>
		/// Performs the actions when the mouse button is pressed down on the control.
		/// </summary>
		/// <param name="sender">The event source.</param>
		/// <param name="e">An instance of <see cref="MouseEventArgs"/> class with event data.</param>
		private void MouseDownHandler(object sender, MouseEventArgs e)
		{
			if(doubleClicked)
				li = liPrevious;
			else
				li = this.GetItemAt(e.X , e.Y);

			if(!(li == null || li == liPrevious)) liPrevious = li;
			
			x = e.X ;
			y = e.Y ;
		}

		#endregion
	}

	#endregion
}
