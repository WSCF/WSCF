using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Thinktecture.ServiceModel.Extensions.Metadata.Tests
{
    /// <summary>
    /// Summary description for UriComparisonTests
    /// </summary>
    [TestClass]
    public class UriComparisonTests
    {
        public UriComparisonTests()
        {            
        }

        private TestContext testContextInstance;

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

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void CompareTwoAbsoluteUris()
        {
            Uri uriA = new Uri("http://www.thinktecture.com/samples/sss/sss.xxx");
            Uri uriB = new Uri("http://www.thinktecture.com/samples");
            
            int lastSegmentIndex = uriB.Segments.Length - 1;
            uriB.Segments[lastSegmentIndex] = uriB.Segments[lastSegmentIndex].Trim('/');
            UriBuilder uriBuilder = new UriBuilder();
            
            int uriDifference = Uri.Compare(uriA, uriB, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.CurrentCultureIgnoreCase);
            int expectedValue = 0;

            Assert.AreEqual(expectedValue, uriDifference, string.Format("Expected value is {0}. However the returned value is {1}", expectedValue, uriDifference));
        }

        [TestMethod]
        public void MakeRelativeTest()
        {
            Uri uriA = new Uri("http://www.thinktecture.com/samples/sss/sss.xxx");
            Uri uriB = new Uri("http://www.thinktecture.com/samples");

            Uri uriC = uriB.MakeRelativeUri(uriA);
            Assert.AreEqual("samples/sss/sss.xxx", uriC.OriginalString);
        }
    }
}
