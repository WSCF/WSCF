using System;
using System.Collections.Generic;
using System.Text;
using System.CodeDom;

namespace Thinktecture.Tools.Web.Services.CodeGeneration
{
    /// <summary>
    /// This interface must be implemented by all type filters.
    /// </summary>
    internal interface ITypeFilter
    {
        bool IsMatching(CodeTypeExtension type);
    }
}
