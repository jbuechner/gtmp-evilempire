using System;

namespace gtmp.evilempire.server.launcher
{
    public static class ConsoleExtensions
    {
        public static IDisposable Foreground(this ConsoleColor color)
        {
            var previous = Console.ForegroundColor;
            Console.ForegroundColor = color;
            return new DelegatedDisposable(() => Console.ForegroundColor = previous);
        }
    }
}