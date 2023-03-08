void Main()
{
	for (int i = 0; i < 100; i++)
	{
		var num = int.Parse(i.ToString());

		Task.Factory.StartNew(async () =>
		{
			// 方法将以同步方式运行，等待时不会让出线程
			// Task.Delay((100 - num) * 100).Wait();
			
			// 方法将以异步方式运行，等待时让出线程
			await Task.Delay((100 - num) * 100);
			
			// 等待时不会让出线程
			// SpinWait.SpinUntil(()=>false,(100 - num) * 100);
			
			Console.WriteLine(num);
		});
	}
}
