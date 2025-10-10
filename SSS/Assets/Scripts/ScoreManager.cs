using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static Unity.Collections.AllocatorManager;
using static StaticClassForAdditionalFunctions;
using Unity.VisualScripting.Antlr3.Runtime;
using System.Threading;
using System.Threading.Tasks;

public class ScoreManager : MonoBehaviour
{
    private static ScoreManager _instance;
    private Player _player;
    private Coroutine _zeroizeKillComboTicksCoroutine;
    private int _currentMinimumAmountCombo;
    private ProgressBar _progerssBarStyleRank;
    private static GameObject _prefubLeaderboard;
    private List<Action<bool>> _listAppliedRankFunction = new();
    private CancellationTokenSource _cts;


    [SerializeField] private int _currentKillCombo = 0;
    [SerializeField] private int _currentScore = 0;
    [SerializeField] private STYLE_RANK _currentRankStyle = STYLE_RANK.D;

    public static FieldInfo prefubFieldLeaderboard; // используем в Leaderboard, чтоб там не получать префаб при каждом создании лидерборда

    [NonSerialized] public RectTransform transformSpawnComboAdd;
    [NonSerialized] public RectTransform transformSpawnSkillUsed;

    public enum STYLE_RANK { D, C, B, A, S}
    public enum TYPE_APPEARING_MESSAGE { ComboAdded, SkillUsed, ComboMultyKill, RankImproved, SkillCombo, MasterOfSkills }

    public float styleMultiplier; // коэффициент усиления получения ресурсов от текущего ранга
    public float timeZeroizeKillComboTicks = 5; // время для сбрасывания текущего комбо за убийства. Начальное время при загрузке сцены
    public float minTimeZeroizeKillComboTicks = 1; // минимальное время для сбрасывания комбо за убийство (меньше нельзя)
    public float secondsAdditionalForZeroizeKillComboTicksByTimer = -1; // количество секунд прибавляемых ко времени сбрасывания комбо за убийства по срабатыванию таймера
    public float timeForAddSecondsForZeroizeKillComboTicks = 60; // время, через которое ко времени сбрасывания комбо за убийства прибавляется secondsAdditionalForZeroizeKillComboTicksByTimer
    public float timeFromStartLevel = 0; 
    public int maxKillCombo = 0; 



    public static ScoreManager Instance
    {
        get
        {
            if (_instance == null)
            {
                var obj = new GameObject("ScoreManager");
                _instance = obj.AddComponent<ScoreManager>();
                //DontDestroyOnLoad(obj);
            }
            return _instance;
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
        //DontDestroyOnLoad(gameObject);

        AssignParametersAndProperties(AdjustScoreManagerParameters.scoreManagerParameters, this);

        _prefubLeaderboard = Resources.Load<GameObject>(C.Paths.PrefubLeaderboard);
        prefubFieldLeaderboard = Resources.Load<FieldInfo>(C.Paths.FieldLeaderboardScaled);

        AppearingSprite.Initialize(); // инициализируем в тамошнем классе справочные данные для dictionaryPropertiesSprites
        AppearingText.Initialize(); // инициализируем в тамошнем классе справочные данные для dictionaryPropertiesSprites

        _cts = new CancellationTokenSource(); // пока не используем, но путь будет xD
    }

    private void Start()
    {
        _progerssBarStyleRank = _player.progerssBarStyleRank;

        CurrentRankStyle = STYLE_RANK.D;
        CurrentKillCombo = CurrentKillCombo;
        CurrentScore = CurrentScore;
        CurrentMinimumAmountCombo = CurrentMinimumAmountCombo;

        _progerssBarStyleRank.Initialize(rankProperties[CurrentRankStyle].min, rankProperties[CurrentRankStyle].max);

        StartCoroutine(ChangeComboTime(secondsAdditionalForZeroizeKillComboTicksByTimer));

    }

    private void Update()
    {
        timeFromStartLevel += Time.deltaTime;
    }



    #region ActionsCombo


    private int _currentActionsCombo;
    private int _indexTargetActionInClaster;
    private bool _blockActionCombo = false;
    private int _currentAmountInvalidActionsForStopCombo = 0;
    private Coroutine _zeroizeActionComboTicksCoroutine;

    public static List<List<string>> clastersSynergisticActions = new()
    {
        new List<string> { "SomeSpell1", "SomeSpell2", "SomeSpell3" }
    };
    public float timeZeroizeActionComboTicks = 2;
    public float timeBlockActionCombo = 5;
    public float multiplayerActionCombo = 10;
    public int thresholdAmountInvalidActionsForStopCombo = 3;

    private int CurrentActionsCombo
    {
        get { return _currentActionsCombo; }
        set
        {
            if (value == 0)
            {
                _currentAmountInvalidActionsForStopCombo = 0;
                if (_currentActionsCombo != 0)
                {
                    StartCoroutine(BlockActionComboForTime());
                }
            }
            else if (value > 1) // если комбо активностей равно 1 или 0, ничего не спавним
            {
                //InvokeAppearingSprite(TYPE_APPEARING_MESSAGE.SkillCombo);
                InvokeAppearingText(TYPE_APPEARING_MESSAGE.SkillCombo);
            }

            _currentActionsCombo = value;

            if (value > 1) CurrentKillCombo += (int)(value * multiplayerActionCombo); // если комбо активностей равно 1 или 0, ничего основному комбо не добавляем
        }
    }


    // пожалуй, будем вызывать при каждой activity (то есть при нажатии на спел или на предмет)
    public void UpActionCombo(int addictingCombo = 0, string nameAction = "")
    {
        if (!_blockActionCombo)
            {foreach (List<string> clasterSynergisticActions in clastersSynergisticActions)
            {
                if (clasterSynergisticActions.Contains(nameAction)) // если интересующая нас активность есть в массиве
                {
                    if (CurrentActionsCombo == 0) // если мы только входим в комбо. Подразумевается, что можем войти в комбо не с начала списка активностей, а с середины или под конец,
                                                  // а если комбо уже идёт, то нужно проверять условие, является ли текущая активность следующей по списку
                    {
                        CurrentActionsCombo = addictingCombo; // блокируем бонус за единичное комбо уже непосредственно в свойстве CurrentActionsCombo
                        _indexTargetActionInClaster = clasterSynergisticActions.IndexOf(nameAction) + 1; // индекс следующей за текущей активности в данном списке активностей

                        // Останавливаем предыдущую корутину (если она существует)
                        if (_zeroizeActionComboTicksCoroutine != null)
                        {
                            CoroutineManager.Instance.StopManagedCoroutine(this.gameObject, _zeroizeActionComboTicksCoroutine);
                        }

                        // Запускаем новую корутину
                        _zeroizeActionComboTicksCoroutine = CoroutineManager.Instance.StartManagedCoroutine(this.gameObject, ZeroizeActionComboTicks());

                        if (clasterSynergisticActions.Count == 1 || clasterSynergisticActions.Count == _indexTargetActionInClaster)
                            CurrentActionsCombo = 0; // если у нас был всего один элемент в списке (вряд ли) или прожата была последняя активность в списке
                    }
                    else
                    {
                        if (nameAction == clasterSynergisticActions[_indexTargetActionInClaster]) // проверяем, что текущая активность соответствует требуемой следующей активности в комбо
                        {
                            CurrentActionsCombo += addictingCombo;
                            _indexTargetActionInClaster += 1;

                            // Останавливаем предыдущую корутину (если она существует)
                            if (_zeroizeActionComboTicksCoroutine != null)
                            {
                                CoroutineManager.Instance.StopManagedCoroutine(this.gameObject, _zeroizeActionComboTicksCoroutine);
                            }

                            // Запускаем новую корутину
                            _zeroizeActionComboTicksCoroutine = CoroutineManager.Instance.StartManagedCoroutine(this.gameObject, ZeroizeActionComboTicks());

                            if (clasterSynergisticActions.Count == _indexTargetActionInClaster) // если текущая активность в комбо последняя в списке
                            {
                                CurrentActionsCombo = 0;
                            }
                        }
                        else
                        {
                            ControlAmountInvalidActions();
                        }
                    }
                }
                else
                {
                    ControlAmountInvalidActions();
                }
            }
        }
     
    }

    private void ControlAmountInvalidActions()
    {
        _currentAmountInvalidActionsForStopCombo++;
        if (_currentAmountInvalidActionsForStopCombo >= thresholdAmountInvalidActionsForStopCombo)
        {
            CurrentActionsCombo = 0;
        }
    }

    IEnumerator ZeroizeActionComboTicks()
    {
        yield return new WaitForSeconds(timeZeroizeActionComboTicks); // Ждем 1 секунду

        // Сбрасываем комбо после задержки
        CurrentActionsCombo = 0;
        _zeroizeActionComboTicksCoroutine = null; // Сбрасываем ссылку на корутину
    }
    IEnumerator BlockActionComboForTime()
    {
        _blockActionCombo = true;

        yield return new WaitForSeconds(timeBlockActionCombo); // Ждем 1 секунду

        _blockActionCombo = false;
    }

    #endregion


#region MasterOfSkills

    [NonSerialized] public int amountUpKomboMasterOfSkills = 1000;
    [NonSerialized] public bool isMasterOfSkillsReady = true;
    [NonSerialized] public float timeCallDownMasterOfSkills = 10f;

    public void AchivementMasterOfSkills()
    {
        if (isMasterOfSkillsReady)
        {
            if (_player.Inventory.listSpellsInInventory.Count < _player.CountAvailableSpellPlaces) // бонус получаем только при максимально заполненном инвентаре заклинаний 
            {
                return;
            }

            bool allSpellsInCD = true;
            foreach (Spell spell in _player.Inventory.listSpellsInInventory)
            {
                if (spell.isReady)
                {
                    allSpellsInCD = false;
                    break;
                }
            }

            if (allSpellsInCD)
            {
                //InvokeAppearingSprite(TYPE_APPEARING_MESSAGE.MasterOfSkills);
                InvokeAppearingText(TYPE_APPEARING_MESSAGE.MasterOfSkills);
                UpCombo(amountUpKomboMasterOfSkills);
                StartCoroutine(CallDownMasterOfSkills());
            }
        }
    }


    IEnumerator CallDownMasterOfSkills()
    {
        isMasterOfSkillsReady = false;

        yield return new WaitForSeconds(timeCallDownMasterOfSkills);

        isMasterOfSkillsReady = true;
        AchivementMasterOfSkills();

    }

    #endregion


    [Serializable]
    public struct RankProperties
    {
        public int min;
        public int max;
        public float styleMultiplier;
        public Action<bool> functionRank; // это ссылка на лямбда-функцию, в которой будем получать ссылку на фунцию объекта ScoreManager.Instance. Хотя можно было бы просто получать ссылки
                                        // на статические методы класса ScoreManager без лямбда функции. 

    }

#region SomeRankWasReached

    public void RankСReached(bool isApplying)
    {
        if (isApplying)
        {
            _player.ChangeUnitParametersByPercentage(new Dictionary<string, float> { { C.DK.damage, 10f }, 
                                                                                     { C.DK.increasingGettingExperienceByKillComboTickPercentage, 10f }, 
                                                                                     { C.DK.increasingGettingMoneyByKillComboTickPercentage, 10f } }, true);
        }
        else
        {
            _player.ChangeUnitParametersByPercentage(new Dictionary<string, float> { { C.DK.damage, 10f },
                                                                                     { C.DK.increasingGettingExperienceByKillComboTickPercentage, 10f },
                                                                                     { C.DK.increasingGettingMoneyByKillComboTickPercentage, 10f } }, false);
        }

    }
    public void RankBReached(bool isApplying)
    {
        if (isApplying)
        {
            _player.ChangeUnitParametersByPercentage(new Dictionary<string, float> { { C.DK.damage, 10f }, 
                                                                                     { C.DK.increasingGettingExperienceByKillComboTickPercentage, 10f }, 
                                                                                     { C.DK.increasingGettingMoneyByKillComboTickPercentage, 10f } }, true);
        }
        else
        {
            _player.ChangeUnitParametersByPercentage(new Dictionary<string, float> { { C.DK.damage, 10f },
                                                                                     { C.DK.increasingGettingExperienceByKillComboTickPercentage, 10f },
                                                                                     { C.DK.increasingGettingMoneyByKillComboTickPercentage, 10f } }, false);
        }

    }
    public void RankAReached(bool isApplying)
    {
        if (isApplying)
        {
            _player.ChangeUnitParametersByPercentage(new Dictionary<string, float> { { C.DK.damage, 100f } }, true);
        }
        else
        {
            _player.ChangeUnitParametersByPercentage(new Dictionary<string, float> { { C.DK.damage, 100f } }, false);
        }

    }
    public void RankSReached(bool isApplying)
    {
        if (isApplying)
        {
            _player.ChangeUnitParametersByPercentage(new Dictionary<string, float> { { C.DK.damage, 1000f } }, true);
        }
        else
        {
            _player.ChangeUnitParametersByPercentage(new Dictionary<string, float> { { C.DK.damage, 1000f } }, false);
        }

    }

#endregion



    public Dictionary<STYLE_RANK, RankProperties> rankProperties = new Dictionary<STYLE_RANK, RankProperties> { };

    public STYLE_RANK CurrentRankStyle
    {
        get { return _currentRankStyle; }
        set
        {
            List<Action<bool>> actionsForAddOrRomove = new();
            if (value > _currentRankStyle)
            {
                for (STYLE_RANK i = _currentRankStyle + 1; i <= value; i++) // текущий ранг пропускаем, целевой включаем
                {
                    if (rankProperties[i].functionRank != null)
                    {
                        Debug.Log("RANGGGGGG " + i);
                        rankProperties[i].functionRank?.Invoke(true);
                        actionsForAddOrRomove.Add(rankProperties[i].functionRank);
                    }
                }
                _listAppliedRankFunction.AddRange(actionsForAddOrRomove);
            }
            else
            {
                for (STYLE_RANK i = _currentRankStyle; i > value; i--) // текущий ранг включаем, целевой пропускаем
                {
                    if (rankProperties[i].functionRank != null)
                    {
                        rankProperties[i].functionRank?.Invoke(false);
                        actionsForAddOrRomove.Add(rankProperties[i].functionRank);
                    }
                }
                foreach (Action<bool> item in actionsForAddOrRomove)
                {
                    _listAppliedRankFunction.Remove(item);                    
                }
            }
            _player.rankStyle.CurrentStyleRank = value;
            _currentRankStyle = value;

            if (value != STYLE_RANK.D)
            {
                //InvokeAppearingSprite(TYPE_APPEARING_MESSAGE.RankImproved);
                InvokeAppearingText(TYPE_APPEARING_MESSAGE.RankImproved);
            }


            styleMultiplier = rankProperties[value].styleMultiplier;

            _progerssBarStyleRank.Initialize(rankProperties[value].min, rankProperties[value].max);

            EventBus.Instance.RankWasChanged(value); // изменяем UI
        }
    }

    private void SetRank(int value)
    {
        foreach (var properties in rankProperties)
        {
            if (value >= properties.Value.min && value <= properties.Value.max)
            {
                if (CurrentRankStyle != properties.Key)
                {
                    CurrentRankStyle = properties.Key;
                    //_progerssBarStyleRank.CurrentValue = value;

                    Debug.Log("Ранг: " + CurrentRankStyle);
                }
                return; // Важно: выходим из цикла, как только нашли подходящий диапазон
            }
        }

        Debug.LogError("Значение вне допустимого диапазона!"); // Если не попали ни в один диапазон
    }


    public int CurrentMinimumAmountCombo
    {
        get { return _currentMinimumAmountCombo; }
        set
        {
            if (_currentMinimumAmountCombo == CurrentKillCombo)
            {
                CurrentKillCombo = value;
            }
            _currentMinimumAmountCombo = value;
        }
    }

    public int CurrentKillCombo
    {
        get { return _currentKillCombo; }
        set
        {
            _currentKillCombo = value;
            if (value > 0)
            {
                //Debug.Log(value);
                //InvokeAppearingSprite(TYPE_APPEARING_MESSAGE.ComboAdded);
                InvokeAppearingText(TYPE_APPEARING_MESSAGE.ComboAdded);
            }

            // Вызываем событие, если есть подписчики
            EventBus.Instance.KillComboWasChanged(value);
            SetRank(value);
            _progerssBarStyleRank.CurrentValue = value;

            if (value > maxKillCombo) // обновляем максимальное комбо, если текущее комбо превышает текущее максимальное
            {
                maxKillCombo = value;
            }
        }
    }

    public int CurrentScore
    {
        get { return _currentScore; }
        set
        {
            _currentScore = value;
            EventBus.Instance.ScoreWasChanged(value);
        }
    }

    IEnumerator ChangeComboTime(float timeChange)
    {
        while (true)
        {
            yield return new WaitForSeconds(timeForAddSecondsForZeroizeKillComboTicks);
            if (timeZeroizeKillComboTicks > minTimeZeroizeKillComboTicks)
            {
                timeZeroizeKillComboTicks += timeChange;
            }
        }
    }

    public void InvokeAppearingSprite(TYPE_APPEARING_MESSAGE typeAppearingMessage)
    {
        AppearingSprite sciptAppearingSprite = Instantiate(GameManager.Instance.prefubAppearingSprite).GetComponent<AppearingSprite>();
        sciptAppearingSprite.SetProperlyAnimationAndPosition(typeAppearingMessage);
    }

    public void InvokeAppearingText(TYPE_APPEARING_MESSAGE typeAppearingMessage)
    {
        AppearingText sciptAppearingSprite = Instantiate(GameManager.Instance.prefubAppearingText).GetComponent<AppearingText>();
        sciptAppearingSprite.SetProperlyPosition(typeAppearingMessage);
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
        //Debug.Log(GetInstanceID());
        CurrentKillCombo = CurrentMinimumAmountCombo;
        _zeroizeKillComboTicksCoroutine = null; // Сбрасываем ссылку на корутину
    }



    public CancellationTokenSource leaderboardCts; // в ScoreManager.Instance и UpdateLeaderboardServerAsync, GetScoreLeaderboarderAsync проверяем,
    // не прервали мы показ лидерборда (по сути может быть только при новом спавне лидерборда). А нет, не только, мы теперь это проверяем при любом вызове GetActualLeaderboardAsync

    public async void GetAndShowActualLeaderboardAsync(Leaderboard.INSTANTIATION_CONTEXT instContext) // void - ибо не хотим обрабатывать ошибки в более выосоком контексте, нам вообще
    // всё равно, как это закончится. Нас всё устроит, всё обработано в await ScoreManager.Instance.GetAndShowActualLeaderboardAsync();
    {
        try
        {
            await GetActualLeaderboardAsync();

            //                           ВВВВВВВВВВНННННННННИИИИИИИИИИИММММММММММААААААААААНННННННННННИИИИИИИИИИИИИИЕЕЕЕЕЕЕЕЕЕЕЕ!!!!!!!!!!!! !!!!!!!!!!!!!
            // Instantiate(_prefubLeaderboard, _player.UI.position, Quaternion.identity, _player.UI); ЗАДАЁТ ГЛОБАЛЬНУЮ ПОЗИЦИЮ ДЛЯ ОБЪЕКТА. ТО ЕСТЬ ЧТОБ ОН ЗАСПАВНИЛСЯ В НУЛЕВОЙ ТОЧКЕ
            // ОТНОСИТЕЛЬНО РОДИТЕЛЯ НУЖНО УКАЗЫВАТЬ ВОТ ЭТО ВОТ: _player.UI.position --- ГЛОБАЛЬНУЮ ПОЗИЦИЮ РОДИТЕЛЯ !!!
            Debug.Log("И вот тут создаём");
            // ↓ НЕПОСРЕДСТВЕННО после await - создаём UI
            var parent = Player.instance.UIUpscaledMod
                ? _player.placementLeaderbaord
                : _player.UI;
            Leaderboard leaderboard = Instantiate(_prefubLeaderboard, _player.UI.position, Quaternion.identity, parent).GetComponent<Leaderboard>();
            leaderboard.AdjustLeaderboardAtInstantiate(instContext);
        }
        catch (OperationCanceledException)
        {
            Debug.Log("🔄 Leaderboard call canceled - duplicate prevention");
            return;
        }
    }

    public async Task GetActualLeaderboardAsync(CancellationToken externalToken = default) // по умолчанию работает токен только на отмену повторения наших асинхронных операций leaderboardCts
    // , но можно передать ещё дополнительный токен, вызывющийся, например, при уничтожении лидерборда (в теории не всегда при уничтожении лидерборда мы будем унитожать ScoreManager.Instance)
    {
        try
        {
            leaderboardCts?.Cancel();
            leaderboardCts?.Dispose();
            leaderboardCts = new CancellationTokenSource();

            // Создаём linked token source для объединения токенов
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                leaderboardCts.Token, externalToken);

            var token = linkedCts.Token;

            await PlayFabManager.Instance.UpdateLeaderboardServerAsync(token);
            await PlayFabManager.Instance.GetScoreLeaderboarderAsync(token);
        }
        catch (OperationCanceledException)
        {
            Debug.Log("🔄 Leaderboard data update canceled - duplicate prevention");
            throw;            
        }
    }


    private void OnDestroy()
    {
        _cts?.Cancel();
        _cts?.Dispose();

        leaderboardCts?.Cancel();
        leaderboardCts?.Dispose();

        //Debug.Log(GetInstanceID()); 
        if (_instance == this) _instance = null;
        CoroutineManager.Instance.StopAllCoroutinesFor(gameObject);
        StopAllCoroutines();
    }

}
