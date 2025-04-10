using UnityEngine;

public class Leaderboard : MonoBehaviour
{
    [SerializeField] private RectTransform rectTransformPlaceForFields;

    private FieldLeaderboard prefubField;

    private void Awake()
    {
        prefubField = ScoreManager.prefubFieldLeaderboard; // не очень, конечно, безопасно, но едва ли лидерборд будет спавниться до ScoreManager

        foreach (var fieldLeaderboardInfo in PlayFabManager.Instance.lastLeaderboardStatsInfo)
        {
            FieldLeaderboard scriptFieldLeaderboard = Instantiate(prefubField, Vector3.zero, Quaternion.identity, rectTransformPlaceForFields);
            scriptFieldLeaderboard.textName.SetNotLocalizableText(fieldLeaderboardInfo.Key);
            scriptFieldLeaderboard.textScore.SetNotLocalizableText(fieldLeaderboardInfo.Value.ToString());
            //scriptFieldLeaderboard.imageIcon


        }
    }


    public void CloseLeaderboard()
    {
        Destroy(gameObject);
    }


}
