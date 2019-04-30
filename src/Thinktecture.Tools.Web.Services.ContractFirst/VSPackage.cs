﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Thinktecture.Tools.Web.Services.ContractFirst.MenuItems.SolutionExplorerItemContextMenu;
using Thinktecture.Tools.Web.Services.ContractFirst.MenuItems.SolutionExplorerProjectContextMenu;
using Thinktecture.Tools.Web.Services.ContractFirst.MenuItems.ToolsMenu;
using Task = System.Threading.Tasks.Task;

namespace Thinktecture.Tools.Web.Services.ContractFirst
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", "2.1.1", IconResourceID = 400)] // Info on this package for Help/About
    [Guid(PackageGuidString)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public sealed class VSPackage : AsyncPackage
    {
        private const string PackageGuidString = "ede032b5-9e2d-4576-a53a-58f117366ce4";

        public static DTE2 DTE { get; private set; }

        internal static ServiceFacade ServiceFacade { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VSPackage"/> class.
        /// </summary>
        public VSPackage()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
        }

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await base.InitializeAsync(cancellationToken, progress);

            // runs in the background thread and doesn't affect the responsiveness of the UI thread.
            await Task.Delay(5000);

            // Switches to the UI thread in order to consume some services used in command initialization
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            // Query service asynchronously from the UI thread
            DTE = await GetServiceAsync(typeof(DTE)) as DTE2;
            ServiceFacade = new ServiceFacade(DTE);

            // Initializes the command asynchronously now on the UI thread
            await ItemContextMenuItem.InitializeAsync(this);
            await CreateWsdlMenuItem.InitializeAsync(this);
            await EditWsdlMenuItem.InitializeAsync(this);
            await CreateDataContractCodeMenuItem.InitializeAsync(this);
            await CreateWebServiceCodeMenuItem.InitializeAsync(this);
            await ImplementWsdlMenuItem.InitializeAsync(this);
            await WscfMenuItem.InitializeAsync(this);
        }
    }

    #endregion
}
