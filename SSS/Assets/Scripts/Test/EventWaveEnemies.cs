using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "Mage", menuName = "RPG/Classes/Mage")]
public class EventWaveEnemies : BaseEvent
{
    public int arara = 3;

    public override string ClassName { get { return "WaveEnemies"; } set { className = value; } }

    public override void DrawEditorGUI()
    {
#if UNITY_EDITOR
        arara = EditorGUILayout.IntField("Duration", arara);
#endif
    }

}
