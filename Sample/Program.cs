using System;
using System.Threading.Tasks;
using MrMeeseeks.DIE.Sample;

internal class Program
{
    private static void Main()
    {
        try
        {
            using var container = Container.DIE_CreateContainer();
            var asdf = container.Create();
            Console.WriteLine("Hello, World!");
            var threadLocal = asdf.Dependency;
            
            // Action that prints out ThreadName for the current thread
            var action = () =>
            {
                // If ThreadName.IsValueCreated is true, it means that we are not the
                // first action to run on this thread.
                var repeat = threadLocal.IsValueCreated;

                Console.WriteLine("ThreadName = {0} {1}", threadLocal.Value?.Value.Value, repeat ? "(repeat)" : "");
            };

            // Launch eight of them.  On 4 cores or less, you should see some repeat ThreadNames
            Parallel.Invoke(action, action, action, action, action, action, action, action);
        }
        catch (Exception)
        {
            // ignored
        }
    }
}