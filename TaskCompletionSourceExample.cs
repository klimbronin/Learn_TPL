using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication2
{
	/// <summary>
	/// Для демонстрации возможностей TaskCompletionSource
	/// </summary>
	public static class TaskCompletionSourceExample
	{
		/// <summary>
		/// Результат задачи
		/// </summary>
		public static Task<List<string>> TaskRes;

		/// <summary>
		/// </summary>
		public static List<string> Func2(int count, int start, int step, 
			Button btnOutText, Utils.BtnDelegate progress)
		{
			List<string> files = new List<string>();
			for (int i = start; i <= count; i += step)
			{
				string file = @"C:\" + i + ".txt";
				FileStream fs = File.OpenWrite(file);
				// имитация работы
				Thread.Sleep(300);
				string str = "this is file number " + i;
				var bytes = UTF8Encoding.UTF8.GetBytes(str);
				fs.Write(bytes, 0, bytes.Length);
				// имитация работы
				Thread.Sleep(300);
				fs.Close();
				// имитация работы
				Thread.Sleep(300);
				files.Add(file);
				// имитация работы
				Thread.Sleep(300);

				if (progress != null)
				{
					var form = btnOutText.FindForm();
					form.BeginInvoke(progress, new object[] {btnOutText, "Проверить", i});
					// или так
					//form.BeginInvoke(new Utils.BtnDelegate(SetBtnProgress), new object[] {i});
				}
			}
			return files;
		}

		/// <summary>
		/// Отобразить прогресс выполнения задачи
		/// </summary>
		public static void SetBtnProgress(Button btn, string prefix, int x)
		{
			btn.Text = prefix + ". " + x;
		}
	}
}