using System;
using System.Collections.Generic;
using System.Text;

namespace Thinktecture.Tools.Web.Services.CodeGeneration
{
    public enum CodeTypeMemberKind
    {        
        Unknown,
        Field,
        Method,
        Property,
        Event,
        Snippet,
        Constructor,
        StaticConstructor
    }
}
