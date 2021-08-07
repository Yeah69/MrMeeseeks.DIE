using Microsoft.CodeAnalysis;
using System;

namespace MrMeeseeks.DIE
{
    public interface IInitialize
    {
        void Initialize();
    }

    class InitializeImpl : IInitialize
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
