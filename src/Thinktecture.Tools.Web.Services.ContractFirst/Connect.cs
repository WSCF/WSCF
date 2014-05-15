using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Collections;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

using EnvDTE;
using Extensibility;

using Microsoft.VisualStudio.CommandBars;

using EnvDTE80;

using Thinktecture.Tools.Web.Services.CodeGeneration;
using Thinktecture.Tools.Web.Services.WsdlWizard;

using DteConstants = EnvDTE.Constants;

namespace Thinktecture.Tools.Web.Services.ContractFirst
{
    #region Read me for Add-in installation and setup information.
    // When run, the Add-in wizard prepared the registry for the Add-in.
    // At a later time, if the Add-in becomes unavailable for reasons such as:
    //   1) You moved this project to a computer other than which is was originally created on.
    //   2) You chose 'Yes' when presented with a message asking if you wish to remove the Add-in.
    //   3) Registry corruption.
    // you will need to re-register the Add-in by building the WsContractFirstSetup project 
    // by right clicking the project in the Solution Explorer, then choosing install.
    #endregion

    /// <summary>
    ///   The object for implementing an Add-in.
    /// </summary>
    /// <seealso class='IDTExtensibility2' />
    //[ComVisible(true)]
    //[StructLayout(LayoutKind.Sequential)]
    //
    // NOTE: Enable comments starting with *** to enable the simple logging mechanism we use.
    //
    [Guid("6C6DA7B1-0758-41f4-9669-04ED4B037669"), ProgId("Thinktecture.Tools.Web.Services.ContractFirst.Connect")]
    public class Connect : Object, IDTExtensibility2, IDTCommandTarget
    {
        // BDS: Save the last file name and the file counter in a hash table.
        private Hashtable fileCounters = null;
        private bool addDuplictedTypesAlert = false;
        private bool overwriteFiles = false;
        private DTE2 applicationObject;
        private AddIn addInInstance;
        private VisualStudio visualStudio;
    	private CommandBarPopup subMenuPopup;
    	private CommandBarPopup webSubMenuPopup;
    	private CommandBarPopup projectSubMenuPopup;
		private CommandBarPopup webProjectSubMenuPopup;
    	private CommandBarPopup toolsSubMenuPopup;
    	private SelectionEvents selectionEvents;
    	private OutputWindowWriter outputWindowWriter;

        /// <summary>
        ///		Implements the constructor for the Add-in object.
        ///		Place your initialization code within this method.
        /// </summary>
        public Connect()
        {
            if (fileCounters == null)
            {
                fileCounters = new Hashtable();
            }
            AppLog.LogMessage("A new instance of WSCF Connect class is created.");
        }

        /// <summary>
        ///      Implements the OnConnection method of the IDTExtensibility2 interface.
        ///      Receives notification that the Add-in is being loaded.
        /// </summary>
        /// <param term='application'>
        ///      Root object of the host application.
        /// </param>
        /// <param term='connectMode'>
        ///      Describes how the Add-in is being loaded.
        /// </param>
        /// <param term='addInInst'>
        ///      Object representing this Add-in.
        /// </param>
        /// <seealso class='IDTExtensibility2' />
        public void OnConnection(object application, ext_ConnectMode connectMode, object addInInst, ref Array custom)
        {
            AppLog.LogMessage("Entering OnConnection method.");

            applicationObject = (DTE2)application;
            // Initialize VsHelper.
            visualStudio = new VisualStudio(applicationObject);

        	outputWindowWriter = new OutputWindowWriter(applicationObject);

        	addInInstance = (AddIn)addInInst;
            object[] contextGUIDS = new object[] { };
            CommandBar cmdBar;
            Command cmdObj;

            if (connectMode == ext_ConnectMode.ext_cm_Startup ||
                connectMode == ext_ConnectMode.ext_cm_AfterStartup ||
                connectMode == ext_ConnectMode.ext_cm_UISetup)
            {
                AppLog.LogMessage("Creating commands.");

                Commands2 commands = (Commands2)applicationObject.Commands;
                CommandBars commandBars = (CommandBars)applicationObject.CommandBars;

            	CommandBar subMenuCommandBar = null;
            	CommandBar webSubMenuCommandBar = null;
            	CommandBar projectSubMenuCommandBar = null;
				CommandBar webProjectSubMenuCommandBar = null;
            	CommandBar toolsSubMenuCommandBar = null;

            	try
            	{
            		CommandBar itemCommandBar = FindCommandBarByName(commandBars, "Item");
            		CommandBar webItemCommandBar = FindCommandBarByName(commandBars, "Web Item");
            		CommandBar projectCommandBar = FindCommandBarByName(commandBars, "Project");
            		CommandBar webProjectCommandBar = FindCommandBarByName(commandBars, "Web Project Folder");
            		CommandBar toolsCommandBar = GetToolsCommandBar(commandBars);

					if (itemCommandBar != null)
					{
						subMenuPopup = (CommandBarPopup)itemCommandBar.Controls.Add(MsoControlType.msoControlPopup, Type.Missing, Type.Missing, 1, true);
						subMenuPopup.Caption = "WSCF.blue";
						subMenuCommandBar = subMenuPopup.CommandBar;
					}
					else
					{
						AppLog.LogMessage("The 'Item' CommandBar could not be found.");
					}

					if (webItemCommandBar != null)
					{
						webSubMenuPopup = (CommandBarPopup)webItemCommandBar.Controls.Add(MsoControlType.msoControlPopup, Type.Missing, Type.Missing, 1, true);
						webSubMenuPopup.Caption = "WSCF.blue";
						webSubMenuCommandBar = webSubMenuPopup.CommandBar;
					}
					else
					{
						AppLog.LogMessage("The 'Web Item' CommandBar could not be found.");
					}

            		if (projectCommandBar != null)
            		{
            			projectSubMenuPopup = (CommandBarPopup)projectCommandBar.Controls.Add(MsoControlType.msoControlPopup, Type.Missing, Type.Missing, 1, true);
            			projectSubMenuPopup.Caption = "WSCF.blue";
            			projectSubMenuCommandBar = projectSubMenuPopup.CommandBar;
            		}
					else
					{
						AppLog.LogMessage("The 'Project' CommandBar could not be found.");
					}

            		if (webProjectCommandBar != null)
            		{
            			webProjectSubMenuPopup = (CommandBarPopup)webProjectCommandBar.Controls.Add(MsoControlType.msoControlPopup, Type.Missing, Type.Missing, 1, true);
            			webProjectSubMenuPopup.Caption = "WSCF.blue";
            			webProjectSubMenuCommandBar = webProjectSubMenuPopup.CommandBar;
            		}
					else
					{
						AppLog.LogMessage("The 'Web Project Folder' CommandBar could not be found.");
					}

            		if (toolsCommandBar != null)
            		{
            			toolsSubMenuPopup = (CommandBarPopup)toolsCommandBar.Controls.Add(MsoControlType.msoControlPopup, Type.Missing, Type.Missing, 1, true);
            			toolsSubMenuPopup.Caption = "WSCF.blue";
            			toolsSubMenuCommandBar = toolsSubMenuPopup.CommandBar;
            		}
					else
					{
						AppLog.LogMessage("The 'Tools' CommandBar could not be found.");
					}

                    selectionEvents = applicationObject.Events.SelectionEvents;
					selectionEvents.OnChange += OnSelectionChanged;
            	}
            	catch (Exception ex)
            	{
            		AppLog.LogMessage(ex.ToString());
            	}

				// Create the 'Web Services Contract-First...' Tools entry
				try
				{
					if (toolsSubMenuCommandBar != null)
					{
						Command command = commands.AddNamedCommand2(
							addInInstance,
							"WsContractFirst",
							"Web Services Contract-First...",
							"Executes the command for WsContractFirstAddin",
							true,
							190,
							ref contextGUIDS,
							(int)vsCommandStatus.vsCommandStatusUnsupported + (int)vsCommandStatus.vsCommandStatusEnabled,
							(int)vsCommandStyle.vsCommandStylePictAndText,
							vsCommandControlType.vsCommandControlTypeButton);

						command.AddControl(toolsSubMenuCommandBar, 1);

						AppLog.LogMessage("Command bar is added to the Tools menu.");
					}
				}
				catch (ArgumentException)
				{
				}
				catch (Exception e)
				{
					AppLog.LogMessage(e.Message);
				}

            	// Create the 'Generate Web Service Code...' context-menu entry
                try
                {
                	if (subMenuCommandBar != null || webSubMenuCommandBar != null)
                	{
                		// Create the add-in command
                		cmdObj = commands.AddNamedCommand2(
                			addInInstance,
                			"WsContractFirstContextMenu",
                			"Generate Web Service Code...",
                			"Executes the command for WsContractFirstAddin ContextMenu",
                			true,
                			190,
                			ref contextGUIDS,
                			(int)vsCommandStatus.vsCommandStatusUnsupported + (int)vsCommandStatus.vsCommandStatusEnabled,
                			(int)vsCommandStyle.vsCommandStylePictAndText,
                			vsCommandControlType.vsCommandControlTypeButton);

                		if (subMenuCommandBar != null)
                		{
                			cmdObj.AddControl(subMenuCommandBar, 1);
                		}

                		if (webSubMenuCommandBar != null)
                		{
                			// BDS 11/21/2005: Add this menu item to the web project 
                			// template.
                			cmdObj.AddControl(webSubMenuCommandBar, 1);
                		}

						AppLog.LogMessage("Generate Web Serive Code menu item is added.");
                	}
                }
                catch (ArgumentException e)
                {
                    AppLog.LogMessage(e.Message);
                }
                catch (Exception ex)
                {
                    AppLog.LogMessage(ex.Message);
                }

                // Create the 'Edit WSDL Interface Description...' context-menu entry
                try
                {
					if (subMenuCommandBar != null || webSubMenuCommandBar != null)
                	{
                		// Create the add-in command
                		cmdObj = commands.AddNamedCommand2(
                			addInInstance,
                			"EditWsdlContextMenu",
                			"Edit WSDL Interface Description...",
                			"Executes the command for WsContractFirstAddin ContextMenu",
                			true,
                			190,
                			ref contextGUIDS,
                			(int)vsCommandStatus.vsCommandStatusUnsupported + (int)vsCommandStatus.vsCommandStatusEnabled,
                			(int)vsCommandStyle.vsCommandStylePictAndText,
                			vsCommandControlType.vsCommandControlTypeButton);

                		if (subMenuCommandBar != null)
                		{
                			cmdObj.AddControl(subMenuCommandBar, 2);
                		}

                		if (webSubMenuCommandBar != null)
                		{
                			// BDS 11/21/2005: Add this menu item to the web project 
                			// template.
                			cmdObj.AddControl(webSubMenuCommandBar, 2);
                		}

                		AppLog.LogMessage("Edit WSDL menu item is added");
                	}
                }
                catch (ArgumentException e)
                {
                    AppLog.LogMessage(e.Message);
                }
                catch (Exception ex)
                {
                    AppLog.LogMessage(ex.Message);
                }

                // Create the 'Create WSDL Interface Description...' context-menu entry
                try
                {
					if (subMenuCommandBar != null || webSubMenuCommandBar != null)
                	{
                		// Create the add-in command
                		cmdObj = commands.AddNamedCommand2(
                			addInInstance,
                			"CreateWsdlContextMenu",
                			"Create WSDL Interface Description...",
                			"Executes the command for WsContractFirstAddin ContextMenu",
                			true,
                			190,
                			ref contextGUIDS,
                			(int)vsCommandStatus.vsCommandStatusUnsupported + (int)vsCommandStatus.vsCommandStatusEnabled,
                			(int)vsCommandStyle.vsCommandStylePictAndText,
                			vsCommandControlType.vsCommandControlTypeButton);

                		if (subMenuCommandBar != null)
                		{
                			cmdObj.AddControl(subMenuCommandBar, 1);
                		}

                		if (webSubMenuCommandBar != null)
                		{
                			// BDS 11/21/2005: Add this menu item to the web project 
                			// template.
                			cmdObj.AddControl(webSubMenuCommandBar, 1);
                		}

                		AppLog.LogMessage("Create WSDL interface desc menu item is added");
                	}
                }
                catch (ArgumentException e)
                {
                    AppLog.LogMessage(e.Message);
                }
                catch (Exception ex)
                {
                    AppLog.LogMessage(ex.Message);
                }


                // Create the 'Choose WSDL to implement...' context-menu entry
                try
                {
                	if (projectSubMenuCommandBar != null || webProjectSubMenuCommandBar != null)
                	{
                		// Create the add-in command
                		cmdObj = commands.AddNamedCommand2(
                			addInInstance,
                			"WsContractFirstContextMenu2",
                			"Choose WSDL to Implement...",
                			"Executes the command for WsContractFirstAddin ContextMenu",
                			true,
                			190,
                			ref contextGUIDS,
                			(int)vsCommandStatus.vsCommandStatusUnsupported + (int)vsCommandStatus.vsCommandStatusEnabled,
                			(int)vsCommandStyle.vsCommandStylePictAndText,
                			vsCommandControlType.vsCommandControlTypeButton);

                		if (projectSubMenuCommandBar != null)
                		{
                			cmdObj.AddControl(projectSubMenuCommandBar, 1);
                		}

                		if (webProjectSubMenuCommandBar != null)
                		{
                			// BDS 11/21/2005: Add this menu item to the web project 
                			// template. 
                			cmdObj.AddControl(webProjectSubMenuCommandBar, 1);
                		}

                		AppLog.LogMessage("Choose WSDL menu item is added");
                	}
                }
                catch (ArgumentException e)
                {
                    AppLog.LogMessage(e.Message);
                }
                catch (Exception ex)
                {
                    AppLog.LogMessage(ex.Message);
                }

				// Create the 'Generate code...' context-menu entry
				try
				{
					if (subMenuCommandBar != null || webSubMenuCommandBar != null)
					{
						// Create the add-in command
						cmdObj = commands.AddNamedCommand2(
							addInInstance,
							"GenerateCodeMenu",
							"Generate Data Contract Code...",
							"Executes the command for WsContractFirstAddin ContextMenu",
							true,
							190,
							ref contextGUIDS,
							(int)vsCommandStatus.vsCommandStatusUnsupported + (int)vsCommandStatus.vsCommandStatusEnabled,
							(int)vsCommandStyle.vsCommandStylePictAndText,
							vsCommandControlType.vsCommandControlTypeButton);

						if (subMenuCommandBar != null)
						{
							cmdObj.AddControl(subMenuCommandBar, 2);
						}

						if (webSubMenuCommandBar != null)
						{
							// BDS 11/21/2005: Add this menu item to the web project 
							// template. 
							cmdObj.AddControl(webSubMenuCommandBar, 2);
						}

						AppLog.LogMessage("Generate code menu item is added");
					}
				}
				catch (ArgumentException e)
				{
					AppLog.LogMessage(e.Message);
				}

				// Create the 'Paste XML as Schema' Edit menu entry.
				try
				{
					CommandBar menuBarCommandBar = FindCommandBarByName(commandBars, "MenuBar");

					if (menuBarCommandBar != null)
					{
						// Create the add-in command
						cmdObj = commands.AddNamedCommand2(
							addInInstance,
							"PasteSchemaMenu",
							"Paste XML as Schema",
							"Pastes the XML on the clipboard as XSD schema.",
							true,
							239,
							ref contextGUIDS,
							(int)vsCommandStatus.vsCommandStatusSupported + (int)vsCommandStatus.vsCommandStatusEnabled,
							(int)vsCommandStyle.vsCommandStylePictAndText,
							vsCommandControlType.vsCommandControlTypeButton);



						CommandBarControl editControl = menuBarCommandBar.Controls["Edit"];
						CommandBarPopup editPopup = (CommandBarPopup)editControl;
						CommandBarControl pasteControl = editPopup.CommandBar.Controls["Paste"];

						cmdObj.AddControl(editPopup.CommandBar, pasteControl != null ? pasteControl.Index + 1 : 1);

						AppLog.LogMessage("Paste Schema code menu item is added");
					}
					else
					{
						AppLog.LogMessage("The 'MenuBar' CommandBar could not be found.");
					}

				}
				catch (Exception ex)
				{
					AppLog.LogMessage(ex.Message);
				}
            }

            AppLog.LogMessage("Leaving OnConnection method.");
        }

        /// <summary>
        ///     Implements the OnDisconnection method of the IDTExtensibility2 interface.
        ///     Receives notification that the Add-in is being unloaded.
        /// </summary>
        /// <param term='disconnectMode'>
        ///      Describes how the Add-in is being unloaded.
        /// </param>
        /// <param term='custom'>
        ///      Array of parameters that are host application specific.
        /// </param>
        /// <seealso class='IDTExtensibility2' />
        public void OnDisconnection(ext_DisconnectMode disconnectMode, ref Array custom)
        {
            AppLog.LogMessage("Entering OnDisconnection method.");

            try
            {
                if (disconnectMode == ext_DisconnectMode.ext_dm_HostShutdown |
                    disconnectMode == ext_DisconnectMode.ext_dm_UserClosed)
                {
					if (selectionEvents != null)
					{
						selectionEvents.OnChange -= OnSelectionChanged;						
					}

                	RemoveCommandBarPopup(subMenuPopup);
					RemoveCommandBarPopup(webSubMenuPopup);
					RemoveCommandBarPopup(projectSubMenuPopup);
					RemoveCommandBarPopup(webProjectSubMenuPopup);
                	RemoveCommandBarPopup(toolsSubMenuPopup);

                    RemoveCommand("WsContractFirst");
                    RemoveCommand("WsContractFirstContextMenu");
                    RemoveCommand("EditWsdlContextMenu");
                    RemoveCommand("CreateWsdlContextMenu");
                    RemoveCommand("WsContractFirstContextMenu2");
                    RemoveCommand("GenerateCodeMenu");
					RemoveCommand("PasteSchemaMenu");
                }

            }
            catch (Exception ex)
            {
                AppLog.LogMessage(ex.Message);
                throw;
            }

            AppLog.LogMessage("Leaving OnDisconnection method.");
        }

    	/// <summary>
        ///      Implements the OnAddInsUpdate method of the IDTExtensibility2 interface.
        ///      Receives notification that the collection of Add-ins has changed.
        /// </summary>
        /// <param term='custom'>
        ///      Array of parameters that are host application specific.
        /// </param>
        /// <seealso class='IDTExtensibility2' />
        public void OnAddInsUpdate(ref Array custom)
        {
        }

        /// <summary>
        ///      Implements the OnStartupComplete method of the IDTExtensibility2 interface.
        ///      Receives notification that the host application has completed loading.
        /// </summary>
        /// <param term='custom'>
        ///      Array of parameters that are host application specific.
        /// </param>
        /// <seealso class='IDTExtensibility2' />
        public void OnStartupComplete(ref Array custom)
        {
        }

        /// <summary>
        ///      Implements the OnBeginShutdown method of the IDTExtensibility2 interface.
        ///      Receives notification that the host application is being unloaded.
        /// </summary>
        /// <param term='custom'>
        ///      Array of parameters that are host application specific.
        /// </param>
        /// <seealso class='IDTExtensibility2' />
        public void OnBeginShutdown(ref Array custom)
        {
        }

        /// <summary>
        ///      Implements the QueryStatus method of the IDTCommandTarget interface.
        ///      This is called when the command's availability is updated
        /// </summary>
        /// <param term='commandName'>
        ///		The name of the command to determine state for.
        /// </param>
        /// <param term='neededText'>
        ///		Text that is needed for the command.
        /// </param>
        /// <param term='status'>
        ///		The state of the command in the user interface.
        /// </param>
        /// <param term='commandText'>
        ///		Text requested by the neededText parameter.
        /// </param>
        /// <seealso class='Exec' />
        public void QueryStatus(string commandName, vsCommandStatusTextWanted neededText, ref vsCommandStatus status, ref object commandText)
        {
            AppLog.LogMessage("Entering QueryStatus method.");

            try
            {
				status = vsCommandStatus.vsCommandStatusInvisible;

				if (commandName == GetCommandName("EditWsdlContextMenu"))
                {
                    UIHierarchy uiHierarchy = (UIHierarchy)applicationObject.Windows.Item(
                        DteConstants.vsWindowKindSolutionExplorer).Object;
                    foreach (UIHierarchyItem item in (Array)uiHierarchy.SelectedItems)
                    {
                        string itemName = item.Name;

                        if (itemName.IndexOf(".wsdl") > -1)
                        {
                            status = vsCommandStatus.vsCommandStatusEnabled | vsCommandStatus.vsCommandStatusSupported;
                            AppLog.LogMessage("Edit WSDL command is enabled.");
                            break;
                        }
                    }
                }

				if (commandName == GetCommandName("WsContractFirstContextMenu"))
                {
                    UIHierarchy uiHierarchy = (UIHierarchy)applicationObject.Windows.Item(
                        DteConstants.vsWindowKindSolutionExplorer).Object;
                    foreach (UIHierarchyItem item in (Array)uiHierarchy.SelectedItems)
                    {
                        string itemName = item.Name;

                        if (itemName.IndexOf(".wsdl") > -1)
                        {
                            status = vsCommandStatus.vsCommandStatusEnabled | vsCommandStatus.vsCommandStatusSupported;
                            AppLog.LogMessage("WSCF context menu is enabled.");
                            break;
                        }
                    }
                }

				if (commandName == GetCommandName("CreateWsdlContextMenu"))
                {
                    UIHierarchy uiHierarchy = (UIHierarchy)applicationObject.Windows.Item(
                        DteConstants.vsWindowKindSolutionExplorer).Object;
                    foreach (UIHierarchyItem item in (Array)uiHierarchy.SelectedItems)
                    {
                        string itemName = item.Name;

                        if (itemName.IndexOf(".xsd") > -1)
                        {
                            status = vsCommandStatus.vsCommandStatusEnabled | vsCommandStatus.vsCommandStatusSupported;
                            AppLog.LogMessage("Create WSDL command is enabled");
                            break;
                        }
                    }
                }

				if (commandName == GetCommandName("GenerateCodeMenu"))
				{
					UIHierarchy uiHierarchy = (UIHierarchy)applicationObject.Windows.Item(
						DteConstants.vsWindowKindSolutionExplorer).Object;
					foreach (UIHierarchyItem item in (Array)uiHierarchy.SelectedItems)
					{
						string itemName = item.Name;

						if (itemName.IndexOf(".xsd") > -1 || itemName.IndexOf(".wsdl") > -1)
						{
							status = vsCommandStatus.vsCommandStatusEnabled | vsCommandStatus.vsCommandStatusSupported;
							AppLog.LogMessage("Generate code command is enabled");
							break;
						}
					}
				}

                if (neededText == vsCommandStatusTextWanted.vsCommandStatusTextWantedNone)
                {
					if (commandName == GetCommandName("WsContractFirstContextMenu2"))
                    {
                        status = vsCommandStatus.vsCommandStatusEnabled | vsCommandStatus.vsCommandStatusSupported;
                        AppLog.LogMessage("WSCF menu 2 command is enabled");
                    }
					if (commandName == GetCommandName("WsContractFirst"))
                    {
                        status = vsCommandStatus.vsCommandStatusSupported | vsCommandStatus.vsCommandStatusEnabled;
                        AppLog.LogMessage("Contract first command is enabled");
                    }
                }

				if (neededText == vsCommandStatusTextWanted.vsCommandStatusTextWantedNone)
				{
					if (commandName == GetCommandName("PasteSchemaMenu"))
					{
						status = vsCommandStatus.vsCommandStatusSupported;
						if (visualStudio.SelectedProject != null)
						{
							status = vsCommandStatus.vsCommandStatusSupported | vsCommandStatus.vsCommandStatusEnabled;
							AppLog.LogMessage("Paste Schema command is enabled");							
						}
					}
				}
            }
            catch (Exception ex)
            {
                AppLog.LogMessage(ex.Message);
                throw;
            }

            AppLog.LogMessage("Leaving QueryStatus method.");
        }

        /// <summary>
        ///      Implements the Exec method of the IDTCommandTarget interface.
        ///      This is called when the command is invoked.
        /// </summary>
        /// <param term='commandName'>
        ///		The name of the command to execute.
        /// </param>
        /// <param term='executeOption'>
        ///		Describes how the command should be run.
        /// </param>
        /// <param term='varIn'>
        ///		Parameters passed from the caller to the command handler.
        /// </param>
        /// <param term='varOut'>
        ///		Parameters passed from the command handler to the caller.
        /// </param>
        /// <param term='handled'>
        ///		Informs the caller if the command was handled or not.
        /// </param>
        /// <seealso class='Exec' />
        public void Exec(string commandName, vsCommandExecOption executeOption, ref object varIn, ref object varOut, ref bool handled)
        {
            AppLog.LogMessage("Entering Exec method.");

            handled = false;

            if (commandName == "Thinktecture.Tools.Web.Services.ContractFirst.Connect.WsContractFirstContextMenu2" ||
                commandName == "Thinktecture.Tools.Web.Services.ContractFirst.Connect.WsContractFirst" ||
                commandName == "Thinktecture.Tools.Web.Services.ContractFirst.Connect.WsContractFirstContextMenu")
            {
                handled = ProcessCodeGenerationRequest();
                return;
            }
        	if (commandName == "Thinktecture.Tools.Web.Services.ContractFirst.Connect.CreateWsdlContextMenu")
        	{
        		handled = ProcessWsdlWizardRequest(false);
        		return;
        	}
        	if (commandName == "Thinktecture.Tools.Web.Services.ContractFirst.Connect.EditWsdlContextMenu")
        	{
        		handled = ProcessWsdlWizardRequest(true);
        		return;
        	}
        	if (commandName == "Thinktecture.Tools.Web.Services.ContractFirst.Connect.GenerateCodeMenu")
        	{
        		handled = ProcessXsdCodeGenerationRequest();
        		return;
        	}
			if (commandName == "Thinktecture.Tools.Web.Services.ContractFirst.Connect.PasteSchemaMenu")
			{
				handled = ProcessPasteSchemaRequest();
				return;
			}

            AppLog.LogMessage("Leaving Exec method");
        }

        private bool ProcessWsdlWizardRequest(bool roundtrip)
        {
            VisualStudioSelectedItem selectedItem = this.visualStudio.SelectedItem;
            if (selectedItem == null)
            {
                MessageBox.Show("No selected item.",
                        "Web Services Contract-First WSDL Wizard", MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
            }

            if (!selectedItem.HasProject)
            {
                MessageBox.Show("Cannot create a WSDL contract for items outside of a project.",
                    "Web Services Contract-First WSDL Wizard", MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);

                return true;
            }

            string currentDir = selectedItem.Directory;
            string projectRootDir = selectedItem.ParentProject.ProjectDirectory;
            string metdataFile = selectedItem.FileName;

            WsdlWizardForm wizard = null;

            try
            {
                if (roundtrip)
                    wizard = new WsdlWizardForm(metdataFile, true);
                else
                    wizard = new WsdlWizardForm(metdataFile);
                wizard.WsdlLocation = currentDir;
                wizard.DefaultPathForImports = "";
                wizard.ProjectRootDirectory = projectRootDir;
                wizard.ShowDialog();

                string wsdlFile = "";
                if (wizard.DialogResult == DialogResult.OK)
                {
                    if (wizard.WsdlLocation.Length > 0)
                    {
                        wsdlFile = wizard.WsdlLocation;

                        // BDS: Fixed a bug here. We did not check the round-tripping mode before we 
                        // create the file. This caused the wizard to create duplicate .wsdl files 
                        // in the project root folder.
                        if (!roundtrip)
                        {
                            AddFileToCurrentProject(wsdlFile);
                        }

                        if (wizard.OpenCodeGenDialog)
                        {
                            ProcessCodeGenerationRequest(wsdlFile);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,
                    "WSDL Wizard", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                wizard.Close();
                return false;
            }

            return true;
        }

        /// <summary>
        /// Handles the code generation workflow.
        /// </summary>
        private bool ProcessCodeGenerationRequestEx(string wsdlFromWizard)
        {
            return false;
        } // End of ProcessCodeGenerationRequestFunction.

        // This method is not current being used.
        private bool ProcessXsdCodeGenerationRequest()
        {
			if (!CanGenerateCode())
			{
				return false;
			}

			try
			{
				VisualStudioProject project = visualStudio.SelectedProject;
				IEnumerable<VisualStudioSelectedItem> selectedItems = visualStudio.SelectedItems;

				if (selectedItems.Count() == 0)
				{
					MessageBox.Show(
						"Cannot generate code for items outside of a project.",
						"Web Services Contract-First code generation",
						MessageBoxButtons.OK,
						MessageBoxIcon.Exclamation);

					return true;
				}

				foreach (VisualStudioSelectedItem selectedItem in selectedItems)
				{
					string extension = Path.GetExtension(selectedItem.FileName).ToLower();
					if (extension == ".xsd" || extension == ".wsdl") continue;

					MessageBox.Show(
						"Data Contracts can only be generated for .xsd or .wsdl files.",
						"Web Services Contract-First code generation",
						MessageBoxButtons.OK,
						MessageBoxIcon.Exclamation);

					return true;
				}

				string[] dataContractFiles = selectedItems.Select(i => i.FileName).ToArray();
				XsdCodeGenDialog dialogForm = new XsdCodeGenDialog(dataContractFiles);
				if (!project.IsWebProject)
				{
					dialogForm.Namespace = project.AssemblyNamespace;                					
				}
				dialogForm.TargetFileName = project.GetDefaultDestinationFilename(dataContractFiles[0]);

                if (dialogForm.ShowDialog() == DialogResult.Cancel)
				{
					return false;
				}

				CodeGenerationOptions options = new CodeGenerationOptions();
				options.GenerateDataContracts = true;
				options.DataContractFiles = dataContractFiles;
				options.GenerateProperties = dialogForm.PublicProperties;
			    options.VirtualProperties = dialogForm.VirtualProperties;
				options.GenerateCollections = dialogForm.Collections;
				options.GenerateSeparateFiles = dialogForm.GenerateMultipleFiles;
				options.OverwriteExistingFiles = dialogForm.OverwriteFiles;
				options.AdjustCasing = dialogForm.AdjustCasing;
				options.EnableDataBinding = dialogForm.DataBinding;
				options.GenerateOrderIdentifiers = dialogForm.OrderIdentifiers;
				options.GenerateTypedLists = dialogForm.GenericLists;
				options.ClrNamespace = dialogForm.Namespace;
				options.OutputFileName = dialogForm.TargetFileName;
				options.OutputLocation = GetOutputDirectory();
				options.ProjectDirectory = project.ProjectDirectory;
				options.Language = project.ProjectLanguage;
				options.ProjectName = project.ProjectName;

				CodeGenerator codeGenerator = new CodeGenerator();
				CodeWriterOutput output = codeGenerator.GenerateCode(options);

				AddGeneratedFilesToProject(output);

				// Finally add the project references.
				AddAssemblyReferences();

				MessageBox.Show("Code generation successfully completed.", "WSCF.Blue", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			catch (Exception ex)
			{
				AppLog.LogMessage(ex.ToString());
				MessageBox.Show(ex.ToString(), "CodeGeneration", MessageBoxButtons.OK, MessageBoxIcon.Error);
				// TODO: Log the exception.
				//System.Diagnostics.Debugger.Break();
			}
			return true;
        } // End of ProcessXsdCodeGenerationRequestFunction.

        /// <summary>
        /// 
        /// </summary>
        /// <param name="wsdlFile"></param>
        /// <returns></returns>
        public bool ProcessCodeGenerationRequest(string wsdlFile)
        {
            if (!File.Exists(wsdlFile))
            {
                return false;
            }
            if (!CanGenerateCode())
            {
                return false;
            }
            return ProcessCodeGenerationRequestCore(wsdlFile);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool ProcessCodeGenerationRequest()
        {
            if (!CanGenerateCode())
            {
                return false;
            }

            VisualStudioSelectedItem selectedItem = this.visualStudio.SelectedItem;

            if (selectedItem.IsProject)
            {
                return ProcessCodeGenerationRequestCore(string.Empty);
            }
            else
            {
                return ProcessCodeGenerationRequestCore(selectedItem.FileName);
            }            
        }

        private bool ProcessCodeGenerationRequestCore(string wsdlFile)
        {
            try
            {
                VisualStudioProject project = this.visualStudio.SelectedProject;
                VisualStudioSelectedItem selectedItem = this.visualStudio.SelectedItem;

                // Fist display the UI and get the options.
                WebServiceCodeGenDialogNew dialog = new WebServiceCodeGenDialogNew();                
				if (!project.IsWebProject)
				{
					dialog.DestinationNamespace = project.AssemblyNamespace;                					
				}
                dialog.DestinationFilename = project.GetDefaultDestinationFilename(wsdlFile);
                
                if (!selectedItem.IsProject)
                {
                    //dialog.WsdlLocation = selectedItem.FileName;
                    dialog.WsdlLocation = wsdlFile;
                }
                if (dialog.ShowDialog() == DialogResult.Cancel)
                {
                    return false;
                }

                wsdlFile = dialog.WsdlPath;
                // Try the Rpc2DocumentLiteral translation first.
                // wsdlFile = TryTranslateRpc2DocumentLiteral(wsdlFile);

                CodeGenerationOptions options = new CodeGenerationOptions();
                options.MetadataLocation = wsdlFile;
                options.ClrNamespace = dialog.DestinationNamespace;
                options.OutputFileName = dialog.DestinationFilename;
                options.OutputLocation = GetOutputDirectory();
            	options.ProjectDirectory = project.ProjectDirectory;
				options.Language = project.ProjectLanguage;
            	options.ProjectName = project.ProjectName;
                // TODO: Infer the config file type according to the project type
                // and merge the generated config file with the existing one.
                options.ConfigurationFile = "output.config";
                options.GenerateService = dialog.ServiceCode;
                options.GenerateProperties = dialog.GenerateProperties;
                options.VirtualProperties = dialog.VirtualProperties;
            	options.FormatSoapActions = dialog.FormatSoapActions;
                options.GenerateCollections = dialog.Collections;
                options.GenerateTypedLists = dialog.GenericList;
                options.EnableDataBinding = dialog.EnableDataBinding;
                options.GenerateOrderIdentifiers = dialog.OrderIdentifiers;
                options.GenerateAsyncCode = dialog.AsyncMethods;
                options.GenerateSeparateFiles = dialog.GenerateMultipleFiles;
                options.AdjustCasing = dialog.ChangeCasing;
                options.OverwriteExistingFiles = dialog.Overwrite;
                options.EnableWsdlEndpoint = dialog.EnabledWsdlEndpoint;
            	options.GenerateSvcFile = dialog.GenerateSvcFile;
            	options.ConcurrencyMode = dialog.ConcurrencyMode;
            	options.InstanceContextMode = dialog.InstanceContextMode;
            	options.UseSynchronizationContext = dialog.UseSynchronizationContext;
            	options.MethodImplementation = dialog.MethodImplementation;

				outputWindowWriter.Clear();

                CodeGenerator codeGenerator = new CodeGenerator();
                CodeWriterOutput output = codeGenerator.GenerateCode(options);

                AddGeneratedFilesToProject(output);

                // Finally add the project references.
                AddAssemblyReferences();

                // add custom assembly references if necessary
                if(options.EnableWsdlEndpoint)
                {
                    AddMetadataExtensionsReference();
                }

                MessageBox.Show("Code generation successfully completed.", "WSCF.Blue", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
			catch (ClientServiceGenerationException ex)
			{
				AppLog.LogMessage(ex.ToString());

				const string separator = "---------------------------------------------------------------------------------";
				foreach (string message in ex.Messages)
				{
					outputWindowWriter.WriteMessage(message + "\r\n" + separator + "\r\n");
				}

				MessageBox.Show("Errors were found while importing the contract. Please check the 'WSCF.blue' pane in the Output window for more information.", 
					"CodeGeneration", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
            catch (Exception ex)
            {
                AppLog.LogMessage(ex.ToString());
                MessageBox.Show(ex.ToString(), "CodeGeneration", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // TODO: Log the exception.
                //System.Diagnostics.Debugger.Break();
            }
            return true;
        }

    	private bool ProcessPasteSchemaRequest()
    	{
    		try
    		{
				if (visualStudio.SelectedProject == null)
				{
					MessageBox.Show(
						"You cannot paste schema items outside of a project.",
						"Web Services Contract-First Paste Schema",
						MessageBoxButtons.OK,
						MessageBoxIcon.Exclamation);

					return true;
				}

				IDataObject dataObject = Clipboard.GetDataObject();
				if (dataObject == null)
				{
					MessageBox.Show(
						"There was no data found in the clipboard.",
						"Web Services Contract-First Paste Schema",
						MessageBoxButtons.OK,
						MessageBoxIcon.Exclamation);

					return true;					
				}

				string xml = (string)dataObject.GetData(typeof(string));
				if (xml == null)
				{
					MessageBox.Show(
						"There was no string data found in the clipboard.",
						"Web Services Contract-First Paste Schema",
						MessageBoxButtons.OK,
						MessageBoxIcon.Exclamation);

					return true;			
				}

    			XElement schemaXml;
    			try
    			{
					schemaXml = XElement.Parse(xml);
    			}
    			catch (Exception)
    			{
					MessageBox.Show(
						"The data found in the clipboard is not valid XML.",
						"Web Services Contract-First Paste Schema",
						MessageBoxButtons.OK,
						MessageBoxIcon.Exclamation);

					return true;
    			}

				XmlSchemaSet schemas = new XmlSchemaSet();
				XmlSchemaInference inference = new XmlSchemaInference();

				using (XmlReader reader = schemaXml.CreateReader())
				{
					inference.InferSchema(reader, schemas);
				}

    			XmlWriterSettings settings = new XmlWriterSettings {Indent = true};

                foreach (XmlSchema schema in schemas.Schemas())
    			{
					if (schema.Items.Count == 0) continue;

    				string schemaName = ((XmlSchemaElement)(schema.Items[0])).Name;
    				string fileName = Path.Combine(visualStudio.SelectedItem.Directory, Path.ChangeExtension(schemaName, "xsd"));

					if (File.Exists(fileName))
					{
						DialogResult dialogResult = MessageBox.Show(
							"A file named '" + Path.GetFileName(fileName) + "' already exist in the project. Do you want to overwrite this file?",
							"Web Services Contract-First Paste Schema",
							MessageBoxButtons.YesNo,
							MessageBoxIcon.Question);

						if (dialogResult == DialogResult.No) continue;
					}

    				using (XmlWriter writer = XmlWriter.Create(fileName, settings))
    				{
    					if (writer != null)
    					{
    						schema.Write(writer);
    					}
    				}
					if (File.Exists(fileName))
					{
						AddFileToCurrentProject(fileName);
					}
    			}
    		}
    		catch (Exception ex)
    		{
				AppLog.LogMessage(ex.ToString());
				MessageBox.Show(ex.ToString(), "Web Services Contract-First Paste Schema", MessageBoxButtons.OK, MessageBoxIcon.Error);
    		}

    		return true;
    	}

        private void AddGeneratedFilesToProject(CodeWriterOutput output)
        {
            foreach (string file in output.CodeFileNames)
            {
                AddFileToCurrentProject(file);
            }
			if (!string.IsNullOrEmpty(output.ConfigurationFile) && File.Exists(output.ConfigurationFile))
			{
				AddFileToCurrentProject(output.ConfigurationFile);				
			}
        }

        private void AddFileToCurrentProject(string file)
        {
            VisualStudioProject project = this.visualStudio.SelectedProject;
            project.AddFile(file);
        }

        private bool CanGenerateCode()
        {
            // Do we have a project?
            if (visualStudio.SelectedProject == null)
            {
                DisplayMessage("Cannot generate code for items outside of a project.");
                return false;
            }
            return true;
        }

        private string GetOutputDirectory()
        {
            VisualStudioSelectedItem selectedItem = this.visualStudio.SelectedItem;
            VisualStudioProject project = selectedItem.ParentProject;
            string outputDirectory;

            if (project.IsWebProject)
            {
                string serviceName = Path.GetFileNameWithoutExtension(selectedItem.FileName);
				outputDirectory = project.AddCodeFolderToWebProject(serviceName);
            }
            else
            {
                outputDirectory = selectedItem.Directory;
            }

            return outputDirectory;
        }

        private static void DisplayMessage(string message)
        {
            MessageBox.Show(message, "Web Services Contract-First code generation", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

    	private void OnSelectionChanged()
    	{
			if (visualStudio.SelectedItems.Count() == 0) return;

    		bool menuVisible = false;
    		foreach (VisualStudioSelectedItem selectedItem in visualStudio.SelectedItems)
    		{
				if (selectedItem.FileName == null) continue;

    			if (selectedItem.FileName.EndsWith(".wsdl", StringComparison.OrdinalIgnoreCase)
    			    || selectedItem.FileName.EndsWith(".xsd", StringComparison.OrdinalIgnoreCase))
    			{
    				menuVisible = true;
    				break;
    			}
    		}

			ToggleSubMenuVisibility(menuVisible);
    	}

    	#region Private helper methods

		private void ToggleSubMenuVisibility(bool visible)
		{
			if (subMenuPopup.Visible == visible && webSubMenuPopup.Visible == visible)
			{
				return;
			}

			subMenuPopup.Visible = visible;
			webSubMenuPopup.Visible = visible;

			if (applicationObject.Version == "10.0")
			{
				subMenuPopup.CommandBar.Visible = visible;
				webSubMenuPopup.CommandBar.Visible = visible;
			}
		}

    	private string GetCommandName(string command)
    	{
    		return addInInstance.ProgID + "." + command;
    	}

		private static CommandBar FindCommandBarByName(CommandBars commandBars, string name)
		{
			return commandBars.OfType<CommandBar>()
				.Where(cb => cb.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
				.FirstOrDefault();
		}

		private static CommandBar GetToolsCommandBar(CommandBars commandBars)
		{
			try
			{
				// Tools can only be located through the indexer. WTF?!
				return commandBars["Tools"];
			}
			catch
			{
				return null;
			}
		}

    	private void RemoveCommand(string command)
    	{
    		Command cmd = null;
    		try
    		{
    			cmd = applicationObject.Commands.OfType<Command>()
					.Where(c => c.Name == GetCommandName(command))
					.FirstOrDefault();
    		}
    		catch (Exception ex)
    		{
				AppLog.LogMessage(ex.ToString());
    		}

    		if (cmd != null)
    		{
    			cmd.Delete();
    		}
    	}

		private static void RemoveCommandBarPopup(CommandBarPopup commandBarPopup)
		{
			if (commandBarPopup != null)
			{
				commandBarPopup.Delete(true);
			}
		}

        private void AddAssemblyReferences()
        {
            VisualStudioProject project = visualStudio.SelectedProject;
            project.AddReference("System");
            project.AddReference("System.Xml");
            project.AddReference("System.Runtime.Serialization");
            project.AddReference("System.ServiceModel");
            project.AddReference("System.Configuration");
        }  
  
        /// <summary>
        /// Add more assembly references beyond the basic Fx assemblies
        /// </summary>
        private void AddMetadataExtensionsReference()
        {
            string addinDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string extensionPath = Path.Combine(addinDirectory, "Thinktecture.ServiceModel.Extensions.Metadata.dll");

            VisualStudioProject project = this.visualStudio.SelectedProject;
            project.AddReference(extensionPath);
        }

        #endregion
    }
}