using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication2
{
	/// <summary>
	/// Обратная связь через BeginInvoke и через SyncronizationContext
	/// </summary>
	public partial class Form1
	{
		/// <summary>
		/// Обратная связь через SyncronizationContext
		/// </summary>
		private void btnSyncCont_Click(object sender, EventArgs e)
		{
			ShowOnScreen(Utils.crlf + "Кнопки Стоп нет, надо ждать ..." +
			             Utils.crlf + "--------------" + Utils.crlf);

			btnSyncCont.Text = "Поехали";
			SynchronizationContext sync = SynchronizationContext.Current;
			Task.Factory.StartNew(
				() =>
				{
					for (int i = 0; i <= 5; i++)
					{
						string file = @"C:\" + i + ".txt";
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

						sync.Post(delegate { btnSyncCont.Text = "Поехали " + i; }, null);
					}
				}).
				ContinueWith(
					(res) => { sync.Post(delegate { btnSyncCont.Text = "Поехали. Завершено"; }, null); });
		}

		private AsyncResult asyncRes;

		private DelegateFuncForInvoke caller;

		private delegate List<string> DelegateFuncForInvoke(Button btn, Utils.BtnDelegate progress);

		private void btnInvStart_Click(object sender, EventArgs e)
		{
			asyncRes = null;
			btnInvStop.Text = "Завершить";
			ShowOnScreen(Utils.crlf + "Есть кнопка Стоп. По кнопке Проверить выдаётся инфа. На диске C: создаются файлики ..." +
			             Utils.crlf + "--------------" + Utils.crlf);
			caller = new DelegateFuncForInvoke(FuncForInvoke);

			asyncRes = (AsyncResult) caller.BeginInvoke(btnInvChk, new Utils.BtnDelegate(SetBtnProgress), Callback, null);
			// а вот если вызвать так, то выполняется видимо синхронно в основном потоке и приложение зависает
			//asyncRes = BeginInvoke(caller, new object[] { 10, 1, 1, button8, new BtnDelegate(SetBtnProgress) });
		}

		private void SetBtnProgress(Button btn, string prefix, int x)
		{
			btn.Text = prefix + ". " + x;
		}

		private List<string> FuncForInvoke(Button btnOutText, Utils.BtnDelegate progress)
		{
			List<string> files = new List<string>();
			for (int i = 1; i <= 10; i++)
			{
				if (asyncRes.EndInvokeCalled) return files;

				string file = @"C:\" + i + ".txt";
				FileStream fs = File.OpenWrite(file);
				Thread.Sleep(300);
				string str = "this is file number " + i;
				var bytes = UTF8Encoding.UTF8.GetBytes(str);
				fs.Write(bytes, 0, bytes.Length);
				// иммитация работы
				Thread.Sleep(300);
				fs.Close();
				// иммитация работы
				Thread.Sleep(300);
				files.Add(file);
				// иммитация работы
				Thread.Sleep(300);

				if (progress != null)
				{
					BeginInvoke(progress, new object[] {btnOutText, "Проверить", i});
					// или так
					//BeginInvoke(new BtnDelegate(SetBtnProgress), new object[] {i});
				}

				if (asyncRes.EndInvokeCalled) return files;
			}
			return files;
		}

		private void Callback(IAsyncResult ar)
		{
			if (InvokeRequired)
			{
				BeginInvoke(new Action<IAsyncResult>(Callback), new[] {ar});
				return;
			}

			btnInvChk.Text = "Проверить что делается";
			btnInvStop.Text = "Завершено";

			caller = null;
		}

		private void btnInvChk_Click(object sender, EventArgs e)
		{
			if (asyncRes == null) return;
			MessageBox.Show("EndInvokeCalled - " + asyncRes.EndInvokeCalled + Utils.crlf + "Завершено - " +
			                asyncRes.IsCompleted);
		}

		private void btnInvStop_Click(object sender, EventArgs e)
		{
			// вместо того, чтобы объявить дополнительный флаг и обработки этого флага в FuncForInvoke
			// используем EndInvokeCalled
			asyncRes.EndInvokeCalled = true;

			// если вызвать EndInvoke, то "зависнем" ожидая окончание выполнения задачи
			// List<string> files = caller.EndInvoke(asyncRes);
		}
	}
}