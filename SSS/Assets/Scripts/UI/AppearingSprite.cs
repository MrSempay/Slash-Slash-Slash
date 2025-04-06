using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using static ScoreManager;

public class AppearingSprite : MonoBehaviour
{

    private Animator animator;
    private string fullPath;
    private float _timeBeforeDisappearing = 2f;
    private Sprite sprite;
    private new RectTransform transform;
    private static Dictionary<TYPE_APPEARING_MESSAGE, ParametersAppearingSprite> dictionaryPropertiesSprites;

    [SerializeField] private SpriteRenderer selfSprite;

    private class ParametersAppearingSprite
    {
        public string nameVisualisation; // эту штуку инициализируем в конструкторе класса при создании объекта ParametersAppearingSprite в словаре dictionaryNamesAnimations
        public RectTransform transformParentForSpawnPosition; // эту штуку инициализируем в Initialize

        public ParametersAppearingSprite(string nameVisualisation, RectTransform transformParentForSpawnPosition)
        {
            this.nameVisualisation = nameVisualisation;
            this.transformParentForSpawnPosition = transformParentForSpawnPosition;
        }
    }
    
    public static void Initialize()
    {
        dictionaryPropertiesSprites = new Dictionary<TYPE_APPEARING_MESSAGE, ParametersAppearingSprite>()
        {
            { TYPE_APPEARING_MESSAGE.ComboAdded, new ParametersAppearingSprite("ComboAdd", GameObject.Find("ComboAddSpawn").GetComponent<RectTransform>()) },
            { TYPE_APPEARING_MESSAGE.SkillUsed, new ParametersAppearingSprite("SkillUsed", GameObject.Find("SkillUsedSpawn").GetComponent<RectTransform>()) },
            { TYPE_APPEARING_MESSAGE.ComboMultyKill, new ParametersAppearingSprite("ComboMultyKill", GameObject.Find("ComboMultyKillSpawn").GetComponent<RectTransform>()) },
            { TYPE_APPEARING_MESSAGE.RankImproved, new ParametersAppearingSprite("RankImproved", GameObject.Find("RankImprovedSpawn").GetComponent<RectTransform>()) },
            { TYPE_APPEARING_MESSAGE.SkillCombo, new ParametersAppearingSprite("SkillCombo", GameObject.Find("SkillComboSpawn").GetComponent<RectTransform>()) },
            { TYPE_APPEARING_MESSAGE.MasterOfSkills, new ParametersAppearingSprite("MasterOfSkills", GameObject.Find("MasterOfSkillsSpawn").GetComponent<RectTransform>()) },
        };
    }

    private void Awake()
    {
        transform = GetComponent<RectTransform>();
        animator = GetComponent<Animator>();



    }



    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetProperlyAnimationAndPosition(TYPE_APPEARING_MESSAGE typeAppearingMessage)
    {
        string nameAnimation = dictionaryPropertiesSprites[typeAppearingMessage].nameVisualisation;
        RectTransform rectTranformParentGroup = dictionaryPropertiesSprites[typeAppearingMessage].transformParentForSpawnPosition;

        if (StaticClassForAdditionalFunctions.AnimationExists(nameAnimation, animator))
        {
            animator.Play(nameAnimation);
        }
        else
        {
            animator.enabled = false;
            fullPath = C.DK.PathFolderImagesForAppearingMessages + nameAnimation;
            sprite = Resources.Load<Sprite>(fullPath);
            if (sprite != null)
                selfSprite.sprite = sprite;
        }

        // Устанавливаем родительский Transform
        transform.SetParent(rectTranformParentGroup);

        // Дополнительные настройки (необязательно)
        transform.localPosition = Vector3.zero; // Обнуляем локальную позицию
        transform.localRotation = Quaternion.identity; // Обнуляем локальный поворот

        Vector2 size = transform.sizeDelta;

        float height = size.y;

        BiasAllAnotherAppearingSpritesInGroupDown(height, rectTranformParentGroup);

        StartCoroutine(StartDisappearingMessageTimer());
    }


    private void BiasAllAnotherAppearingSpritesInGroupDown(float height, RectTransform rectTransformParent)
    {
        foreach (RectTransform childRect in rectTransformParent)
        {
            childRect.localPosition = new Vector3(childRect.localPosition.x, childRect.localPosition.y - height, 0);
        }
    }

    IEnumerator StartDisappearingMessageTimer()
    {
        yield return new WaitForSeconds(_timeBeforeDisappearing);
        Destroy(gameObject);
    }


    //IEnumerator RiseCurrentMusicTick(AudioClip explosionSound)
    //{
    //    yield return null;

    //    _audioMusicComponent.clip = explosionSound;
    //    _audioMusicComponent.Play();
    //    while (_audioMusicComponent.volume < GameManager.Instance.currentSettings.VolumeMusic)
    //    {
    //        //Debug.Log("???");
    //        _audioMusicComponent.volume += 0.015f;
    //        yield return new WaitForSeconds(_timeTickOfChangingVolumeBetweenMusic);
    //    }
    //}



}
