using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Thinktecture.Tools.Web.Services.Wscf.Environment
{
    public static class ValidationHelper
    {
        public static bool IsWindowsFileName(string fileName)
        {
            const string validWindowsFileNamePattern =
                @"^(?!^(PRN|AUX|CLOCK\$|NUL|CON|COM\d|LPT\d|\..*)(\..+)?$)[^\x00-\x1f\\?*:\"";|/]+$";
            return Regex.IsMatch(fileName, validWindowsFileNamePattern);
        }

        public static bool IsDotNetNamespace(string namespaceName)
        {
            const string validDotNetNamespaceIdentifierPattern =
                @"^(?:(?:((?![^_\p{L}\p{Nl}])[\p{L}\p{Mn}\p{Mc}\p{Nd}\p{Nl}\p{Pc}\p{Cf}]+)\u002E?)+)(?<!\u002E)$";
            return Regex.IsMatch(namespaceName, validDotNetNamespaceIdentifierPattern);
        }
    }
}
