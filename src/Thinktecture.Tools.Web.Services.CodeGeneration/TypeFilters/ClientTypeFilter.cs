using System;
using System.Collections.Generic;
using System.Text;
using System.CodeDom;
using System.Diagnostics;

namespace Thinktecture.Tools.Web.Services.CodeGeneration
{
    /// <summary>
    /// This class implements the type filter for filtering client types.
    /// </summary>
    internal sealed class ClientTypeTypeFilter : ITypeFilter
    {
        #region ITypeFilter Members

        public bool IsMatching(CodeTypeExtension type)
        {            
            CodeTypeDeclaration ctd = (CodeTypeDeclaration)type.ExtendedObject;
            Debug.Assert(ctd != null, "Invalid type");

            if (ctd.BaseTypes.Count > 0)
            {
                foreach (CodeTypeReference ctr in ctd.BaseTypes)
                {
                    if (ctr.BaseType == "System.ServiceModel.ClientBase`1")
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        #endregion
    }
}
