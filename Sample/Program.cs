using MrMeeseeks.DIE.Sample;
using StrongInject;

System.Console.WriteLine("Hello, world!");
{
    using var container = new Container();
    System.Console.WriteLine(((MrMeeseeks.DIE.IContainer<IContext>) container).Resolve().Text);
}
{
    /*
    using var strongInjectContainer = new StrongInjectContainer();
    using var owned = strongInjectContainer.Resolve();
    System.Console.WriteLine(owned.Value.Text);
    */
}

