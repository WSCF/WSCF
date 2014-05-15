using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.ServiceModel.Description;
using System.Xml.Serialization;

using Microsoft.CSharp;
using Microsoft.VisualBasic;

namespace Thinktecture.Tools.Web.Services.CodeGeneration
{
    /// <summary>
    /// This class works as the main interface between the client and the code generation API.
    /// </summary>
    public sealed class CodeGenerator
    {
        #region Private fields

        private InternalCodeGenerationOptions codeGenerationOptions;
        private CodeDomProvider codeProvider;

        #endregion

        #region Public methods

        /// <summary>
        /// Executes the code generation workflow.
        /// </summary>        
        public CodeWriterOutput GenerateCode(CodeGenerationOptions options)
        {
            // Step 1 - Parse the code generation options and create the code provider.
            codeGenerationOptions = CodeGenerationOptionsParser.ParseCodeGenerationOptions(options);

			CreateCodeProvider();

        	ExtendedCodeDomTree extendedCodeDomTree;

			if (options.GenerateDataContracts)
			{
				// Step 2 - Build the set of XML schemas.
				XmlSchemas schemas = MetadataFactory.GetXmlSchemas(codeGenerationOptions.MetadataResolverOptions);
                
				// Step 3 - Generate the data contract code and get the CodeNamespace.
				DataContractGenerator dataContractGenerator = new DataContractGenerator(schemas, codeGenerationOptions.PrimaryOptions, codeProvider);
				CodeNamespace codeNamespace = dataContractGenerator.GenerateCode();

				// Step 4 - Wrap the CodeDOM in the custom object model.
				extendedCodeDomTree = new ExtendedCodeDomTree(codeNamespace, codeGenerationOptions.WriterOptions.Language, null);
			}
			else
			{
				// Step 2 - Build the service metadata.
				MetadataSet metadataSet = MetadataFactory.GetMetadataSet(codeGenerationOptions.MetadataResolverOptions);

				// Step 3 - Generate the client/service code and get the CodeNamespace.
				ClientServiceGenerator clientServiceGenerator = new ClientServiceGenerator(metadataSet, codeGenerationOptions.PrimaryOptions, codeProvider);
				CodeNamespace codeNamespace = clientServiceGenerator.GenerateCode();

				// Step 4 - Wrap the CodeDOM in the custom object model.
				extendedCodeDomTree = new ExtendedCodeDomTree(codeNamespace, codeGenerationOptions.WriterOptions.Language, clientServiceGenerator.Configuration);
			}

            // Step 5 - Apply the code decorations.
            CodeDecorators decorators = new CodeDecorators();
            decorators.ApplyDecorations(extendedCodeDomTree, codeGenerationOptions.CustomOptions);

            // Step 6 - Restore the original CodeDOM.
        	CodeNamespace cns = extendedCodeDomTree.UnwrapCodeDomTree();
				
            // Step 6 - Finally, write out the code to physical files.
            return CodeWriter.Write(cns, extendedCodeDomTree.Configuration, codeGenerationOptions.WriterOptions, extendedCodeDomTree.TextFiles, codeProvider);            
        }

        public bool TryMoveDownloadedContract(string dstDirectory)
        {
            throw new Exception("This method is not implemented");
        }

        #endregion

        #region Private methods

        private void CreateCodeProvider()
        {
            if (codeGenerationOptions.AllOptions.Language == CodeLanguage.CSharp)
            {
                codeProvider = new CSharpCodeProvider();
            }
            else if (codeGenerationOptions.AllOptions.Language == CodeLanguage.VisualBasic)
            {
                codeProvider = new VBCodeProvider();
            }
        }

        #endregion
    }
}
