using System;
using System.Collections.Generic;
using System.Text;

namespace Thinktecture.Tools.Web.Services.CodeGeneration
{
    /// <summary>
    /// This class implements the type filter for filtering data contracts.
    /// </summary>
    internal sealed class DataContractTypeFilter : ITypeFilter
    {
        #region ITypeFilter Members

        public bool IsMatching(CodeTypeExtension type)
        {
            return (type.FindAttribute("System.Xml.Serialization.XmlTypeAttribute") != null
				|| type.FindAttribute("System.Xml.Serialization.XmlRootAttribute") != null);
        }

        #endregion
    }
}
