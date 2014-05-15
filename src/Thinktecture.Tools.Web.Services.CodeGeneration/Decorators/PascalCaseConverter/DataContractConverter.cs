using System;
using System.Collections.Generic;
using System.Text;
using System.CodeDom;

namespace Thinktecture.Tools.Web.Services.CodeGeneration
{
    internal sealed class DataContractConverter : PascalCaseConverterBase
    {
        #region Constructors

		public DataContractConverter(CodeTypeExtension typeExtension, ExtendedCodeDomTree code)
            : base(typeExtension, code)
        {
        }

        #endregion

        #region Protected Method Overrides

        protected override bool CanConvertTypeName(CodeTypeExtension typeExtension)
        {
            return true;
        }

        protected override bool CanConvertMember(CodeTypeMemberExtension memberExtension)
        {
            // For Data Contracts we change only the public proeperties/fields.
            if (memberExtension.Kind == CodeTypeMemberKind.Property || memberExtension.Kind == CodeTypeMemberKind.Field)
            {
                return ((memberExtension.ExtendedObject.Attributes & MemberAttributes.Public) == MemberAttributes.Public);
            }
            return true;
        }

        protected override void OnTypeNameChanged(CodeTypeExtension typeExtension, string oldName, string newName)
        {
            // Preserve Name values of XmlTypeAttribute or XmlRootAttribute here, because 
            // the XML names can be different than the .NET class name. This occurs, for example, 
            // when two XSD types have the same localname, but different XML namespaces.
            // The code generator then renames the second .NET class, while the names in XML
            // attributes should not be changed.
            // See also: http://wscfblue.codeplex.com/workitem/12733.

            // If [XmlTypeAttribute(TypeName="XXX")] already exists, preserve the XXX value.
            CodeAttributeDeclaration xmlType =
                typeExtension.FindAttribute("System.Xml.Serialization.XmlTypeAttribute");
            if (xmlType != null)
            {
                CodeAttributeArgument typeName = xmlType.FindArgument("TypeName");
                if (typeName != null)
                {
                    CodePrimitiveExpression expr = (CodePrimitiveExpression)typeName.Value;
                    oldName = expr.Value.ToString();
                }
            }

            // Prepare the XmlTypeAttribute attribute to specify the type name on the wire.
            xmlType =
                new CodeAttributeDeclaration("System.Xml.Serialization.XmlTypeAttribute",
                new CodeAttributeArgumentExtended("TypeName",
                new CodePrimitiveExpression(oldName), true));

            // Add/merge the XmlTypeAttribute attribute to ctd.
            typeExtension.AddAttribute(xmlType);

            // Prepare the XmlRootAttribute attribute to specify the type name on the wire.
            CodeAttributeDeclaration xmlRoot = 
                new CodeAttributeDeclaration("System.Xml.Serialization.XmlRootAttribute",
                new CodeAttributeArgumentExtended("ElementName",
                new CodePrimitiveExpression(oldName), true));

            // Add/merge XmlRootAttribute attribute to ctd.
            typeExtension.AddAttribute(xmlRoot);
        }

        protected override void OnFieldNameChanged(CodeTypeMemberExtension memberExtension, string oldName, string newName)
        {
            OnFieldOrPropertyNameChanged(memberExtension, oldName, newName);

			// Make sure the field name change is reflected in the field name references.
			ConvertFieldReferencesInConstructors(memberExtension.Parent.Constructors, oldName, newName);
        }

        protected override void OnPropertyNameChanged(CodeTypeMemberExtension memberExtension, string oldName, string newName)
        {
            OnFieldOrPropertyNameChanged(memberExtension, oldName, newName);
            CodeMemberProperty property = (CodeMemberProperty)memberExtension.ExtendedObject;

            // Look up for the data binding statement and change the property name there.
            foreach (CodeStatement statement in property.SetStatements)
            {
                CodeExpressionStatement expStatement = statement as CodeExpressionStatement;                
                // Continue if the statement is not a CodeExpressionStatement.
                if (expStatement == null)
                {
                    continue;
                }
                
                CodeMethodInvokeExpression miExp = expStatement.Expression as CodeMethodInvokeExpression;
                // Continue if the statement is not a CodeMethodInvokeExpression.
                if (miExp == null)
                {
                    continue;
                }

                // Modify the property name in parameters.
                foreach (CodeExpression pExp in miExp.Parameters)
                {
                    CodePrimitiveExpression priExp = pExp as CodePrimitiveExpression;

                    // Continue if the statment is not a CodePrimitiveExpression.
                    if (priExp == null)
                    {
                        continue;    
                    }
                    
                    if (priExp.Value.ToString() == oldName)
                    {
                        priExp.Value = newName;
                    }
                }
            }
        }

        protected override void OnEventNameChanged(CodeTypeMemberExtension memberExtension, string oldName, string newName)
        {
            // NOP
        }

        protected override void OnMethodNameChanged(CodeTypeMemberExtension memberExtension, string oldName, string newName)
        {
            // NOP
        }

        protected override void OnEnumMemberChanged(CodeTypeMemberExtension memberExtension, string oldName, string newName)
        {
			// Fix references found in DefaultValue attributes.
        	foreach (CodeTypeExtension type in Code.DataContracts)
        	{
        		foreach (CodeTypeMemberExtension member in type.Fields)
        		{
        			CodeAttributeDeclaration attribute = member.FindAttribute("System.ComponentModel.DefaultValueAttribute");
					if (attribute == null) continue;

        			CodeAttributeArgument argument = attribute.Arguments[0];
        			CodeFieldReferenceExpression argumentValue = argument.Value as CodeFieldReferenceExpression;
        			if (argumentValue == null) continue;

        			string baseTypeName = ((CodeTypeReferenceExpression)argumentValue.TargetObject).Type.BaseType;
        			string nameOfTypeInAttribute = PascalCaseConverterHelper.GetPascalCaseName(baseTypeName);
        			string nameOfTypeBeingChanged = memberExtension.Parent.ExtendedObject.Name;

        			if (argumentValue.FieldName == oldName && nameOfTypeInAttribute == nameOfTypeBeingChanged)
        			{
        				argumentValue.FieldName = newName;
        			}
        		}

                // Fix references found in constructor where default values are set.
                // This is required for fixed references to enum values.
                // e.g. <xs:attribute ref="xlink:type" fixed="simple"/>
                foreach (CodeTypeMemberExtension ctorExtension in type.Constructors)
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
                            // Do we have a field reference on the right side of the assignment statement?
                            CodeFieldReferenceExpression fieldRef = assignStatement.Right as CodeFieldReferenceExpression;
                            if (fieldRef != null)
                            {
                                // Does the referenced field belong to a type reference?
                                if (typeof(CodeTypeReferenceExpression) == fieldRef.TargetObject.GetType())
                                {
                                    string baseTypeName = ((CodeTypeReferenceExpression)fieldRef.TargetObject).Type.BaseType;
                                    string nameOfTypeForField = PascalCaseConverterHelper.GetPascalCaseName(baseTypeName);
                                    string nameOfTypeBeingChanged = memberExtension.Parent.ExtendedObject.Name;

                                    // Change the field name if it's changed.
                                    if (fieldRef.FieldName == oldName && nameOfTypeForField == nameOfTypeBeingChanged)
                                    {
                                        // Fix the field name first.
                                        fieldRef.FieldName = newName;

                                        // Also fix the name in the type reference.
                                        ((CodeTypeReferenceExpression)fieldRef.TargetObject).Type.BaseType = nameOfTypeForField;
                                    }
                                }
                            }
                        }
                    }
                }
        	}

            // Before adding the XmlEnumAttribute attribute to the CodeTypeMember
            // we have to make sure that the following attributes are not present.
			// If the 'XmlEnumAttribute' is already present the original value was not a valid name
			// and there should be no attempt to perform a rename.
            if (memberExtension.FindAttribute("System.Xml.Serialization.XmlAttributeAttribute") != null ||
                memberExtension.FindAttribute("System.Xml.Serialization.XmlAnyElementAttribute") != null ||
                memberExtension.FindAttribute("System.Xml.Serialization.XmlAnyAttributeAttribute") != null ||
				memberExtension.FindAttribute("System.Xml.Serialization.XmlEnumAttribute") != null)
            {
                // We cannot proceed.
                return;
            }
            // Create a CodeAttributeDeclaration for XmlEnumAttribute attribute and 
            // add it to the attributes collection.
            CodeAttributeDeclaration xmlEnum = new CodeAttributeDeclaration
                ("System.Xml.Serialization.XmlEnumAttribute");
            xmlEnum.Arguments.Add(new CodeAttributeArgumentExtended("Name",
                new CodePrimitiveExpression(oldName), true));

            // Finally add it to the custom attributes collection.
            memberExtension.AddAttribute(xmlEnum);
        }

        #endregion

        #region Private Methods

        private void OnFieldOrPropertyNameChanged(CodeTypeMemberExtension memberExtension, string oldName, string newName)
        {
            // Here we basically have two cases. Array and non-array.
            // If it's an non-array type, we have to decorate it with either
            // XmlAttributeAttribute attribute or XmlElementAttribute attribute.
            // If it's an array type we have to decorate it with XmlArrayAttribute
            // attribute.

            // There is one quickie we can try before anything nevertheless.
            // Regardless of whether the member type is an array type or not
            // member can already have an XmlElementAttribute attribute. 
            // If this is the case we can simply add the XML type name information to that.
            CodeAttributeDeclaration xmlElementAttribute = memberExtension.FindAttribute("System.Xml.Serialization.XmlElementAttribute");
            if (xmlElementAttribute != null)
            {
                // Create a new CodeAttributeDeclaration with required arguments
                xmlElementAttribute = new CodeAttributeDeclaration("System.Xml.Serialization.XmlElementAttribute",
                    new CodeAttributeArgumentExtended(
                    "ElementName", new CodePrimitiveExpression(oldName), true));

                // Add the newly created attribute to CodeTypeMember.
                memberExtension.AddAttribute(xmlElementAttribute);
                // No need to proceed, so simply return.
                return;
            }

            // Let's first handle the non-array case.
            // And then handl the array case.
            if (!PascalCaseConverterHelper.IsArray(memberExtension))
            {
                // See if we can spot the XmlAttributeAttribute attribute.
                CodeAttributeDeclaration xmlAttribute = memberExtension.FindAttribute("System.Xml.Serialization.XmlAttributeAttribute");
                // If we could, then let's add the AttributeName argument to it.
                if (xmlAttribute != null)
                {
                    // Create a new CodeAttributeDeclaration with required arguments.
                    CodeAttributeDeclaration xmlAttributeAttribute =
                        new CodeAttributeDeclaration("System.Xml.Serialization.XmlAttributeAttribute",
                        new CodeAttributeArgumentExtended(
                        "AttributeName", new CodePrimitiveExpression(oldName), true));

                    // Add the newly created attribute to CodeTypeMember.
                    memberExtension.AddAttribute(xmlAttributeAttribute);
                }
                else
                {
                    // We arrive here if we could not spot the XmlAttributeAttribute attribute.
                    // Therefore we can add the XmlElementAttribute attribute.
                    // However, before we proceed we have to check whether any of the following attributes
                    // already exists.
                    if (memberExtension.FindAttribute("System.Xml.Serialization.XmlTextAttribute") != null ||
                        memberExtension.FindAttribute("System.Xml.Serialization.XmlIgnoreAttribute") != null ||
                        memberExtension.FindAttribute("System.Xml.Serialization.XmlAnyElementAttribute") != null ||
                        memberExtension.FindAttribute("System.Xml.Serialization.XmlAnyAttributeAttribute") != null)
                    {
                        // We cannot add XmlElementAttribute attribute here.
                        return;
                    }

                    // Create a new CodeAttributeDeclaration with required arguments
                    xmlElementAttribute = new CodeAttributeDeclaration("System.Xml.Serialization.XmlElementAttribute",
                        new CodeAttributeArgumentExtended(
                        "ElementName", new CodePrimitiveExpression(oldName), true));

                    // Add the newly created attribute to CodeTypeMember.
                    memberExtension.AddAttribute(xmlElementAttribute);
                }
            }
            else
            {
                // We arrive here if we have an array type.
                // We can proceed to adding XmlArrayAttribue attribute if following attributes are 
                // not present.
                if (memberExtension.FindAttribute("System.Xml.Serialization.XmlTextAttribute") != null ||
                    memberExtension.FindAttribute("System.Xml.Serialization.XmlAnyElementAttribute") != null ||
                    memberExtension.FindAttribute("System.Xml.Serialization.XmlAnyAttributeAttribute") != null)
                {
                    // We cannot add XmlElementAttribute attribute here.
                    return;
                }

                // Create a new CodeAttributeDeclaration for XmlArrayAttribute with required arguments.
                CodeAttributeDeclaration xmlArrayAttribute =
                    new CodeAttributeDeclaration("System.Xml.Serialization.XmlArrayAttribute",
                    new CodeAttributeArgumentExtended(
                    "ElementName", new CodePrimitiveExpression(oldName), true));

                // Add the newly created CodeAttributeDeclaration to the attributes collection of 
                // CodeTypeMemeber.
                memberExtension.AddAttribute(xmlArrayAttribute);
            }
        }

        #endregion
    }
}
