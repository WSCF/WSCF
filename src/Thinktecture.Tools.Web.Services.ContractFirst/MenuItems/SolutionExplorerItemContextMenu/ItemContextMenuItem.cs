using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using Task = System.Threading.Tasks.Task;

namespace Thinktecture.Tools.Web.Services.ContractFirst.MenuItems.SolutionExplorerItemContextMenu
{
    internal static class ItemContextMenuItem
    {
        private static void BeforeQueryStatus(object sender, EventArgs e)
        {
            if (sender is OleMenuCommand menu) menu.Visible = IsVisible();
        }

        private static bool IsVisible()
        {
            return CreateWsdlMenuItem.IsVisible() ||
                   EditWsdlMenuItem.IsVisible() ||
                   CreateDataContractCodeMenuItem.IsVisible();
        }

        // Asynchronous initialization
        public static async Task InitializeAsync(AsyncPackage package)
        {
            var commandService = (IMenuCommandService)await package.GetServiceAsync(typeof(IMenuCommandService));

            var cmdId = new CommandID(VSCommandTable.PackageGuids.VSPackageCmdSetGuid, VSCommandTable.CommandIds.SolutionExplorerItemContextMenuItem);
            var cmd = new OleMenuCommand((s, e) => { }, cmdId);
            cmd.BeforeQueryStatus += BeforeQueryStatus;

            commandService.AddCommand(cmd);
        }
    }
}
