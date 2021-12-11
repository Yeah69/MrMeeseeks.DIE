using System;
using MrMeeseeks.DIE;
using MrMeeseeks.DIE.Sample;

Console.WriteLine("Hello, world!");
var container = new ValueTupleContainer();
Console.WriteLine(((IContainer<IValueTupleBase>) container).Resolve());