using System;
using System.IO
;
namespace Thinktecture.Tools.Web.Services.ContractFirst
{
	/// <summary>
	/// Summary description for LogHelper.
	/// </summary>
	public class LogHelper
	{
		public static void LogToFile(string filename, string message)
		{
			StreamWriter sw = new StreamWriter(filename + "_" + DateTime.Now.ToLongTimeString().Replace(":", ".") + ".log", true);
			sw.WriteLine(message);
			sw.Flush();
			sw.Close();
		}
	}
}
