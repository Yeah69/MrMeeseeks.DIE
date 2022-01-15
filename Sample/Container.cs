﻿using MrMeeseeks.DIE;
using MrMeeseeks.DIE.Sample;

[assembly:ContainerInstanceAggregation(typeof(IContainerInstance))]
[assembly:ScopeInstanceAggregation(typeof(IScopeInstance))]
[assembly:ScopeRootAggregation(typeof(IScopeRoot))]
[assembly:TransientAggregation(typeof(ITransient))]
[assembly:DecoratorAggregation(typeof(IDecorator<>))]
[assembly:CompositeAggregation(typeof(IComposite<>))]