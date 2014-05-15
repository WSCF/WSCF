using System.Collections.Generic;
using Thinktecture.Tools.Web.Services.CodeGeneration.Decorators;

namespace Thinktecture.Tools.Web.Services.CodeGeneration
{
    /// <summary>
    /// This class abstracts several ICodeDecorators and provides the necessary
    /// interface to manipulate them.
    /// </summary>
    internal sealed class CodeDecorators
    {
        #region Private fields

        // THINK: May be we should fallback to ICodeDecorator[] instead of List<T> 
        // if List does not add too much value in this case ;).
        private List<ICodeDecorator> decorators;

        #endregion

        #region Constructors

        public CodeDecorators()
        {
            Initialize();
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Initializes the list of decorators.
        /// </summary>
        private void Initialize()
        {
            // Decorators are executed in the order they are 
            // added to the list here.
            decorators = new List<ICodeDecorator>();
            decorators.Add(new PascalCaseConverter());
            decorators.Add(new ArraysToCollectionsConverter());
            decorators.Add(new ArraysToListsConverter());            
            decorators.Add(new ServiceTypeGenerator());
            decorators.Add(new ConfigurationDecorator());
            decorators.Add(new SvcFileGenerator());
        	decorators.Add(new ActionDecorator());
            decorators.Add(new VirtualPropertyDecorator());
            decorators.Add(new AutoSetSpecifiedPropertiesDecorator());
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Invokes all ICodeDecorator(s) in the decorators collection.
        /// </summary>
        public void ApplyDecorations(ExtendedCodeDomTree code, CustomCodeGenerationOptions options)
        {
            foreach (ICodeDecorator decorator in decorators)
            {
                decorator.Decorate(code, options);
            }
        }

        #endregion
    }
}