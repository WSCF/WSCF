using System;
using System.Collections.Generic;
using System.Text;
using System.CodeDom;
using System.Diagnostics;

namespace Thinktecture.Tools.Web.Services.CodeGeneration
{
    /// <summary>
    /// This class implements the code decorator for converting type/member names from
    /// Camel case to Pascal case.
    /// </summary>
    internal class PascalCaseConverter : ICodeDecorator
    {
        // Reference to the GeneratedCode instance.
        ExtendedCodeDomTree code;
        // Reference to the options.
        CustomCodeGenerationOptions options;

        #region ICodeDecorator Members

        public void Decorate(ExtendedCodeDomTree code, CustomCodeGenerationOptions options)
        {
            // Notify if we get any null references.
            Debug.Assert(code != null, "code parameter could not be null.");
            Debug.Assert(options != null, "options parameter could not be null.");

            // We apply this decorator only if this option is turned on.
            if (options.AdjustCasing)
            {
                // Initialize the state.
                this.code = code;
                this.options = options;
                DecorateInternal();
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// This function passes the relevent FilteredTypes collections to the 
        /// DecorateCore function.
        /// </summary>
        private void DecorateInternal()
        {
            // Send the data contracts.
            DecorateCore(code.DataContracts);
            // Send the message contracts.
            DecorateCore(code.MessageContracts);
            // Send the service contracts.
            DecorateCore(code.ServiceContracts);
            // Send the service types
            DecorateCore(code.ServiceTypes);
            // Send the client types.
            DecorateCore(code.ClientTypes);
        }

        /// <summary>
        /// Contains the core logic of pascal case conversion. 
        /// </summary>        
        private void DecorateCore(FilteredTypes types)
        {
            // Perform this action for all extensions (ext) in the data contracts list.
            foreach (CodeTypeExtension typeExtension in types)
            {
                // Get the converter for this type.
                PascalCaseConverterBase converter = PascalCaseConverterFactory.GetPascalCaseConverter(typeExtension, code);
                // Execute the converter.
                string oldName;
                string newName = converter.Convert(out oldName);
                UpdateTypeReferences(oldName, newName);
            }
        }

        /// <summary>
        /// Itterates through the generated code and reflects the type name changes.
        /// </summary>
        private void UpdateTypeReferences(string oldName, string newName)
        {
            UpdateTypeReferencesInternal(code.DataContracts, oldName, newName);
            UpdateTypeReferencesInternal(code.MessageContracts, oldName, newName);
            UpdateTypeReferencesInternal(code.ServiceContracts, oldName, newName);
            UpdateTypeReferencesInternal(code.ServiceTypes, oldName, newName);
            UpdateTypeReferencesInternal(code.ClientTypes, oldName, newName);
            UpdateTypeReferencesInternal(code.UnfilteredTypes, oldName, newName);
        }

        private void UpdateTypeReferencesInternal(FilteredTypes types, string oldName, string newName)
        {
            foreach (CodeTypeExtension ext in types)
            {
                CodeRefactoringAgent updater = new CodeRefactoringAgent();
                updater.Refactor(ext, oldName, newName);
            }
        }

        #endregion       
    }
}
