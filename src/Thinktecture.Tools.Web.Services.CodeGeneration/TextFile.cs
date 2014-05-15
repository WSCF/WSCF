namespace Thinktecture.Tools.Web.Services.CodeGeneration
{
	/// <summary>
	/// A text file that is included with the code output.
	/// </summary>
    public class TextFile
    {
		/// <summary>
		/// Initializes a new instance of the <see cref="TextFile"/> class.
		/// </summary>
		/// <param name="filename">The filename.</param>
		/// <param name="content">The content.</param>
    	public TextFile(string filename, string content)
        {
            Filename = filename;
            Content = content;
        }

		/// <summary>
		/// Gets the filename.
		/// </summary>
    	public string Filename { get; private set; }

		/// <summary>
		/// Gets the content of the file.
		/// </summary>
    	public string Content { get; private set; }
    }
}
