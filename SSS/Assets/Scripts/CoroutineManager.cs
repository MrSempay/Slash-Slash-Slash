using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineManager : MonoBehaviour
{
    private static CoroutineManager _instance;

    public static CoroutineManager Instance
    {
        get
        {
            if (_instance == null)
            {
                var obj = new GameObject("CoroutineManager");
                _instance = obj.AddComponent<CoroutineManager>();
                DontDestroyOnLoad(obj);
            }
            return _instance;
        }
    }

    // Dictionary ��� �������� ������� �� ��������
    private Dictionary<GameObject, List<Coroutine>> activeCoroutines = new Dictionary<GameObject, List<Coroutine>>();

    // ����� ��� ������� �������� � ��������� ���������
    public Coroutine StartManagedCoroutine(GameObject owner, IEnumerator coroutine)
    {
        Coroutine startedCoroutine = StartCoroutine(coroutine);

        // ���� ��� ������� ��� ���� ��������, ��������� �����
        if (!activeCoroutines.ContainsKey(owner))
        {
            activeCoroutines[owner] = new List<Coroutine>();
        }
        activeCoroutines[owner].Add(startedCoroutine);

        return startedCoroutine;
    }

    // ����� ��� ��������� ���� ������� ��� ������������ �������
    public void StopAllCoroutinesFor(GameObject owner)
    {
        if (activeCoroutines.TryGetValue(owner, out List<Coroutine> coroutines))
        {
            foreach (var coroutine in coroutines)
            {
                StopCoroutine(coroutine);
            }
            activeCoroutines.Remove(owner);
        }
    }

    // ����� ��� ��������� ���������� ��������
    public void StopManagedCoroutine(GameObject owner, Coroutine coroutine)
    {
        if (activeCoroutines.TryGetValue(owner, out List<Coroutine> coroutines))
        {
            if (coroutines.Contains(coroutine))
            {
                StopCoroutine(coroutine);
                coroutines.Remove(coroutine);
            }
        }
    }
}