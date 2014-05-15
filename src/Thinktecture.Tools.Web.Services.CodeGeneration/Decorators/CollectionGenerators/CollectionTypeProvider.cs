using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.CodeDom;
using System.Diagnostics;

namespace Thinktecture.Tools.Web.Services.CodeGeneration
{
    internal class CollectionTypeProvider : ICollectionTypeProvider
    {
        #region ICollectionTypeProvider Members

        public CodeTypeReference CreateCollectionType(CodeTypeReference entityType, ExtendedCodeDomTree code)
        {
            Debug.Assert(entityType != null, "Argument entityType could not be null.");

			// Do not create a ByteCollection as this is not compatiable with base64Binary!
			if (entityType.BaseType == typeof(byte).FullName) return entityType;

            CodeTypeDeclaration collectionType = GenerateCollectionType(entityType);
            code.DataContracts.Add(new CodeTypeExtension(collectionType));

            CodeTypeReference collectionRef = new CodeTypeReference(collectionType.Name);
            return collectionRef;
        }

        #endregion

        #region Private Methods

        private static CodeTypeDeclaration GenerateCollectionType(CodeTypeReference entityType)
        {
			string entityTypeName = entityType.IsNullableType()
				? "Nullable" + Type.GetType(entityType.TypeArguments[0].BaseType).Name
				: entityType.BaseType;
            string collectionTypeName = GetCollectionTypeName(entityTypeName);
            CodeTypeDeclaration collectionType = new CodeTypeDeclaration(collectionTypeName);
            CodeTypeReference baseType = new CodeTypeReference(typeof(Collection<>));

			if (entityType.IsNullableType())
			{
				baseType.TypeArguments.Add(entityType.CloseNullableType());
			}
			else
			{
				baseType.TypeArguments.Add(entityType.BaseType);
			}

            collectionType.BaseTypes.Add(baseType);
            return collectionType;
        }

    	private static string GetCollectionTypeName(string entityTypeName)
        {
            // Disqualify the type name (e.g. Use String instead of System.String).
            string prefix = entityTypeName;
            int lastDot = entityTypeName.LastIndexOf('.');

            if (lastDot >= 0 && ++lastDot < entityTypeName.Length)
            {
                prefix = entityTypeName.Substring(lastDot);
            }

            return string.Format("{0}Collection", prefix);
        }

        #endregion
    }
}
