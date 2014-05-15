using System;
using System.Collections.Generic;
using System.Text;
using System.CodeDom;

namespace Thinktecture.Tools.Web.Services.CodeGeneration
{
    internal interface ICollectionTypeProvider
    {
        CodeTypeReference CreateCollectionType(CodeTypeReference entityType, ExtendedCodeDomTree code);
    }
}
