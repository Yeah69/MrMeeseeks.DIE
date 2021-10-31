using MrMeeseeks.DIE.Sample;
using StrongInject;

System.Console.WriteLine("Hello, world!");
System.Console.WriteLine(new Container().Run((c, _) => c.Text, new object()));
System.Console.WriteLine(new StrongInjectContainer().Run(c => c.Text));

