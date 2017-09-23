using System;

namespace Thinktecture.Tools.Web.Services.ContractFirst
{
    internal static class VSCommandTable
    {
        private static class PackageGuidStrings
        {
            public const string VSPackageGuidString = "{ede032b5-9e2d-4576-a53a-58f117366ce4}";
            public const string VSPackageCmdSetGuidString = "{9bb4c433-e73e-43bc-8040-3fc68d2bfbaa}";
        }

        public static class PackageGuids
        {
            public static Guid VSPackageGuid = Guid.Parse(PackageGuidStrings.VSPackageGuidString);
            public static Guid VSPackageCmdSetGuid = Guid.Parse(PackageGuidStrings.VSPackageCmdSetGuidString);
        }

        public static class CommandIds
        {
            public const int CreateWsdl = 0x0100;
            public const int EditWsdl = 0x0110;
            public const int CreateContractCode = 0x0120;
            public const int CreateServiceCode = 0x0130;
            public const int ImplementWsdlCommand = 0x0140;
            public const int ContractFirstCommand = 0x0150;
            public const int SolutionExplorerItemContextMenuItem = 0x1030;
        }

    }

}
