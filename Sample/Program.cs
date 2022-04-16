using System;

internal class Program
{
    private static void Main()
    {
        Console.WriteLine("Hello, world!");
    }

    private class NopDisposable : global::System.IDisposable
    {
        internal static global::System.IDisposable Instance { get; } = new NopDisposable();
        public void Dispose()
        {
        }
    }

    private class NopAsyncDisposable : global::System.IAsyncDisposable
    {
        internal static global::System.IAsyncDisposable Instance { get; } = new NopAsyncDisposable();
        public global::System.Threading.Tasks.ValueTask DisposeAsync() => global::System.Threading.Tasks.ValueTask.CompletedTask;
    }

    private class SyncToAsyncDisposable : global::System.IAsyncDisposable
    {
        private readonly IDisposable _disposable;

        internal SyncToAsyncDisposable(IDisposable disposable) => _disposable = disposable;

        public global::System.Threading.Tasks.ValueTask DisposeAsync()
        {
            _disposable.Dispose();
            return global::System.Threading.Tasks.ValueTask.CompletedTask;
        }
    }
}