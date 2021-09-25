using Microsoft.CodeAnalysis;

namespace MrMeeseeks.DIE
{
    internal record WellKnownTypes(
        INamedTypeSymbol Container,
        INamedTypeSymbol SpyAttribute,
        INamedTypeSymbol Disposable,
        INamedTypeSymbol AsyncDisposable,
        INamedTypeSymbol ValueTask,
        INamedTypeSymbol ValueTask1,
        INamedTypeSymbol Task1,
        INamedTypeSymbol ObjectDisposedException)
    {
        public static bool TryCreate(Compilation compilation, out WellKnownTypes wellKnownTypes)
        {
            var iContainer = compilation.GetTypeOrReport("MrMeeseeks.DIE.IContainer`1");
            var iDisposable = compilation.GetTypeOrReport("System.IDisposable");
            var iAsyncDisposable = compilation.GetTypeOrReport("System.IAsyncDisposable");
            var valueTask = compilation.GetTypeOrReport("System.Threading.Tasks.ValueTask");
            var valueTask1 = compilation.GetTypeOrReport("System.Threading.Tasks.ValueTask`1");
            var task1 = compilation.GetTypeOrReport("System.Threading.Tasks.Task`1");
            var objectDisposedException = compilation.GetTypeOrReport("System.ObjectDisposedException");

            var spyAttribute = compilation
                .GetTypeByMetadataName(typeof(SpyAttribute).FullName ?? "");

            if (iContainer is null
                || spyAttribute is null
                || iDisposable is null
                || iAsyncDisposable is null
                || valueTask is null
                || valueTask1 is null
                || task1 is null
                || objectDisposedException is null)
            {
                wellKnownTypes = null!;
                return false;
            }

            wellKnownTypes = new WellKnownTypes(
                Container: iContainer,
                SpyAttribute: spyAttribute,
                Disposable: iDisposable,
                AsyncDisposable: iAsyncDisposable,
                ValueTask: valueTask,
                ValueTask1: valueTask1,
                Task1: task1,
                ObjectDisposedException: objectDisposedException);

            return true;
        }
    }
}