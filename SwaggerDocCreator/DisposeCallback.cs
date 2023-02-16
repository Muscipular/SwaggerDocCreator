using System;

namespace SwaggerDocCreator;

class DisposeCallback : IDisposable
{
    private Action _action;

    public DisposeCallback(Action action)
    {
        _action = action;
    }

    public void Dispose()
    {
        _action?.Invoke();
    }
}