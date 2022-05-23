﻿using System;
using System.ComponentModel.Design;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using Thinktecture.Tools.Web.Services.ContractFirst.VsObjectWrappers;
using Task = System.Threading.Tasks.Task;

namespace Thinktecture.Tools.Web.Services.ContractFirst.MenuItems.SolutionExplorerItemContextMenu
{
    internal static class CreateDataContractCodeMenuItem
    {
        private static void BeforeQueryStatus(object sender, EventArgs e)
        {
            if (sender is OleMenuCommand menu) menu.Visible = IsVisible();
        }

        public static bool IsVisible()
        {
            var vs = new VisualStudio(VSPackage.DTE);
            if (!vs.IsItemSelected) return false;
            return vs.SelectedItems.Count() == 1 && vs.SelectedItems.Any(i => i.FileName.EndsWith(".xsd") || i.FileName.EndsWith(".wsdl"));
        }

        private static void MenuItemCallbackHandler(object sender, EventArgs e)
        {
            VSPackage.ServiceFacade.ExecuteCommand(WscfCommand.GenerateDataContractCode);
        }

        // Asynchronous initialization
        public static async Task InitializeAsync(AsyncPackage package)
        {
            var commandService = (IMenuCommandService)await package.GetServiceAsync(typeof(IMenuCommandService));

            if (commandService != null)
            {
                var cmdId = new CommandID(VSCommandTable.PackageGuids.VSPackageCmdSetGuid, VSCommandTable.CommandIds.CreateContractCode);
                var cmd = new OleMenuCommand(MenuItemCallbackHandler, cmdId);
                cmd.BeforeQueryStatus += BeforeQueryStatus;

                commandService.AddCommand(cmd); 
            }
        }
    }
}
