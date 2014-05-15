using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.CodeDom;

namespace Thinktecture.Tools.Web.Services.CodeGeneration
{
    internal sealed class MessageContractConverter : PascalCaseConverterBase
    {
        #region Constructors

		public MessageContractConverter(CodeTypeExtension typeExtension, ExtendedCodeDomTree code)
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
            // We can only convert the fields.
            if (memberExtension.Kind == CodeTypeMemberKind.Field)
            {
                if (memberExtension.FindAttribute("System.ServiceModel.MessageBodyMemberAttribute") != null)
                {
                    return true;
                }
                if (memberExtension.FindAttribute("System.ServiceModel.MessageHeaderAttribute") != null)
                {
                    return true;
                }
            }
            return false;
        }

        protected override void OnTypeNameChanged(CodeTypeExtension typeExtension, string oldName, string newName)
        {
            // Prepare the MessageContractAttribute attribute to specify the type name on the wire.
            CodeAttributeDeclaration messageContractAttribute = typeExtension.FindAttribute("System.ServiceModel.MessageContractAttribute");
            if (messageContractAttribute != null)
            {
                CodeAttributeArgument wrapperNameArgument = messageContractAttribute.Arguments
                    .OfType<CodeAttributeArgument>()
                    .FirstOrDefault(arg => arg.Name.Equals("WrapperName", StringComparison.OrdinalIgnoreCase));

                if (wrapperNameArgument == null)
                    return;

                CodePrimitiveExpression wrapperNameValue = wrapperNameArgument.Value as CodePrimitiveExpression;
                if (wrapperNameValue != null && !string.IsNullOrEmpty((string)wrapperNameValue.Value))
                {
                    string newWrapperNameValue = PascalCaseConverterHelper.GetPascalCaseName((string)wrapperNameValue.Value);
                    wrapperNameArgument.Value = new CodePrimitiveExpression(newWrapperNameValue);
                }
            }
            else
            {
                CodeAttributeDeclaration xmlType =
                    new CodeAttributeDeclaration("System.ServiceModel.MessageContractAttribute",
                    new CodeAttributeArgumentExtended("WrapperName",
                    new CodePrimitiveExpression(oldName), true));

                // Add the XmlTypeAttribute attribute to ctd.
                typeExtension.AddAttribute(xmlType);
            }
        }

        protected override void OnFieldNameChanged(CodeTypeMemberExtension memberExtension, string oldName, string newName)
        {
            // Field can be either a body member or a message header.
            // First see if the field is a body member.
            CodeAttributeDeclaration bodyMember = memberExtension.FindAttribute("System.ServiceModel.MessageBodyMemberAttribute");
            // If this is a body member, modify the MessageBodyMemberAttribute to include the wire name.
            if (bodyMember != null)
            {                
                CodeAttributeDeclaration newBodyMember =
                new CodeAttributeDeclaration("System.ServiceModel.MessageBodyMemberAttribute",
                new CodeAttributeArgumentExtended("Name",
                new CodePrimitiveExpression(oldName), true));

                memberExtension.AddAttribute(newBodyMember);                
            }

            // Now check whether the field is a message header.
            CodeAttributeDeclaration header = memberExtension.FindAttribute("System.ServiceModel.MessageHeaderAttribute");
            if (header != null)
            {
                CodeAttributeDeclaration newHeader =
                new CodeAttributeDeclaration("System.ServiceModel.MessageHeaderAttribute",
                new CodeAttributeArgumentExtended("Name",
                new CodePrimitiveExpression(oldName), true));

                memberExtension.AddAttribute(newHeader);                
            }  
            
            // Make sure the field name change is reflected in the field name references.
            ConvertFieldReferencesInConstructors(memberExtension.Parent.Constructors, oldName, newName);
        }

        protected override void OnPropertyNameChanged(CodeTypeMemberExtension memberExtension, string oldName, string newName)
        {
            // NOP
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
            // NOP
        }

        #endregion

        #region Private Methods

    	#endregion
    }
}
