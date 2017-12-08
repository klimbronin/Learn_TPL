using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication2
{
	/// <summary>
	/// Пример с Mutex
	/// </summary>
	public partial class Form1
	{
		private bool flagMutex;

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

				ShowOnScreen("Режим тестирования Mutex. Это второй процесс" + Utils.crlf);

				Task.Factory.StartNew(
					() =>
					{
						ShowOnScreen("Ждём освобождения KlimMutex, в первом процессе можно нажать Release Mutex. Это второй процесс" +
						             Utils.crlf);
						Mutex m;
						if (Mutex.TryOpenExisting("KlimMutex", MutexRights.FullControl, out m))
						{
							m.WaitOne();
							ShowOnScreen("Дождались освобождения KlimMutex. Это второй процесс" + Utils.crlf);
							this.TopMost = true;
						}
						else
						{
							ShowOnScreen("Не найден KlimMutex, длеть нечего :). Это второй процесс" + Utils.crlf);
						}
					});
			}
		}

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
				ShowOnScreen(Utils.crlf + "Создаём KlimMutex и захватываем его" + Utils.crlf +
				             "Запускаем другой процесс, который будет ждать освобождения KlimMutex" + Utils.crlf +
				             "Ждите появления второго окна с инcтрукциями" + Utils.crlf+
							 "Окна не закрывать до выполнения инструкции - обработчиков нет :)" + Utils.crlf);

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
	}
}