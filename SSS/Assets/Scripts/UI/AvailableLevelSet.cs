using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AvailableLevelSet : MonoBehaviour
{
    public Action OnStartLevel;

    [SerializeField] private RectTransform _scrollContainer;
    private int _currentAmountLevels = 0;

    public void UpdateLevelSet()
    {
        if (_currentAmountLevels != GameManager.Instance.MaxReachedLevel + 1) // чтоб только если значение изменилось в необходимом количестве уровней, мы обновляли список.
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
            _currentAmountLevels = GameManager.Instance.MaxReachedLevel + 1;
        }
    }

    private void Awake()
    {
        GameManager.Instance.OnMaxReachedLevelWasChanged += UpdateLevelSet;
    }

    void Start()
    {
        
    }
    private void OnEnable()
    {
        CoroutineManager.Instance.StartManagedCoroutine(gameObject, RefreshLayout());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnMaxReachedLevelWasChanged -= UpdateLevelSet;

        CoroutineManager.Instance.StopAllCoroutinesFor(gameObject);
    }
    private void OnChooseLevel(string nameLevel)
    {
        OnStartLevel?.Invoke();

        GameManager.Instance.currentLevelInOrder = GameManager.Instance.orderLevels.IndexOf(nameLevel);
        GameManager.Instance.ChangeSceneTroughDialogue(nameLevel);
    }

    private IEnumerator RefreshLayout()
    {
        _scrollContainer.GetComponent<VerticalLayoutGroup>().spacing += 0.01f;
        yield return null;
        _scrollContainer.GetComponent<VerticalLayoutGroup>().spacing -= 0.01f;
        //LayoutRebuilder.ForceRebuildLayoutImmediate(_scrollContainer);
        // Или
        //Canvas.ForceUpdateCanvases();
    }



    // Вызвать после добавления/изменения элементов
}
