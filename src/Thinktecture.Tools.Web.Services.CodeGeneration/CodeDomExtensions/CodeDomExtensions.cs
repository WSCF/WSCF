using System;
using System.CodeDom;
using System.Linq;

namespace Thinktecture.Tools.Web.Services.CodeGeneration
{
	/// <summary>
	///		Extension methods to the CodeDom classes.
	/// </summary>
	public static class CodeDomExtensions
	{
		/// <summary>
		/// Finds an argument in an attribute declaration.
		/// </summary>
		/// <param name="attributeDeclaration">The attribute declaration.</param>
		/// <param name="argumentName">Name of the argument.</param>
		/// <returns>The argument if found; otherwise, <c>null</c>.</returns>
		/// <remarks>The argument name is case-sensitive.</remarks>
		public static CodeAttributeArgument FindArgument(this CodeAttributeDeclaration attributeDeclaration, string argumentName)
		{
			return attributeDeclaration.Arguments
				.OfType<CodeAttributeArgument>()
				.FirstOrDefault(a => a.Name == argumentName);
		}

		/// <summary>
		/// Tests if the <see cref="CodeTypeReference"/> represents a <see cref="Nullable"/> type.
		/// </summary>
		/// <param name="reference">The code type reference.</param>
		/// <returns><c>true</c> if the type is a <see cref="Nullable"/>; otherwise, <c>false</c>.</returns>
		public static bool IsNullableType(this CodeTypeReference reference)
		{
			return Type.GetType(reference.BaseType) == typeof(Nullable<>);
		}

		/// <summary>
		/// Creates a closed generic nullable type from the current <see cref="CodeTypeReference"/>.
		/// </summary>
		/// <param name="reference">The code type reference.</param>
		/// <returns>A <see cref="Type"/> representation of the nullable type.</returns>
		/// <exception cref="InvalidOperationException">Thrown if the code type reference does not represent a nullable type.</exception>
		public static Type CloseNullableType(this CodeTypeReference reference)
		{
			if (!reference.IsNullableType())
			{
				throw new InvalidOperationException("The current code type reference is not a nullable type.");
			}
			Type valueType = Type.GetType(reference.TypeArguments[0].BaseType);
			return typeof(Nullable<>).MakeGenericType(valueType);
		}
	}
}