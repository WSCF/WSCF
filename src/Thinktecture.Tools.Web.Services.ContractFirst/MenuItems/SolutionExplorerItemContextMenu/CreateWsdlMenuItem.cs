using System;
using System.ComponentModel.Design;
using System.Linq;
using Microsoft.VisualStudio.Shell;
using Thinktecture.Tools.Web.Services.ContractFirst.VsObjectWrappers;

namespace Thinktecture.Tools.Web.Services.ContractFirst.MenuItems.SolutionExplorerItemContextMenu
{
    internal static class CreateWsdlMenuItem
    {
        private static void BeforeQueryStatus(object sender, EventArgs e)
        {
            if (sender is OleMenuCommand menu) menu.Visible = IsVisible();
        }

        public static bool IsVisible()
        {
            var vs = new VisualStudio(VSPackage.DTE);
            if (!vs.IsItemSelected) return false;
            return vs.SelectedItems.Count() == 1 && vs.SelectedItems.Any(i => i.FileName.EndsWith(".xsd"));
        }

        private static void MenuItemCallbackHandler(object sender, EventArgs e)
        {
            VSPackage.ServiceFacade.ExecuteCommand(WscfCommand.CreateWsdl);
        }

        public static void Register(MenuCommandService mcs)
        {
            var cmdId = new CommandID(VSCommandTable.PackageGuids.VSPackageCmdSetGuid, VSCommandTable.CommandIds.CreateWsdl);
            var menu = new OleMenuCommand(MenuItemCallbackHandler, cmdId);
            menu.BeforeQueryStatus += BeforeQueryStatus;
            mcs.AddCommand(menu);
        }

    }
}
