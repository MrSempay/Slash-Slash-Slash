using UnityEngine;

public class Leaderboard : MonoBehaviour
{
    [SerializeField] private RectTransform rectTransformPlaceForFields;

    private FieldInfo _prefubField;

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
    }


    public void CloseLeaderboard()
    {
        Destroy(gameObject);
    }


}
