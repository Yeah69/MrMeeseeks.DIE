using System;
using System.Collections.Generic;
using Xunit;
using MrMeeseeks.DIE.Configuration.Attributes;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Generics.OpenGenericCreate.UserDefinedElements.InitParams;

internal sealed class Class<TVanilla, TExactMatch, TMoreStrict>
    where TExactMatch : struct 
    where TMoreStrict : class, IList<TVanilla>, new()
{
    internal void Initialize(
        IList<TVanilla> listVanilla,
        IList<TExactMatch> listExactMatch,
        IList<TMoreStrict> listLessStrict) { }
}

[Initializer(typeof(Class<,,>), "Initialize")]
[CreateFunction(typeof(Class<,,>), "Create")]
internal sealed partial class Container
{

    [UserDefinedInitializerParametersInjection(typeof(Class<,,>))]
    private void DIE_InitParams<TVanilla, TExactMatch, TLessStrict>(
        out IList<TVanilla> listVanilla,
        out IList<TExactMatch> listExactMatch,
        out IList<TLessStrict> listLessStrict)
        where TExactMatch : struct 
        where TLessStrict : class
    {
        listVanilla = new List<TVanilla>();
        listExactMatch = new List<TExactMatch>();
        listLessStrict = new List<TLessStrict>();
    }
}

public sealed class Tests
{
    [Fact]
    public void Test()
    {
        using var container = Container.DIE_CreateContainer();
        var instance = container.Create<int, DateTime, List<int>>();
    }
}