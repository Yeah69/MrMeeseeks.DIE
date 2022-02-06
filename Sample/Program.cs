using System;
using MrMeeseeks.DIE;
using MrMeeseeks.DIE.Sample;

Console.WriteLine("Hello, world!");
var container = new Container();
Console.WriteLine(((IContainer<Dependency>) container).Resolve());