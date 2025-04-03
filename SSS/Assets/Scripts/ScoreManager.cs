using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    private static ScoreManager _instance;
    private Player _player;
    private Coroutine _zeroizeKillComboTicksCoroutine;
    private ProgressBar _progerssBarStyleRank;
    private GameObject _prefubAppearingSprite;

    [SerializeField] private int _currentKillCombo = 0;
    [SerializeField] private STYLE_RANK _currentRankStyle = 0;
    public enum STYLE_RANK { S, A, B, C, D }
    public enum TYPE_APPEARING_MESSAGE { ComboAdded, SkillUsed }

    public int styleMultiplier = 1;
    public float timeZeroizeKillComboTicks = 5; // время для сбрасывания текущего комбо за убийства


    public static ScoreManager Instance
    {
        get
        {
            if (_instance == null)
            {
                var obj = new GameObject("ScoreManager");
                _instance = obj.AddComponent<ScoreManager>();
                DontDestroyOnLoad(obj);
            }
            return _instance;
        }
    }


    [Serializable]
    public struct RankProperties
    {
        public int min;
        public int max;
        public int styleMultiplier;
    }

    public Dictionary<STYLE_RANK, RankProperties> rankProperties = new Dictionary<STYLE_RANK, RankProperties>
    {
        { STYLE_RANK.D, new RankProperties { min = 0, max = 10, styleMultiplier = 1 } },
        { STYLE_RANK.C, new RankProperties { min = 11, max = 25, styleMultiplier = 2 } },
        { STYLE_RANK.B, new RankProperties { min = 26, max = 50, styleMultiplier = 3 } },
        { STYLE_RANK.A, new RankProperties { min = 51, max = 100, styleMultiplier = 4 } },
        { STYLE_RANK.S, new RankProperties { min = 101, max = int.MaxValue, styleMultiplier = 5 } } // Для упрощения - до бесконечности
    };

    public STYLE_RANK CurrentRankStyle
    {
        get { return _currentRankStyle; }
        set
        {
            _currentRankStyle = value;

            if (value == STYLE_RANK.C)
            {
                AppearingSprite sciptAppearingSprite = Instantiate(_prefubAppearingSprite).GetComponent<AppearingSprite>();
                sciptAppearingSprite.SetProperlyAnimationAndPosition(TYPE_APPEARING_MESSAGE.ComboAdded);
            }
            if (value == STYLE_RANK.B)
            {
                AppearingSprite sciptAppearingSprite = Instantiate(_prefubAppearingSprite).GetComponent<AppearingSprite>();
                sciptAppearingSprite.SetProperlyAnimationAndPosition(TYPE_APPEARING_MESSAGE.SkillUsed);
            }

            EventBus.Instance.RankWasChanged(value);
        }
    }

    public void SetRank(int value)
    {
        foreach (var properties in rankProperties)
        {
            if (value >= properties.Value.min && value <= properties.Value.max)
            {
                if (CurrentRankStyle != properties.Key)
                {
                    CurrentRankStyle = properties.Key;
                    styleMultiplier = properties.Value.styleMultiplier;

                    _progerssBarStyleRank.Initialize(properties.Value.min, properties.Value.max);
                    //_progerssBarStyleRank.CurrentValue = value;

                    Debug.Log("Ранг: " + CurrentRankStyle);
                }
                return; // Важно: выходим из цикла, как только нашли подходящий диапазон
            }
        }

        Debug.LogError("Значение вне допустимого диапазона!"); // Если не попали ни в один диапазон
    }

    public int CurrentKillCombo
    {
        get { return _currentKillCombo; }
        set
        {
            _currentKillCombo = value;

            // Вызываем событие, если есть подписчики
            EventBus.Instance.KillComboWasChanged(value);
            SetRank(value);
            _progerssBarStyleRank.CurrentValue = value;

        }
    }


    public void Initialize(Player player) { _player = player; } // увы, эта штука вызывается после Awake, а значит к _player мы можем обращаться только в Start

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        _prefubAppearingSprite = Resources.Load<GameObject>(C.DK.PrefabAppearingSprite);



    }

    private void Start()
    {
        _progerssBarStyleRank = _player.progerssBarStyleRank;        

        CurrentRankStyle = STYLE_RANK.D;
        CurrentKillCombo = 0;

        _progerssBarStyleRank.Initialize(rankProperties[CurrentRankStyle].min, rankProperties[CurrentRankStyle].max);

    }

    public void UpCombo(int addictingCombo)
    {
        // Останавливаем предыдущую корутину (если она существует)
        if (_zeroizeKillComboTicksCoroutine != null)
        {
            CoroutineManager.Instance.StopManagedCoroutine(this.gameObject, _zeroizeKillComboTicksCoroutine);
        }

        // Запускаем новую корутину
        _zeroizeKillComboTicksCoroutine = CoroutineManager.Instance.StartManagedCoroutine(this.gameObject, ZeroizeKillComboTicks());

        CurrentKillCombo += addictingCombo;
    }

    IEnumerator ZeroizeKillComboTicks()
    {
        yield return new WaitForSeconds(timeZeroizeKillComboTicks); // Ждем 1 секунду

        // Сбрасываем комбо после задержки
        CurrentKillCombo = 0;
        _zeroizeKillComboTicksCoroutine = null; // Сбрасываем ссылку на корутину
    }
}
