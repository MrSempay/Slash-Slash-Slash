using System.Collections.Generic;
using System;
using UnityEngine;

public class CleanupManager
{
    private static readonly List<IDisposable> _disposablesAll = new();
    private static readonly List<IDisposable> _disposablesScene = new();

    public static void Register(IDisposable disposable)
    {
        _disposablesAll.Add(disposable);
    }
    public static void RegisterDisposeSceneChanged(IDisposable disposable)
    {
        _disposablesScene.Add(disposable);
    }

    public static void DisposeAll()
    {
        foreach (var d in _disposablesAll)
            d.Dispose();
        foreach (var d in _disposablesScene)
            d.Dispose();

        _disposablesAll.Clear();
        _disposablesScene.Clear();
    }

    public static void DisposeSceneDisposes()
    {
        foreach (var d in _disposablesScene)
            d.Dispose();

        _disposablesScene.Clear();
    }
}
