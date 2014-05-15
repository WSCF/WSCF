using System;

namespace Thinktecture.Tools.Web.Services.Wscf.Environment
{
	/// <summary>
	/// Used to enforce that arguments are valid.
	/// </summary>
	public class Enforce
	{
		/// <summary>
		/// Enforces that the specified argument is not <c>null</c>.
		/// </summary>
		/// <typeparam name="T">The type of the argument.</typeparam>
		/// <param name="instance">The instance.</param>
		/// <param name="name">The name of the argument.</param>
		/// <returns>The argument if not <c>null</c>.</returns>
		/// <exception cref="ArgumentNullException">Thrown if the argument is <c>null.</c></exception>
		public static T IsNotNull<T>(T instance, string name) where T : class
		{
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentException("name");
			}
            if (instance == null)
            {
            	throw new ArgumentNullException(name);
            }
			return instance;
		}
	}
}