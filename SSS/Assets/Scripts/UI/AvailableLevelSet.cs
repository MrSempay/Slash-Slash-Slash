using UnityEngine;

public class AvailableLevelSet : MonoBehaviour
{
    [SerializeField] private RectTransform _scrollContainer; 

    public void UpdateLevelSet()
    {
        foreach (RectTransform child in _scrollContainer)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i <= GameManager.Instance.MaxReachedLevel; i++)
        {
            int index = i; // <-- локальная копия
            string levelName = GameManager.Instance.orderLevels[index]; // ещё лучше сразу взять строку
            GameManager.Instance.InstanceTextButton(true, _scrollContainer, levelName, () => OnChooseLevel(levelName));
        }
    }

    private void OnChooseLevel(string nameLevel)
    {
        GameManager.Instance.currentLevelInOrder = GameManager.Instance.orderLevels.IndexOf(nameLevel);
        GameManager.Instance.ChangeSceneTroughDialogue(nameLevel);
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    
}
