using System;
using System.Collections.Generic;
using System.Text;
using System.CodeDom;
using System.Diagnostics;

namespace Thinktecture.Tools.Web.Services.CodeGeneration
{
    /// <summary>
    /// This class defines a special type of CodeTypeMemberExtension
    /// which is only used to extend CodeTypeDeclarations.
    /// </summary>
    [DebuggerStepThrough]
    public class CodeTypeExtension : AttributableCodeDomObject
    {
        #region Private Fields

        private CodeTypeKind kind;
        private FilteredTypeMembers fields;
        private FilteredTypeMembers properties;
        private FilteredTypeMembers methods;
        private FilteredTypeMembers constructors;
        private FilteredTypeMembers events;
        private FilteredTypeMembers unknown;

        #endregion

        #region Constructors

        public CodeTypeExtension(CodeTypeDeclaration codeTypeDeclaration)
            : base((CodeTypeMember)codeTypeDeclaration)
        {
            this.fields = new FilteredTypeMembers(codeTypeDeclaration.Members);
            this.properties = new FilteredTypeMembers(codeTypeDeclaration.Members);
            this.methods = new FilteredTypeMembers(codeTypeDeclaration.Members);
            this.constructors = new FilteredTypeMembers(codeTypeDeclaration.Members);
            this.events = new FilteredTypeMembers(codeTypeDeclaration.Members);
            this.unknown = new FilteredTypeMembers(codeTypeDeclaration.Members);
        }

        #endregion

        #region Public Properties

        public CodeTypeKind Kind
        {
            get { return kind; }
            set { kind = value; }
        }

        public FilteredTypeMembers Fields
        {
            get { return this.fields; }
        }

        public FilteredTypeMembers Properties
        {
            get { return this.properties; }
        }

        public FilteredTypeMembers Methods
        {
            get { return this.methods; }
        }

        public FilteredTypeMembers Constructors
        {
            get { return this.constructors; }
        }

        public FilteredTypeMembers Events
        {
            get { return this.events; }
        }

        public FilteredTypeMembers Unknown
        {
            get { return this.unknown; }
        }

        #endregion
    }
}
