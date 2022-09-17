# Welcome to MrMeeseeks.DIE

DIE is a secret agency organized by a bunch of Mr. Meeseekses. Its goal is to gather the information necessary to resolve your dependencies. Therefore, …

> The acronym DIE stands for **D**ependency **I**njection DI**E**

Let the secret agency DIE compose these info in order to build factory methods which create instances of types of your choice.

## Introduction

MrMeeseeks.DIE (in this documentation just referred to as DIE) is a compile-time dependency injection container for .Net. As such it generates factory methods which create instances that you need. Instead of relying on reflection the generated code uses the good old `new` operator to create instances like you would probably do yourself if you'd create a pure DI container.

## Nuget

The easiest way of using DIE is by getting it through nuget. Here is the package page:

https://www.nuget.org/packages/MrMeeseeks.DIE/

Either search for `MrMeeseeks.DIE` in the nuget manager of the IDE of your choice.

Or call following PowerShell-command:

> Install-Package MrMeeseeks.DIE

Or manually insert the package reference into the target `.csproj`:

```xml
<PackageReference Include="MrMeeseeks.DIE" Version="[preferrably the current version]" />
```

## Documentation

Please visit https://die.mrmeeseeks.dev/ for a documentation.
