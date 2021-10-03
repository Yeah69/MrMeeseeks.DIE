using MrMeeseeks.DIE.Sample;
using StrongInject;

System.Console.WriteLine("Hello, world!");
//System.Console.WriteLine(new Container().Resolve().Text);
System.Console.WriteLine(new StrongInjectContainer().Run(c => c.Text));

