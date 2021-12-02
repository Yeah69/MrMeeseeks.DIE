using System.Collections.Generic;
using MrMeeseeks.DIE.Sample;

System.Console.WriteLine("Hello, world!");
using var container = new DecoratorMultiContainer();
System.Console.WriteLine(((MrMeeseeks.DIE.IContainer<IReadOnlyList<IDecoratedMulti>>) container).Resolve());