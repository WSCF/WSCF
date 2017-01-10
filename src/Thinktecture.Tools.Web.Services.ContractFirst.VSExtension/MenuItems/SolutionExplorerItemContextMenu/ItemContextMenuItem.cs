using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;

namespace Thinktecture.Tools.Web.Services.ContractFirst.VSExtension.MenuItems.SolutionExplorerItemContextMenu
{
    internal class ItemContextMenuItem
    {
        private static void BeforeQueryStatus(object sender, EventArgs e)
        {
            var menu = sender as OleMenuCommand;
            if (null != menu) menu.Visible = IsVisible();
        }

        public static bool IsVisible()
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
