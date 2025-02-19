using UnityEngine;


// ����������� ����� ��� ������ �������, ������� ������ ���� �������� ����� � �� ������� �� ������ ���������.
public static class StaticClassForAdditionalFunctions : object
{
    // ������������ ���� ������� ������ ����� ����� �������
    public static float GetAngle(Vector2 point1, Vector2 point2)
    {
        float deltaY = point2.y - point1.y;
        float deltaX = point2.x - point1.x;
        float angleInRadians = Mathf.Atan2(deltaY, deltaX); // �������
        return angleInRadians * Mathf.Rad2Deg; // �������
    }

}
