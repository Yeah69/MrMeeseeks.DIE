using System;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.DIE.UserUtility;

namespace MrMeeseeks.DIE.Sample;

internal sealed class Disposable : IDisposable { public void Dispose() { } }

internal sealed class Class(ClassBelow _, Disposable __) : ITransientScopeRoot;

internal sealed class ClassBelow(ClassS _, Disposable __) : ITransientScopeRoot;

internal sealed class ClassS(ClassBelowS _, Disposable __) : IScopeRoot;

internal sealed class ClassBelowS(Disposable __) : IScopeRoot;

[CreateFunction(typeof(Class), "Create")]
internal sealed partial class Container;
