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
            Debug.Log("Ебля блядоносная");
        player.OnExperienceChanged -= ChangeExperienceTextUI;
        player.OnMoneyChanged -= ChangeMoneyTextUI;
        player.OnLevelChanged -= ChangeLevelTextUI;
        player.OnLevelUpChanged -= ChangeLevelUpTextUI;
        player.OnKillComboChanged -= ChangeComboTextUI;
    }
}
