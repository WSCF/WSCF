using System;
using System.Collections.Generic;
using System.Text;
using System.CodeDom;

namespace Thinktecture.Tools.Web.Services.CodeGeneration
{
    internal sealed class ClientTypeConverter : PascalCaseConverterBase
    {
        #region Contructors

        public ClientTypeConverter(CodeTypeExtension typeExtension, ExtendedCodeDomTree code)
            : base(typeExtension, code)
        {
        }

        #endregion

        #region Protected Method Overrides

        protected override bool CanConvertTypeName(CodeTypeExtension typeExtension)
        {
            return false;
        }

        protected override bool CanConvertMember(CodeTypeMemberExtension memberExtension)
        {
            if (memberExtension.Kind == CodeTypeMemberKind.Method)
            {
                return ((memberExtension.ExtendedObject.Attributes & MemberAttributes.Public) == MemberAttributes.Public);
            }
            return false;
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
