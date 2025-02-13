using System.Collections.Generic;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration.Attributes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace MrMeeseeks.DIE.Test.Implementation.Choice.CollectionPreservesOrder;

internal class Class;

internal sealed class SubClassA : Class;
internal sealed class SubClassB : Class;
internal sealed class SubClassC : Class;
internal sealed class SubClassD : Class;
internal sealed class SubClassE : Class;
internal sealed class SubClassF : Class;
internal sealed class SubClassG : Class;
internal sealed class SubClassH : Class;
internal sealed class SubClassI : Class;
internal sealed class SubClassJ : Class;
internal sealed class SubClassK : Class;
internal sealed class SubClassL : Class;
internal sealed class SubClassM : Class;
internal sealed class SubClassN : Class;
internal sealed class SubClassO : Class;
internal sealed class SubClassP : Class;
internal sealed class SubClassQ : Class;
internal sealed class SubClassR : Class;
internal sealed class SubClassS : Class;
internal sealed class SubClassT : Class;
internal sealed class SubClassU : Class;
internal sealed class SubClassV : Class;
internal sealed class SubClassW : Class;
internal sealed class SubClassX : Class;
internal sealed class SubClassY : Class;
internal sealed class SubClassZ : Class;

[ImplementationCollectionChoice(
    typeof(Class), 
    typeof(SubClassA), typeof(SubClassB), typeof(SubClassC), typeof(SubClassD), typeof(SubClassE), typeof(SubClassF), 
    typeof(SubClassG), typeof(SubClassH), typeof(SubClassI), typeof(SubClassJ), typeof(SubClassK), typeof(SubClassL), 
    typeof(SubClassM), typeof(SubClassN), typeof(SubClassO), typeof(SubClassP), typeof(SubClassQ), typeof(SubClassR), 
    typeof(SubClassS), typeof(SubClassT), typeof(SubClassU), typeof(SubClassV), typeof(SubClassW), typeof(SubClassX), 
    typeof(SubClassY), typeof(SubClassZ))]
[CreateFunction(typeof(IReadOnlyList<Class>), "Create")]
internal sealed partial class Container;

public sealed class Tests
{
    [Fact]
    public async Task Test()
    {
        await using var container = Container.DIE_CreateContainer();
        var instances = container.Create();
        Assert.True(instances is [SubClassA, SubClassB, SubClassC, SubClassD, SubClassE, SubClassF, SubClassG, 
            SubClassH, SubClassI, SubClassJ, SubClassK, SubClassL, SubClassM, SubClassN, SubClassO, SubClassP, 
            SubClassQ, SubClassR, SubClassS, SubClassT, SubClassU, SubClassV, SubClassW, SubClassX, SubClassY, 
            SubClassZ]);
    }
}