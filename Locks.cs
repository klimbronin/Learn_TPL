using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication2
{
	/// <summary>
	/// Блокировки
	/// </summary>
	internal class Locks
	{
		private const string logFile = @"C:\lock_log.txt";
		private const string logFile2 = @"C:\lock_log2.txt";

		/// <summary>
		/// 
		/// </summary>
		public class WriteLog
		{
			private string log;
			private object locker = new object();
			private Utils.DelegateOutToScreen funcOut;

			public WriteLog(string log, Utils.DelegateOutToScreen func)
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
						funcOut.BeginInvoke(threadName + " зашёл" + Utils.crlf, null, null);
					}

					string str = line + Utils.crlf;
					funcOut.BeginInvoke("Действие :" + str, null, null);

					var sw = File.AppendText(logFile);
					sw.WriteLine(str.ToCharArray(), 0, str.Length);

					if (sleepTimeOut > 0)
					{
						Thread.Sleep(sleepTimeOut*1000);
					}

					sw.Close();
				}
				catch
				{
					funcOut.BeginInvoke(
						"ERR " + (string.IsNullOrEmpty(threadName) ? "" : "(" + threadName + ")") + ": " + line + Utils.crlf,
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
				lock (typeof (WriteLog))
				{
					WriteToLog(log, line, sleepTimeOut);
				}
			}

			public void WriteUseMonitor(string line, int sleepTimeOut)
			{
				funcOut.BeginInvoke(Thread.CurrentThread.Name + " зашёл" + Utils.crlf, null, null);
				Monitor.Enter(locker);
				try
				{
					string str = line + Utils.crlf;
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
					funcOut.BeginInvoke("Ошибка: " + line + Utils.crlf, null, null);
				}
				finally
				{
					Monitor.Exit(locker);
				}
			}
		}

		/// <summary>
		/// </summary>
		public static void Do(int variant, Utils.DelegateOutToScreen ShowOnScreen, Utils.DelegateOutToConsole ShowInOutput)
		{
			if (variant == 0)
			{
				#region зачем нужны блокировки

				File.Create(logFile).Close();
				ShowOnScreen(Utils.crlf);
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
				Task.WaitAll(new[] {t1, t2, t3});
				// Application.DoEvents() - чтобы фоновые потоки успели через делегат и BeginEnvoke отписаться раньше основного потока
				Application.DoEvents();
				ShowOnScreen(Utils.crlf +
				             "Гонка между потоками - кто-то по воле случая успел, а кто-то нет => ошибки => Нужны нормальные блокировки" +
				             Utils.crlf);

				#endregion зачем нужны блокировки
			}
			else if (variant == 1)
			{
				#region - lock(this) - bad code

				ShowOnScreen(Utils.crlf + "Смотрим в каком случае есть выигрыш по времени" + Utils.crlf + Utils.crlf);
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
							logWrite.WriteDirectNoLocksToOtherLog(logFile2,
								"2-ый поток lock (logWrite) записывает напрямую в другой файл и спит", 2);
						}
					});
				Task.WaitAll(new[] {t1, t2});
				sw.Stop();
				long time = sw.ElapsedMilliseconds;
				Application.DoEvents();
				ShowOnScreen("поток1 (logWrite - lock(this)) + поток2 (lock(logWrite) - logWrite.другой_метод) = " + time +
				             " (мсек)" + Utils.crlf + Utils.crlf);				

				// locker
				File.Create(logFile).Close();
				File.Create(logFile2).Close();
				sw.Restart();
				// lock(locker) блокирует только код внутри метода WriteUseLocker
				t1 = Task.Factory.StartNew(() => { logWrite.WriteUseLocker("1-ый поток записывает через locker и спит", 2); });
				// и это совершенно не мешает другому потоку работать в другом методе => нет потери производительности и невозможен deadlock
				t2 = Task.Factory.StartNew(
					() =>
					{
						logWrite.WriteDirectNoLocksToOtherLog(logFile2, "2-ый поток записывает напрямую в другой файл и спит", 2);
					});
				Task.WaitAll(new[] {t1, t2});
				sw.Stop();
				time = sw.ElapsedMilliseconds;
				Application.DoEvents();
				ShowOnScreen("поток1 (logWrite - lock(locker)) + поток2 (logWrite.другой_метод) = " + time + " (мсек)" +
				             Utils.crlf + Utils.crlf);

				// logWrite.WriteUseLocker с lock(locker) блокирует только код внутри метода WriteUseLocker
				// и если этот метод вызвать ещё раз, то один поток также будет ждать другого и по времени здесь не выиграем
				// но зато нет рисков возникновения deadlock
				// а при использовании других методов объекта, в которых уже нет lock(locker), мы не будем терять время

				#endregion - (this) - bad code
			}
			else if (variant == 2)
			{
				#region - lock(typeOf(...)) - bad code

				ShowOnScreen(Utils.crlf + "Смотрим в каком случае есть выигрыш по времени" + Utils.crlf + Utils.crlf);
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
						lock (typeof (WriteLog))
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
						lock (typeof (WriteLog))
						{
							logWrite2.WriteDirectNoLocks("2-ой поток lock(typeOf(...)), logwrite2 записывает и спит", 2);
						}
					});
				Task.WaitAll(new[] {t1, t2});
				sw.Stop();
				long time = sw.ElapsedMilliseconds;
				Application.DoEvents();
				ShowOnScreen("поток1 (lock(typeof(WriteLog)) - logWrite...) + поток2 (lock(typeof(WriteLog)) - logWrite2...) = " +
				             time + " (мсек)" + Utils.crlf + Utils.crlf);

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
				Task.WaitAll(new[] {t1, t2});
				sw.Stop();
				time = sw.ElapsedMilliseconds;
				Application.DoEvents();
				ShowOnScreen("поток1 (logWrite) + поток2 (logWrite2) = " + time + " (мсек)" + Utils.crlf + Utils.crlf);

				#endregion - lock(typeOf(...)) - bad code
			}
			else if (variant == 3)
			{
				#region - lock("строка") - bad code

				ShowOnScreen(Utils.crlf + "Смотрим в каком случае есть выигрыш по времени" + Utils.crlf + Utils.crlf);
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
				Task.WaitAll(new[] {t1, t2});
				sw.Stop();
				long time = sw.ElapsedMilliseconds;
				Application.DoEvents();
				ShowOnScreen("поток1 (lock(\"...\") - logWrite...) + поток1 (lock(\"...\") - logWrite2...) = " + time + " (мсек)" +
				             Utils.crlf + Utils.crlf);

				// lock ("строка") внутри метода, который вызывается из разных потоков для 2 разных экземпляров
				File.Create(logFile).Close();
				File.Create(logFile2).Close();
				sw.Restart();
				t1 = Task.Factory.StartNew(
					() => { logWrite.WriteLockString("1-ый поток logwrite записывает через lock(\"СТРОКА\") и спит", 2); });
				t2 =
					Task.Factory.StartNew(
						() => { logWrite2.WriteLockString("2-ый поток logwrite2 записывает через lock(\"СТРОКА\") и спит", 2); });
				Task.WaitAll(new[] {t1, t2});
				sw.Stop();
				time = sw.ElapsedMilliseconds;
				Application.DoEvents();
				ShowOnScreen("поток1 (logWrite - lock(\"СТРОКА\")) + поток2 (logWrite2 - lock(\"СТРОКА\") = " + time +
				             " (мсек)" + Utils.crlf + Utils.crlf);

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
				Task.WaitAll(new[] {t1, t2});
				sw.Stop();
				time = sw.ElapsedMilliseconds;
				Application.DoEvents();
				ShowOnScreen("поток1 (lock(\"СТРОКА 1\") - logWrite...) + поток2 (lock(\"ДРУГАЯ СТРОКА\") - logWrite2...) = " + time +
				             " (мсек)" +
				             Utils.crlf + Utils.crlf +
				             "Когда лочится одна и та же область памяти (переменная), то и потоки мешают друг другу, хотя каждый работает со своим экземпляром класса" +
				             Utils.crlf + Utils.crlf);

				#endregion - lock("строка") - bad code
			}
			else if (variant == 4)
			{
				#region + lock(private object locker) <=> Monitor.Enter(locker) / Monitor.Exit(locker)

				File.Create(logFile).Close();
				ShowOnScreen(Utils.crlf);
				var logWrite = new WriteLog(logFile, ShowOnScreen);

				ShowOnScreen("Будет висяк из-за использования основного потока :)" + Utils.crlf + Utils.crlf);
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
			else if (variant == 5)
			{
				#region Monitor.Enter / Monitor.Exit

				File.Create(logFile).Close();
				ShowOnScreen(Utils.crlf);
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
			else if (variant == 6)
			{
				#region deadlock example

				int cnt = Process.GetCurrentProcess().Threads.Count;
				ShowOnScreen(Utils.crlf + "Кол-во потоков до deadlock " + cnt + Utils.crlf);

				Task.Factory.StartNew(
					() =>
					{
						// если разбавить пример ShowOnScreen(...), то видимо из-за BeginInvoke и как следствие переключений между потоками 
						// удаётся  избежать блокировок
						ShowInOutput("Thread 1 start");
						lock (typeof (int))
						{
							Thread.Sleep(1000);
							lock (typeof (float))
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
						lock (typeof (float))
						{
							Thread.Sleep(1000);
							lock (typeof (int))
							{
								ShowInOutput("Thread 2 locks float and int");
							}
						}
						ShowInOutput("Thread 2 finish");
					});

				Thread.Sleep(3000);
				cnt = Process.GetCurrentProcess().Threads.Count;
				ShowOnScreen("Кол-во потоков после deadlock " + cnt +
				             ". Кликни ещё раз - видно как их число растёт и не уменьшается :)" + Utils.crlf);

				#endregion deadlock example
			}
			else if (variant == 7)
			{
				#region ManualResetEvent 1. Пример c Reset

				string str = "Используем ManualResetEvent c Reset. " + Utils.crlf +
				             "В этом случае ManualResetEvent можно заменить на AutoResetEvent и не вызывать Reset" + Utils.crlf +
				             "Оба случая эквивалент Monitor.Enter/Exit. Смотри последовательное выполнение потоков - 3 сек";
				ShowInOutput(str);
				ShowOnScreen(Utils.crlf + str + Utils.crlf);
				Application.DoEvents();

				var mre = new ManualResetEvent(true);
				Task.Factory.StartNew(
					() =>
					{
						string str1 = "поток 1, ждём WaitOne (пока событие не перейдёт в сигнальное состояние)";
						ShowInOutput(str1);
						ShowOnScreen(str1 + Utils.crlf);

						mre.WaitOne();
						mre.Reset();

						str1 =
							"поток 1, дождались, пошли дальше и вызвали Reset, чтобы другие потоки использующее это же событие стопанулись на WaitOne";
						ShowInOutput(str1);
						ShowOnScreen(str1 + Utils.crlf);

						Thread.Sleep(2000);

						str1 =
							"поток 1, поспал 2 сек, перевел событие в сигнальное событие, чтобы другие потоки ожидающие WaitOne могли работать";
						ShowInOutput(str1);
						ShowOnScreen(str1 + Utils.crlf);

						mre.Set();
					});
				Task.Factory.StartNew(
					() =>
					{
						string str2 = "поток 2, ждём WaitOne (пока событие не перейдёт в сигнальное состояние)";
						ShowInOutput(str2);
						ShowOnScreen(str2 + Utils.crlf);

						mre.WaitOne();
						mre.Reset();

						str2 =
							"поток 2, дождались, пошли дальше и вызвали Reset, чтобы другие потоки использующее это же событие стопанулись на WaitOne";
						ShowInOutput(str2);
						ShowOnScreen(str2 + Utils.crlf);

						Thread.Sleep(1000);

						str2 =
							"поток 2, поспал 2 сек, перевел событие в сигнальное событие, чтобы другие потоки ожидающие WaitOne могли работать";
						ShowInOutput(str2);
						ShowOnScreen(str2 + Utils.crlf);

						mre.Set();
					});

				#endregion ManualResetEvent 1. Пример c Reset
			}
			else if (variant == 8)
			{
				#region ManualResetEvent 2. Пример без Reset

				string str = "Используем ManualResetEvent без Reset. Потоки выполняются параллельно. " + Utils.crlf +
				             "Изначально событие в несигнальном состоянии. Ждать 3 сек";
				ShowInOutput(str);
				ShowOnScreen(Utils.crlf + str + Utils.crlf);
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
						ShowOnScreen(str1 + Utils.crlf);
						sb.AppendLine(str1);

						mre.WaitOne();

						str1 = "поток 1, дождались и пошли дальше";
						ShowInOutput(str1);
						sb.AppendLine(str1);
						ShowOnScreen(str1 + Utils.crlf);

						Thread.Sleep(2000);

						str1 = "поток 1, поспал 2 сек, пошел дальше";
						ShowInOutput(str1);
						sb.AppendLine(str1);
						ShowOnScreen(str1 + Utils.crlf);
					});
				Task.Factory.StartNew(
					() =>
					{
						string str2 = "поток 2, ждём WaitOne (пока событие не перейдёт в сигнальное состояние)";
						ShowInOutput(str2);
						sb.AppendLine(str2);
						ShowOnScreen(str2 + Utils.crlf);

						mre.WaitOne();

						str2 = "поток 2, дождались и пошли дальше";
						ShowInOutput(str2);
						sb.AppendLine(str2);
						ShowOnScreen(str2 + Utils.crlf);

						Thread.Sleep(2000);

						str2 = "поток 2, поспал 2 сек, пошел дальше";
						ShowInOutput(str2);
						sb.AppendLine(str2);
						ShowOnScreen(str2 + Utils.crlf);
					});
				Task.Factory.StartNew(
					() =>
					{
						str = "какой-то поток, поспал 1 сек и выставил событие в сигнальное состояние. Смотрим кашу :)";
						ShowInOutput(str);
						sb.AppendLine(str);
						ShowOnScreen(str + Utils.crlf);

						mre.Set();
					});

				#endregion ManualResetEvent 2. Пример без Reset
			}
			else if (variant == 9)
			{
				#region Monitor .Wait/.Pulse

				string str = "Monitor .Wait/.Pulse";
				ShowInOutput(str);
				ShowOnScreen(Utils.crlf + str + Utils.crlf);
				Application.DoEvents();

				var locker = new object();
				Task.Factory.StartNew(
					() =>
					{
						string str1 = "поток 1 зашёл, будет захватывать locker";
						ShowInOutput(str1);
						ShowOnScreen(str1 + Utils.crlf);

						// или lock (locker) отсюда и до конца
						// т.к. Monitor.Wait/Pulse работают только внутри блокировки, т.е. внутри Monitor.Enter/Exit или блока lock
						Monitor.Enter(locker); // ждём пока не захватим блокировку

						str1 =
							"поток 1 захватил locker, Monitor.Wait(locker) освободил locker для других потоков и ждёт пока они отработают";
						ShowInOutput(str1);
						ShowOnScreen(str1 + Utils.crlf);
						// освобожадем блокировку, вызванную Monitor.Enter, и зависаем, пока не получим от другого потока Monitor.Pulse
						Monitor.Wait(locker);

						str1 = "поток 1, дождался окончания Monitor.Wait(locker) и спит 2 сек";
						ShowInOutput(str1);
						ShowOnScreen(str1 + Utils.crlf);
						Thread.Sleep(2000);

						str1 =
							"поток 1 Monitor.Pulse(locker), Monitor.Exit(locker) - конец блока кода с блокировкой. Pulse - на всяк случай, чтобы не завис никто";
						ShowInOutput(str1);
						ShowOnScreen(str1 + Utils.crlf);
						Monitor.Pulse(locker); // говорим, что мы всё закончили и разрешаем другим ожидающим потокам продолжить работу
						Monitor.Exit(locker);
					});
				Task.Factory.StartNew(
					() =>
					{
						string str2 = "поток 2, зашел, будет захватывать locker";
						ShowInOutput(str2);
						ShowOnScreen(str2 + Utils.crlf);

						// или lock (locker) отсюда и до конца
						// т.к. Monitor.Wait/Pulse работают только внутри блокировки, т.е. внутри Monitor.Enter/Exit или блока lock
						Monitor.Enter(locker);

						str2 = "поток 2 захватил locker и спит 1 сек";
						ShowInOutput(str2);
						ShowOnScreen(str2 + Utils.crlf);
						Thread.Sleep(1000);

						str2 = "поток 2 Monitor.Pulse(locker) освободил блокировку для других потоков";
						ShowInOutput(str2);
						ShowOnScreen(str2 + Utils.crlf);
						Monitor.Pulse(locker);

						// этот кусочек можно выключить для более быстрой отработки примера
						/**/
						str2 = "поток 2 Monitor.Wait(locker) - ждёт пока locker освободится";
						ShowInOutput(str2);
						ShowOnScreen(str2 + Utils.crlf);
						Monitor.Wait(locker);
						str2 = "поток 2 дождался окончания Monitor.Wait(locker)";
						ShowInOutput(str2);
						ShowOnScreen(str2 + Utils.crlf);
						/**/

						str2 =
							"поток 2 Monitor.Pulse(locker), Monitor.Exit(locker) - конец блока кода с блокировкой. Pulse - на всяк случай, чтобы не завис никто";
						ShowInOutput(str2);
						ShowOnScreen(str2 + Utils.crlf);
						Monitor.Pulse(locker);
						Monitor.Exit(locker);
					});

				#endregion Monitor .Wait/.Pulse
			}
			else if (variant == 10)
			{
				#region SpinLock .Enter/Exit <=> Monitor .Enter/Exit

				string str = "Сравнение Monitor .Enter/Exit и SpinLock .Enter/.Exit. SpinLock быстрее немного работает";
				ShowInOutput(str);
				ShowOnScreen(Utils.crlf + str + Utils.crlf + Utils.crlf);
				Application.DoEvents();

				var sw = new Stopwatch();
				sw.Start();
				var locker = new object();
				var m1 = Task.Factory.StartNew(
					() =>
					{
						string str1 = "поток 1 зашёл, будет захватывать locker";
						ShowInOutput(str1);
						ShowOnScreen(str1 + Utils.crlf);

						Monitor.Enter(locker);
						str1 = "поток 1 захватил locker, отписался, освободил locker, завершил работу";
						ShowInOutput(str1);
						ShowOnScreen(str1 + Utils.crlf);
						Monitor.Exit(locker);
					});
				var m2 = Task.Factory.StartNew(
					() =>
					{
						string str1 = "поток 2 зашёл, будет захватывать locker";
						ShowInOutput(str1);
						ShowOnScreen(str1 + Utils.crlf);

						Monitor.Enter(locker);
						str1 = "поток 2 захватил locker, отписался, освободил locker, завершил работу";
						ShowInOutput(str1);
						ShowOnScreen(str1 + Utils.crlf);
						Monitor.Exit(locker);
					});
				Task.WaitAll(new[] {m1, m2});
				sw.Stop();
				Application.DoEvents();
				str = "Пример с Monitor .Enter/Exit отработал за " + sw.ElapsedMilliseconds + " миллисек";
				ShowInOutput(str);
				ShowOnScreen(str + Utils.crlf + Utils.crlf);
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
						ShowOnScreen(str1 + Utils.crlf);

						locked = false; // true нельзя передать
						spinLock.Enter(ref locked);
						str1 = "поток 1 захватил spinLock, отписался, освободил spinLock, завершил работу";
						ShowInOutput(str1);
						ShowOnScreen(str1 + Utils.crlf);
						spinLock.Exit();
					});
				var s2 = Task.Factory.StartNew(
					() =>
					{
						string str1 = "поток 2 зашёл, будет захватывать spinLock";
						ShowInOutput(str1);
						ShowOnScreen(str1 + Utils.crlf);

						locked = false; // true нельзя передать
						spinLock.Enter(ref locked);
						str1 = "поток 2 захватил spinLock, отписался, освободил spinLock, завершил работу";
						ShowInOutput(str1);
						ShowOnScreen(str1 + Utils.crlf);
						spinLock.Exit();
					});
				Task.WaitAll(new[] {s1, s2});
				sw2.Stop();
				Application.DoEvents();
				str = "Пример с SpinLock .Enter/Exit отработал за " + sw2.ElapsedMilliseconds + "миллисек";
				ShowInOutput(str);
				ShowOnScreen(str + Utils.crlf);

				#endregion SpinLock .Enter/Exit <=> Monitor .Enter/Exit
			}
		}
	}
}