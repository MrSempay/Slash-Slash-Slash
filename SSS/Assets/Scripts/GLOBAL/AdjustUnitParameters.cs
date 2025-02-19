using System.Collections.Generic;
using UnityEngine;

public static class AdjustUnitParameters
{
    public static readonly Dictionary<string, Dictionary<string, object>> unitParameters = new Dictionary<string, Dictionary<string, object>>()
    {
        // �������������� ������� ��� ����������
        { "Player", new Dictionary<string, object>() { 
            { "healthMax", 10000 }, 
            { "damageReduction", 100 }, 
            { "speed", 2 }, 
            { "jumpForce", 12 }, 
            { "damage", 10 } } },
        { "MeleeEnemy", new Dictionary<string, object>() {
            { "healthMax", 150 },
            { "damageReduction", 150 },
            { "speed", 4 },
            { "jumpForce", 14 },
            { "som", 14 },
            { "damage", 5 } } },
        { "Door", new Dictionary<string, object>() {
            { "healthMax", 15000 } } }
    };

    // �������� �������� �� ������� �� �������� ����� � ���������
    public static object GetParameter(string unitName, string parameterName)
    {
        if (unitParameters.ContainsKey(unitName) && unitParameters[unitName].ContainsKey(parameterName))
        {
            return unitParameters[unitName][parameterName];
        }
        return null;
    }

    // �������� ���� ������� ��� ���������� ����� �� ��� �����
    public static object GetSetupOfUnit(string unitName)
    {
        if (unitParameters.ContainsKey(unitName))
        {
            return unitParameters[unitName];
        }
        return null;
    }

}
