using Microsoft.CodeAnalysis;
using System;

namespace MrMeeseeks.DIE.Spy
{
    internal interface IInitialize
    {
        void Initialize();
    }

    internal class InitializeImpl : IInitialize
    {
        private readonly GeneratorInitializationContext context;
        private readonly Func<ISyntaxReceiver> syntaxReceiverFactory;

        public InitializeImpl(
            GeneratorInitializationContext context,
            Func<ISyntaxReceiver> syntaxReceiverFactory)
        {
            this.context = context;
            this.syntaxReceiverFactory = syntaxReceiverFactory;
        }

        public void Initialize() => context.RegisterForSyntaxNotifications(() => syntaxReceiverFactory());
    }
}
