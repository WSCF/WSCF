using System;
using System.Collections.Generic;
using System.Text;

namespace Thinktecture.Tools.Web.Services.CodeGeneration
{
    /// <summary>
    /// This class implements the code decorator for converting arrays to 
    /// typed lists.
    /// </summary>
    public class ArraysToListsConverter : ICodeDecorator
    {
        #region ICodeDecorator Members

        public void Decorate(ExtendedCodeDomTree code, CustomCodeGenerationOptions options)
        {
            if (options.GenerateTypedLists)
            {
                CollectionTypeGenerator generator = new CollectionTypeGenerator(new ListTypeProvider(), code);
                generator.Execute();
            }
        }

        #endregion
    }
}
