using System;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Leaderboard : MonoBehaviour
{
    public enum INSTANTIATION_CONTEXT { FinishLevel, Defeat };

    [SerializeField] private RectTransform _rectTransformPlaceForFields;  
    [SerializeField] private TextEdit _textNotification;
    [SerializeField] private Sprite place_1;
    [SerializeField] private Sprite place_2;
    [SerializeField] private Sprite place_3;
    [SerializeField] private Sprite place_another;
    [SerializeField] private RectTransform _rtContainerButtons;
    [SerializeField] private GameObject _buttonCloseLeaderboard;

    private HorizontalOrVerticalLayoutGroup _HOVLayoutGroupPlaceForFields;  
    private HorizontalOrVerticalLayoutGroup _HOVLayoutGroupContainerButtons;
    private FieldInfo _prefubField;
    private bool _lastLoginingWasFailed = false;
    private GameObject _buttonShowLeaderboard;
    private CancellationTokenSource _cts;
    private INSTANTIATION_CONTEXT _instContext; // по идее просто хранит то значение, с которым мы заспавнились. Нужно только для вызова через функцию LoginSuccess

    private static Leaderboard _instance;

    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(_instance.gameObject);
        }
        _instance = this;

        _HOVLayoutGroupPlaceForFields = _rectTransformPlaceForFields.GetComponent<HorizontalOrVerticalLayoutGroup>();
        _HOVLayoutGroupContainerButtons = _rtContainerButtons.GetComponent<HorizontalOrVerticalLayoutGroup>();

        _prefubField = ScoreManager.prefubFieldLeaderboard; // не очень, конечно, безопасно, но едва ли лидерборд будет спавниться до ScoreManager
        ////Debug.Log(GetInstanceID());

        _buttonShowLeaderboard = GameManager.Instance.InstanceTextButton(false, Player.instance.buttonShowLeaderboardPlacement, C.Just.ShowLeaderboard, ShowLeaderboardButtonClick);
        _buttonShowLeaderboard.SetActive(false);

        _cts = new CancellationTokenSource();

        PlayFabManager.Instance.OnLoginSuccess += LoginSuccess;
    }

    private void OnEnable()
    {
        GameManager.Instance.RefreshLayout(gameObject, _HOVLayoutGroupPlaceForFields);
        GameManager.Instance.RefreshLayout(gameObject, _HOVLayoutGroupContainerButtons);
    }


    public void CloseLeaderboardButtonClick()
    {
        gameObject.SetActive(false);
        _buttonShowLeaderboard.SetActive(true);
    }

    public void NextLevel()
    {
        GameManager.Instance.GoToRequiredLevel();
    }

    public void AdjustLeaderboardAtInstantiate(INSTANTIATION_CONTEXT instContext) // по идее вызывается 1 раз при создании...
    {
        AdjustButtons(instContext);
        UpdateLeaderboard(instContext);
    }

    private void ShowLeaderboardButtonClick()
    {
        gameObject.SetActive(true);
        _buttonShowLeaderboard.SetActive(false);
    }

    private async void LoginSuccess()
    {
        if (_lastLoginingWasFailed)
        {
            try
            {
                var token = _cts.Token;

                await ScoreManager.Instance.GetActualLeaderboardAsync(token);

                UpdateLeaderboard(_instContext);

                _lastLoginingWasFailed = false;
            }
            catch (OperationCanceledException)
            {
                // Корректная отмена - игнорируем
            }
        }
    }

    private void AdjustButtons(INSTANTIATION_CONTEXT instContext) // по идее вызывается 1 раз при создании...
    {
        if (PlayFabManager.Instance.lastLeaderboardStatsInfo.Count > 0)
        {
            switch (instContext)
            {
                case INSTANTIATION_CONTEXT.FinishLevel:
                    if (GameManager.Instance.currentLevelInOrder != GameManager.Instance.orderLevels.Count - 1) // если не достигли последнего уровня
                    {
                        GameManager.Instance.InstanceTextButton(true, _rtContainerButtons, C.Just.NextLevel, NextLevel);
                    }
                    else
                    {
                        Instantiate(GameManager.Instance.prefubButtonBigMainMenu, _rtContainerButtons).onClick.AddListener(GoToMainMenu);
                        Instantiate(GameManager.Instance.prefubButtonBigTitles, _rtContainerButtons).onClick.AddListener(GoToTitles);
                    }
                    break;

                case INSTANTIATION_CONTEXT.Defeat:
                    Instantiate(GameManager.Instance.prefubButtonBigMainMenu, _rtContainerButtons).onClick.AddListener(GoToMainMenu);
                    Instantiate(GameManager.Instance.prefubBigButtonRetry, _rtContainerButtons).onClick.AddListener(RetryLevel);
                    break;
            }
        }
        else
        {
            switch (instContext)
            {
                case INSTANTIATION_CONTEXT.FinishLevel:
                    GameManager.Instance.InstanceTextButton(true, _rtContainerButtons, C.Just.NextLevel, NextLevel);
                    break;
                case INSTANTIATION_CONTEXT.Defeat:
                    Instantiate(GameManager.Instance.prefubButtonBigMainMenu, _rtContainerButtons).onClick.AddListener(GoToMainMenu);
                    Instantiate(GameManager.Instance.prefubBigButtonRetry, _rtContainerButtons).onClick.AddListener(RetryLevel);
                    break;
            }
        }
        if (instContext == INSTANTIATION_CONTEXT.Defeat)
        {
            _buttonCloseLeaderboard.SetActive(false);
        }
    }

    private void UpdateLeaderboard(INSTANTIATION_CONTEXT instContext) // короче, по идее это пока что (10.10.2025) instContext нужен только для настройки кнопок, их настраиваем в отдельном
    // методе, тут оставим параметр для возможных дальнейших манипуляций.
    {
        if (PlayFabManager.Instance.lastLeaderboardStatsInfo.Count > 0)
        {
            int place_number = 0;
            foreach (var fieldLeaderboardInfo in PlayFabManager.Instance.lastLeaderboardStatsInfo)
            {
                FieldInfo scriptFieldLeaderboard = Instantiate(_prefubField, Vector3.zero, Quaternion.identity, _rectTransformPlaceForFields);
                ////Debug.Log(scriptFieldLeaderboard);
                ////Debug.Log(scriptFieldLeaderboard.textNameInfo);
                ////Debug.Log(fieldLeaderboardInfo);
                ////Debug.Log(fieldLeaderboardInfo.Key);
                scriptFieldLeaderboard.textNameInfo.SetNotLocalizableText(fieldLeaderboardInfo.Key);
                scriptFieldLeaderboard.textValueInfo.SetNotLocalizableText(fieldLeaderboardInfo.Value.ToString());
                switch (place_number)
                {
                    case 0:
                        scriptFieldLeaderboard.imageIcon.sprite = place_1;
                        break;
                    case 1:
                        scriptFieldLeaderboard.imageIcon.sprite = place_2;
                        break;
                    case 2:
                        scriptFieldLeaderboard.imageIcon.sprite = place_3;
                        break;
                }

                if (place_number > 2)
                {
                    scriptFieldLeaderboard.imageIcon.sprite = place_another;
                }

                _textNotification.Text = "";
                //scriptFieldLeaderboard.imageIcon

                place_number++;
            }

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
        }
        else
        {
            _textNotification.Text = C.Notifications.CantGetLeaderboard;
            _lastLoginingWasFailed = true;
        }
    }

    private void GoToMainMenu()
    {
        GameManager.Instance.ChangeScene(C.NameScene.MainMenu);
    }
    private void RetryLevel()
    {
        GameManager.Instance.GoToSameLevel();
    }

    private void GoToTitles()
    {
        GameManager.Instance.ChangeScene(C.NameScene.Titles);
    }

    private void OnDestroy()
    {
        _cts?.Cancel();
        _cts?.Dispose();

        Destroy(_buttonShowLeaderboard);

        if (PlayFabManager.Instance != null)
        {
            PlayFabManager.Instance.OnLoginSuccess -= LoginSuccess;
        }

        CoroutineManager.Instance.StopAllCoroutinesFor(gameObject);
    }


}
