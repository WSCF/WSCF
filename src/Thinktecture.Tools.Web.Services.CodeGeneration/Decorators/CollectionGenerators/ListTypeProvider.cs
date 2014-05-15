using System;
using System.Collections.Generic;
using System.Text;
using System.CodeDom;
using System.Diagnostics;

namespace Thinktecture.Tools.Web.Services.CodeGeneration
{
    internal class ListTypeProvider : ICollectionTypeProvider
    {
        #region ICollectionTypeProvider Members

        public CodeTypeReference CreateCollectionType(CodeTypeReference entityType, ExtendedCodeDomTree code)
        {
            Debug.Assert(entityType != null, "Argument entity type could not be null.");

			// Do not create a ByteCollection as this is not compatiable with base64Binary!
			if (entityType.BaseType == typeof(byte).FullName) return entityType;

            CodeTypeReference ctr = new CodeTypeReference(typeof(List<>));

			if (entityType.IsNullableType())
			{
				ctr.TypeArguments.Add(entityType.CloseNullableType());
			}
			else
			{
				ctr.TypeArguments.Add(entityType.BaseType);
			}
            return ctr;
        }

        #endregion
    }
}
