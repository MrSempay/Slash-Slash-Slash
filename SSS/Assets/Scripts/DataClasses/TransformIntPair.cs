using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable] // ”казываем, что класс можно сериализовать
public class TransformIntPair
{
    public Transform target;
    public int enemyCount;
    public List<TypesAndAmountEnemies> typesAndAmountEnemies;
}