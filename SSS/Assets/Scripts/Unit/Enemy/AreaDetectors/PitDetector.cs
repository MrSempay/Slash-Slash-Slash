using UnityEngine;

public class PitDetector : MonoBehaviour
{
    public delegate void DetectedPit(bool jumpOfPit); // ������� � �������� ���������, ��� ������� �� ��-�� ���. ������ ������ � ������ Jump ����� �������������� �������� �� ������ ��������� ��
    public event DetectedPit OnDetectedPit;
    private void OnTriggerExit2D(Collider2D other)
    {
        // ������������� �� ������� � ��������� FsmStateWalkEnemy
        if (other.gameObject.CompareTag("Ground")) OnDetectedPit?.Invoke(true);
    }
}
