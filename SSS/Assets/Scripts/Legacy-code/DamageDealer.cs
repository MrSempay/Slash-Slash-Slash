using UnityEngine;

public class DamageDealer : MonoBehaviour
{
    public float damageAmount = 10f; // ���������� �����, ���������� �� ���
    public float damageInterval = 1f; // �������� ����� ���������� ����� (� ��������)

    private float timer; // ������ ��� ������� ������� ����� ���������� �����
    private GameObject currentTarget; //������� ����

    void Update()
    {
        if (currentTarget != null)
        {
            timer += Time.deltaTime; // ����������� ������

            if (timer >= damageInterval)
            {
                timer = 0f; // ���������� ������
                DamageReceiver damageReceiver = currentTarget.GetComponent<DamageReceiver>();
                if (damageReceiver != null)
                {
                    damageReceiver.TakeDamage(damageAmount); // ������� ����
                }
                else
                {
                    currentTarget = null; //�������� ����.
                }
            }
        }
    }

    //����� OnTriggerEnter ���������� ��� ��������� ���������� (� ������������� ������ Is Trigger) � ���� ������� ����������

    void OnTriggerEnter2D(Collider2D other)
    {
        DamageReceiver damageReceiver = other.GetComponent<DamageReceiver>();
        if (damageReceiver != null)
        {
            currentTarget = other.gameObject;
        }
    }

    //����� OnTriggerExit ���������� ��� ������ ���������� �� ���������� ����
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject == currentTarget)
        {
            currentTarget = null;
            timer = 0;
        }
    }


}