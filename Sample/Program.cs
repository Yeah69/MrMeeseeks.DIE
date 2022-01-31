using System;
using MrMeeseeks.DIE;
using MrMeeseeks.DIE.Test.ScopeSpecificAttributesTestsWithDecorator;

Console.WriteLine("Hello, world!");
var container = new Container();
Console.WriteLine(((IContainer<TransientScopeRootSpecific>) container).Resolve());