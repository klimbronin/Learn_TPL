using System;
using System.Windows.Forms;

namespace WindowsFormsApplication2
{
	/// <summary>
	/// </summary>
	public static class Utils
	{
		/// <summary>
		/// </summary>
		public static string crlf = Environment.NewLine;

		/// <summary>
		/// </summary>
		public delegate void BtnDelegate(Button btn, string text, int step);

		/// <summary>
		/// </summary>
		public delegate void DelegateOutToScreen(string line);

		/// <summary>
		/// </summary>
		public delegate void DelegateOutToConsole(string line);
	}
}