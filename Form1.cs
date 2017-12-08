using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication2
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Write text to form
		/// </summary>
		public void ShowOnScreen(string text)
		{
			if (InvokeRequired)
			{
				// так блокировку не получим
				BeginInvoke(new Action<string>(ShowOnScreen), text);
				// если 2 потока одновременно сюда попадут, то блокировку получим
				//Invoke(new Action<string>(ShowOnScreen), line);
				return;
			}
			textBox1.Text += text;
		}

		/// <summary>
		/// Write line to Visual Studio Output Window
		/// </summary>
		public void ShowInOutput(string line)
		{
			Console.WriteLine(line);
			// Debug.WriteLine
		}

		/// <summary>
		/// Очистить
		/// </summary>
		private void btnClear_Click(object sender, EventArgs e)
		{
			textBox1.Text = "";
		}

		/// <summary>
		/// Потоки и переменные
		/// </summary>
		private void btnThread_Click(object sender, EventArgs e)
		{
			// CLR назначает каждому потоку свой стек, так что локальные переменные хранятся раздельно
			// а к static-данным и ref-данным, передаваемым в качестве параметров, у всех потоков одинаковый доступ, т.е.
			// 1 поток легко может напартачить другому

			var btn = sender as Button;
			int variant = btn == btnThread ? 0 : btn == btnThread1 ? 1 : btn == btnThread2 ? 2 : 3;
			btnThread.Enabled = btnThread1.Enabled = btnThread2.Enabled = btnThread3.Enabled = false;

			var obj = new Threads_and_Variables(this);
			obj.Do(variant);

			btnThread.Enabled = btnThread1.Enabled = btnThread2.Enabled = btnThread3.Enabled = true;
		}

		/// <summary>
		///  Ассинхронная работа через BackgroundWorker
		/// </summary>
		private void bgwRun_Click(object sender, EventArgs e)
		{
			var bw = BackGroundWorkerExample.BW;
			if (bw != null || (bw != null && bw.IsBusy)) return;

			BackGroundWorkerExample.UserCancel = false;
			bgwRun.Text = "Поехали";
			bgwStop.Text = "Остановить";
			ShowOnScreen(Utils.crlf + "На диске C: создаются файлики. Есть кнопка Стоп ..." +
			             Utils.crlf + "--------------" + Utils.crlf);

			int count;
			Int32.TryParse(txtBgw.Text, out count);
			if (count == 0) return;
			BackGroundWorkerExample.Do(bgwRun, bgwStop, count);
		}

		/// <summary>
		/// Остановить ассинхронную работу через BackgroundWorker
		/// </summary>
		private void bgwStop_Click(object sender, EventArgs e)
		{
			var bw = BackGroundWorkerExample.BW;
			if (bw != null && bw.IsBusy)
			{
				BackGroundWorkerExample.UserCancel = true;
				bw.CancelAsync();
			}
		}

		/// <summary>
		/// Связка Task и TaskCompletionSource
		/// </summary>
		private void btnTask_TCS_Click(object sender, EventArgs e)
		{
			var func = new Func<int, int, int, List<string>>(
				(count, start, step) =>
				{
					List<string> files = new List<string>();
					for (int i = start; i <= count; i += step)
					{
						string file = @"C:\" + i + ".txt";
						FileStream fs = File.OpenWrite(file);
						string str = "this is file number " + i;
						var bytes = UTF8Encoding.UTF8.GetBytes(str);
						fs.Write(bytes, 0, bytes.Length);
						fs.Close();
						files.Add(file);
						// иммитация работы
						Thread.Sleep(200);
					}
					return files;
				}
				);

			ShowOnScreen(Utils.crlf + "Немного повисим из-за WaitAll.  На диске C: создаются файлики ..." +
			             Utils.crlf + "--------------" + Utils.crlf);

			int countFiles = 10;
			// t делает нечётные файлики
			Task t = new Task(() => { func(countFiles, 1, 1); });
			t.Start();

			// tcs делает чётные файлики
			var tcs = new TaskCompletionSource<List<string>>();
			Task.Factory.StartNew(() =>
			{
				var files = func(countFiles, 2, 2);
				tcs.SetResult(files);
				// можно и Exception добавить
				//tcs.SetException(new Exception("Остановлено пользователем"));
			});
			Task.WaitAll(new Task[] {t, tcs.Task});

			ShowOnScreen("Task всё завершил" + Utils.crlf +
			             "TaskCompletionSource тоже завершил" + Utils.crlf +
			             "TaskCompletionSource.Exception - " +
			             (tcs.Task.Exception == null ? "пусто" : tcs.Task.Exception.ToString()) + Utils.crlf +
			             "TaskCompletionSource.Result: " + Utils.crlf + string.Join(", ", tcs.Task.Result) + " " +
			             Utils.crlf +
			             "--------------" +
			             Utils.crlf);
		}

		/// <summary>
		/// Обернули синхронный код в TaskCompletionSourcе и засунули в фоновый поток
		/// </summary>
		private void btnTCS_Click(object sender, EventArgs e)
		{
			ShowOnScreen(Utils.crlf + "Кнопки Стоп нет. По кнопке Проверить выдаётся инфа. На диске C: создаются файлики ..." +
			             Utils.crlf + "--------------" + Utils.crlf);

			btnChkTCS.Text = "Проверить";
			Utils.BtnDelegate progress = new Utils.BtnDelegate(TaskCompletionSourceExample.SetBtnProgress);

			var tcs = new TaskCompletionSource<List<string>>();
			TaskCompletionSourceExample.TaskRes = tcs.Task;
			Task.Factory.StartNew(
				() =>
				{
					var files = TaskCompletionSourceExample.Func2(10, 1, 1, btnChkTCS, progress);
					// или так
					//var files2 = Func2(10, 1, 1, button5, new BtnDelegate(this.SetBtnProgress));
					tcs.SetResult(files);
				});
		}

		/// <summary>
		/// Проверка - что там делает "обёрнутый" синхронный код
		/// </summary>
		private void btnChkTCS_Click(object sender, EventArgs e)
		{
			// Result и ContinueWith неявно вызывают метод Wait, т.е. при обращении к ним "зависнем" ожидая окончание выполнения задачи
			var taskTsc = TaskCompletionSourceExample.TaskRes;
			bool ended = taskTsc.IsCanceled || taskTsc.IsCompleted || taskTsc.IsFaulted;
			string mes = "IsCanceled - " + taskTsc.IsCanceled + Utils.crlf +
			             "IsCompleted - " + taskTsc.IsCompleted + Utils.crlf +
			             "IsFaulted (завершен с ошибкой) - " + taskTsc.IsFaulted + Utils.crlf +
			             "Result - " + Utils.crlf +
			             (ended ? string.Join(", ", taskTsc.Result) : "пока пусто");
			MessageBox.Show(mes);
		}

		/// <summary>
		/// async - await для сохранения последовательности выполнения команд
		/// </summary>
		private void btnAsyncStart1_Click(object sender, EventArgs e)
		{
			ShowOnScreen(
				$"{Utils.crlf} async-await без зависа основного потока {Utils.crlf} и не нужен Invoke/BeginInvoke для сохранения последовательности выполнения кода {Utils.crlf}{Utils.crlf}");
			(new AsyncAwait()).DoAsyncAwait(this, 2);
		}

		/// <summary>
		/// async - await - демонстрация, того что код действительно выполняется в другом потоке
		/// </summary>
		private void btnAsyncStart2_Click(object sender, EventArgs e)
		{
			ShowOnScreen(Utils.crlf + "Запуск async-метода. Данный код является внешним для async-метода. ThreadId - " +
			             Thread.CurrentThread.ManagedThreadId + Utils.crlf);
			(new AsyncAwait()).DoAsyncAwait(this, 2);
			ShowOnScreen("Возврат во внешний код. ThreadId - " + Thread.CurrentThread.ManagedThreadId + Utils.crlf);
		}

		/// <summary>
		/// async -await демонстрация блокировки
		/// </summary>
		private void btnAsyncLock_Click(object sender, EventArgs e)
		{
			string str = @"в строке await создаётся задача-обещание, которая будет выполнена через какое-то время
await передаёт управление во внешний код, в котором .Wait(5000)
т.е. основной поток замораживается на 5 сек (иначе зависнем навсегда) и ждёт окончания async-метода
после того как задача-обещание Delay() отработает, то выполнение перейдёт к коду после await в основном потоке
но основной поток заморожен => блокировка
чтобы её избежать выполнение кода после await нужно продолжить не в основном потоке
а в потоке Task'а, указанного в await (.ConfigureAwait(false))
тогда async-метод нормально завершится и передаст результат в основной поток
который как раз и ждёт этого, после чего основной поток пойдёт дальше";
			ShowOnScreen(Utils.crlf + str + Utils.crlf + Utils.crlf);

			var t = (new AsyncAwait()).MethodAsync();
			// завис на 5 сек
			t.Wait(5000);
			// int res = MethodAsync().Result; // тоже завис. т.к. Result неявно вызывает Wait

			ShowOnScreen("с Wait в вызывающем потоке для таска из await нужно быть аккуратнее :)");
		}

		/// <summary>
		/// lock, Monitor.Enter/Exit, Monitor.Wait/Pulse = ManualResetEvent.WaiteOne+Reset/Set = AutoResetEvent.WaiteOne/Set
		/// </summary>
		private void btnLocksRun_Click(object sender, EventArgs e)
		{
			Locks.Do(cmbLocks.SelectedIndex, ShowOnScreen, ShowInOutput);
		}
	}
}