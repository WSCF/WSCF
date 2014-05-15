using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.CodeDom;
using System.Globalization;

namespace Thinktecture.Tools.Web.Services.CodeGeneration
{
    /// <summary>
    /// This class contains the helper methods used for Pascal 
    /// case conversion.
    /// </summary>
    internal static class PascalCaseConverterHelper
    {
        /// <summary>
        /// Converts a given name to Pascal case.
        /// </summary>
        public static string GetPascalCaseName(string name)
        {
            // Some validations to make debugging easier.
            Debug.Assert(name != null, "name parameter could not be null.");
            // Create a StringBuilder for building the new name.
            StringBuilder newNameBuilder = new StringBuilder();
            // Extract the first letter and change it to upper case.            
            newNameBuilder.Append(name.Substring(0, 1).ToUpper());

            // Do we have more than one letter in name?
            if (name.Length > 1)
            {
                // Then append rest of the letters to the new name being 
                // created.
                newNameBuilder.Append(name.Substring(1));
            }

            // Finally return the new name.
            return newNameBuilder.ToString();
        }

        /// <summary>
        /// Converts a given method name to Pascal case.
        /// </summary>
        public static string GetPascalCaseMethodName(string name)
        {
            // Some validations to make debugging easier.
            Debug.Assert(name != null, "name parameter could not be null.");

            string nameToConvert = name;
            string asyncPrefix = "";

            // Is this a method starting with "Begin"?
            if (name.StartsWith("Begin", false, CultureInfo.CurrentCulture))
            {                
                // Do we have more letters than word "Begin"?
                if (name.Length > 5)
                {
                    asyncPrefix = "Begin";
                    nameToConvert = name.Substring(5);
                }
            }
            // Or is this method starting with "End"?
            else if (name.StartsWith("End", false, CultureInfo.CurrentCulture))
            {                
                // Do we have more letters than word "End"?
                if (name.Length > 3)
                {
                    asyncPrefix = "End";
                    nameToConvert = name.Substring(3);
                }
            }

            // Create the new name by combining async prefix with the Pascal case converted name.
            string newName = string.Format("{0}{1}", asyncPrefix, GetPascalCaseName(nameToConvert));
            return newName;
        }

        /// <summary>
        /// Helper function to evaluvate whether a given property or field 
        /// member has an array type or not.
        /// </summary>
        public static bool IsArray(CodeTypeMemberExtension ext)
        {
            // Switch the CodeTypeMember kind.
            switch (ext.Kind)
            {
                case CodeTypeMemberKind.Field:
                    // This is a field.
                    // Access the internal extended object with a pointer of 
                    // CodeMemberField type.
                    CodeMemberField field = (CodeMemberField)ext.ExtendedObject;
                    // Return true if the filed type is an array type.
                    // Return false otherwise.
                    return field.Type.ArrayElementType != null;
                case CodeTypeMemberKind.Property:
                    // This is a property.
                    // Access the internal extended object with a pointer of 
                    // CodeMemberProperty type.
                    CodeMemberProperty property = (CodeMemberProperty)ext.ExtendedObject;
                    // Return true if the property type is an array type.
                    // Return false otherwise.
                    return property.Type.ArrayElementType != null;
                default:
                    // We don't support any other CodeTypeMember kind.
                    // Therefore return false.
                    return false;
            }
        }

        public static T GetTypedCodeExpression<T>(CodeExpression expression)
        {
            if (typeof(T) == expression.GetType())
            {
                return (T)(object)expression;
            }
            return default(T);
        }
    }
}
