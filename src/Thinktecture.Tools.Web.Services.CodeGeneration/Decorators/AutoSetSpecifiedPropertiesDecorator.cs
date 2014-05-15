using System.CodeDom;
using System.Linq;

namespace Thinktecture.Tools.Web.Services.CodeGeneration.Decorators
{
    /// <summary>
    /// Automatically set the ___Specified property to true when setter on matching property is called.
    /// </summary>
    public class AutoSetSpecifiedPropertiesDecorator : ICodeDecorator
    {
        /// <summary>
        /// Applies the decorator to the extended CodeDom tree.
        /// </summary>
        /// <param name="code">The extended CodeDom tree.</param>
        /// <param name="options">The custom code generation options.</param>
        public void Decorate(ExtendedCodeDomTree code, CustomCodeGenerationOptions options)
        {
            foreach (CodeTypeExtension dataContract in code.DataContracts)
            {
                foreach (CodeTypeMemberExtension memberExtension in dataContract.Properties)
                {
                    CodeMemberProperty property = memberExtension.ExtendedObject as CodeMemberProperty;
                    if (property == null) continue;

                    CodeMemberProperty specifiedProperty = FindMatchingSpecifiedProperty(dataContract, property.Name);

                    if (specifiedProperty == null) continue;

                    // Change the Statements of set part of the property; add a statement to set "this.____Specified = true;"
                    CodeStatement specifiedPropertySetTrueStatement = new CodeAssignStatement(
                        new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), specifiedProperty.Name),
                        new CodePrimitiveExpression(true));
                    property.SetStatements.Add(specifiedPropertySetTrueStatement);
                }
            }
        }

        private static CodeMemberProperty FindMatchingSpecifiedProperty(CodeTypeExtension type, string propertyName)
        {
            return (from member in type.Properties 
                    where member.ExtendedObject.Name == (propertyName + "Specified") 
                    select (CodeMemberProperty)member.ExtendedObject).FirstOrDefault();
        }
    }
}
