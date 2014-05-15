using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;

namespace Thinktecture.ServiceModel.Extensions.Metadata
{
    [ServiceContract(Name="MetadataService", SessionMode=SessionMode.NotAllowed, Namespace="http://www.thinktecture.com/servicemodel/extensions/metadataservice")]
    internal interface IMetadataService
    {
        [OperationContract(Name="GetMetadata")]
        [WebGet(BodyStyle=WebMessageBodyStyle.Bare, RequestFormat=WebMessageFormat.Xml, ResponseFormat=WebMessageFormat.Xml, UriTemplate="*")]
        Message GetMetadata();
    }
}
