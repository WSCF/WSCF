using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel;

namespace Thinktecture.Tools.Web.Services.ServiceDescription
{
    [ServiceContract(Name = Constants.InternalContractName)]
    internal interface IDummyContract
    {
        [OperationContract]
        void DummyOperation();
    }
}
