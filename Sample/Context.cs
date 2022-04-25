﻿using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Sample;
 
namespace MrMeeseeks.DIE.Test.Generics.Interface_SingleAndImplementationDoubleButOneFixed;

internal interface IInterface<T0> {}

internal abstract class BaseClass<T0, T1> : IInterface<T0> {}

internal class Class<T0> : BaseClass<T0, string> {}

[CreateFunction(typeof(IInterface<int>), "Create")]
internal partial class Container {}