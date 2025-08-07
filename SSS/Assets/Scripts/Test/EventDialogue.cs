using UnityEditor.Rendering;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "Warrior", menuName = "RPG/Classes/Warrior")]
public class EventDialogue : BaseEvent
{
    public int mudadayo = 5;

    public override string ClassName { get { return "Dialogue"; } set { className = value; } }

    public override void DrawEditorGUI()
    {
        mudadayo = EditorGUILayout.IntField("Duration", mudadayo);
    }
}
