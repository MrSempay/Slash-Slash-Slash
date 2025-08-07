using UnityEngine;

public abstract class BaseEvent : ScriptableObject
{
    protected string className;
    public abstract string ClassName { get; set; }
    public virtual void DrawEditorGUI() { }
}
