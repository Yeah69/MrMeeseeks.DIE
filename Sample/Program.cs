using System;
using System.Collections.Generic;
using System.Threading.Tasks;

internal class Program
{
    private static void Main()
    {
        try
        {
            //using var container = Container.DIE_CreateContainer(); 
            //var asdf = container.Create()(3);
            
            Console.WriteLine("Hello, World!");
        }
        catch (Exception)
        {
            // ignored
        }
    }
    
    public static void Dispose(List<List<object>> disposables)
    {
        var aggregateException = new AggregateException(Inner(disposables));
        if (aggregateException.InnerExceptions.Count > 0)
            throw aggregateException;

        return;

        static IEnumerable<Exception> Inner(List<List<object>> disposables)
        {
            for (var i = disposables.Count - 1; i >= 0; i--)
            {
                /*foreach (var exception in DisposeChunk(disposables[i]))
                {
                    yield return exception;
                }*/
            }
            yield break;
        }
    }
    
    public static async IAsyncEnumerable<Exception> DisposeChunkAsyncEnumerable(List<object> disposables)
    {
        for (var i = disposables.Count - 1; i >= 0; i--)
        {
            switch (disposables[i])
            {
                case IDisposable disposable:
                    if (MrMeeseeks.DIE.DisposeUtility_0_0.Dispose_0_1(disposable) is { } exception_0)
                        yield return exception_0;
                    break;
                case IAsyncDisposable asyncDisposable:
                    if (await MrMeeseeks.DIE.DisposeUtility_0_0.DisposeAsync_0_2(asyncDisposable) is { } exception_1)
                        yield return exception_1;
                    break;
            }
        }
    }
    
    public static async ValueTask<List<Exception>> DisposeChunkAsync(List<object> disposables)
    {
        List<Exception> exceptions = new List<Exception>();
        for (var i = disposables.Count - 1; i >= 0; i--)
        {
            switch (disposables[i])
            {
                case IDisposable disposable:
                    if (MrMeeseeks.DIE.DisposeUtility_0_0.Dispose_0_1(disposable) is { } exception_0)
                        exceptions.Add(exception_0);
                    break;
                case IAsyncDisposable asyncDisposable:
                    if (await MrMeeseeks.DIE.DisposeUtility_0_0.DisposeAsync_0_2(asyncDisposable) is { } exception_1)
                        exceptions.Add(exception_1);
                    break;
            }
        }

        return exceptions;
    }
}