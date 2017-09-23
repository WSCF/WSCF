using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;

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
                   CreateDataContractCodeMenuItem.IsVisible() ;
        }

        public static void Register(MenuCommandService mcs)
        {
            var cmdId = new CommandID(VSCommandTable.PackageGuids.VSPackageCmdSetGuid, VSCommandTable.CommandIds.SolutionExplorerItemContextMenuItem);
            var menu = new OleMenuCommand((s, e) => { }, cmdId);
            menu.BeforeQueryStatus += BeforeQueryStatus;
            mcs.AddCommand(menu);
        }
    }
}
