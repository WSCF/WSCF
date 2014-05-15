using Thinktecture.ServiceModel.Extensions.Metadata;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Thinktecture.ServiceModel.Extensions.Metadata.Tests
{
    /// <summary>
    ///This is a test class for StaticMetadataBehaviorTest and is intended
    ///to contain all StaticMetadataBehaviorTest Unit Tests
    ///</summary>
    [TestClass()]
    public class StaticMetadataBehaviorTest
    {
        #region Private Members

        private TestContext testContextInstance;

        #endregion

        #region Public Properties

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #endregion

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

        #region Test Methods

        /// <summary>
        /// Tests whether GetMetadataServiceUri returns the expected value when
        /// metadataUrl is an absolute url and sits on the same base address as
        /// the hostBaseAddress.
        /// </summary>
        [TestMethod()]
        [DeploymentItem("Thinktecture.ServiceModel.Extensions.Metadata.dll")]
        public void GetMetadataServiceUriForAbsoluteMetadataUriAtSameBaseUriTest()
        {
            string metadataUrl = "http://www.thinktecture.com/samples/restaurantservice/wsdl";
            StaticMetadataBehavior_Accessor target = new StaticMetadataBehavior_Accessor(metadataUrl, "c:\\host\\contracts\\restaurantservice.wsdl");
            Uri hostBaseAddress = new Uri("http://www.thinktecture.com/samples/restaurantservice");
            Uri expected = new Uri(metadataUrl);
            Uri actual;
            actual = target.GetMetadataServiceUri(hostBaseAddress);
            Assert.AreEqual(expected, actual);            
        }

        /// <summary>
        /// Tests whether GetMetadataServiceUri returns the expected value when
        /// metadataUrl is an absolute url and sits on a different base address to
        /// the hostBaseAddress.
        /// </summary>
        [TestMethod()]
        [DeploymentItem("Thinktecture.ServiceModel.Extensions.Metadata.dll")]
        public void GetMetadataServiceUriForAbsoluteMetadataUriAtDifferentBaseUriTest()
        {
            string metadataUrl = "http://schemas.thinktecture.com/samples/restaurantservice/wsdl";
            StaticMetadataBehavior_Accessor target = new StaticMetadataBehavior_Accessor(metadataUrl, "c:\\host\\contracts\\restaurantservice.wsdl");
            Uri hostBaseAddress = new Uri("http://www.thinktecture.com/samples/restaurantservice");
            Uri expected = new Uri(metadataUrl);
            Uri actual;
            actual = target.GetMetadataServiceUri(hostBaseAddress);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Tests whether GetMetadataServiceUri returns the expected value when
        /// metadataUrl is a relative url.
        /// </summary>       
        [TestMethod()]
        [DeploymentItem("Thinktecture.ServiceModel.Extensions.Metadata.dll")]
        public void GetMetadataServiceUriForRelativeMetadataUriTest()
        {
            string metadataUrl = "metadata\\wsdl";
            StaticMetadataBehavior_Accessor target = new StaticMetadataBehavior_Accessor(metadataUrl, "c:\\host\\contracts\\restaurantservice.wsdl");
            Uri hostBaseAddress = new Uri("http://www.thinktecture.com/samples/restaurantservice");
            Uri expected = null;
            Uri.TryCreate(hostBaseAddress, metadataUrl, out expected);
            Uri actual;
            actual = target.GetMetadataServiceUri(hostBaseAddress);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Tests whether GetMetadataServiceUri throws the expected exception when
        /// metadataUrl is an absolute uri whose scheme is different to http.
        /// </summary>       
        [TestMethod()]
        [DeploymentItem("Thinktecture.ServiceModel.Extensions.Metadata.dll")]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetMetadataServiceUriForAbsoluteNonHttpMetadataUriTest()
        {
            string metadataUrl = "ftp://ftp.thinktecture.com/samples/schemas/restaurantservice/wsdl";
            StaticMetadataBehavior_Accessor target = new StaticMetadataBehavior_Accessor(metadataUrl, "c:\\host\\contracts\\restaurantservice.wsdl");
            Uri hostBaseAddress = new Uri("http://www.thinktecture.com/samples/restaurantservice");            
            Uri actual = target.GetMetadataServiceUri(hostBaseAddress);            
        }

        /// <summary>
        /// Tests whether GetMetadataServiceUri throws the expected exception when
        /// metadataUrl relative but formatted incorrectly.
        /// </summary>       
        [TestMethod()]
        [DeploymentItem("Thinktecture.ServiceModel.Extensions.Metadata.dll")]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetMetadataServiceUriForMalFormattedRelativeMetadataUriTest()
        {
            string metadataUrl = null;
            StaticMetadataBehavior_Accessor target = new StaticMetadataBehavior_Accessor(metadataUrl, "c:\\host\\contracts\\restaurantservice.wsdl");
            Uri hostBaseAddress = new Uri("http://www.thinktecture.com/samples/restaurantservice");            
            Uri actual = target.GetMetadataServiceUri(hostBaseAddress);            
        }

        /// <summary>
        /// Tests whether GetMetadataServiceUri throws the expected exception when
        /// metadataUrl is an relative and there is no hostbase address specified.
        /// </summary>       
        [TestMethod()]
        [DeploymentItem("Thinktecture.ServiceModel.Extensions.Metadata.dll")]
        [ExpectedException(typeof(InvalidOperationException))]        
        public void GetMetadataServiceUriForRelativeMetadataUriWhenHostBaseAddressIsNullTest()
        {
            string metadataUrl = "restaurantservice/wsdl";
            StaticMetadataBehavior_Accessor target = new StaticMetadataBehavior_Accessor(metadataUrl, "c:\\host\\contracts\\restaurantservice.wsdl");
            Uri hostBaseAddress = null;            
            Uri actual = target.GetMetadataServiceUri(hostBaseAddress);            
        }

        #endregion
    }
}
