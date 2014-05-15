using System;
using System.Collections.Generic;
using System.Text;

namespace Thinktecture.Tools.Web.Services.CodeGeneration
{
    /// <summary>
    /// This class implements the type filter for filtering message contracts.
    /// </summary>
    internal sealed class MessageContractTypeFilter : ITypeFilter
    {
        #region ITypeFilter Members

        public bool IsMatching(CodeTypeExtension type)
        {
            return (type.FindAttribute("System.ServiceModel.MessageContractAttribute") != null);
        }

        #endregion
    }

}
