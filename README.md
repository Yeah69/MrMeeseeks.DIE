# Welcome to MrMeeseeks.DIE

[![Gitter](https://img.shields.io/gitter/room/nwjs/nw.js.svg)](https://matrix.to/#/#Yeah69_MrMeeseeks.DIE:gitter.im)
[![SOQnA](https://img.shields.io/badge/StackOverflow-QnA-green.svg)](http://stackoverflow.com/questions/tagged/MrMeeseeks.DIE)

DIE is a secret agency organized by a bunch of Mr. Meeseekses. Its goal is to gather the information necessary to resolve your dependencies. Therefore, …

> The acronym DIE stands for **D**ependency **I**njection DI**E**

Let the secret agency DIE compose these info in order to build factory methods which create instances of types of your choice.

## Introduction

MrMeeseeks.DIE (in this documentation just referred to as DIE) is a compile-time dependency injection container for .Net. As such it generates factory methods which create instances that you need. Instead of relying on reflection the generated code uses the good old `new` operator to create instances like you would probably do yourself if you'd create a pure DI container.

## Nuget

The easiest way to use DIE is to get it via nuget. Here is the package page:

https://www.nuget.org/packages/MrMeeseeks.DIE/

Either search for MrMeeseeks.DIE in the nuget manager of the IDE of your choice.

Or call the following PowerShell command:

```powershell
Install-Package MrMeeseeks.DIE
```

Or manually insert the package reference into the target `.csproj`:

```xml
<PackageReference Include="MrMeeseeks.DIE" Version="[preferrably the current version]" />
```

Or manually add the package reference to the target `.csproj`:

```xml
<PackageReference Include="MrMeeseeks.DIE" Version="[preferrably the current version]" />
```

## Characteristics Of DIE

- Compile-Time Code Generation
    - Incomplete configurations will most likely result in a failed build
- Unambiguousness
    - Container doesn't resolve ambiguity through assumptions
    - Configuration features to resolve ambiguities
- Convenience
    - Default behaviors designed to reduce the amount of configuration required
    - Optional marker interfaces can be used for configurations
    - Mass configuration (e.g., register all implementations with a single configuration)
- Flexibility
    - Allows opt-in configuration style
    - Allows opt-out configuration style
- Feature richness
    - Scoping
    - Async support
    - Generics support
    - User-defined elements (factories, custom parameters, …)
    - Generated factories (Func<…>, Lazy<…>)
    - Decorators & Composites
    - Collection injections (IEnumerable<…>, IAsyncEnumerable<…>, IList<…> and many more)
- Maximum transparency
    - Only your configuration code needs to know about DIE
    - The rest of your code base can remain oblivious

## Documentation

Please visit https://die.mrmeeseeks.dev/ for a documentation.
