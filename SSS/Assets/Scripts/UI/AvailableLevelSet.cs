using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static StaticClassForAdditionalFunctions;

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
        //CoroutineManager.Instance.StartManagedCoroutine(gameObject, RefreshLayout());
        GameManager.Instance.RefreshLayout(gameObject, _scrollContainer.GetComponent<HorizontalOrVerticalLayoutGroup>());
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
        float timeDelay = GameManager.Instance.currentSettings.Orientation == LANGUAGE.Vertical ? 2f : 0.1f;
        StartCoroutine(DelayForUpdatingOrientation(nameLevel, timeDelay));
        
        OnStartLevel?.Invoke(); // 10.10.2025 - используется для установки нужной ориентации при переходе из главного менюн на уровни, и всё.
    }

    private IEnumerator DelayForUpdatingOrientation(string nameLevel, float timeDelay) // задержка нужна сугубо для обновления ориентации на Horizontal через OnStartLevel?.Invoke(),
    // ибо при смене сцены оно нормально отрисоваться не успевает, мда...
    {
        yield return new WaitForSecondsRealtime(timeDelay);

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

}
