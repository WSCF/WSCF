using System;
using System.Collections.Generic;
using System.Text;
using EnvDTE;
using System.IO;

namespace Thinktecture.Tools.Web.Services.ContractFirst
{
    internal class VisualStudioSelectedItem
    {
        #region Private Members

        private SelectedItem selectedItem;
        private string[] fileNames;

        #endregion

        public VisualStudioSelectedItem(SelectedItem selectedItem)
        {
            this.selectedItem = selectedItem;
            Initialize();
        }

        public string[] AllFileNames
        {
            get { return this.fileNames; }
        }

        /// <summary>
        /// Gets the currently selected items file name. If the item is 
        /// composed using several files the first file name in the file 
        /// name array is returned.
        /// </summary>
        public string FileName
        {
            get
            {
                if (this.fileNames.Length > 0)
                {
                    return this.fileNames[0];
                }
                return null;
            }
        }

        /// <summary>
        /// Gets the directory containing the currently selected item.
        /// </summary>
        public string Directory
        {
            get
            {
                return Path.GetDirectoryName(this.FileName);
            }
        }

        /// <summary>
        /// Gets a boolean value indicating whether the currently selected item 
        /// belongs to a project or not.
        /// </summary>
        public bool HasProject
        {
            get
            {
                // Return true if the project is selected.
                if (IsProject)
                {
                    return true;
                }
                else
                {
                    // Check the selected project item's containing project.
                    return (this.selectedItem.ProjectItem.ContainingProject != null);
                }
            }
        }

        /// <summary>
        /// Gets a boolean value indicating whether the selected item is a project 
        /// or not.
        /// </summary>
        public bool IsProject
        {
            get
            {
                return (this.selectedItem.Project != null);
            }
        }

        public VisualStudioProject ParentProject
        {
            get
            {
                if (IsProject)
                {
                    return new VisualStudioProject(this.selectedItem.Project);
                }
                else
                {
                    if (HasProject)
                    {
                        return new VisualStudioProject(this.selectedItem.ProjectItem.ContainingProject);
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }

        private void Initialize()
        {
            ExtractFileNames();
        }

        private void ExtractFileNames()
        {
            if (IsProject)
            {
                this.fileNames = new string[1] { this.selectedItem.Project.FullName };
            }
            else
            {
				if (selectedItem.ProjectItem == null)
				{
					fileNames = new string[0];
					return;
				}
                int filesCount = this.selectedItem.ProjectItem.FileCount;
                this.fileNames = new string[filesCount];
                for (short i = 1; i <= (short)this.selectedItem.ProjectItem.FileCount; i++)
                {
                    this.fileNames[i - 1] = this.selectedItem.ProjectItem.get_FileNames(i);
                }
            }            
        }
    }
}
