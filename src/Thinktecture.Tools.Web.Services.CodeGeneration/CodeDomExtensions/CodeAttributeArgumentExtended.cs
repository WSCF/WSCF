using System;
using System.Collections.Generic;
using System.Text;
using System.CodeDom;
using System.Diagnostics;

namespace Thinktecture.Tools.Web.Services.CodeGeneration
{
    /// <summary>
    /// This type extends the existing CodeAttributeArgument type to add support for 
    /// specifying whether an argument is the default argument of a given attribute.
    /// Always use this type for constructing arguments from decorators. 
    /// </summary>
    [DebuggerStepThrough]
    public class CodeAttributeArgumentExtended : CodeAttributeArgument
    {
        private bool isDefault = false;

        public CodeAttributeArgumentExtended(string name, CodeExpression value)
            : base(name, value)
        {
        }

        public CodeAttributeArgumentExtended(CodeExpression value)
            : base(value)
        {
        }

        public CodeAttributeArgumentExtended(string name, CodeExpression value, bool isdefault)
            : base(name, value)
        {
            this.isDefault = isdefault;
        }

        public bool Default
        {
            get
            {
                return this.isDefault;
            }
            set
            {
                this.isDefault = value;
            }
        }

    }
}
