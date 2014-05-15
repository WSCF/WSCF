using System;
using System.Collections.Generic;
using System.Text;

namespace Thinktecture.Tools.Web.Services.CodeGeneration
{
    /// <summary>
    /// This class implements the type filter for filtering service contracts.
    /// </summary>
    internal sealed class ServiceContractTypeFilter : ITypeFilter
    {
        #region ITypeFilter Members

        public bool IsMatching(CodeTypeExtension type)
        {
            return (type.FindAttribute("System.ServiceModel.ServiceContractAttribute") != null);
        }

        #endregion
    }
}
