using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace WindowsFormsApplication2
{
	public static class BackGroundWorkerExample
	{
		/// <summary>
		/// Собственно BackgroundWorker
		/// </summary>
		public static BackgroundWorker BW;

		/// <summary>
		/// Пользователь нажал стоп
		/// </summary>
		public static bool UserCancel;

		private static Button m_btnRun;
		private static Button m_btnStop;

		/// <summary>
		/// </summary>
		public static void Do(Button btnRun, Button btnStop, int count)
		{
			m_btnRun = btnRun;
			m_btnStop = btnStop;

			BW = new BackgroundWorker {WorkerReportsProgress = true, WorkerSupportsCancellation = true};
			BW.DoWork += Bw_DoWork;
			BW.ProgressChanged += Bw_ProgressChanged;
			BW.RunWorkerCompleted += Bw_RunWorkerCompleted;
			BW.RunWorkerAsync(count);
		}


		private static void Bw_DoWork(object sender, DoWorkEventArgs e)
		{
			int count = (int) e.Argument;
			for (int i = 1; i <= count; i++)
			{
				if (BW.CancellationPending) return;

				// выводим результат в %
				int res = i*100/count;
				BW.ReportProgress(res);
				string file = @"C:\" + i + ".txt";
				e.Result = file;

				FileStream fs = File.OpenWrite(file);
				// имитация работы
				Thread.Sleep(400);
				string str = "this is file number " + i;
				var bytes = UTF8Encoding.UTF8.GetBytes(str);
				fs.Write(bytes, 0, bytes.Length);
				// имитация работы
				Thread.Sleep(400);
				fs.Close();
				// имитация работы
				Thread.Sleep(400);
			}
		}

		private static void Bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			m_btnRun.Text = "Поехали " + e.ProgressPercentage + " %";
		}

		private static void Bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			m_btnRun.Text = "Поехали";
			m_btnStop.Text = UserCancel ? "Прервано пользователем" : e.Cancelled ? "Завершено с ошибкой" : "Завершено успешно";
			m_btnStop.FindForm().Text = e.Result.ToString();

			BW.Dispose();
			BW = null;
		}
	}
}