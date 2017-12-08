using System.Threading.Tasks;

namespace WindowsFormsApplication2
{
	/// <summary>
	/// </summary>
	public class Threads_and_Variables
	{
		private readonly Form1 m_formOutput;

		private class ClassVar
		{
			/// <summary>
			/// </summary>
			public int Count;
		}

		/// <summary>
		/// Конструктор
		/// </summary>
		public Threads_and_Variables(Form1 formOutput)
		{
			m_formOutput = formOutput;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="variant"></param>
		public void Do(int variant)
		{
			Task t2;
			int count;
			string str;
			string res = null;

			if (variant == 0 || variant == 1)
			{
				count = 6;
				str = Utils.crlf +
				      "2 потока выводят числа с 1 до N = 6, N передаётся в метод как параметр, N-- и вывод на кран." +
				      Utils.crlf + "Потоки не влияют друг на друга" + Utils.crlf;
				m_formOutput.ShowOnScreen(str);
				m_formOutput.ShowInOutput(str);

				// фоновый поток
				// вариант1
				t2 = Task.Factory.StartNew(() => { res = Go(count); });
				// вариант2
				//var tcs = new TaskCompletionSource<string>();
				//Task.Factory.StartNew(() => { tcs.SetResult(Go(count)); });

				// главный поток
				m_formOutput.ShowOnScreen(Go(count) + Utils.crlf);

				// вывод результата фонового потока
				// вариант 1
				Task.WaitAll(new[] {t2});
				m_formOutput.ShowOnScreen(res + Utils.crlf);
				// вариант 2
				//m_formOutput.ShowOnScreen(tcs.Task.Result + Utils.crlf);

				str = "--------------" + Utils.crlf;
				m_formOutput.ShowOnScreen(str);
				m_formOutput.ShowInOutput(str);
			}

			if (variant == 0 || variant == 2)
			{
				str = Utils.crlf +
				      "2 потока выводят числа с 1 до N = 6, N передаётся в метод по ссылке, N-- и вывод на кран." + Utils.crlf +
				      "Потоки работают с одной и тоже N и влияют друг на друга" + Utils.crlf +
				      "Выводятся 12345 / 1234 или 1234 / 12345 и даже 1234 / 1234 - как повезёт" + Utils.crlf;
				m_formOutput.ShowOnScreen(str);
				m_formOutput.ShowInOutput(str);
				count = 6;
				t2 = Task.Factory.StartNew(() => { res = GoRef(ref count) + Utils.crlf; });
				m_formOutput.ShowOnScreen(GoRef(ref count) + Utils.crlf);
				Task.WaitAll(new[] {t2});
				m_formOutput.ShowOnScreen(res);
				str = "--------------" + Utils.crlf;
				m_formOutput.ShowOnScreen(str);
				m_formOutput.ShowInOutput(str);
			}
			if (variant == 0 || variant == 3)
			{
				str = Utils.crlf +
				      "2 потока выводят числа с 1 до ClassVar.Count, ClassVar передаётся в метод как параметр, ClassVar.Count-- и вывод на кран." +
				      Utils.crlf +
				      "Потоки работаю с одним и тем же экземпляром ClassVar и вляяют друг на друга с соответствующими последствиями" +
				      Utils.crlf +
				      "Выводятся 12345 / 1234 или 1234 / 12345 и даже 1234 / 1234 - как повезёт" + Utils.crlf;
				m_formOutput.ShowOnScreen(str);
				m_formOutput.ShowInOutput(str);
				ClassVar classVar = new ClassVar {Count = 6};
				t2 = Task.Factory.StartNew(() => { res = GoClassParam(classVar) + Utils.crlf; });
				m_formOutput.ShowOnScreen(GoClassParam(classVar) + Utils.crlf);
				Task.WaitAll(new[] {t2});
				m_formOutput.ShowOnScreen(res);
				str = "--------------" + Utils.crlf;
				m_formOutput.ShowOnScreen(str);
				m_formOutput.ShowInOutput(str);
			}
		}

		private string Go(int count)
		{
			string str = "";
			count--;
			for (int i = 1; i <= count; i++)
			{
				str += i.ToString();
			}
			m_formOutput.ShowInOutput(str);
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
			m_formOutput.ShowInOutput(str);
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
			m_formOutput.ShowInOutput(str);
			return str;
		}
	}
}