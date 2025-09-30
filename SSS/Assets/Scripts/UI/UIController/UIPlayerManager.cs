using TMPro;
using UnityEngine;
using static EnemyNearDetector;
using static ScoreManager;

public class UIPlayerManager : UIController
{
    [SerializeField] private TextEdit currentLevelUI;
    [SerializeField] private TextEdit currentLevelUpUI;
    [SerializeField] private TextEdit currentComboUI;
    [SerializeField] private TextEdit currentRankUI;
    [SerializeField] private TextEdit CurrentScoreUI;
    [SerializeField] private TextEdit currentMoneyUI;
    [SerializeField] private TextEdit currentExperienceUI;
    [SerializeField] private Player player;
    [SerializeField] private RectTransform rtTopCenterPanel;
    [SerializeField] private RectTransform rtBottomCenterPanel;
    [SerializeField] private RectTransform rtBottomLeftPanel;
    [SerializeField] private RectTransform rtTopLeftPanel;

    private Vector2 bottomLeft = Vector2.zero;
    private Vector2 bottomRight;
    private Vector2 topLeft;
    private Vector2 topRight;
    private Vector2 topCenter;
    private Vector2 bottomCenter;

    public void Awake()
    {
        //Debug.Log("CERFFFFFFFFFFFFFFFFFFFFFFFF");

        player.OnExperienceChanged += ChangeExperienceTextUI;
        player.OnMoneyChanged += ChangeMoneyTextUI;
        player.OnLevelChanged += ChangeLevelTextUI;
        player.OnLevelUpChanged += ChangeLevelUpTextUI;
        //player.OnScoreChanged += ChangeScoreTextUI;
        //player.OnKillComboChanged += ChangeComboTextUI;

        EventBus.Instance.OnKillKomboWasChanged.AddListener(ChangeComboTextUI);
        EventBus.Instance.OnRankWasChanged.AddListener(ChangeRankTextUI);
        EventBus.Instance.OnScoreWasChanged.AddListener(ChangeScoreTextUI);

        Camera cam = Camera.main;

        // половина высоты и ширины
        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        // центр камеры
        Vector3 c = cam.transform.position;

        // углы (в мировых координатах)
        bottomLeft = new Vector2(c.x - halfWidth, c.y - halfHeight);
        bottomRight = new Vector2(c.x + halfWidth, c.y - halfHeight);
        topLeft = new Vector2(c.x - halfWidth, c.y + halfHeight);
        topRight = new Vector2(c.x + halfWidth, c.y + halfHeight);
        topCenter = new Vector2(c.x, c.y + halfHeight);
        bottomCenter = new Vector2(c.x, c.y - halfHeight);

        // в рамках тестирования у нас есть несколько префабов Player, и у некоторых из них отсутствют эти элементы. Чтоб не ловить ошибки при тестировании, сделали так.
        if (rtTopLeftPanel != null)
        {
            rtTopLeftPanel.position = topLeft;
        }
        if (rtBottomLeftPanel != null)
        {
            rtBottomLeftPanel.position = bottomLeft;
        }
        if (rtTopCenterPanel != null) 
        {
            rtTopCenterPanel.position = topCenter;
        }
        if (rtBottomCenterPanel != null)
        {
            rtBottomCenterPanel.position = bottomCenter;
        }
    }

    private void ChangeLevelTextUI(int level)
    {
        currentLevelUI.SetNotLocalizableText(level.ToString());
    }
    
    private void ChangeLevelUpTextUI(int levelUp)
    {
        currentLevelUpUI.SetNotLocalizableText(levelUp.ToString());
    }

    private void ChangeMoneyTextUI(float money)
    {
        currentMoneyUI.SetNotLocalizableText(money.ToString());
    }

    private void ChangeExperienceTextUI(float experience)
    {
        currentExperienceUI.SetNotLocalizableText(experience.ToString() + "/" + player.experienceToNextLevel.ToString());
    }
    
    private void ChangeComboTextUI(int combo)
    {
        currentComboUI.SetNotLocalizableText(combo.ToString());
    }   
    private void ChangeRankTextUI(STYLE_RANK rank)
    {
        currentRankUI.SetNotLocalizableText(rank.ToString());
    }  
    private void ChangeScoreTextUI(int score)
    {
        CurrentScoreUI.SetNotLocalizableText(score.ToString());
    }

    private void OnDestroy()
    {
            //Debug.Log("Ебля блядоносная");
        player.OnExperienceChanged -= ChangeExperienceTextUI;
        player.OnMoneyChanged -= ChangeMoneyTextUI;
        player.OnLevelChanged -= ChangeLevelTextUI;
        player.OnLevelUpChanged -= ChangeLevelUpTextUI;
        player.OnKillComboChanged -= ChangeComboTextUI;
    }
}
