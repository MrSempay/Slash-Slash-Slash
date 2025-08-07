#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Linq;

[CustomEditor(typeof(ScenarioCreator))]
public class ScenarioCreatorEditor : Editor
{
    private SerializedProperty eventsProp;
    private Type[] eventTypes;
    private string[] typeNames;
    private int selectedTypeIndex = 0;
    private int dragFromIndex = -1;
    private Vector2 scrollPos;

    private void OnEnable()
    {
        eventsProp = serializedObject.FindProperty("events");
        CacheEventTypes();
    }

    private void CacheEventTypes()
    {
        Type baseType = typeof(BaseEvent);
        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => baseType.IsAssignableFrom(p) && !p.IsAbstract)
            .ToArray();

        eventTypes = types;
        typeNames = types.Select(t => t.Name.Replace("Event", "")).ToArray();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // 1. Выбор типа для добавления
        EditorGUILayout.BeginHorizontal();
        selectedTypeIndex = EditorGUILayout.Popup(selectedTypeIndex, typeNames);
        if (GUILayout.Button("Add New", GUILayout.Width(100)))
        {
            AddNewEvent(eventTypes[selectedTypeIndex]);
        }
        EditorGUILayout.EndHorizontal();

        // 2. Отображение списка с возможностью прокрутки
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Events:", EditorStyles.boldLabel);

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        for (int i = 0; i < eventsProp.arraySize; i++)
        {
            Rect eventRect = EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawEventElement(i, eventRect);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();

            // Обработка перетаскивания
            HandleDragAndDrop(i, eventRect);
        }
        EditorGUILayout.EndScrollView();

        serializedObject.ApplyModifiedProperties();
    }

    private void AddNewEvent(Type eventType)
    {
        var newEvent = CreateInstance(eventType) as BaseEvent;
        newEvent.name = $"{eventType.Name}_{Guid.NewGuid().ToString("N").Substring(0, 4)}";
        newEvent.ClassName = typeNames[selectedTypeIndex]; 

        string path = GetUniqueAssetPath($"{eventType.Name}");
        AssetDatabase.CreateAsset(newEvent, path);

        eventsProp.arraySize++;
        eventsProp.GetArrayElementAtIndex(eventsProp.arraySize - 1).objectReferenceValue = newEvent;

        AssetDatabase.SaveAssets();
    }

    private void DrawEventElement(int index, Rect itemRect)
    {
        SerializedProperty eventProp = eventsProp.GetArrayElementAtIndex(index);
        BaseEvent evt = (BaseEvent)eventProp.objectReferenceValue;

        if (evt == null)
        {
            EditorGUILayout.HelpBox("Missing Event!", MessageType.Error);
            return;
        }

        // Фиксированная ширина для всего элемента (например, 300 пикселей)
        EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(170));

        // Заголовок с кнопкой удаления
        EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

        // Иконка для перетаскивания
        GUIStyle dragHandle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fixedWidth = 20
        };
        EditorGUILayout.LabelField("≡", dragHandle);

        EditorGUILayout.LabelField(evt.ClassName, EditorStyles.boldLabel, GUILayout.ExpandWidth(true));

        if (GUILayout.Button("×", GUILayout.Width(20)))
        {
            RemoveEvent(index, evt);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            return;
        }
        EditorGUILayout.EndHorizontal();

        // Редактирование параметров
        Editor editor = Editor.CreateEditor(evt);
        editor.OnInspectorGUI();

        EditorGUILayout.EndVertical();
    }

    private void HandleDragAndDrop(int currentIndex, Rect itemRect)
    {
        Event current = Event.current;

        switch (current.type)
        {
            case EventType.MouseDown:
                if (itemRect.Contains(current.mousePosition) &&
                    new Rect(itemRect.x, itemRect.y, 20, itemRect.height).Contains(current.mousePosition))
                {
                    dragFromIndex = currentIndex;
                    current.Use();
                }
                break;

            case EventType.MouseDrag:
                if (dragFromIndex == currentIndex)
                {
                    DragAndDrop.PrepareStartDrag();
                    DragAndDrop.SetGenericData("EventIndex", currentIndex);
                    DragAndDrop.StartDrag("Moving Event");
                    current.Use();
                }
                break;

            case EventType.DragUpdated:
                if (itemRect.Contains(current.mousePosition) &&
                    DragAndDrop.GetGenericData("EventIndex") is int sourceIndex &&
                    sourceIndex != currentIndex)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Move;
                    current.Use();
                }
                break;

            case EventType.DragPerform:
                if (itemRect.Contains(current.mousePosition) &&
                    DragAndDrop.GetGenericData("EventIndex") is int srcIndex &&
                    srcIndex != currentIndex)
                {
                    MoveEvent(srcIndex, currentIndex);
                    DragAndDrop.AcceptDrag();
                    current.Use();
                }
                break;
        }
    }

    private void MoveEvent(int fromIndex, int toIndex)
    {
        serializedObject.Update();

        // Получаем ссылки на элементы
        SerializedProperty fromProp = eventsProp.GetArrayElementAtIndex(fromIndex);
        SerializedProperty toProp = eventsProp.GetArrayElementAtIndex(toIndex);

        // Сохраняем значения
        object fromValue = fromProp.objectReferenceValue;
        object toValue = toProp.objectReferenceValue;

        // Меняем местами
        fromProp.objectReferenceValue = (UnityEngine.Object)toValue;
        toProp.objectReferenceValue = (UnityEngine.Object)fromValue;

        serializedObject.ApplyModifiedProperties();
    }

    private string GetUniqueAssetPath(string baseName)
    {
        string folderPath = "Assets/Data/Events";
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string path = $"{folderPath}/{baseName}_{DateTime.Now:yyyyMMddHHmmss}.asset";
        return AssetDatabase.GenerateUniqueAssetPath(path);
    }

    private void RemoveEvent(int index, BaseEvent evt)
    {
        eventsProp.DeleteArrayElementAtIndex(index);

        string path = AssetDatabase.GetAssetPath(evt);
        if (!string.IsNullOrEmpty(path))
        {
            AssetDatabase.DeleteAsset(path);
        }
        else
        {
            DestroyImmediate(evt, true);
        }

        AssetDatabase.SaveAssets();
    }
}
#endif