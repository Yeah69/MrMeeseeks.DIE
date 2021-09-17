﻿using Microsoft.CodeAnalysis;
using System;

namespace MrMeeseeks.DIE.Spy
{
    internal interface IInitialize
    {
        void Initialize();
    }

    internal class InitializeImpl : IInitialize
    {
        private readonly GeneratorInitializationContext _context;
        private readonly Func<ISyntaxReceiver> _syntaxReceiverFactory;

        public InitializeImpl(
            GeneratorInitializationContext context,
            Func<ISyntaxReceiver> syntaxReceiverFactory)
        {
            _context = context;
            _syntaxReceiverFactory = syntaxReceiverFactory;
        }

        public void Initialize() => _context.RegisterForSyntaxNotifications(() => _syntaxReceiverFactory());
    }
}
