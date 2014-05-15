using System;
using System.Collections.Generic;
using System.Text;

namespace Thinktecture.Tools.Web.Services.CodeGeneration
{
    internal class PascalCaseConverterFactory
    {
		public static PascalCaseConverterBase GetPascalCaseConverter(CodeTypeExtension typeExtension, ExtendedCodeDomTree code)
        {
            switch (typeExtension.Kind)
            {
                case CodeTypeKind.DataContract:
                    return new DataContractConverter(typeExtension, code);
                case CodeTypeKind.MessageContract:
					return new MessageContractConverter(typeExtension, code);
                case CodeTypeKind.ServiceContract:
					return new ServiceContractConverter(typeExtension, code);
                case CodeTypeKind.ClientType:
					return new ClientTypeConverter(typeExtension, code);
                default:
                    return null;
            }
        }
    }
}