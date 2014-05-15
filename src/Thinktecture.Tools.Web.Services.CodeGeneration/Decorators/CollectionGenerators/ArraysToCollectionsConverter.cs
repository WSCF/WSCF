using System;
using System.Collections.Generic;
using System.Text;

namespace Thinktecture.Tools.Web.Services.CodeGeneration
{
    /// <summary>
    /// This class implements the code decorator for converting arrays to
    /// collections.
    /// </summary>
    internal class ArraysToCollectionsConverter : ICodeDecorator
    {
        #region ICodeDecorator Members

        public void Decorate(ExtendedCodeDomTree code, CustomCodeGenerationOptions options)
        {
            if (options.GenerateCollections)
            {
                CollectionTypeGenerator generator = new CollectionTypeGenerator(new CollectionTypeProvider(), code);
                generator.Execute();
            }
        }

        #endregion
    }
}
