using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Experimental.SceneManagement; // Äëÿ PrefabStageUtility

public class FindAndReplaceName : EditorWindow
{
    public string findString = "";
    public string replaceString = "";
    public bool searchInPrefabs = false;

    [MenuItem("Tools/Find and Replace Name")]
    public static void ShowWindow()
    {
        GetWindow<FindAndReplaceName>("Find and Replace Name");
    }

    void OnGUI()
    {
        GUILayout.Label("Find and Replace in Names", EditorStyles.boldLabel);

        findString = EditorGUILayout.TextField("Find:", findString);
        replaceString = EditorGUILayout.TextField("Replace:", replaceString);
        searchInPrefabs = EditorGUILayout.Toggle("Search in Open Prefabs:", searchInPrefabs);

        if (GUILayout.Button("Find and Replace"))
        {
            FindAndReplace();
        }
    }

    void FindAndReplace()
    {
        if (string.IsNullOrEmpty(findString))
        {
            Debug.LogWarning("Find string is empty. Nothing to find and replace.");
            return;
        }

        GameObject[] allObjects;

        if (searchInPrefabs && PrefabStageUtility.GetCurrentPrefabStage() != null)
        {
            // Search in the open prefab stage
            Transform[] allTransforms = PrefabStageUtility.GetCurrentPrefabStage().scene.GetRootGameObjects()[0].GetComponentsInChildren<Transform>(true); // Get all transforms in prefab
            allObjects = new GameObject[allTransforms.Length];
            for (int i = 0; i < allTransforms.Length; i++)
            {
                allObjects[i] = allTransforms[i].gameObject;
            }
        }
        else
        {
            // Search in the scene
            allObjects = FindObjectsOfType<GameObject>();
        }

        Undo.RegisterCompleteObjectUndo(allObjects, "Find and Replace Name"); // For Undo functionality

        int replaceCount = 0;
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains(findString))
            {
                string newName = obj.name.Replace(findString, replaceString);
                if (newName != obj.name)
                {
                    obj.name = newName;
                    replaceCount++;
                    EditorUtility.SetDirty(obj); // Mark the object as dirty to save changes
                }
            }
        }

        AssetDatabase.SaveAssets(); // Save changes to assets (e.g., prefabs)

        Debug.Log($"Find and Replace Name: Replaced '{findString}' with '{replaceString}' in {replaceCount} objects.");
    }
}