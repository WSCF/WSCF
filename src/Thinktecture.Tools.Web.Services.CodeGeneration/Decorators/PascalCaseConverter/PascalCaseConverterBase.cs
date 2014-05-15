using System;
using System.Collections.Generic;
using System.Text;
using System.CodeDom;

namespace Thinktecture.Tools.Web.Services.CodeGeneration
{
    internal abstract class PascalCaseConverterBase
    {
        private CodeTypeExtension typeExtension;
        private CodeTypeDeclaration type;

    	protected ExtendedCodeDomTree Code;

        #region Constructors

    	protected PascalCaseConverterBase(CodeTypeExtension codeTypeExtension, ExtendedCodeDomTree code)
        {
            this.typeExtension = codeTypeExtension;
        	this.Code = code;
        	this.type = (CodeTypeDeclaration)codeTypeExtension.ExtendedObject;
        }

        #endregion

        #region Public Methods

        public string Convert(out string oldName)
        {
            string newName = ConvertTypeName(out oldName);
            ConvertMembers();
            return newName;
        }

    	#endregion

		#region Protected Methods

    	protected void ConvertFieldReferencesInConstructors(FilteredTypeMembers constructors, string oldName, string newName)
    	{
    		// Do this for all constructors we have.
    		foreach(CodeTypeMemberExtension ctorExtension in constructors)
    		{
    			// Get a reference to the actual constructor object.
    			CodeConstructor constructor = (CodeConstructor)ctorExtension.ExtendedObject;
                
    			// Do this for all statements we have in the constructor.
    			foreach (CodeStatement statement in constructor.Statements)
    			{
    				// Is this an assign statement?
    				CodeAssignStatement assignStatement = statement as CodeAssignStatement;
    				if (assignStatement != null)
    				{
    					// Do we have a field reference on the left side of the assignment statement?
    					CodeFieldReferenceExpression fieldRef = assignStatement.Left as CodeFieldReferenceExpression;
    					if (fieldRef != null)
    					{
    						// Does the referenced field belong to the this object?
    						if (typeof(CodeThisReferenceExpression) == fieldRef.TargetObject.GetType())
    						{
    							// Change the field name if it's changed.
    							if (fieldRef.FieldName == oldName)
    							{
    								fieldRef.FieldName = newName;
    							}
    						}
    					}
    				}
    			}
    		}
    	}

    	#endregion

        #region Private Methods

        /// <summary>
        /// Changes the given type's name to Pascal case.
        /// </summary>        
        private string ConvertTypeName(out string oldName)
        {
            // Holds the original type name.
            oldName = this.typeExtension.ExtendedObject.Name;
            // Change the type name to Pascal case.
            string newName = PascalCaseConverterHelper.GetPascalCaseName(oldName);
            if (oldName != newName)
            {
                this.typeExtension.ExtendedObject.Name = newName;
                OnTypeNameChanged(this.typeExtension, oldName, newName);
            }
            return newName;            
        }

        /// <summary>
        /// Converts field, property, method names in a type to Pascal case.
        /// </summary>        
        private void ConvertMembers()
        {
            if (this.type.IsClass)
            {
                ConvertTypeMemberNames();
            }
            else if (this.type.IsEnum)
            {
                ConvertEnumMemberNames();
            }
            else if (this.type.IsInterface)
            {
                ConvertTypeMemberNames();
            }
            else if (this.type.IsStruct)
            {
                ConvertTypeMemberNames();
            }
        }

        /// <summary>
        /// Converts all member names in a given class to Pascal case.
        /// </summary>        
        private void ConvertTypeMemberNames()
        {
            // Do this for all CodeTypeMember(s) in type.Members collection.
            foreach (CodeTypeMember member in type.Members)
            {
                // Cast the member reference to CodeTypeMemberExtension type.
                CodeTypeMemberExtension memberExtension = (CodeTypeMemberExtension)member;
                switch (memberExtension.Kind)
                {
                    case CodeTypeMemberKind.Field:
                        // We perform the pascal case conversion only in the public members.
                        if (CanConvertMember(memberExtension))
                        {
                            ConvertField(memberExtension);
                        }
                        break;
                    case CodeTypeMemberKind.Property:
                        // We perform the pascal case conversion only in the public members.
                        if (CanConvertMember(memberExtension))
                        {
                            ConvertProperty(memberExtension);
                        }
                        break;
                    case CodeTypeMemberKind.Method:
                        if (CanConvertMember(memberExtension))
                        {
                            ConvertMethod(memberExtension);
                        }
                        break;
                }
            }
        }
        
        /// <summary>
        /// Converts all member names in a given enumeration to Pascal case.
        /// </summary>        
        private void ConvertEnumMemberNames()
        {
            // Do this for all members in the enumeration.
            foreach (CodeTypeMember member in type.Members)
            {
                // Cast the member reference to CodeTypeMemberExtension type.
                CodeTypeMemberExtension ext = (CodeTypeMemberExtension)member;
                // We perform the pascal case conversion only in the public members.
                if ((ext.ExtendedObject.Attributes & MemberAttributes.Public) == MemberAttributes.Public ||
                    (ext.ExtendedObject.Attributes & MemberAttributes.Final) == MemberAttributes.Final)
                {
                    // Call the function for converting an enum member name to Pascal case.
                    ConvertEnumMemberName(ext);
                }
            }
        }

        /// <summary>
        /// Contains the logic for converting property name to pascal case.
        /// </summary>
        /// <param name="ext"></param>
        private void ConvertProperty(CodeTypeMemberExtension typeMemberExtension)
        {
            // Get a copy of the original type name.
            string oldName = typeMemberExtension.ExtendedObject.Name;
            // Change the field/property name to Pascal case.
            string newName = PascalCaseConverterHelper.GetPascalCaseName(oldName);
            if (oldName != newName)
            {
                typeMemberExtension.ExtendedObject.Name = newName;
                OnPropertyNameChanged(typeMemberExtension, oldName, newName);
            }
        }

        /// <summary>
        /// Contains the logic for converting field name to pascal case.
        /// </summary>
        /// <param name="ext"></param>
        private void ConvertField(CodeTypeMemberExtension typeMemberExtension)
        {
            // Get a copy of the original type name.
            string oldName = typeMemberExtension.ExtendedObject.Name;
            // Change the field/property name to Pascal case.
            string newName = PascalCaseConverterHelper.GetPascalCaseName(oldName);
            if (oldName != newName)
            {
                typeMemberExtension.ExtendedObject.Name = newName;
                OnFieldNameChanged(typeMemberExtension, oldName, newName);
            }
        }

        private void ConvertEnumMemberName(CodeTypeMemberExtension typeMemberExtension)
        {
            // Get a copy of the original type name.
            string oldName = typeMemberExtension.ExtendedObject.Name;
            // Change the field/property name to Pascal case.
            string newName = PascalCaseConverterHelper.GetPascalCaseName(oldName);
            if (oldName != newName)
            {
                typeMemberExtension.ExtendedObject.Name = newName;
                OnEnumMemberChanged(typeMemberExtension, oldName, newName);
            }
        }

        /// <summary>
        /// Contains the core logic for converting a method name to Pascal case.
        /// </summary>        
        private void ConvertMethod(CodeTypeMemberExtension memberExtension)
        {
            // Get a copy of the original name.
            string oldName = memberExtension.ExtendedObject.Name;
            // Change the method name to Pascal case.
            string newName = PascalCaseConverterHelper.GetPascalCaseMethodName(oldName);
            if (oldName != newName)
            {
                memberExtension.ExtendedObject.Name = newName;
                OnMethodNameChanged(memberExtension, oldName, newName);
            }
        }


        #endregion

        #region Abstract Methods
        protected abstract bool CanConvertTypeName(CodeTypeExtension typeExtension);
        protected abstract bool CanConvertMember(CodeTypeMemberExtension memberExtension);
        protected abstract void OnTypeNameChanged(CodeTypeExtension typeExtension, string oldName, string newName);
        protected abstract void OnFieldNameChanged(CodeTypeMemberExtension memberExtension, string oldName, string newName);
        protected abstract void OnPropertyNameChanged(CodeTypeMemberExtension memberExtension, string oldName, string newName);
        protected abstract void OnEventNameChanged(CodeTypeMemberExtension memberExtension, string oldName, string newName);
        protected abstract void OnMethodNameChanged(CodeTypeMemberExtension memberExtension, string oldName, string newName);
        protected abstract void OnEnumMemberChanged(CodeTypeMemberExtension memberExtension, string oldName, string newName);

        #endregion
    }
}
