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
	
	// Task.Run(A)相当于Task.Factory.StartNew(A, CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
	// Task.Factory.StartNew(A)相当于Task.Factory.StartNew(A, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Current);
	// 下面childTask不会被等待完成，即使使用了TaskCreationOptions.AttachedToParent
	var parentTask = Task.Run(() =>
		{
			var childTask = new Task(() =>
			{
				Thread.Sleep(3000);
				Console.WriteLine("Child task finished.");
			}, TaskCreationOptions.AttachedToParent);
			childTask.Start();

			Console.WriteLine("Parent task finished.");
		});

	parentTask.Wait();
	Console.WriteLine("Main thread finished.");
}
