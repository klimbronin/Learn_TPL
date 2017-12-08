using System.Threading;
using System.Threading.Tasks;

namespace WindowsFormsApplication2
{
	internal class AsyncAwait
	{
		/// <summary>
		/// </summary>
		public async void DoAsyncAwait(Form1 formOutput, int sleep)
		{
			formOutput.ShowOnScreen(Utils.crlf + "Начало выполнения async-метода. ThreadId - " +
			                        Thread.CurrentThread.ManagedThreadId + Utils.crlf);
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

			formOutput.ShowOnScreen("await-строка вернула: " + res + Utils.crlf);
			formOutput.ShowOnScreen("Выполнение async-метода после await. ThreadId - " + Thread.CurrentThread.ManagedThreadId +
			                        Utils.crlf);
		}

		private Task<string> GoAwait(int sleep)
		{
			return Task.Factory.StartNew(() => SomeMethod(sleep));
			// тоже самое, но подлиннее
			// return Task.Factory.StartNew(() => { return SomeMethod(sleep); });
		}

		private string SomeMethod(int sleep)
		{
			Thread.Sleep(sleep*1000);
			return $"Task из await-части. Поспал {sleep} сек. Завершился. ThreadId - " +
			       Thread.CurrentThread.ManagedThreadId;
		}

		/// <summary>
		/// Для демонстрации блокировки
		/// </summary>
		public async Task<int> MethodAsync()
		{
			// есть блокировка
			await Task.Delay(200);
			// тоже самое, тоже есть блокировка
			// await Task.Delay(200).ConfigureAwait(true);

			// у тут уже нет блокировки
			//await Task.Delay(200).ConfigureAwait(false);

			return 1;
		}
	}
}