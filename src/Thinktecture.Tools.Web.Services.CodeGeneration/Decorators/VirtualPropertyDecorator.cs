using System.CodeDom;
using System.Linq;

namespace Thinktecture.Tools.Web.Services.CodeGeneration.Decorators
{
	/// <summary>
	/// Makes the properties on data contract types virtual.
	/// </summary>
	public class VirtualPropertyDecorator : ICodeDecorator
	{
		/// <summary>
		/// Applies the decorator to the extended CodeDom tree.
		/// </summary>
		/// <param name="code">The extended CodeDom tree.</param>
		/// <param name="options">The custom code generation options.</param>
		public void Decorate(ExtendedCodeDomTree code, CustomCodeGenerationOptions options)
		{
			if (!options.VirtualProperties) return;

			foreach (CodeTypeMemberExtension memberExtension in code.DataContracts.SelectMany(dataContract => dataContract.Properties))
			{
				CodeMemberProperty property = memberExtension.ExtendedObject as CodeMemberProperty;
				if (property != null)
				{
					property.Attributes = MemberAttributes.Public;
				}
			}
		}
	}
}