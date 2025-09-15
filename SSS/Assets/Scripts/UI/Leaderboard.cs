using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Leaderboard : MonoBehaviour
{
    [SerializeField] private RectTransform _rectTransformPlaceForFields;
    [SerializeField] private TextEdit _textNotification;

    private FieldInfo _prefubField;
    private bool _lastLoginingWasFailed = false;
    private GameObject _buttonShowLeaderboard;

    private void Awake()
    {
        _prefubField = ScoreManager.prefubFieldLeaderboard; // не очень, конечно, безопасно, но едва ли лидерборд будет спавниться до ScoreManager

        UpdateLeaderboard();

        _buttonShowLeaderboard = GameManager.Instance.InstanceTextButton(false, Player.instance.buttonShowLeaderboardPlacement, C.Just.ShowLeaderboard, ShowLeaderboardButtonClick);
        _buttonShowLeaderboard.SetActive(false);
            
        PlayFabManager.Instance.OnLoginSuccess += LoginSuccess;
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
            await PlayFabManager.Instance.StartCloudUpdatePlayerStatsNEWAsync();
            await Task.Delay(2000);
            await PlayFabManager.Instance.GetScoreLeaderboarderAsync();

            UpdateLeaderboard();

            _lastLoginingWasFailed = false;
        }
    }
    private void UpdateLeaderboard()
    {
        if (PlayFabManager.Instance.lastLeaderboardStatsInfo.Count > 0)
        {
            foreach (var fieldLeaderboardInfo in PlayFabManager.Instance.lastLeaderboardStatsInfo)
            {
                FieldInfo scriptFieldLeaderboard = Instantiate(_prefubField, Vector3.zero, Quaternion.identity, _rectTransformPlaceForFields);
                //Debug.Log(scriptFieldLeaderboard);
                //Debug.Log(scriptFieldLeaderboard.textNameInfo);
                //Debug.Log(fieldLeaderboardInfo);
                //Debug.Log(fieldLeaderboardInfo.Key);
                scriptFieldLeaderboard.textNameInfo.SetNotLocalizableText(fieldLeaderboardInfo.Key);
                scriptFieldLeaderboard.textValueInfo.SetNotLocalizableText(fieldLeaderboardInfo.Value.ToString());

                _textNotification.Text = "";

                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
                //scriptFieldLeaderboard.imageIcon


            }
        }
        else
        {
            _textNotification.Text = C.Notifications.CantGetLeaderboard;
            _lastLoginingWasFailed = true;
        }
    }

    private void OnDestroy()
    {
        if (PlayFabManager.Instance != null)
        {
            PlayFabManager.Instance.OnLoginSuccess -= LoginSuccess;
        }
    }


}
