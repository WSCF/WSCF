using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System.Linq;

namespace Thinktecture.Tools.Web.Services.ContractFirst.VsObjectWrappers
{
	/// <summary>
	/// Writes message to a 'WSCF.blue' pane in the Output window.
	/// </summary>
	internal class OutputWindowWriter
	{
		private readonly OutputWindowPane outputWindowPane;

		/// <summary>
		/// Initializes a new instance of the <see cref="OutputWindowWriter"/> class.
		/// </summary>
		/// <param name="applicationObject">The application object.</param>
		public OutputWindowWriter(_DTE applicationObject)
		{
            ThreadHelper.ThrowIfNotOnUIThread();
            Window window = applicationObject.Windows.Item(Constants.vsWindowKindOutput);
			OutputWindow outputWindow = (OutputWindow)window.Object;

            outputWindowPane = outputWindow.OutputWindowPanes
				.OfType<OutputWindowPane>()
				.Where(p =>
                {
                    ThreadHelper.ThrowIfNotOnUIThread();
                    return p.Name == "WSCF.blue";
                })
				.FirstOrDefault() ?? outputWindow.OutputWindowPanes.Add("WSCF.blue");

			outputWindowPane.Clear();
			outputWindowPane.Activate();
		}

		/// <summary>
		/// Clears the WSCF.blue pane in the Output window.
		/// </summary>
		public void Clear()
		{
            ThreadHelper.ThrowIfNotOnUIThread();
            outputWindowPane.Clear();
		}

		/// <summary>
		/// Writes a message to the 'WSCF.blue' pane.
		/// </summary>
		/// <param name="message">The message.</param>
		public void WriteMessage(string message)
		{
            ThreadHelper.ThrowIfNotOnUIThread();
            outputWindowPane.OutputString(message);
		}
	}
}