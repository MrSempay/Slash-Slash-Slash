using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Leaderboard : MonoBehaviour
{
    [SerializeField] private RectTransform rectTransformPlaceForFields;

    private FieldInfo _prefubField;
    private GameObject _buttonShowLeaderboard;

    private void Awake()
    {
        _prefubField = ScoreManager.prefubFieldLeaderboard; // не очень, конечно, безопасно, но едва ли лидерборд будет спавниться до ScoreManager

        foreach (var fieldLeaderboardInfo in PlayFabManager.Instance.lastLeaderboardStatsInfo)
        {
            FieldInfo scriptFieldLeaderboard = Instantiate(_prefubField, Vector3.zero, Quaternion.identity, rectTransformPlaceForFields);
            //Debug.Log(scriptFieldLeaderboard);
            //Debug.Log(scriptFieldLeaderboard.textNameInfo);
            //Debug.Log(fieldLeaderboardInfo);
            //Debug.Log(fieldLeaderboardInfo.Key);
            scriptFieldLeaderboard.textNameInfo.SetNotLocalizableText(fieldLeaderboardInfo.Key);
            scriptFieldLeaderboard.textValueInfo.SetNotLocalizableText(fieldLeaderboardInfo.Value.ToString());
            //scriptFieldLeaderboard.imageIcon


        }
        _buttonShowLeaderboard = GameManager.Instance.InstanceTextButton(false, Player.instance.buttonShowLeaderboardPlacement, C.Just.ShowLeaderboard, ShowLeaderboardButtonClick);
        _buttonShowLeaderboard.SetActive(false);
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

    private void OnDestroy()
    {
    }


}
