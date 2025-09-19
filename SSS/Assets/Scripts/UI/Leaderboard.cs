using System.Threading.Tasks;
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

                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
                //scriptFieldLeaderboard.imageIcon

                place_number++;
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
