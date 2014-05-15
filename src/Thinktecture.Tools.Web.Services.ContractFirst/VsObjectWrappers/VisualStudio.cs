using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnvDTE;
using System.Diagnostics;
using EnvDTE80;

namespace Thinktecture.Tools.Web.Services.ContractFirst
{
    internal class VisualStudio
    {
        #region Private Fields

        private DTE2 applicationObject;

        #endregion

        #region Constructors

        public VisualStudio(DTE2 applicationObject)
        {
            this.applicationObject = applicationObject;
        }

        #endregion

        #region Public Methods

        public void AddTask(string description,
            string category,
            string subcategory,
            vsTaskPriority priority,
            vsTaskIcon icon,
            bool checkable,
            string releventFile,
            int lineNumber,
            bool canUserDelete,
            bool flushItem,
            bool allowDuplicates)
        {
            Debug.Assert(description != null, "description parameter could not be null.");
            Debug.Assert(category != null, "category parameter could not be null.");
            Debug.Assert(subcategory != null, "subcategory parameter could not be null.");

            // Get a reference to the tasks window.
            Window win = applicationObject.Windows.Item(Constants.vsWindowKindTaskList);
            // Activate it.
            win.Activate();
            // Get a reference to the tasks list.
            TaskList taskList = (TaskList)win.Object;

            // If we don't allow duplicates we have to itterate the tasks list to 
            // see if there is a duplicate.

            // Do this for all task items.
            for (int ti = 1; ti <= taskList.TaskItems.Count; ti++)
            {
                // Get a reference to the task item.
                TaskItem existingItem = taskList.TaskItems.Item(ti);
                // Match attributes and see if this task is as same as the one being added.
                // If the task is matching all Category, SubCategory and Description properties
                // must be similar to the pertaining parameters. We don't care about the case here.
                if (string.Compare(category, existingItem.Category, true) == 0 &&
                    string.Compare(subcategory, existingItem.SubCategory, true) == 0 &&
                    string.Compare(description, existingItem.Description, true) == 0)
                {
                    // We don't have to proceed.
                    return;
                }
            }

            // Add the new task item.
            TaskItem newItem = taskList.TaskItems.Add(category, subcategory, description, priority, icon,
                checkable, releventFile, lineNumber, canUserDelete, flushItem);

            // Finally activate the task bar.
            ActivateTaskBar();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the currently selected project.
        /// </summary>
        public VisualStudioProject SelectedProject
        {
            get
            {
                // First try to read the project from the selected item (assuming a project is 
                // currently selected). 
                Project selectedProject = this.applicationObject.SelectedItems.Item(1).Project;

                // Do we have a project?
                if (selectedProject != null)
                {
                    return new VisualStudioProject(selectedProject);
                }

                // If not, try to read the project containing the currently selected item.
                ProjectItem projectItem = applicationObject.SelectedItems.Item(1).ProjectItem;
                if (projectItem != null)
                {
                    selectedProject = projectItem.ContainingProject;
                    return new VisualStudioProject(selectedProject);
                }

                // No, we could not find the project. So returning null.
                return null;
            }
        }

        public bool IsItemSelected
        {
            get
            {
                return (this.applicationObject.SelectedItems.Count > 0 &&
                    this.applicationObject.SelectedItems.SelectionContainer != null &&
                    this.applicationObject.SelectedItems.SelectionContainer.Count > 0);
            }
        }

        public VisualStudioSelectedItem SelectedItem
        {
            get
            {
                if (this.applicationObject.SelectedItems.Count == 0)
                {
                    return null;
                }
                return new VisualStudioSelectedItem(this.applicationObject.SelectedItems.Item(1));
            }
        }

    	public IEnumerable<VisualStudioSelectedItem> SelectedItems
    	{
    		get
    		{
				foreach (SelectedItem item in applicationObject.SelectedItems)
    			{
    				yield return new VisualStudioSelectedItem(item);
    			}
    		}
    	}

        #endregion

        #region Private Methods

        /// <summary>
        /// Helper method to activate the task bar.
        /// Usually invoked when a new item is added to the task bar.
        /// </summary>
        private void ActivateTaskBar()
        {
            object cin = "Add-ins and Macros";
            object cout = new object();

            this.applicationObject.Commands.Raise("{1496A755-94DE-11D0-8C3F-00C04FC2AAE2}", 2200,
                    ref cin, ref cout);
        }

        #endregion
    }
}
