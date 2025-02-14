using UnityEngine;


// —татический класс дл€ вызова функций, которые должны быть доступны извне и не завис€т от логики контекста.
public static class StaticClassForAdditionalFunctions : object
{
    // –ассчитывает угол наклона пр€мой между двум€ точками
    public static float GetAngle(Vector2 point1, Vector2 point2)
    {
        float deltaY = point2.y - point1.y;
        float deltaX = point2.x - point1.x;
        float angleInRadians = Mathf.Atan2(deltaY, deltaX); // радианы
        return angleInRadians * Mathf.Rad2Deg; // градусы
    }

}
