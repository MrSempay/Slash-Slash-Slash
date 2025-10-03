using System;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Leaderboard : MonoBehaviour
{
    [SerializeField] private RectTransform _rectTransformPlaceForFields;  
    [SerializeField] private TextEdit _textNotification;
    [SerializeField] private Sprite place_1;
    [SerializeField] private Sprite place_2;
    [SerializeField] private Sprite place_3;
    [SerializeField] private Sprite place_another;
    [SerializeField] private RectTransform _rtContainerButtons;

    private HorizontalOrVerticalLayoutGroup _HOVLayoutGroupPlaceForFields;  
    private HorizontalOrVerticalLayoutGroup _HOVLayoutGroupContainerButtons;
    private FieldInfo _prefubField;
    private bool _lastLoginingWasFailed = false;
    private GameObject _buttonShowLeaderboard;
    private CancellationTokenSource _cts;

    private void Awake()
    {
        _HOVLayoutGroupPlaceForFields = _rectTransformPlaceForFields.GetComponent<HorizontalOrVerticalLayoutGroup>();
        _HOVLayoutGroupContainerButtons = _rtContainerButtons.GetComponent<HorizontalOrVerticalLayoutGroup>();

        _prefubField = ScoreManager.prefubFieldLeaderboard; // не очень, конечно, безопасно, но едва ли лидерборд будет спавниться до ScoreManager
        //Debug.Log(GetInstanceID());
        UpdateLeaderboard();

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

                await PlayFabManager.Instance.StartCloudUpdatePlayerStatsNEWAsync();
                token.ThrowIfCancellationRequested();
                await Task.Delay(2000);
                token.ThrowIfCancellationRequested();
                await PlayFabManager.Instance.GetScoreLeaderboarderAsync();

                UpdateLeaderboard();

                _lastLoginingWasFailed = false;
            }
            catch (OperationCanceledException)
            {
                // Корректная отмена - игнорируем
            }
        }
    }
    private void UpdateLeaderboard()
    {
        if (PlayFabManager.Instance.lastLeaderboardStatsInfo.Count > 0)
        {
            int place_number = 0;
            foreach (var fieldLeaderboardInfo in PlayFabManager.Instance.lastLeaderboardStatsInfo)
            {
                FieldInfo scriptFieldLeaderboard = Instantiate(_prefubField, Vector3.zero, Quaternion.identity, _rectTransformPlaceForFields);
                //Debug.Log(scriptFieldLeaderboard);
                //Debug.Log(scriptFieldLeaderboard.textNameInfo);
                //Debug.Log(fieldLeaderboardInfo);
                //Debug.Log(fieldLeaderboardInfo.Key);
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
            if (GameManager.Instance.currentLevelInOrder != GameManager.Instance.orderLevels.Count - 1) // если не достигли последнего уровня
            {
                GameManager.Instance.InstanceTextButton(true, _rtContainerButtons, C.Just.NextLevel, NextLevel);
            }
            else
            {
                foreach (RectTransform rtButton in _rtContainerButtons)
                {
                    Destroy(rtButton.gameObject); // по-хорошему ещё бы как-то listener-ов убрать, ну да ладно

                }
                Instantiate(GameManager.Instance.prefubButtonBigMainMenu, _rtContainerButtons).onClick.AddListener(GoToMainMenu);
                Instantiate(GameManager.Instance.prefubButtonBigTitles, _rtContainerButtons).onClick.AddListener(GoToTitles);
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

    private void GoToTitles()
    {
        GameManager.Instance.ChangeScene(C.NameScene.Titles);
    }

    private void OnDestroy()
    {
        _cts?.Cancel();
        _cts?.Dispose();

        if (PlayFabManager.Instance != null)
        {
            PlayFabManager.Instance.OnLoginSuccess -= LoginSuccess;
        }

        CoroutineManager.Instance.StopAllCoroutinesFor(gameObject);
    }


}
