using System;

namespace gtmp.evilempire.server.launcher
{
    public sealed class DelegatedDisposable : IDisposable
    {
        readonly Action _action;

        public DelegatedDisposable(Action action)
        {
            this._action = action;
        }

        public void Dispose()
        {
            this._action?.Invoke();
        }
    }
}