using MrMeeseeks.DIE.Sample;

System.Console.WriteLine("Hello, world!");
using var container = new DecoratorScopeRootContainer();
System.Console.WriteLine(((MrMeeseeks.DIE.IContainer<IDecoratedScopeRoot>) container).Resolve());