using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace Thinktecture.Tools.Web.Services.ContractFirst.MenuItems.ToolsMenu
{
    internal static class WscfMenuItem
    {
        private static void BeforeQueryStatus(object sender, EventArgs e)
        {
            if (sender is OleMenuCommand menu) menu.Visible = IsVisible();
        }

        private static bool IsVisible()
        {
            return true;
        }

        private static void MenuItemCallbackHandler(object sender, EventArgs e)
        {
            VSPackage.ServiceFacade.ExecuteCommand(WscfCommand.GenerateWebServiceCode);
        }

        // Asynchronous initialization
        public static async Task InitializeAsync(AsyncPackage package)
        {
            var commandService = (IMenuCommandService)await package.GetServiceAsync(typeof(IMenuCommandService));

            var cmdId = new CommandID(VSCommandTable.PackageGuids.VSPackageCmdSetGuid, VSCommandTable.CommandIds.ContractFirstCommand);
            var cmd = new OleMenuCommand(MenuItemCallbackHandler, cmdId);
            cmd.BeforeQueryStatus += BeforeQueryStatus;

            commandService.AddCommand(cmd);
        }
    }
}