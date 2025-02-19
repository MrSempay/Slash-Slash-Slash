using NavMeshPlus.Components;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.IO;
using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using System.Text.RegularExpressions;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.EventSystems.EventTrigger;

public class TestingFallZone : MonoBehaviour
{
    List<int> listOfNumbersFallZones = new List<int>();
    int i = 0;
    NavMeshPath path;
    NavMeshQueryFilter filter;
    public LineRenderer lineRenderer;
    int areaMask;

    void Start()
    {
        path = new NavMeshPath();
        Transform parent = transform; // Ссылка на Transform родительского объекта (текущего)

        foreach (Transform child in parent)
        {
            listOfNumbersFallZones.Add(child.gameObject.GetComponent<NavMeshModifierVolume>().area);
            Debug.Log("Area " + listOfNumbersFallZones[i]);
            i++;
        }
        areaMask = ~((1 << 3) | (1 << 4) | (1 << 5) | (1 << 6)); ;
        //int areaMask = CreateAreaMaskExcluding(listOfNumbersFallZones);
        filter = new NavMeshQueryFilter();
        filter.areaMask = areaMask;
        NavMesh.CalculatePath(new Vector3(0, 0, 0), new Vector3(28,21,0), filter, path);
        Debug.Log(NavMesh.AllAreas);
        Debug.Log(path.corners.Length);
        Debug.Log(filter.areaMask);
    }

    // Update is called once per frame
    void Update()
    {
        NavMesh.CalculatePath(new Vector3(0, 0, 0), new Vector3(28, 21, 0), areaMask, path);
        DrawPath(path);
    }

    // Вспомогательный метод для создания маски, исключающей указанные зоны
    int CreateAreaMaskExcluding(List<int> areaIndices)
    {
        int mask = NavMesh.AllAreas; // Начинаем со всех зон

        foreach (int areaIndex in areaIndices)
        {
            mask &= ~(1 << areaIndex); // Инвертируем бит для указанной зоны
        }
        return mask;
    }

    void DrawPath(NavMeshPath path)
    {
        if (lineRenderer == null || path == null) return;
        // Устанавливаем количество точек LineRenderer
        lineRenderer.positionCount = path.corners.Length;
        Debug.Log(path.corners[0]);
        // Устанавливаем позиции точек LineRenderer
        lineRenderer.SetPositions(path.corners);
    }
}
