using System;
using MrMeeseeks.DIE;
using MrMeeseeks.DIE.Sample;

Console.WriteLine("Hello, world!");
var container = new ConstructorChoiceContainer();
Console.WriteLine(((IContainer<DateTime>) container).Resolve());