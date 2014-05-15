using System;
using System.Collections.Generic;
using System.Text;

namespace Thinktecture.Tools.Web.Services.CodeGeneration
{
    /// <summary>
    /// This interface must be implemented by all code decorators. 
    /// </summary>
    public interface ICodeDecorator
    {        
        void Decorate(ExtendedCodeDomTree code, CustomCodeGenerationOptions options);
    }
}
