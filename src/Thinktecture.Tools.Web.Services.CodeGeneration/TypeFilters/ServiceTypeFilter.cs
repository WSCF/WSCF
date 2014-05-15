using System;
using System.Collections.Generic;
using System.Text;

namespace Thinktecture.Tools.Web.Services.CodeGeneration
{
    /// <summary>
    /// This class implements the type filter for filtering service types.
    /// </summary>
    internal sealed class ServiceTypeTypeFilter : ITypeFilter
    {
        #region ITypeFilter Members

        public bool IsMatching(CodeTypeExtension type)
        {
            return (type.FindAttribute("System.ServiceModel.ServiceBehaviorAttribute") != null);
        }

        #endregion
    }
}
