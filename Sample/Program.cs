using System;
using MrMeeseeks.DIE;
using MrMeeseeks.DIE.Test.ScopeSpecificAttributesTestsWithImplementations;

Console.WriteLine("Hello, world!");
var container = new Container();
Console.WriteLine(((IContainer<TransientScope>) container).Resolve());