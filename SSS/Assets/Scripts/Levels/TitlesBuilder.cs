using UnityEngine;

public class TitlesBuilder : MonoBehaviour
{
    [SerializeField] private RectTransform _placementNotifications;

    private void Awake()
    {
        GameManager.Instance.notificationPlacement = _placementNotifications;
        AudioManager.Instance.StartCertainMusicInLoop("TitlesTheme");
    }
}
