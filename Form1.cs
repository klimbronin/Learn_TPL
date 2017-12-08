using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.IO;
using System.Reflection.Emit;
using System.Runtime.Remoting.Messaging;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication2
{
	public delegate void DelegateOutToScreen(string line);

	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		public static string crlf = Environment.NewLine;

		private void Form1_Load(object sender, EventArgs e)
		{
			cmbLocks.SelectedIndex = 0;

			if (Program.TestMutex)
			{
				foreach (var control in this.Controls)
				{
					var pds = TypeDescriptor.GetProperties(control).Find("Enabled", false);
					pds?.SetValue(control, false);
				}

				ShowOnScreen("Режим тестирования Mutex. Это второй процесс" + crlf);

				Task.Factory.StartNew(
					() =>
					{
						ShowOnScreen("Ждём освобождения KlimMutex, в первом процессе можно нажать Release Mutex. Это второй процесс" + crlf);
						Mutex m;
						if (Mutex.TryOpenExisting("KlimMutex", MutexRights.FullControl, out m))
						{
							m.WaitOne();
							ShowOnScreen("Дождались освобождения KlimMutex. Это второй процесс" + crlf);
							this.TopMost = true;
						}
						else
						{
							ShowOnScreen("Не найден KlimMutex, длеть нечего :). Это второй процесс" + crlf);
						}

					});
			}
		}

		private void btnClear_Click(object sender, EventArgs e)
		{
			textBox1.Text = "";
		}

		public void ShowOnScreen(string line)
		{
			if (InvokeRequired)
			{
				BeginInvoke(new Action<string>(ShowOnScreen), line);	// так блокировку не получим
				//Invoke(new Action<string>(ShowOnScreen), line);		// если 2 потока одновременно сюда попадут, то блокировку получим
				return;
			}
			textBox1.Text += line;
		}

		/// <summary>
		/// Write line to Visual Studio Output Window
		/// </summary>
		public void ShowInOutput(string line)
		{
			Console.WriteLine(line);
			// можно и через Debug.WriteLine так, как, например, в asp net не работает Console.WriteLine
		}

		#region  Потоки и переменные

		private class ClassVar
		{
			public int Count;
		}

		private void btnThread1_Click(object sender, EventArgs e)
		{
			btnThread.Tag = 1;
			btnThread_Click(null, null);
		}

		private void btnThread2_Click(object sender, EventArgs e)
		{
			btnThread.Tag = 2;
			btnThread_Click(null, null);
		}

		private void btnThread3_Click(object sender, EventArgs e)
		{
			btnThread.Tag = 3;
			btnThread_Click(null, null);
		}

		private void btnThread_Click(object sender, EventArgs e)
		{
			// CLR назначает каждому потоку свой стек, так что локальные переменные хранятся раздельно
			// а к static-данным и ref-данным, передаваемым в качестве параметров, у всех потоков одинаковый доступ, т.е.
			// 1 поток легко может напартачить другому

			int variant = btnThread.Tag is int ? (int) btnThread.Tag : 0;
			btnThread.Enabled = btnThread1.Enabled = btnThread2.Enabled = btnThread3.Enabled = false;

			Task t2;
			int count;
			string str;
			string res = null;

			if (variant == 0 || variant == 1)
			{
				count = 6;
				str = crlf + "2 потока выводят числа с 1 до N = 6, N передаётся в метод как параметр, N-- и вывод на кран." +
						 crlf + "Потоки не влияют друг на друга" + crlf;
				ShowOnScreen(str);
				ShowInOutput(str);

				// фоновый поток
				// вариант1
				t2 = Task.Factory.StartNew(() => { res = Go(count); });
				// вариант2
				//var tcs = new TaskCompletionSource<string>();
				//Task.Factory.StartNew(() => { tcs.SetResult(Go(count)); });

				// главный поток
				ShowOnScreen(Go(count) + crlf);
				// а если фоновый поток вывести так, то тупо зависаем на ShowOnScreen или Task.WaitAll, не помогает даже lock(locker) в ShowOnScreen
				// хотя судя по выводу в консоль (окно output) фоновый поток всё сделал и в консоли вывел, а вот форма зависает
				//var back = Task.Factory.StartNew(() => { ShowOnScreen(Go(count)+crlf); });
				//Task.WaitAll(new[] { back });

				// вывод результата фонового потока
				// вариант 1
				Task.WaitAll(new[] {t2});
				ShowOnScreen(res + crlf);
				// вариант 2
				//ShowOnScreen(tcs.Task.Result + crlf);

				str = "--------------" + crlf;
				ShowOnScreen(str);
				ShowInOutput(str);
			}

			if (variant == 0 || variant == 2)
			{
				str = crlf +
				      "2 потока выводят числа с 1 до N = 6, N передаётся в метод по ссылке, N-- и вывод на кран." + crlf +
				      "Потоки работают с одной и тоже N и влияют друг на друга" + crlf +
				      "Выводятся 12345 / 1234 или 1234 / 12345 и даже 1234 / 1234 - как повезёт" + crlf;
				ShowOnScreen(str);
				ShowInOutput(str);
				count = 6;
				t2 = Task.Factory.StartNew(() => { res = GoRef(ref count) + crlf; });
				ShowOnScreen(GoRef(ref count) + crlf);
				Task.WaitAll(new[] {t2});
				ShowOnScreen(res);
				str = "--------------" + crlf;
				ShowOnScreen(str);
				ShowInOutput(str);
			}
			if (variant == 0 || variant == 3)
			{
				str = crlf +
				      "2 потока выводят числа с 1 до ClassVar.Count, ClassVar передаётся в метод как параметр, ClassVar.Count-- и вывод на кран." +
				      crlf + "Потоки работаю с одним и тем же экземпляром ClassVar и вляяют друг на друга с соответствующими последствиями" + crlf +
				      "Выводятся 12345 / 1234 или 1234 / 12345 и даже 1234 / 1234 - как повезёт" + crlf;
				ShowOnScreen(str);
				ShowInOutput(str);
				ClassVar classVar = new ClassVar {Count = 6};
				t2 = Task.Factory.StartNew(() => { res = GoClassParam(classVar) + crlf; });
				ShowOnScreen(GoClassParam(classVar) + crlf);
				Task.WaitAll(new[] {t2});
				ShowOnScreen(res);
				str = "--------------" + crlf;
				ShowOnScreen(str);
				ShowInOutput(str);
			}

			btnThread.Tag = 0;
			btnThread.Enabled = btnThread1.Enabled = btnThread2.Enabled = btnThread3.Enabled = true;
		}

		private string Go(int count)
		{
			string str = "";
			count--;
			for (int i = 1; i <= count; i++)
			{
				str += i.ToString();
			}
			ShowInOutput(str);
			return str;
		}

		private string GoRef(ref int count)
		{
			count--;
			string str = "";
			for (int i = 1; i <= count; i++)
			{
				str = str + i;
			}
			ShowInOutput(str);
			return str;
		}

		private string GoClassParam(ClassVar arg)
		{
			arg.Count--;
			string str = "";
			for (int i = 1; i <= arg.Count; i++)
			{
				str = str + i;
			}
			ShowInOutput(str);
			return str;
		}

		#endregion  Потоки и переменные		

		#region Ассинхронная работа через BackgroundWorker

		private BackgroundWorker bw;
		private bool m_userCanceled;

		private void bgwRun_Click(object sender, EventArgs e)
		{
			if (bw != null || (bw != null && bw.IsBusy)) return;

			m_userCanceled = false;
			bgwRun.Text = "Поехали";
			bgwStop.Text = "Остановить";

			int count;
			Int32.TryParse(txtBgw.Text, out count);
			if (count == 0) return;

			bw = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = true };
			bw.DoWork += Bw_DoWork;
			bw.ProgressChanged += Bw_ProgressChanged;
			bw.RunWorkerCompleted += Bw_RunWorkerCompleted;
			bw.RunWorkerAsync(count);
		}

		private void Bw_DoWork(object sender, DoWorkEventArgs e)
		{
			int count = (int)e.Argument;
			for (int i = 1; i <= count; i++)
			{
				if (bw.CancellationPending) return;

				// выводим результат в %
				int res = i * 100 / count;
				bw.ReportProgress(res);
				string file = @"C:\" + i + ".txt";
				e.Result = file;

				FileStream fs = File.OpenWrite(file);
				Thread.Sleep(1000);
				string str = "this is file number " + i;
				var bytes = UTF8Encoding.UTF8.GetBytes(str);
				fs.Write(bytes, 0, bytes.Length);
				Thread.Sleep(1000);
				fs.Close();
				Thread.Sleep(1000);
			}
		}

		private void Bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			bgwRun.Text = "Поехали " + e.ProgressPercentage + " %";
		}

		private void Bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			bgwRun.Text = "Поехали";
			bgwStop.Text = m_userCanceled ? "Прервано пользователем" : e.Cancelled ? "Завершено с ошибкой" : "Завершено успешно";
			Text = e.Result.ToString();

			bw.Dispose();
			bw = null;
		}

		private void bgwStop_Click(object sender, EventArgs e)
		{
			if (bw != null && bw.IsBusy)
			{
				m_userCanceled = true;
				bw.CancelAsync();
			}
		}

		#endregion Ассинхронная работа через BackgroundWorker

		#region Связка Task и TaskCompletionSource

		private void btmTask_TCS_Click(object sender, EventArgs e)
		{
			int count = 10;

			// t делает нечётные файлики
			Task t = new Task(() => { Func(count, 1, 1); });
			t.Start();

			// tcs делает чётные файлики
			var tcs = new TaskCompletionSource<List<string>>();
			Task.Factory.StartNew(() =>
			{
				var files = Func(count, 2, 2);
				tcs.SetResult(files);
				// можно и Exception добавить
				//tcs.SetException(new Exception("Остановлено пользователем"));
			});
			Task.WaitAll(new Task[] { t, tcs.Task });

			string mes = "Task всё завершил" + crlf +
						 "TaskCompletionSource тоже завершил" + crlf +
						 "TaskCompletionSource.Exception - " +
						 (tcs.Task.Exception == null ? "пусто" : tcs.Task.Exception.ToString()) + crlf +
						 "TaskCompletionSource.Result " + crlf + string.Join(", ", tcs.Task.Result);
			MessageBox.Show(mes);
		}

		private List<string> Func(int count, int start, int step)
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
				Thread.Sleep(1000);
			}
			return files;
		}

		#endregion Связка Task и TaskCompletionSource

		#region Обернули синхронный код в TaskCompletionSourcе и засунули в фоновый поток

		private Task<List<string>> taskTsc;
		delegate void BtnDelegate(Button btn, string text, int step);

		private void btnTCS_Click(object sender, EventArgs e)
		{
			btnChkTCS.Text = "Проверить";
			BtnDelegate progress = new BtnDelegate(this.SetBtnProgress);

			var tcs = new TaskCompletionSource<List<string>>();
			taskTsc = tcs.Task;
			Task.Factory.StartNew(
				() =>
				{
					var files = Func2(10, 1, 1, btnChkTCS, progress);
					// или так
					//var files2 = Func2(10, 1, 1, button5, new BtnDelegate(this.SetBtnProgress));
					tcs.SetResult(files);
				});
		}

		private List<string> Func2(int count, int start, int step, Button btnOutText, BtnDelegate progress)
		{
			List<string> files = new List<string>();
			for (int i = start; i <= count; i += step)
			{
				string file = @"C:\" + i + ".txt";
				FileStream fs = File.OpenWrite(file);
				Thread.Sleep(1000);
				string str = "this is file number " + i;
				var bytes = UTF8Encoding.UTF8.GetBytes(str);
				fs.Write(bytes, 0, bytes.Length);
				Thread.Sleep(1000);
				fs.Close();
				Thread.Sleep(1000);
				files.Add(file);
				Thread.Sleep(1000);

				if (progress != null)
				{
					BeginInvoke(progress, new object[] { btnOutText, "Проверить", i });
					// или так
					//BeginInvoke(new BtnDelegate(SetBtnProgress), new object[] {i});
				}
			}
			return files;
		}

		private void SetBtnProgress(Button btn, string prefix, int x)
		{
			btn.Text = prefix + ". " + x;
		}

		private void btnChkTCS_Click(object sender, EventArgs e)
		{
			// Result и ContinueWith неявно вызывают метод Wait, т.е. при обращении к ним "зависнем" ожидая окончание выполнения задачи
			bool ended = taskTsc.IsCanceled || taskTsc.IsCompleted || taskTsc.IsFaulted;
			string mes = "IsCanceled - " + taskTsc.IsCanceled + crlf +
						 "IsCompleted - " + taskTsc.IsCompleted + crlf +
						 "IsFaulted (завершен с ошибкой) - " + taskTsc.IsFaulted + crlf +
						 "Result - " + crlf +
						 (ended ? string.Join(", ", taskTsc.Result) : "пока пусто");
			MessageBox.Show(mes);
		}

		#endregion Обернули синхронный код в TaskCompletionSourcе и засунули в фоновый поток

		#region SyncronizationContext

		private SynchronizationContext sync;

		private void btnSyncCont_Click(object sender, EventArgs e)
		{
			btnSyncCont.Text = "Поехали";
			sync = SynchronizationContext.Current;
			Task.Factory.StartNew(
				() =>
				{
					for (int i = 0; i <= 5; i++)
					{
						string file = @"C:\" + i + ".txt";
						FileStream fs = File.OpenWrite(file);
						Thread.Sleep(1000);
						string str = "this is file number " + i;
						var bytes = UTF8Encoding.UTF8.GetBytes(str);
						fs.Write(bytes, 0, bytes.Length);
						Thread.Sleep(1000);
						fs.Close();
						Thread.Sleep(1000);

						sync.Post(delegate { btnSyncCont.Text = "Поехали " + i; }, null);
					}
				}).
				ContinueWith(
					(res) =>
					{
						sync.Post(delegate { btnSyncCont.Text = "Поехали. Завершено"; }, null);
					});
		}

		#endregion SyncronizationContext

		#region BeginInvoke - EndInvoke

		private AsyncResult asyncRes;
		private DelegateFuncForInvoke caller;
		delegate List<string> DelegateFuncForInvoke(Button btn, BtnDelegate progress);

		private void btnInvStart_Click(object sender, EventArgs e)
		{
			asyncRes = null;
			btnInvStop.Text = "Завершить"; // пока не работает
			caller = new DelegateFuncForInvoke(FuncForInvoke);

			asyncRes = (AsyncResult)caller.BeginInvoke(btnInvChk, new BtnDelegate(SetBtnProgress), Callback, null);
			// а вот если вызвать так, то выполняется видимо синхронно в основном потоке и приложение зависает
			//asyncRes = BeginInvoke(caller, new object[] { 10, 1, 1, button8, new BtnDelegate(SetBtnProgress) });
		}

		private List<string> FuncForInvoke(Button btnOutText, BtnDelegate progress)
		{
			List<string> files = new List<string>();
			for (int i = 1; i <= 10; i++)
			{
				if (asyncRes.EndInvokeCalled) return files;

				string file = @"C:\" + i + ".txt";
				FileStream fs = File.OpenWrite(file);
				Thread.Sleep(1000);
				string str = "this is file number " + i;
				var bytes = UTF8Encoding.UTF8.GetBytes(str);
				fs.Write(bytes, 0, bytes.Length);
				Thread.Sleep(1000);
				fs.Close();
				Thread.Sleep(1000);
				files.Add(file);
				Thread.Sleep(1000);

				if (progress != null)
				{
					BeginInvoke(progress, new object[] { btnOutText, "Проверить", i });
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
				BeginInvoke(new Action<IAsyncResult>(Callback), new[] { ar });
				return;
			}

			btnInvChk.Text = "Проверить что делается";
			btnInvStop.Text = "Завершено";

			caller = null;

			// ar == asyncRes возвращает true
		}

		private void btnInvChk_Click(object sender, EventArgs e)
		{
			if (asyncRes == null) return;
			MessageBox.Show("EndInvokeCalled - " + asyncRes.EndInvokeCalled + crlf + "Завершено - " + asyncRes.IsCompleted);
		}

		private void btnInvStop_Click(object sender, EventArgs e)
		{
			// вместо того, чтобы объявить дополнительный флаг и обработки этого флага в FuncForInvoke
			// используем EndInvokeCalled, допустимо это или нет - хз
			asyncRes.EndInvokeCalled = true;

			// если вызвать EndInvoke, то произодёт аналог Task.Wait, т.е. "зависнем" ожидая окончание выполнения задачи
			// List<string> files = caller.EndInvoke(asyncRes);
		}

		#endregion BeginInvoke - EndInvoke

		#region async - await

		private void btnAsyncStart1_Click(object sender, EventArgs e)
		{
			ShowOnScreen($"{crlf} async-await без зависа основного потока {crlf} и не нужен Invoke/BeginInvoke для сохранения последовательности выполнения кода {crlf}{crlf}");
			DoAsyncAwait(2);
		}

		private async void DoAsyncAwait(int sleep)
		{
			ShowOnScreen("Начало выполнения async-метода. ThreadId - " + Thread.CurrentThread.ManagedThreadId + crlf);
			string res;
			// вариант 1. основной поток не зависнет
			res = await GoAwait(sleep);
			// вариант 2. основной поток не зависнет
			// res = await Task.Factory.StartNew<string>(() => SomeMethod(sleep));
			// вариант 3. основной поток не зависнет
			/*
			var t = Task.Factory.StartNew(() => SomeMethod(sleep));
			res = await t;
			/**/
			// вариант 4. основной поток зависнет, т.к. Result неявно вызовет Wait
			// res = Task.Factory.StartNew(() => SomeMethod(sleep)).Result;

			ShowOnScreen("await-строка вернула: " + res + crlf);
			ShowOnScreen("Выполнение async-метода после await. ThreadId - " + Thread.CurrentThread.ManagedThreadId + crlf);
		}

		private Task<string> GoAwait(int sleep)
		{
			return Task.Factory.StartNew(() => SomeMethod(sleep));
			// тоже самое, но подлиннее
			// return Task.Factory.StartNew(() => { return SomeMethod(sleep); });
		}

		private string SomeMethod(int sleep)
		{
			Thread.Sleep(sleep * 1000);
			return $"Task из await-части. Поспал {sleep} сек. Завершился. ThreadId - " +
			       Thread.CurrentThread.ManagedThreadId;
		}

		private void btnAsyncStart2_Click(object sender, EventArgs e)
		{
			ShowOnScreen(crlf + "Запуск async-метода. Данный код является внешним для async-метода. ThreadId - " +
			             Thread.CurrentThread.ManagedThreadId + crlf);
			DoAsyncAwait(2);
			ShowOnScreen("Возврат во внешний код. ThreadId - " + Thread.CurrentThread.ManagedThreadId + crlf);
		}

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
			ShowOnScreen(crlf + str + crlf + crlf);

			var t = MethodAsync();
			// завис на 5 сек
			t.Wait(5000);
			// int res = MethodAsync().Result; // тоже завис. т.к. Result неявно вызывает Wait

			ShowOnScreen("с Wait в вызывающем потоке для таска из await нужно быть аккуратнее :)");
		}

		private async Task<int> MethodAsync()
		{
			// есть блокировка
			await Task.Delay(200); /* <===> */ // await Task.Delay(200).ConfigureAwait(true);
			// нет блокировки
			//await Task.Delay(200).ConfigureAwait(false);
			return 1;
		}

		#endregion async - await

		#region lock, Monitor.Enter/Exit, Monitor.Wait/Pulse <=> ManualResetEvent.WaiteOne+Reset/Set <=> AutoResetEvent.WaiteOne/Set

		private void btnLocksRun_Click(object sender, EventArgs e)
		{
			int ind = cmbLocks.SelectedIndex;
			if (ind == 0)
			{
				#region зачем нужны блокировки

				File.Create(logFile).Close();
				ShowOnScreen(crlf);
				var logWrite = new WriteLog(logFile, ShowOnScreen);

				var t1 = Task.Factory.StartNew(
					() =>
					{
						Thread.CurrentThread.Name = "write_with_lock_1";
						logWrite.WriteLockThis(
							"1-ый поток записывает через lock (даже bad code - lock(this, \"строка\", typeOf) и спит", 2);
					});
				var t2 = Task.Factory.StartNew(
					() =>
					{
						Thread.CurrentThread.Name = "write_with_lock_2";
						logWrite.WriteUseLocker("2-ый поток записывает через lock (locker) и спит", 2);
					});
				var t3 = Task.Factory.StartNew(
					() =>
					{
						Thread.CurrentThread.Name = "write_direct_without_lock";
						logWrite.WriteDirectNoLocks("3-ий поток записывает напрямую без блокировок и спит", 2);
					});
				Task.WaitAll(new[] { t1, t2, t3 });
				// Application.DoEvents() - чтобы фоновые потоки успели через делегат и BeginEnvoke отписаться раньше основного потока
				Application.DoEvents();
				ShowOnScreen(crlf + "Гонка между потоками - кто-то по воле случая успел, а кто-то нет => ошибки => Нужны нормальные блокировки" + crlf);

				#endregion зачем нужны блокировки
			}
			else if (ind == 1)
			{
				#region - lock(this) - bad code

				ShowOnScreen(crlf);
				var logWrite = new WriteLog(logFile, ShowOnScreen);

				// lock_this
				File.Create(logFile).Close();
				File.Create(logFile2).Close();
				var sw = new Stopwatch();
				sw.Start();
				// lock(this) лочит экземпляр logWrite, к которому есть доступ у другого программиста
				var t1 = Task.Factory.StartNew(
					() => { logWrite.WriteLockThis("1-ый поток logwrite записывает через lock(this) и спит", 2); });
				// другой программист не зная про ваш lock(this) делает lock(logWrite)
				// lock(logWrite) тоже лочит экземпляр logWrite и потоки будут ждать друг друга, причём кто кого будет ждать зависит от воли случая
				// а может случится и deadlock наверно при неудачном раскладе
				var t2 = Task.Factory.StartNew(
					() =>
					{
						lock (logWrite)
						{
							logWrite.WriteDirectNoLocksToOtherLog(logFile2, "2-ый поток lock (logWrite) записывает напрямую в другой файл и спит", 2);
						}
					});
				Task.WaitAll(new[] { t1, t2 });
				sw.Stop();
				long time = sw.ElapsedMilliseconds;
				Application.DoEvents();
				ShowOnScreen("поток1 (logWrite - lock(this)) + поток2 (lock(logWrite) - logWrite.другой_метод) = " + time +
							" (мсек)" + crlf + crlf);

				// locker
				File.Create(logFile).Close();
				File.Create(logFile2).Close();
				sw.Restart();
				// lock(locker) блокирует только код внутри метода WriteUseLocker
				t1 = Task.Factory.StartNew(() => { logWrite.WriteUseLocker("1-ый поток записывает через locker и спит", 2); });
				// и это совершенно не мешает другому потоку работать в другом методе => нет потери производительности и невозможен deadlock
				t2 = Task.Factory.StartNew(
					() => { logWrite.WriteDirectNoLocksToOtherLog(logFile2, "2-ый поток записывает напрямую в другой файл и спит", 2); });
				Task.WaitAll(new[] { t1, t2 });
				sw.Stop();
				time = sw.ElapsedMilliseconds;
				Application.DoEvents();
				ShowOnScreen("поток1 (logWrite - lock(locker)) + поток2 (logWrite.другой_метод) = " + time + " (мсек)" + crlf + crlf);

				// logWrite.WriteUseLocker с lock(locker) блокирует только код внутри метода WriteUseLocker
				// и если этот метод вызвать ещё раз, то один поток также будет ждать другого и по времени здесь не выиграем
				// но зато нет рисков возникновения deadlock
				// а при использовании других методов объекта, в которых уже нет lock(locker), мы не будем терять время

				#endregion - (this) - bad code
			}
			else if (ind == 2)
			{
				#region - lock(typeOf(...)) - bad code

				ShowOnScreen(crlf);
				var logWrite = new WriteLog(logFile, ShowOnScreen);
				var logWrite2 = new WriteLog(logFile2, ShowOnScreen);

				// lock(typeof(WriteLog)) лочит все экземпляры этого класса, в итоге можно получить нехилое падение производительности
				File.Create(logFile).Close();
				File.Create(logFile2).Close();
				var sw = new Stopwatch();
				sw.Start();
				// смысл lock(...) - залочить объект, который принадлежит только нам
				// а результат выполнения typeof(WriteLog) не принадлежит нам эксклюзивно
				// также может сделать и другой программист
				var t1 = Task.Factory.StartNew(
					() =>
					{
						lock (typeof(WriteLog))
						{
							logWrite.WriteDirectNoLocks("1-ый поток lock(typeOf(...)), logwrite записывает и спит", 2);
						}
					});
				// в итоге теряем производительность
				// 2 потока и у каждого свой экземпляр класса WriteLog, а но из-за блокировки выполняются они по времени как синхронный код
				// и где выигрыш от мультипоточности ?
				var t2 = Task.Factory.StartNew(
					() =>
					{
						lock (typeof(WriteLog))
						{
							logWrite2.WriteDirectNoLocks("2-ой поток lock(typeOf(...)), logwrite2 записывает и спит", 2);
						}
					});
				Task.WaitAll(new[] { t1, t2 });
				sw.Stop();
				long time = sw.ElapsedMilliseconds;
				Application.DoEvents();
				ShowOnScreen("поток1 (lock(typeof(WriteLog)) - logWrite...) + поток2 (lock(typeof(WriteLog)) - logWrite2...) = " + time + " (мсек)" + crlf + crlf);

				File.Create(logFile).Close();
				File.Create(logFile2).Close();
				sw.Restart();
				t1 = Task.Factory.StartNew(() =>
				{
					lock (logWrite)
					{
						logWrite.WriteDirectNoLocks("1-ый поток logwrite записывает напрямую и спит", 2);
					}
				});
				t2 = Task.Factory.StartNew(() =>
				{
					lock (logWrite2)
					{
						logWrite2.WriteDirectNoLocks("2-ый поток logwrite2 записывает напрямую и спит", 2);
					}
				});
				Task.WaitAll(new[] { t1, t2 });
				sw.Stop();
				time = sw.ElapsedMilliseconds;
				Application.DoEvents();
				ShowOnScreen("поток1 (logWrite) + поток2 (logWrite2) = " + time + " (мсек)" + crlf + crlf);

				#endregion - lock(typeOf(...)) - bad code
			}
			else if (ind == 3)
			{
				#region - lock("строка") - bad code

				ShowOnScreen(crlf);
				var logWrite = new WriteLog(logFile, ShowOnScreen);
				var logWrite2 = new WriteLog(logFile2, ShowOnScreen);

				// lock ("одна и та же строка") внутри 1 метода используется в 2 потоках для 2 разных экземпляров
				File.Create(logFile).Close();
				File.Create(logFile2).Close();
				var sw = new Stopwatch();
				sw.Start();
				var t1 = Task.Factory.StartNew(
					() =>
					{
						lock ("одна и та же строка")
						{
							logWrite.WriteDirectNoLocks("1-ый поток lock (\"ОДНА И ТА ЖЕ СТРОКА\"), logwrite записывает и спит", 2);
						}
					});
				var t2 = Task.Factory.StartNew(
					() =>
					{
						lock ("одна и та же строка")
						{
							logWrite2.WriteDirectNoLocks("2-ый поток lock (\"ОДНА И ТА ЖЕ СТРОКА\"), logwrite2 записывает и спит", 2);
						}
					});
				Task.WaitAll(new[] { t1, t2 });
				sw.Stop();
				long time = sw.ElapsedMilliseconds;
				Application.DoEvents();
				ShowOnScreen("поток1 (lock(\"...\") - logWrite...) + поток1 (lock(\"...\") - logWrite2...) = " + time + " (мсек)" + crlf + crlf);

				// lock ("строка") внутри метода, который вызывается из разных потоков для 2 разных экземпляров
				File.Create(logFile).Close();
				File.Create(logFile2).Close();
				sw.Restart();
				t1 = Task.Factory.StartNew(
					() => { logWrite.WriteLockString("1-ый поток logwrite записывает через lock(\"СТРОКА\") и спит", 2); });
				t2 =
					Task.Factory.StartNew(
						() => { logWrite2.WriteLockString("2-ый поток logwrite2 записывает через lock(\"СТРОКА\") и спит", 2); });
				Task.WaitAll(new[] { t1, t2 });
				sw.Stop();
				time = sw.ElapsedMilliseconds;
				Application.DoEvents();
				ShowOnScreen("поток1 (logWrite - lock(\"СТРОКА\")) + поток2 (logWrite2 - lock(\"СТРОКА\") = " + time +
							" (мсек)" + crlf + crlf);

				File.Create(logFile).Close();
				File.Create(logFile2).Close();
				sw.Restart();
				t1 = Task.Factory.StartNew(
					() =>
					{
						lock ("строка 1")
						{
							logWrite.WriteDirectNoLocks("1-ый поток logwrite записывает и спит", 2);
						}
					});
				t2 = Task.Factory.StartNew(
					() =>
					{
						lock ("другая строка")
						{
							logWrite2.WriteDirectNoLocks("2-ый поток logwrite записывает и спит", 2);
						}
					});
				Task.WaitAll(new[] { t1, t2 });
				sw.Stop();
				time = sw.ElapsedMilliseconds;
				Application.DoEvents();
				ShowOnScreen("поток1 (lock(\"СТРОКА 1\") - logWrite...) + поток2 (lock(\"ДРУГАЯ СТРОКА\") - logWrite2...) = " + time +
							" (мсек)" +
							crlf + crlf +
							"Когда лочится одна и та же область памяти (переменная), то и потоки мешают друг другу, хотя каждый работает со своим экземпляром класса" +
							crlf + crlf);

				#endregion - lock("строка") - bad code
			}
			else if (ind == 4)
			{
				#region + lock(private object locker) <=> Monitor.Enter(locker) / Monitor.Exit(locker)

				File.Create(logFile).Close();
				ShowOnScreen(crlf);
				var logWrite = new WriteLog(logFile, ShowOnScreen);

				ShowOnScreen("Будет висяк из-за использования основного потока :)" + crlf + crlf);
				Application.DoEvents();

				// для внутренних блокировок лучше использовать специальный объект член-данных
				// тут работаем через приватный locker и проблем нет
				Task.Factory.StartNew(
					() =>
					{
						Thread.CurrentThread.Name = "thread_1";
						logWrite.WriteUseLocker("1-ый поток записывает через locker и спит", 2);
					});
				Task.Factory.StartNew(
					() =>
					{
						Thread.CurrentThread.Name = "thread_2";
						logWrite.WriteUseLocker2("2-ый поток записывает через locker и спит", 3);
					});
				// даже так не проблема
				try
				{
					Thread.CurrentThread.Name = "main_thread";
				}
				catch
				{
				}
				logWrite.WriteUseLocker("Основной поток записывает через locker и спит", 1);
				logWrite.WriteUseLocker2("Основной поток ещё раз записывает через locker и спит", 2);

				#endregion + lock(private object locker) <=> Monitor.Enter(locker) / Monitor.Exit(locker)
			}
			else if (ind == 5)
			{
				#region Monitor.Enter / Monitor.Exit

				File.Create(logFile).Close();
				ShowOnScreen(crlf);
				var logWrite = new WriteLog(logFile, ShowOnScreen);

				var t = Task.Factory.StartNew(
					() =>
					{
						Thread.CurrentThread.Name = "tread_throw_monitor_1";
						logWrite.WriteUseMonitor("1-ый поток записывает через Monitor и спит", 2);
					});
				Task.Factory.StartNew(
					() =>
					{
						Thread.CurrentThread.Name = "tread_throw_monitor_2";
						logWrite.WriteUseMonitor("2-ый поток записывает через Monitor и спит", 2);
					});
				Task.Factory.StartNew(
					() =>
					{
						Thread.CurrentThread.Name = "tread_without_monitor";
						logWrite.WriteDirectNoLocks("3-ый поток записывает напрямую без блокировок и спит", 2);
					});

				#endregion Monitor.Enter / Monitor.Exit
			}
			else if (ind == 6)
			{
				#region deadlock example

				int cnt = Process.GetCurrentProcess().Threads.Count;
				ShowOnScreen(crlf + "Кол-во потоков до deadlock " + cnt + crlf);

				Task.Factory.StartNew(
					() =>
					{
						// если разбавить пример ShowOnScreen(...), то видимо из-за BeginInvoke и как следствие переключений между потоками 
						// удаётся  избежать блокировок
						ShowInOutput("Thread 1 start");
						lock (typeof(int))
						{
							Thread.Sleep(1000);
							lock (typeof(float))
							{
								ShowInOutput("Thread 1 locks int and float");
							}
						}
						ShowInOutput("Thread 1 finish");
					});

				Task.Factory.StartNew(
					() =>
					{
						ShowInOutput("Thread 2 start");
						lock (typeof(float))
						{
							Thread.Sleep(1000);
							lock (typeof(int))
							{
								ShowInOutput("Thread 2 locks float and int");
							}
						}
						ShowInOutput("Thread 2 finish");
					});

				Thread.Sleep(3000);
				cnt = Process.GetCurrentProcess().Threads.Count;
				ShowOnScreen("Кол-во потоков после deadlock " + cnt + ". Кликни ещё раз - видно как их число растёт и не уменьшается :)" + crlf);

				#endregion deadlock example
			}
			else if (ind == 7)
			{
				#region ManualResetEvent 1. Пример c Reset

				string str = "Используем ManualResetEvent c Reset. " + crlf +
							 "В этом случае ManualResetEvent можно заменить на AutoResetEvent и не вызывать Reset" + crlf +
							 "Оба случая эквивалент Monitor.Enter/Exit. Смотри последовательное выполнение потоков - 3 сек";
				ShowInOutput(str);
				ShowOnScreen(crlf + str + crlf);
				Application.DoEvents();

				var mre = new ManualResetEvent(true);
				Task.Factory.StartNew(
					() =>
					{
						string str1 = "поток 1, ждём WaitOne (пока событие не перейдёт в сигнальное состояние)";
						ShowInOutput(str1);
						ShowOnScreen(str1 + crlf);

						mre.WaitOne();
						mre.Reset();

						str1 =
							"поток 1, дождались, пошли дальше и вызвали Reset, чтобы другие потоки использующее это же событие стопанулись на WaitOne";
						ShowInOutput(str1);
						ShowOnScreen(str1 + crlf);

						Thread.Sleep(2000);

						str1 =
							"поток 1, поспал 2 сек, перевел событие в сигнальное событие, чтобы другие потоки ожидающие WaitOne могли работать";
						ShowInOutput(str1);
						ShowOnScreen(str1 + crlf);

						mre.Set();
					});
				Task.Factory.StartNew(
					() =>
					{
						string str2 = "поток 2, ждём WaitOne (пока событие не перейдёт в сигнальное состояние)";
						ShowInOutput(str2);
						ShowOnScreen(str2 + crlf);

						mre.WaitOne();
						mre.Reset();

						str2 =
							"поток 2, дождались, пошли дальше и вызвали Reset, чтобы другие потоки использующее это же событие стопанулись на WaitOne";
						ShowInOutput(str2);
						ShowOnScreen(str2 + crlf);

						Thread.Sleep(1000);

						str2 =
							"поток 2, поспал 2 сек, перевел событие в сигнальное событие, чтобы другие потоки ожидающие WaitOne могли работать";
						ShowInOutput(str2);
						ShowOnScreen(str2 + crlf);

						mre.Set();
					});

				#endregion ManualResetEvent 1. Пример c Reset
			}
			else if (ind == 8)
			{
				#region ManualResetEvent 2. Пример без Reset

				string str = "Используем ManualResetEvent без Reset. Потоки выполняются параллельно. " + crlf +
							 "Изначально событие в несигнальном состоянии. Ждать 3 сек";
				ShowInOutput(str);
				ShowOnScreen(crlf + str + crlf);
				Application.DoEvents();

				var sb = new StringBuilder();
				var mre = new ManualResetEvent(false);
				Task.Factory.StartNew(
					() =>
					{
						// если use одну и ту же str в 2 потоках, которые выполняются параллельно
						// то непонятно какой поток её изменил, а какой выввел на экран/консоль
						// если бы оба потока выполняли один и тот же метод одного и того же экземпляра
						// то при изменении полей-свойств экземпляра получим непонятно что
						string str1 = "поток 1, ждём WaitOne (пока событие не перейдёт в сигнальное состояние)";
						ShowInOutput(str1);
						ShowOnScreen(str1 + crlf);
						sb.AppendLine(str1);

						mre.WaitOne();

						str1 = "поток 1, дождались и пошли дальше";
						ShowInOutput(str1);
						sb.AppendLine(str1);
						ShowOnScreen(str1 + crlf);

						Thread.Sleep(2000);

						str1 = "поток 1, поспал 2 сек, пошел дальше";
						ShowInOutput(str1);
						sb.AppendLine(str1);
						ShowOnScreen(str1 + crlf);
					});
				Task.Factory.StartNew(
					() =>
					{
						string str2 = "поток 2, ждём WaitOne (пока событие не перейдёт в сигнальное состояние)";
						ShowInOutput(str2);
						sb.AppendLine(str2);
						ShowOnScreen(str2 + crlf);

						mre.WaitOne();

						str2 = "поток 2, дождались и пошли дальше";
						ShowInOutput(str2);
						sb.AppendLine(str2);
						ShowOnScreen(str2 + crlf);

						Thread.Sleep(2000);

						str2 = "поток 2, поспал 2 сек, пошел дальше";
						ShowInOutput(str2);
						sb.AppendLine(str2);
						ShowOnScreen(str2 + crlf);
					});
				Task.Factory.StartNew(
					() =>
					{
						str = "какой-то поток, поспал 1 сек и выставил событие в сигнальное состояние. Смотрим кашу :)";
						ShowInOutput(str);
						sb.AppendLine(str);
						ShowOnScreen(str + crlf);

						mre.Set();
					});

				#endregion ManualResetEvent 2. Пример без Reset
			}
			else if (ind == 9)
			{
				#region Monitor .Wait/.Pulse

				string str = "Monitor .Wait/.Pulse";
				ShowInOutput(str);
				ShowOnScreen(crlf + str + crlf);
				Application.DoEvents();

				var locker = new object();
				Task.Factory.StartNew(
					() =>
					{
						string str1 = "поток 1 зашёл, будет захватывать locker";
						ShowInOutput(str1);
						ShowOnScreen(str1 + crlf);

						// или lock (locker) отсюда и до конца
						// т.к. Monitor.Wait/Pulse работают только внутри блокировки, т.е. внутри Monitor.Enter/Exit или блока lock
						Monitor.Enter(locker); // ждём пока не захватим блокировку

						str1 = "поток 1 захватил locker, Monitor.Wait(locker) освободил locker для других потоков и ждёт пока они отработают";
						ShowInOutput(str1);
						ShowOnScreen(str1 + crlf);
						// освобожадем блокировку, вызванную Monitor.Enter, и зависаем, пока не получим от другого потока Monitor.Pulse
						Monitor.Wait(locker);

						str1 = "поток 1, дождался окончания Monitor.Wait(locker) и спит 2 сек";
						ShowInOutput(str1);
						ShowOnScreen(str1 + crlf);
						Thread.Sleep(2000);

						str1 = "поток 1 Monitor.Pulse(locker), Monitor.Exit(locker) - конец блока кода с блокировкой. Pulse - на всяк случай, чтобы не завис никто";
						ShowInOutput(str1);
						ShowOnScreen(str1 + crlf);
						Monitor.Pulse(locker); // говорим, что мы всё закончили и разрешаем другим ожидающим потокам продолжить работу
						Monitor.Exit(locker);
					});
				Task.Factory.StartNew(
					() =>
					{
						string str2 = "поток 2, зашел, будет захватывать locker";
						ShowInOutput(str2);
						ShowOnScreen(str2 + crlf);

						// или lock (locker) отсюда и до конца
						// т.к. Monitor.Wait/Pulse работают только внутри блокировки, т.е. внутри Monitor.Enter/Exit или блока lock
						Monitor.Enter(locker);

						str2 = "поток 2 захватил locker и спит 1 сек";
						ShowInOutput(str2);
						ShowOnScreen(str2 + crlf);
						Thread.Sleep(1000);

						str2 = "поток 2 Monitor.Pulse(locker) освободил блокировку для других потоков";
						ShowInOutput(str2);
						ShowOnScreen(str2 + crlf);
						Monitor.Pulse(locker);

						// этот кусочек можно выключить для более быстрой отработки примера
						/**/
						str2 = "поток 2 Monitor.Wait(locker) - ждёт пока locker освободится";
						ShowInOutput(str2);
						ShowOnScreen(str2 + crlf);
						Monitor.Wait(locker);
						str2 = "поток 2 дождался окончания Monitor.Wait(locker)";
						ShowInOutput(str2);
						ShowOnScreen(str2 + crlf);
						/**/

						str2 = "поток 2 Monitor.Pulse(locker), Monitor.Exit(locker) - конец блока кода с блокировкой. Pulse - на всяк случай, чтобы не завис никто";
						ShowInOutput(str2);
						ShowOnScreen(str2 + crlf);
						Monitor.Pulse(locker);
						Monitor.Exit(locker);
					});

				#endregion Monitor .Wait/.Pulse
			}
			else if (ind == 10)
			{
				#region SpinLock .Enter/Exit <=> Monitor .Enter/Exit

				string str = "Сравнение Monitor .Enter/Exit и SpinLock .Enter/.Exit. SpinLock быстрее немного работает";
				ShowInOutput(str);
				ShowOnScreen(crlf + str + crlf + crlf);
				Application.DoEvents();

				var sw = new Stopwatch();
				sw.Start();
				var locker = new object();
				var m1 = Task.Factory.StartNew(
					() =>
					{
						string str1 = "поток 1 зашёл, будет захватывать locker";
						ShowInOutput(str1);
						ShowOnScreen(str1 + crlf);

						Monitor.Enter(locker);
						str1 = "поток 1 захватил locker, отписался, освободил locker, завершил работу";
						ShowInOutput(str1);
						ShowOnScreen(str1 + crlf);
						Monitor.Exit(locker);
					});
				var m2 = Task.Factory.StartNew(
					() =>
					{
						string str1 = "поток 2 зашёл, будет захватывать locker";
						ShowInOutput(str1);
						ShowOnScreen(str1 + crlf);

						Monitor.Enter(locker);
						str1 = "поток 2 захватил locker, отписался, освободил locker, завершил работу";
						ShowInOutput(str1);
						ShowOnScreen(str1 + crlf);
						Monitor.Exit(locker);
					});
				Task.WaitAll(new[] {m1, m2});
				sw.Stop();
				Application.DoEvents();
				str = "Пример с Monitor .Enter/Exit отработал за " + sw.ElapsedMilliseconds + " миллисек";
				ShowInOutput(str);
				ShowOnScreen(str + crlf + crlf);
				Application.DoEvents();

				var sw2 = new Stopwatch();
				sw2.Start();
				bool locked;
				var spinLock = new SpinLock();
				var s1 = Task.Factory.StartNew(
					() =>
					{
						string str1 = "поток 1 зашёл, будет захватывать spinLock";
						ShowInOutput(str1);
						ShowOnScreen(str1 + crlf);

						locked = false; // true нельзя передать
						spinLock.Enter(ref locked);
						str1 = "поток 1 захватил spinLock, отписался, освободил spinLock, завершил работу";
						ShowInOutput(str1);
						ShowOnScreen(str1 + crlf);
						spinLock.Exit();
					});
				var s2 = Task.Factory.StartNew(
					() =>
					{
						string str1 = "поток 2 зашёл, будет захватывать spinLock";
						ShowInOutput(str1);
						ShowOnScreen(str1 + crlf);

						locked = false; // true нельзя передать
						spinLock.Enter(ref locked);
						str1 = "поток 2 захватил spinLock, отписался, освободил spinLock, завершил работу";
						ShowInOutput(str1);
						ShowOnScreen(str1 + crlf);
						spinLock.Exit();
					});
				Task.WaitAll(new[] { s1, s2 });
				sw2.Stop();
				Application.DoEvents();
				str = "Пример с SpinLock .Enter/Exit отработал за " + sw2.ElapsedMilliseconds + "миллисек";
				ShowInOutput(str);
				ShowOnScreen(str + crlf);

				#endregion SpinLock .Enter/Exit <=> Monitor .Enter/Exit
			}
		}

		private const string logFile = @"C:\lock_log.txt";
		private const string logFile2 = @"C:\lock_log2.txt";

		public class WriteLog
		{
			private string log;
			private object locker = new object();
			private DelegateOutToScreen funcOut;

			public WriteLog(string log, DelegateOutToScreen func)
			{
				this.log = log;
				funcOut = func;
			}

			private void WriteToLog(string logFile, string line, int sleepTimeOut)
			{
				string threadName = Thread.CurrentThread.Name;
				try
				{
					if (!string.IsNullOrEmpty(threadName))
					{
						funcOut.BeginInvoke(threadName + " зашёл" + crlf, null, null);
					}

					string str = line + crlf;
					funcOut.BeginInvoke("Действие :" + str, null, null);

					var sw = File.AppendText(logFile);
					sw.WriteLine(str.ToCharArray(), 0, str.Length);

					if (sleepTimeOut > 0)
					{
						Thread.Sleep(sleepTimeOut * 1000);
					}

					sw.Close();
				}
				catch
				{
					funcOut.BeginInvoke("ERR " + (string.IsNullOrEmpty(threadName) ? "" : "(" + threadName + ")") + ": " + line + crlf,
						null, null);
				}
			}

			public void WriteLockThis(string line, int sleepTimeOut)
			{
				lock (this)
				{
					WriteToLog(log, line, sleepTimeOut);
				}
			}

			public void WriteLockString(string line, int sleepTimeOut)
			{
				lock ("строка")
				{
					WriteToLog(log, line, sleepTimeOut);
				}
			}

			public void WriteDirectNoLocks(string line, int sleepTimeOut)
			{
				WriteToLog(log, line, sleepTimeOut);
			}

			public void WriteDirectNoLocksToOtherLog(string otherFile, string line, int sleepTimeOut)
			{
				WriteToLog(otherFile, line, sleepTimeOut);
			}			

			public void WriteUseLocker(string line, int sleepTimeOut)
			{
				// здесь конечно locker надо перенести внутрь WriteToLog, но WriteToLog также use для демонстрации плохого кода
				lock (locker)
				{
					WriteToLog(log, line, sleepTimeOut);
				}
			}

			public void WriteUseLocker2(string line, int sleepTimeOut)
			{
				// здесь конечно locker надо перенести внутрь WriteToLog, но WriteToLog также use для демонстрации плохого кода
				lock (locker)
				{
					WriteToLog(log, line + ". Это другой метод", sleepTimeOut);
				}
			}

			public void WriteUseLockTypeOf(string line, int sleepTimeOut)
			{
				lock (typeof(WriteLog))
				{
					WriteToLog(log, line, sleepTimeOut);
				}
			}

			public void WriteUseMonitor(string line, int sleepTimeOut)
			{
				funcOut.BeginInvoke(Thread.CurrentThread.Name + " зашёл" + crlf, null, null);
				Monitor.Enter(locker);
				try
				{
					string str = line + crlf;
					funcOut.BeginInvoke("Действие :" + str, null, null);
					var sw = File.AppendText(log);
					sw.WriteLine(str.ToCharArray(), 0, str.Length);
					if (sleepTimeOut > 0)
					{
						Thread.Sleep(sleepTimeOut*1000);
					}
					sw.Close();
				}
				catch
				{
					funcOut.BeginInvoke("Ошибка: " + line + crlf, null, null);
				}
				finally
				{
					Monitor.Exit(locker);
				}
			}
		}

		#endregion lock, Monitor.Enter/Exit, Monitor.Wait/Pulse <=> ManualResetEvent.WaiteOne+Reset/Set <=> AutoResetEvent.WaiteOne/Set

		#region Mutex - взаимодействие между процессами. Mutex.WaitOne\[TryOpenExisting+]ReleaseMutex

		private bool flagMutex;

		private void btnMutex_Click(object sender, EventArgs e)
		{
			if (flagMutex)
			{
				Mutex m;
				if (Mutex.TryOpenExisting("KlimMutex", MutexRights.FullControl, out m))
				{
					// WaitOne и ReleaseMutex надо вызывать из одного и того же потока, иначе будет error
					m.ReleaseMutex();
					m.Dispose();
				}

				btnMutex.Text = "Mutex";
				flagMutex = false;
			}
			else
			{
				flagMutex = true;
				btnMutex.Text = "Release Mutex";
				ShowOnScreen(crlf + "Создаём KlimMutex и захватываем его" + crlf +
				             "Запускаем другой процесс, который будет ждать освобождения KlimMutex" + crlf +
				             "Ждите появления второго окна с инcтрукциями" + crlf);

				var sid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
				var allowAll = new MutexAccessRule(sid, MutexRights.FullControl, AccessControlType.Allow);
				var sec = new MutexSecurity();
				sec.AddAccessRule(allowAll);

				var m = new Mutex(false, "KlimMutex");
				m.SetAccessControl(sec);

				Task.Factory.StartNew(
					() =>
					{
						string exe = Application.ExecutablePath;
						var process = new Process {StartInfo = new ProcessStartInfo(exe, "Mutex")};
						process.Start();
					});

				m.WaitOne();
			}
		}

		#endregion Mutex - взаимодействие между процессами. Mutex.WaitOne\[TryOpenExisting+]ReleaseMutex
	}
}
