﻿using System.Collections.Generic;
using MrMeeseeks.DIE.Configuration.Attributes;

namespace MrMeeseeks.DIE.Sample;

internal class Class<TVanilla, TExactMatch, TMoreStrict>
    where TExactMatch : struct 
    where TMoreStrict : class, IList<TVanilla>, new()
{
    internal required IList<TVanilla> ListVanilla { get; init; }
    internal required IList<TExactMatch> ListExactMatch { get; init; }
    internal required IList<TMoreStrict> ListMoreStrict { get; init; }
}

[CreateFunction(typeof(Class<,,>), "Create")]
internal sealed partial class Container
{
    private Container() {}

    [UserDefinedPropertiesInjection(typeof(Class<,,>))]
    private void DIE_Props<TVanilla, TExactMatch, TLessStrict>(
        out IList<TVanilla> ListVanilla,
        out IList<TExactMatch> ListExactMatch,
        out IList<TLessStrict> ListMoreStrict)
        where TExactMatch : struct 
        where TLessStrict : class
    {
        ListVanilla = new List<TVanilla>();
        ListExactMatch = new List<TExactMatch>();
        ListMoreStrict = new List<TLessStrict>();
    }
}
//*/