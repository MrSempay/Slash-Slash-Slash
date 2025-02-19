using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    // ��� ������ ��������� � ����� �������� (� ����� �� � ���� � ������) ��������� ��������� ������ GlobalGameScript, ��������� � _instance � ����� ������ �� ����
    // ���������. ��� ��������� ���������� ����� ���������� ������ �� ���� �� ��������� (� ��� ��� static ���� _instance, ���������� �� ����� ������), static �� ���
    // �������� Instance ����� ��� ����, ���� ����� ���� ���������� ������� ��������� ������� ������. ����� � Awake �� ���������, ���������� �� ��� ���������
    // ������� ������ � ����� �� �� �������, �� �������� ���������� Awake, ���� ��, �� �������� ������ �� ���� � _instance (������, ��� ������ � Awake ����� ��� ����,
    // ����� �� ���� ������� ��� ������ �������� (�� ���� ��) ������� ���������. ������ ��� ������� ��� �� ���� � ����� ������.

    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                var obj = new GameObject("GameManager");
                _instance = obj.AddComponent<GameManager>();
                DontDestroyOnLoad(obj);
            }
            return _instance;
        }
    }

    // ����� ������ ������ �� ������, �� ���-�� ���������������� ��� �������� ����, ��������� ���������� � ����������� �� �������� ������ �� ��� ������ ������� ���. 
    // ���, ������ GameManager.Instance ������� ������
    public void Initialize() { }


    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // ���������� ������������ ����� ����� "Enemy" � ����� �����. ���������� ������������� ����� ���� ���/����������� ��� ������� ���� (���� ����� ��������� ��� ��� �������������,
        // ��� � ��� ���� ��������. ���� � ������� �������� ���� � ������ �� �������� ���������, ����� ����������� �������� �������� ����������� � ��� ������ ��� ����� ��������
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Enemy"), LayerMask.NameToLayer("Enemy"));
    }
}