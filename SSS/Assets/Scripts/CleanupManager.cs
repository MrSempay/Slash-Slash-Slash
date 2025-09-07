using System.Collections.Generic;
using System;
using UnityEngine;

public class CleanupManager
{
    private static readonly List<IDisposable> _disposables = new();

    public static void Register(IDisposable disposable)
    {
        _disposables.Add(disposable);
    }

    public static void DisposeAll()
    {
        foreach (var d in _disposables)
            d.Dispose();

        _disposables.Clear();
    }
}
