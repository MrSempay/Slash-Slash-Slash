using System.Collections.Generic;
using System.Reflection;
using System;
using UnityEngine;
using NUnit.Framework.Internal;
using static UnityEngine.RuleTile.TilingRuleOutput;
using System.Collections;
using Unity.VisualScripting;
using System.Linq;
using System.IO;
using System.Threading.Tasks;

// Статический класс для вызова функций, которые должны быть доступны извне и не зависят от логики контекста.
public static class StaticClassForAdditionalFunctions : object
{
    public enum LANGUAGE { English, Russian, Spanish, Horizontal, Vertical } // БОЖЕ, КАК ЖЕ УЁБИЩНО. Это ПИЗДЕЦ.
    public enum TYPES_INCREASING { Percentage, Absolute }
    public enum TYPE_NOTIFICATION { Success, Warning, Failure };

    // Рассчитывает угол наклона прямой между двумя точками
    public static float GetAngle(Vector2 point1, Vector2 point2)
    {
        float deltaY = point2.y - point1.y;
        float deltaX = point2.x - point1.x;
        float angleInRadians = Mathf.Atan2(deltaY, deltaX); // радианы
        return angleInRadians * Mathf.Rad2Deg; // градусы
    }


    // В функции сами получаем нужный словарь из словаря словарей по имени интересующего нас объекта. Имя также передаём в качестве параметра (хз зачем, можно было бы взять из
    // objectForAssigning. В целом скрипт подходит для вычленения нужных параметров из настроечных скриптов Adjust (там у нас словари словарей)
    public static void AssignParametersAndProperties(Dictionary<string, Dictionary<string, object>> objectsParameters, MonoBehaviour objectForAssigning, string nameOfObject)
    {

        if (objectsParameters == null)
        {
            Debug.LogError("objectsParameters is null!");
            return;
        }

        if (!objectsParameters.ContainsKey(nameOfObject))
        {
            Debug.LogWarning($"Object with name '{nameOfObject}' not found in objectsParameters.");
            return;
        }

        Type type = objectForAssigning.GetType(); // Получаем тип текущего класса
        Dictionary<string, object> objectParameters = objectsParameters[nameOfObject];

        foreach (var kvp in objectParameters)
        {
            string parameterOrPropertyName = kvp.Key;
            object parameterOrPropertyValue = kvp.Value;

            // Получаем поле с именем, соответствующим ключу словаря
            System.Reflection.FieldInfo fieldInfo = type.GetField(parameterOrPropertyName);

            if (fieldInfo != null)
            {
                // Пытаемся преобразовать значение к типу поля
                try
                {
                    object convertedValue = Convert.ChangeType(parameterOrPropertyValue, fieldInfo.FieldType);
                    fieldInfo.SetValue(objectForAssigning, convertedValue); // Присваиваем значение полю
                }
                catch (InvalidCastException e)
                {
                    Debug.LogError($"Could not convert value for parameter '{parameterOrPropertyName}' to type '{fieldInfo.FieldType.Name}': {e.Message}");
                }
            }
            else
            {
                PropertyInfo propertyInfo = type.GetProperty(parameterOrPropertyName); 
                if (propertyInfo != null && propertyInfo.CanWrite) //Убедимся, что свойство существует и доступно для записи
                {
                    // Пытаемся преобразовать значение к типу свойства
                    try
                    {
                        object convertedValue = Convert.ChangeType(parameterOrPropertyValue, propertyInfo.PropertyType);
                        propertyInfo.SetValue(objectForAssigning, convertedValue, null); // Присваиваем значение свойству
                    }
                    catch (InvalidCastException e)
                    {
                        Debug.LogError($"Could not convert value for property '{parameterOrPropertyName}' to type '{propertyInfo.PropertyType.Name}': {e.Message}");
                    }
                }
                else
                {
                    if (propertyInfo == null)
                    {
                        Debug.LogWarning($"Property '{parameterOrPropertyName}' not found in class '{type.Name}'.");
                    }
                    else if (!propertyInfo.CanWrite)
                    {
                        Debug.LogWarning($"Property '{parameterOrPropertyName}' in class '{type.Name}' does not have a setter (is read-only).");
                    }
                }
            }
        }
    }

    // Просто передаём в функцию словарей из имён свойств/параметров и объект, которому нужно присвоить значения этих свойств/параметров)
    public static void AssignParametersAndProperties(Dictionary<string, object> objectsParameters, object objectForAssigning)
    {

        if (objectsParameters == null)
        {
            Debug.LogError("objectsParameters is null!");
            return;
        }

        Type type = objectForAssigning.GetType(); // Получаем тип текущего класса

        foreach (var kvp in objectsParameters)
        {
            string parameterOrPropertyName = kvp.Key;
            object parameterOrPropertyValue = kvp.Value;

            // Получаем поле с именем, соответствующим ключу словаря
            System.Reflection.FieldInfo fieldInfo = type.GetField(parameterOrPropertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

            if (fieldInfo != null)
            {
                // Пытаемся преобразовать значение к типу поля
                try
                {
                    object convertedValue = Convert.ChangeType(parameterOrPropertyValue, fieldInfo.FieldType);
                    fieldInfo.SetValue(objectForAssigning, convertedValue); // Присваиваем значение полю
                }
                catch (InvalidCastException e)
                {
                    Debug.LogWarning($"Could not convert value for parameter '{parameterOrPropertyName}' to type '{fieldInfo.FieldType.Name}': {e.Message}");
                }
            }
            else
            {
                PropertyInfo propertyInfo = type.GetProperty(parameterOrPropertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                if (propertyInfo != null && propertyInfo.CanWrite) //Убедимся, что свойство существует и доступно для записи
                {
                    // Пытаемся преобразовать значение к типу свойства
                    try
                    {
                        object convertedValue = Convert.ChangeType(parameterOrPropertyValue, propertyInfo.PropertyType);
                        propertyInfo.SetValue(objectForAssigning, convertedValue, null); // Присваиваем значение свойству
                    }
                    catch (InvalidCastException e)
                    {
                        Debug.LogError($"Could not convert value for property '{parameterOrPropertyName}' to type '{propertyInfo.PropertyType.Name}': {e.Message}");
                    }
                }
                else
                {
                    if (propertyInfo == null)
                    {
                        Debug.LogWarning($"Property '{parameterOrPropertyName}' not found in class '{type.Name}'.");
                    }
                    else if (!propertyInfo.CanWrite)
                    {
                        Debug.LogWarning($"Property '{parameterOrPropertyName}' in class '{type.Name}' does not have a setter (is read-only).");
                    }
                }
            }
        }
    }

    // Получаем словарь ВСЕХ СВОЙСТВ И ПОЛЕЙ у заданного объекта. Возвращаем этот словарь. В дальнейшем, зачастую, мы присваеваем данные из словаря полям/свойствам какого-нибудь объекта

    public static Dictionary<string, object> GetPropertiesAndFields(object obj)
    {
        Dictionary<string, object> result = new Dictionary<string, object>();

        // Получаем тип объекта
        Type type = obj.GetType();

        // Получаем все поля объекта
        System.Reflection.FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (System.Reflection.FieldInfo field in fields)
        {
            try
            {
                // Добавляем имя и значение поля в словарь
                result[field.Name] = field.GetValue(obj);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error getting value for field {field.Name}: {e.Message}");
            }
        }

        // Получаем все свойства объекта
        PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (PropertyInfo property in properties)
        {
            try
            {
                // Проверяем, доступно ли чтение свойства
                if (property.CanRead)
                {
                    // Добавляем имя и значение свойства в словарь
                    result[property.Name] = property.GetValue(obj);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error getting value for property {property.Name}: {e.Message}");
            }

        }

        return result;
    }

    // Получаем поля и свйоства заданного объекта obj на основе ПЕРЕДАННОГО СЛОВАРЯ С ИМЕНАМИ СВОЙСТВ/ПОЛЕЙ. Только те свойства и поля будут возвращены в результирующем словаре,
    // имена которых были переданы как ключи в ВЫБОРОЧНОМ СЛОВАРЕ. Лучше было бы передавать массив строк, но его что так, что так придётся из словаря получать. Лучше уж делать в функции
    public static Dictionary<string, object> GetPropertiesAndFieldsSelectively(Dictionary<string, object> selectiveDictionary, object obj)
    {
        Dictionary<string, object> result = new Dictionary<string, object>();

        // Получаем тип объекта
        Type type = obj.GetType();

        foreach (var fieldSelectiveDictionary in selectiveDictionary)
        {
            string nameFieldOrProperty = fieldSelectiveDictionary.Key;

            // Пытаемся получить поле
            System.Reflection.FieldInfo field = type.GetField(nameFieldOrProperty, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                try
                {
                    result[nameFieldOrProperty] = field.GetValue(obj); // Получаем значение поля
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"GetPropertiesAndFieldsSelectively: Error getting value for field {nameFieldOrProperty}: {e.Message}");
                }
                continue; // Переходим к следующему элементу
            }

            // Если поле не найдено, пытаемся получить свойство
            PropertyInfo property = type.GetProperty(nameFieldOrProperty, BindingFlags.Public | BindingFlags.Instance);
            if (property != null && property.CanRead)
            {
                try
                {
                    result[nameFieldOrProperty] = property.GetValue(obj); // Получаем значение свойства
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"GetPropertiesAndFieldsSelectively: Error getting value for property {nameFieldOrProperty}: {e.Message}");
                }
            }
            else
            {
                Debug.LogWarning($"GetPropertiesAndFieldsSelectively: Field or readable property with name '{nameFieldOrProperty}' not found in type '{type.FullName}'.");
            }
        }

        return result;
    }

    public static bool HasAnyFile(string path)
    {
        if (!Directory.Exists(path))
            return false;

        string[] files = Directory.GetFiles(path);
        return files.Length > 0;
    }

    public static void Vibrate()
    {
        // Проверяем, поддерживает ли устройство вибрацию.
        // Это не всегда необходимо, но может предотвратить ошибки на некоторых платформах.
        //Debug.Log("Ibo");
        if (GameManager.Instance.currentSettings.vibrationOn)
        {
            #if UNITY_ANDROID && !UNITY_EDITOR
                  Handheld.Vibrate(); // Вызываем вибрацию
            #endif
        }
    }

    public static void VibrateForDuration(long milliseconds)
    {
        // Вибрация заданной длительности (только Android)
        if (Application.platform == RuntimePlatform.Android)
        {
            try
            {
                AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                AndroidJavaObject vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");
                vibrator.Call("vibrate", milliseconds);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Vibration failed: " + e.Message);
            }
        }
    }

    public static void CallFunctionByName(string nameFunction, object objectWhereShouldBeFunction, params object[] parameters) // в качестве objectWhereShouldBeFunction передаём любой скрипт
    {
        //Debug.Log(objectWhereShouldBeFunction);
        //Debug.Log(nameFunction);
        //Debug.Log(parameters);
        var methodInfo = objectWhereShouldBeFunction.GetType().GetMethod(nameFunction);
        if (methodInfo != null)
        {
            // Проверяем, передан ли массив параметров
            try
            {
                if (parameters == null)
                {
                    methodInfo.Invoke(objectWhereShouldBeFunction, null); // Если массив не передан, вызываем метод без параметров
                }
                else
                {
                    methodInfo.Invoke(objectWhereShouldBeFunction, parameters); // Если массив передан, вызываем метод с параметрами
                }
            }
            catch (Exception e)
            { 

            }
        }
        else
        {
            Debug.Log($"Method '{nameFunction}' not found in type '{objectWhereShouldBeFunction.GetType().FullName}'");
        }
    }

    public static bool AnimationExists(string animationName, Animator animator)
    {
        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == animationName)
            {
                return true; // Анимация найдена
            }
        }
        return false; // Анимация не найдена
    }

    public static bool CheckChance(float chancePercentage)
    {
        // Генерируем случайное число от 0 до 100
        float randomNumber = UnityEngine.Random.Range(0f, 100f);

        // Проверяем, меньше ли случайное число, чем заданный шанс
        return randomNumber <= chancePercentage;
    }

    public static UnityEngine.Transform InstanceEmptyObjectAndGetTransform(UnityEngine.Transform parentTransform, string nameObject, Vector3 biasPosition, bool biasFromParent = true)
    {
        UnityEngine.Transform transformEmptyObject = new GameObject(nameObject).GetComponent<UnityEngine.Transform>();
        transformEmptyObject.SetParent(parentTransform, false);
        if (biasFromParent) // относительно родителя двигаем на biasPosition
        {
            transformEmptyObject.localPosition = Vector3.zero + biasPosition;
        }
        else // задаём глобальную позицию biasPosition для объекта
        {
            transformEmptyObject.position = biasPosition;
        }

        return transformEmptyObject;
    }

    // Вспомогательная обёртка. Игнорируем все отмены/ошибки для указанной задачи, Task t не выбросит исключений наружу, никаких
    public static async Task SafeIgnoreErrors(Task t)
    {
        try
        {
            await t;
        }
        catch
        {
            // подавляем любое исключение (ошибка или отмена)
        }
    }

}
