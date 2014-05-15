using System;
using System.Collections.Generic;
using System.Text;

namespace Thinktecture.Tools.Web.Services.CodeGeneration
{
    public enum CodeTypeKind
    {
        Unknown,
        DataContract,
        MessageContract,
        ServiceContract,
        ServiceType,
        ClientType,        
    }
}
