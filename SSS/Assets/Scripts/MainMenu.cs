using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    private void Awake()
    {
        GameManager.Instance.Initialize();
    }

    public void StartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1); // ��������� ����� � ������ ����������� ��� �������� ������ (�� 14.02 - Sample Scene)
    }

    public void ExitGame()
    {
        Debug.Log("���� ���������");
        Application.Quit();
    }

}
