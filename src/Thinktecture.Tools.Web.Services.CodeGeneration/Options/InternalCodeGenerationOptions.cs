using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Thinktecture.Tools.Web.Services.CodeGeneration
{
    /// <summary>
    /// This class abstracts the parsed code generation option types.
    /// </summary>
    [DebuggerStepThrough]
    internal class InternalCodeGenerationOptions
    {
        #region Private fields

        private MetadataResolverOptions metadataResolverOptions;
        private PrimaryCodeGenerationOptions primaryOptions;
        private CustomCodeGenerationOptions customOptions;
        private CodeWriterOptions writerOptions;
        private CodeGenerationOptions allOptions;

        #endregion

        #region Constructors

        public InternalCodeGenerationOptions(MetadataResolverOptions metadataResolverOptions, PrimaryCodeGenerationOptions primaryOptions, CustomCodeGenerationOptions customOptions, CodeWriterOptions writerOptions, CodeGenerationOptions allOptions)
        {
            this.metadataResolverOptions = metadataResolverOptions;
            this.primaryOptions = primaryOptions;
            this.customOptions = customOptions;
            this.writerOptions = writerOptions;
            this.allOptions = allOptions;
        }

        #endregion

        #region Public properties

        public MetadataResolverOptions MetadataResolverOptions
        {
            get { return metadataResolverOptions; }
        }

        /// <summary>
        /// Gets the parsed PrimaryCodeGenerationOptions.
        /// </summary>
        public PrimaryCodeGenerationOptions PrimaryOptions
        {
            get { return primaryOptions; }
        }

        /// <summary>
        /// Gets the parsed CustomCodeGenerationOptions.
        /// </summary>
        public CustomCodeGenerationOptions CustomOptions
        {
            get { return customOptions; }
        }

        /// <summary>
        /// Gets the parsed CodeWriterOptions.
        /// </summary>
        public CodeWriterOptions WriterOptions
        {
            get { return writerOptions; }
        }

        public CodeGenerationOptions AllOptions
        {
            get { return allOptions; }
        }

        #endregion
    }
}
