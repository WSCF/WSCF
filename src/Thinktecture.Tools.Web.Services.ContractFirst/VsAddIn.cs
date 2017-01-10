using System;
using System.Linq;
using System.Runtime.InteropServices;
using EnvDTE;
using EnvDTE80;
using Extensibility;
using Microsoft.VisualStudio.CommandBars;

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
    [Guid("6C6DA7B1-0758-41f4-9669-04ED4B037669"), ProgId("Thinktecture.Tools.Web.Services.ContractFirst.VsAddIn")]
    public class VsAddIn : IDTExtensibility2, IDTCommandTarget
    {
        private AddIn AddInInstance { get; set; }
        private CommandBarPopup subMenuPopup;
        private CommandBarPopup webSubMenuPopup;
        private CommandBarPopup projectSubMenuPopup;
        private CommandBarPopup webProjectSubMenuPopup;
        private CommandBarPopup toolsSubMenuPopup;
        private SelectionEvents selectionEvents;

        private Connect Connect { get; set; }

        /// <summary>
        ///		Implements the constructor for the Add-in object.
        ///		Place your initialization code within this method.
        /// </summary>
        public VsAddIn() : base()
        {
            AppLog.LogMessage($"A new instance of {nameof(VsAddIn)} class is created.");
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

            Connect = new Connect((DTE2)application);

            AddInInstance = (AddIn)addInInst;

            CreateAddInMenus((VsAddIn)addInInst, connectMode);

            AppLog.LogMessage("Leaving OnConnection method.");
        }

        private void CreateAddInMenus(VsAddIn addin, ext_ConnectMode connectMode)
        {
            object[] contextGUIDS = new object[] { };
            //CommandBar cmdBar;
            Command cmdObj;

            if (connectMode == ext_ConnectMode.ext_cm_Startup ||
                connectMode == ext_ConnectMode.ext_cm_AfterStartup ||
                connectMode == ext_ConnectMode.ext_cm_UISetup)
            {
                AppLog.LogMessage("Creating commands.");

                Commands2 commands = (Commands2)Connect.ApplicationObject.Commands;
                CommandBars commandBars = (CommandBars)Connect.ApplicationObject.CommandBars;

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

                    selectionEvents = Connect.ApplicationObject.Events.SelectionEvents;
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
                            AddInInstance,
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
                            AddInInstance,
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
                            AddInInstance,
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
                            AddInInstance,
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
                            AddInInstance,
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
                            AddInInstance,
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
                            AddInInstance,
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
                    UIHierarchy uiHierarchy = (UIHierarchy)Connect.ApplicationObject.Windows.Item(
                        Constants.vsWindowKindSolutionExplorer).Object;
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
                    UIHierarchy uiHierarchy = (UIHierarchy)Connect.ApplicationObject.Windows.Item(
                        Constants.vsWindowKindSolutionExplorer).Object;
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
                    UIHierarchy uiHierarchy = (UIHierarchy)Connect.ApplicationObject.Windows.Item(
                        Constants.vsWindowKindSolutionExplorer).Object;
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
                    UIHierarchy uiHierarchy = (UIHierarchy)Connect.ApplicationObject.Windows.Item(
                        Constants.vsWindowKindSolutionExplorer).Object;
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
                        if (Connect.VisualStudio.SelectedProject != null)
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

            AppLog.LogMessage($"Entering {nameof(this.Exec)} method.");

            switch (commandName)
            {
                case "Thinktecture.Tools.Web.Services.ContractFirst.Connect.WsContractFirstContextMenu2":
                case "Thinktecture.Tools.Web.Services.ContractFirst.Connect.WsContractFirst":
                case "Thinktecture.Tools.Web.Services.ContractFirst.Connect.WsContractFirstContextMenu":
                    handled = Connect.ExecuteCommand(WscfCommand.GenerateWebServiceCode); //ProcessCodeGenerationRequest();
                    break;
                case "Thinktecture.Tools.Web.Services.ContractFirst.Connect.CreateWsdlContextMenu":
                    handled = Connect.ExecuteCommand(WscfCommand.CreateWsdl); // ProcessWsdlWizardRequest(false);
                    break;
                case "Thinktecture.Tools.Web.Services.ContractFirst.Connect.EditWsdlContextMenu":
                    handled = Connect.ExecuteCommand(WscfCommand.EditWsdl); //ProcessWsdlWizardRequest(true);
                    break;
                case "Thinktecture.Tools.Web.Services.ContractFirst.Connect.GenerateCodeMenu":
                    handled = Connect.ExecuteCommand(WscfCommand.GenerateDataContractCode); //ProcessXsdCodeGenerationRequest();
                    break;
                case "Thinktecture.Tools.Web.Services.ContractFirst.Connect.PasteSchemaMenu":
                    handled = Connect.ExecuteCommand(WscfCommand.PasteXmlAsSchema); //ProcessPasteSchemaRequest();
                    break;
                default:
                    handled = false;
                    break;
            }

            AppLog.LogMessage("Leaving Exec method");

        }

        private void OnSelectionChanged()
        {
            if (!Connect.VisualStudio.SelectedItems.Any()) return;

            var menuVisible = false;
            foreach (VisualStudioSelectedItem selectedItem in Connect.VisualStudio.SelectedItems)
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

            if (Connect.ApplicationObject.Version == "10.0")
            {
                subMenuPopup.CommandBar.Visible = visible;
                webSubMenuPopup.CommandBar.Visible = visible;
            }
        }

        private string GetCommandName(string command)
        {
            return AddInInstance.ProgID + "." + command;
        }

        private static CommandBar FindCommandBarByName(CommandBars commandBars, string name)
        {
            return commandBars
                .OfType<CommandBar>()
                .FirstOrDefault(cb => cb.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        private static CommandBar GetToolsCommandBar(CommandBars commandBars)
        {
            try
            {
                // Tools can only be located through the indexer.
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
                cmd = Connect.ApplicationObject.Commands
                    .OfType<Command>()
                    .FirstOrDefault(c => c.Name == GetCommandName(command));
            }
            catch (Exception ex)
            {
                AppLog.LogMessage(ex.ToString());
            }

            cmd?.Delete();
        }

        private static void RemoveCommandBarPopup(CommandBarPopup commandBarPopup)
        {
            commandBarPopup?.Delete(true);
        }

        #endregion

    }
}
