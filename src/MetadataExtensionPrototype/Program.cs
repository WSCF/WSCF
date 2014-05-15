using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Configuration;
using System.Reflection;
using System.IO;

namespace MetadataExtensionPrototype
{
    [ServiceContract]
    interface IFoo
    {
        [OperationContract]
        void DoSomething();
    }

    class Foo : IFoo
    {
        #region IFoo Members

        public void DoSomething()
        {            
        }

        #endregion
    }

    class Program
    {
        static void Main(string[] args)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(Path.Combine(Environment.CurrentDirectory, Assembly.GetExecutingAssembly().GetName().Name + ".exe"));

            ServiceHost serviceHost = new ServiceHost(typeof(Foo));
            serviceHost.Open();
            Console.WriteLine("Running....");
            Console.ReadKey();
        }
    }
}
