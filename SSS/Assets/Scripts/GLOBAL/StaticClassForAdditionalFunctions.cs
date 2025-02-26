using System.Collections.Generic;
using System.Reflection;
using System;
using UnityEngine;


// Статический класс для вызова функций, которые должны быть доступны извне и не зависят от логики контекста.
public static class StaticClassForAdditionalFunctions : object
{
    // Рассчитывает угол наклона прямой между двумя точками
    public static float GetAngle(Vector2 point1, Vector2 point2)
    {
        float deltaY = point2.y - point1.y;
        float deltaX = point2.x - point1.x;
        float angleInRadians = Mathf.Atan2(deltaY, deltaX); // радианы
        return angleInRadians * Mathf.Rad2Deg; // градусы
    }


    public static void AssignParameters(Dictionary<string, Dictionary<string, object>> objectsParameters, MonoBehaviour objectForAssigning, string nameOfObject)
    {
        Type type = objectForAssigning.GetType(); // Получаем тип текущего класса
        Dictionary<string, object> objectParameters = objectsParameters[nameOfObject];

        foreach (var kvp in objectParameters)
        {
            string parameterName = kvp.Key;
            object parameterValue = kvp.Value;

            // Получаем поле с именем, соответствующим ключу словаря
            FieldInfo fieldInfo = type.GetField(parameterName);

            if (fieldInfo != null)
            {
                // Пытаемся преобразовать значение к типу поля
                try
                {
                    object convertedValue = Convert.ChangeType(parameterValue, fieldInfo.FieldType);
                    fieldInfo.SetValue(objectForAssigning, convertedValue); // Присваиваем значение полю
                }
                catch (InvalidCastException e)
                {
                    Debug.LogError($"Could not convert value for parameter '{parameterName}' to type '{fieldInfo.FieldType.Name}': {e.Message}");
                }
            }
            else
            {
                Debug.LogWarning($"Field '{parameterName}' not found in class '{type.Name}'.");
            }
        }
    }

    public static void AssignPropertyValues(Dictionary<string, Dictionary<string, object>> objectsParameters, MonoBehaviour objectForAssigning, string nameOfObject)
    {
        Type type = objectForAssigning.GetType();
        if (!objectsParameters.ContainsKey(nameOfObject))
        {
            Debug.LogError($"Объект с именем '{nameOfObject}' не найден в objectsParameters.");
            return;
        }
        Dictionary<string, object> objectParameters = objectsParameters[nameOfObject];

        foreach (var kvp in objectParameters)
        {
            string propertyName = kvp.Key;
            object propertyValue = kvp.Value;

            // Получаем свойство с именем, соответствующим ключу словаря
            PropertyInfo propertyInfo = type.GetProperty(propertyName);

            if (propertyInfo != null && propertyInfo.CanWrite) //Убедимся, что свойство существует и доступно для записи
            {
                // Пытаемся преобразовать значение к типу свойства
                try
                {
                    object convertedValue = Convert.ChangeType(propertyValue, propertyInfo.PropertyType);
                    propertyInfo.SetValue(objectForAssigning, convertedValue, null); // Присваиваем значение свойству
                }
                catch (InvalidCastException e)
                {
                    Debug.LogError($"Could not convert value for property '{propertyName}' to type '{propertyInfo.PropertyType.Name}': {e.Message}");
                }
            }
            else
            {
                if (propertyInfo == null)
                {
                    Debug.LogWarning($"Property '{propertyName}' not found in class '{type.Name}'.");
                }
                else if (!propertyInfo.CanWrite)
                {
                    Debug.LogWarning($"Property '{propertyName}' in class '{type.Name}' does not have a setter (is read-only).");
                }
            }
        }
    }

}
