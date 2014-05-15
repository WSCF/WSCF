using System;
using System.Collections.Generic;
using System.Text;
using System.CodeDom;
using System.Collections;
using System.Diagnostics;

namespace Thinktecture.Tools.Web.Services.CodeGeneration
{
    /// <summary>
    /// This class decorates the objects of CodeTypeMembers type using object composition.
    /// The decorator contains the core implementation for searching and caching custom 
    /// attributes in the object being decorated.
    /// </summary> 
    /// <remarks>
    /// This class inherits CodeTypeMember class. Here we do this merely for type matching
    /// and not for getting behavior by any means. Therefore clients must not try to access the
    /// behavior of the decorated instance by using members of this class.
    /// For example, you must not use CodeTypeMemberExtension.CustomAttributes to access
    /// the CustomAttributes collection of the object we are decorating. Instead, you have
    /// to use CodeTypeMemberExstension.ExtendedObject.CustomAttributes.
    /// </remarks>
    [DebuggerStepThrough]
    public class CodeTypeMemberExtension : AttributableCodeDomObject
    {
        #region Private members
        
        // Specifies the kind of CodeTypeMember extended by this object.
        private CodeTypeMemberKind kind;
        // Reference to the parent this type member belongs to.
        private CodeTypeExtension parent;
        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of CodeTypeMemberExtension class.
        /// </summary>
        /// <param name="extendObject">An object to be decorated by this instance.</param>
        public CodeTypeMemberExtension(CodeTypeMember extendedObject, CodeTypeExtension parent) 
            : base(extendedObject)
        {                                  
            if (typeof(CodeMemberField) == extendedObject.GetType())
            {
                this.kind = CodeTypeMemberKind.Field;
            }
            else if (typeof(CodeMemberMethod) == extendedObject.GetType())
            {
                this.kind = CodeTypeMemberKind.Method;
            }
            else if (typeof(CodeMemberProperty) == extendedObject.GetType())
            {
                this.kind = CodeTypeMemberKind.Property;
            }
            else if (typeof(CodeMemberEvent) == extendedObject.GetType())
            {
                this.kind = CodeTypeMemberKind.Event;
            }
            else if (typeof(CodeSnippetTypeMember) == extendedObject.GetType())
            {
                this.kind = CodeTypeMemberKind.Snippet;
            }
            else if (typeof(CodeConstructor) == extendedObject.GetType())
            {
                this.kind = CodeTypeMemberKind.Constructor;
            }
            else if (typeof(CodeTypeConstructor) == extendedObject.GetType())
            {
                this.kind = CodeTypeMemberKind.StaticConstructor;
            }
            this.parent = parent;
        }
               
        #endregion

        #region Public properties

        public CodeTypeMemberKind Kind
        {
            get { return this.kind; }
        }

        public CodeTypeExtension Parent
        {
            get { return this.parent; }
        }

        #endregion
    }
}