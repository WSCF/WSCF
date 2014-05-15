using System;
using System.Collections.Generic;
using System.Text;

namespace Thinktecture.Tools.Web.Services.CodeGeneration
{
    internal sealed class ServiceContractConverter : PascalCaseConverterBase
    {
        #region Constructors

		public ServiceContractConverter(CodeTypeExtension typeExtension, ExtendedCodeDomTree code)
            : base(typeExtension, code)
        {
        }

        #endregion

        #region Protected Method Overrides

        protected override bool CanConvertTypeName(CodeTypeExtension typeExtension)
        {
            return true;
        }

        protected override bool CanConvertMember(CodeTypeMemberExtension memberExtension)
        {
            return (memberExtension.Kind == CodeTypeMemberKind.Method);
        }

        protected override void OnTypeNameChanged(CodeTypeExtension typeExtension, string oldName, string newName)
        {
            // NOP
        }

        protected override void OnFieldNameChanged(CodeTypeMemberExtension memberExtension, string oldName, string newName)
        {
            // NOP
        }

        protected override void OnPropertyNameChanged(CodeTypeMemberExtension memberExtension, string oldName, string newName)
        {
            // NOP
        }

        protected override void OnEventNameChanged(CodeTypeMemberExtension memberExtension, string oldName, string newName)
        {
            // NOP
        }

        protected override void OnMethodNameChanged(CodeTypeMemberExtension memberExtension, string oldName, string newName)
        {
            // NOP
        }

        protected override void OnEnumMemberChanged(CodeTypeMemberExtension memberExtension, string oldName, string newName)
        {
            // NOP
        }

        #endregion
    }
}
