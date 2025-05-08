using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using static ScoreManager;

public class RankStyle : MonoBehaviour
{
    private Dictionary<STYLE_RANK, Sprite> _dictionarySpritesOfRankStyle = new();
    private string _folderImagesForRankStyles = C.Paths.FolderImagesForRankStyles;
    private STYLE_RANK _currentStyleRank = STYLE_RANK.D;

    [SerializeField] private Animator _animator;
    [SerializeField] private Image _selfSprite;

    public STYLE_RANK CurrentStyleRank
    {
        get { return _currentStyleRank; }
        set
        {
            string nameAnimationChangingStyle = _currentStyleRank.ToString() + "-" + value.ToString(); // формат: "B-A"

            if (StaticClassForAdditionalFunctions.AnimationExists(nameAnimationChangingStyle, _animator))
            {
                _animator.enabled = true;
                _animator.Play(nameAnimationChangingStyle);
            }
            else
            {
                _animator.enabled = false;
                _selfSprite.sprite = _dictionarySpritesOfRankStyle[value]; // пока что просто устанавливаем спрайт целевого ранга. Как появится общая анимация, будем предварительно
                                                                           // проигрывать её, и после её окончания выставлять уже нужный ранг
            }
            _currentStyleRank = value;
        }
    }

    private void Awake()
    {
        _dictionarySpritesOfRankStyle[STYLE_RANK.D] = Resources.Load<Sprite>(_folderImagesForRankStyles + STYLE_RANK.D.ToString());
        _dictionarySpritesOfRankStyle[STYLE_RANK.C] = Resources.Load<Sprite>(_folderImagesForRankStyles + STYLE_RANK.C.ToString());
        _dictionarySpritesOfRankStyle[STYLE_RANK.B] = Resources.Load<Sprite>(_folderImagesForRankStyles + STYLE_RANK.B.ToString());
        _dictionarySpritesOfRankStyle[STYLE_RANK.A] = Resources.Load<Sprite>(_folderImagesForRankStyles + STYLE_RANK.A.ToString());
        _dictionarySpritesOfRankStyle[STYLE_RANK.S] = Resources.Load<Sprite>(_folderImagesForRankStyles + STYLE_RANK.S.ToString());

        _selfSprite.sprite = _dictionarySpritesOfRankStyle[STYLE_RANK.D];
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
