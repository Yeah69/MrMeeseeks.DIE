using System;
using MrMeeseeks.DIE;
using MrMeeseeks.DIE.Sample;

Console.WriteLine("Hello, world!");
var container = new TransientScopeInstanceContainer();
Console.WriteLine(((IContainer<ITransientScopeWithTransientScopeInstance>) container).Resolve());