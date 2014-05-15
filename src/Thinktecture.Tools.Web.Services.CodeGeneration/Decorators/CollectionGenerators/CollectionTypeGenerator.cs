using System;
using System.Collections.Generic;
using System.Text;
using System.CodeDom;

namespace Thinktecture.Tools.Web.Services.CodeGeneration
{
    internal class CollectionTypeGenerator
    {
        #region Private Members

        private ICollectionTypeProvider collectionTypeProvider;
        private Dictionary<string, CodeTypeReference> generatedTypes;
        private ExtendedCodeDomTree code;

        #endregion

        #region Constructors

        public CollectionTypeGenerator(ICollectionTypeProvider collectionTypeProvider, ExtendedCodeDomTree code)
        {
            this.collectionTypeProvider = collectionTypeProvider;
            this.generatedTypes = new Dictionary<string, CodeTypeReference>();
            this.code = code;
        }

        #endregion

        #region Public Methods

        public void Execute()
        {
            RunConverter(code.DataContracts);
            RunConverter(code.MessageContracts);
            RunConverter(code.ClientTypes);
            RunConverter(code.ServiceContracts);
            RunConverter(code.ServiceTypes);
        }

        #endregion

        #region Private Methods          

        private void RunConverter(FilteredTypes collection)
        {
            // Get the initial count in the collection.
            int icount = collection.Count;

            // Do this for each type found in the filtered type collection.
            for (int i = 0; i < icount; i++ )
            {
                CodeTypeExtension typeExtension = collection[i];
                // Send the fields to members converter
                ConvertMembers(typeExtension.Fields);
                // Send the properties to the members converter.
                ConvertMembers(typeExtension.Properties);
                // Send the methods to the members converter.
                ConvertMembers(typeExtension.Methods);
                // Send the constructors to the members converter.
                ConvertMembers(typeExtension.Constructors);
            }
        }
       
        private void ConvertMembers(FilteredTypeMembers filteredTypeMembers)
        {
            foreach (CodeTypeMemberExtension memberExt in filteredTypeMembers)
            {
                // Move to the next item if this is not convertible.
                if (!IsConvertibleMemeber(memberExt))
                {
                    continue;
                }

                if (memberExt.Kind == CodeTypeMemberKind.Field)
                {
                    CodeMemberField field = (CodeMemberField)memberExt.ExtendedObject;                    
                    field.Type = GetCollectionTypeReference(field.Type);
					HandleShouldSerialize(memberExt);
                }
                else if (memberExt.Kind == CodeTypeMemberKind.Property)
                {
                    CodeMemberProperty property = (CodeMemberProperty)memberExt.ExtendedObject;
                    property.Type = GetCollectionTypeReference(property.Type);
					HandleShouldSerialize(memberExt);
                }
                else if (memberExt.Kind == CodeTypeMemberKind.Method || 
                    memberExt.Kind == CodeTypeMemberKind.Constructor || 
                    memberExt.Kind == CodeTypeMemberKind.StaticConstructor)
                {
                    CodeMemberMethod method = (CodeMemberMethod)memberExt.ExtendedObject;
                    ProcessMethod(method);
                }
            }
        }

		private static void HandleShouldSerialize(CodeTypeMemberExtension fieldPropertyMember)
		{
			CodeTypeDeclaration type = fieldPropertyMember.Parent.ExtendedObject as CodeTypeDeclaration;
			if (type == null) return;

			CodeAttributeDeclaration arrayAttribute = fieldPropertyMember.FindAttribute("System.Xml.Serialization.XmlArrayItemAttribute");
			if (arrayAttribute == null) return;

			CodeAttributeArgument isNullableArgument = arrayAttribute.FindArgument("IsNullable");
			if (isNullableArgument == null) return;

			bool isNullable = (bool)((CodePrimitiveExpression)isNullableArgument.Value).Value;
			if (isNullable) return;

			string name = fieldPropertyMember.ExtendedObject.Name;

			CodeMemberMethod shouldSerializeMethod = new CodeMemberMethod
			{
				Attributes = MemberAttributes.Public,
				Name = "ShouldSerialize" + name,
				ReturnType = new CodeTypeReference(typeof(bool))
			};


			CodeThisReferenceExpression thisReference = new CodeThisReferenceExpression();
			CodeFieldReferenceExpression collectionField = new CodeFieldReferenceExpression(thisReference, name);
			CodeBinaryOperatorExpression notNullExpression = new CodeBinaryOperatorExpression
			{
				Left = collectionField, 
				Operator = CodeBinaryOperatorType.IdentityInequality, 
				Right = new CodePrimitiveExpression(null)
			};

			CodeBinaryOperatorExpression greaterThanZeroExpression = new CodeBinaryOperatorExpression
			{
				Left = new CodePropertyReferenceExpression(collectionField, "Count"),
				Operator = CodeBinaryOperatorType.GreaterThan,
				Right = new CodePrimitiveExpression(0)
			};

			CodeBinaryOperatorExpression andExpression = new CodeBinaryOperatorExpression
			{
				Left = notNullExpression,
				Operator = CodeBinaryOperatorType.BooleanAnd,
				Right = greaterThanZeroExpression
			};

			CodeMethodReturnStatement returnStatement = new CodeMethodReturnStatement(andExpression);
			shouldSerializeMethod.Statements.Add(returnStatement);

			type.Members.Add(shouldSerializeMethod);
		}

        /// <summary>
        /// This method ensures that we can generate a collection type to substitute
        /// given members type.
        /// </summary>
        private bool IsConvertibleMemeber(CodeTypeMemberExtension memberExtension)
        {
            if (memberExtension.Kind == CodeTypeMemberKind.Field)
            {
                CodeMemberField field = (CodeMemberField)memberExtension.ExtendedObject;
                if (field.Type.ArrayElementType == null)
                {
                    return false;
                }

                // The field is not convertible if it is used in a property that has invalid attributes.
                foreach (CodeTypeMemberExtension parent in memberExtension.Parent.Properties)
                {
                    CodeMemberProperty property = (CodeMemberProperty)parent.ExtendedObject;
                    foreach (CodeStatement statement in property.GetStatements)
                    {
                        // Get the return statement for the property getter.
                        CodeMethodReturnStatement returnStatement = statement as CodeMethodReturnStatement;
                        if (returnStatement != null)
                        {
                            // Do we have a field reference on the right side of the assignment statement?
                            CodeFieldReferenceExpression fieldRef = returnStatement.Expression as CodeFieldReferenceExpression;
                            if (fieldRef != null)
                            {
                                // Is the field referenced the one we are checking?
                                if (fieldRef.FieldName == field.Name)
                                {
                                    // Does the property have invalid attributes?
                                    if (HasInvalidAttributes(parent))
                                    {
                                        // If so, then the field should not be processed!
                                        return false;
                                    }
                                }
                            }
                        }
                    }
                }

                // Return true if we don't have any invalid attributes.
                return !HasInvalidAttributes(memberExtension);
            }
            else if (memberExtension.Kind == CodeTypeMemberKind.Property)
            {
                CodeMemberProperty property = (CodeMemberProperty)memberExtension.ExtendedObject;
                if (property.Type.ArrayElementType == null)
                {
                    return false;
                }
                // Return true if we don't have any invalid attributes.
                return !HasInvalidAttributes(memberExtension);
            }
            else if (memberExtension.Kind == CodeTypeMemberKind.Method)
            {
                return true;
            }
            else if (memberExtension.Kind == CodeTypeMemberKind.Constructor ||
           memberExtension.Kind == CodeTypeMemberKind.StaticConstructor)
            {
                return true;
            }
            else
            {
                // Currently we support only converting properties, fields and methods.
                return false;
            }
        }

        /// <summary>
        /// This method checks whether a given CodeTypeMember contains any attributes that will
        /// prevent from converting its type from an array to a collection.
        /// </summary>
        private bool HasInvalidAttributes(CodeTypeMemberExtension memberExtension)
        {
            if (memberExtension.FindAttribute("System.Xml.Serialization.XmlChoiceIdentifierAttribute") != null)
            {
                return true;
            }
            else if (memberExtension.FindAttribute("System.Xml.Serialization.XmlIgnoreAttribute") != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void ProcessMethod(CodeMemberMethod method)
        {            
            // First we process the parameters.
            foreach (CodeParameterDeclarationExpression paramExp in method.Parameters)
            {
                if (paramExp.Type.ArrayElementType != null)
                {
                    paramExp.Type = GetCollectionTypeReference(paramExp.Type, false);
                }
            }

            // Now we process the return type.
            if (method.ReturnType != null && method.ReturnType.ArrayElementType != null)
            {
                method.ReturnType = GetCollectionTypeReference(method.ReturnType, false);
            }
        }

        private CodeTypeReference GetCollectionTypeReference(CodeTypeReference ctr)
        {
            return GetCollectionTypeReference(ctr, true);
        }

        private CodeTypeReference GetCollectionTypeReference(CodeTypeReference ctr, bool create)
        {
            CodeTypeReference nctr = CacheLookup(ctr);
            if (nctr == null)
            {
                if (create)
                {
                    nctr = collectionTypeProvider.CreateCollectionType(ctr, code);
                    CacheNewType(ctr, nctr);
                }
                else
                {
                    nctr = ctr;
                }
            }
            return nctr;
        }

        private CodeTypeReference CacheLookup(CodeTypeReference type)
        {
            if (generatedTypes.ContainsKey(type.BaseType))
            {
                return generatedTypes[type.BaseType];
            }
            return null;
        }

        private void CacheNewType(CodeTypeReference oldTypeRef, CodeTypeReference newTypeRef)
        {
            generatedTypes.Add(oldTypeRef.BaseType, newTypeRef);
        }        

        #endregion
    }
}
