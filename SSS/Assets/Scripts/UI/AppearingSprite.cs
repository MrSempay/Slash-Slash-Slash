using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using static ScoreManager;

public class AppearingSprite : MonoBehaviour
{

    private Animator animator;
    private string fullPath;
    private Sprite sprite;
    private RectTransform transformSpawnComboAdd;
    private RectTransform transformSpawnSkillUsed;
    private Dictionary<TYPE_APPEARING_MESSAGE, ParametersAppearingSprite> dictionaryNamesAnimations = new Dictionary<TYPE_APPEARING_MESSAGE, ParametersAppearingSprite>()
    {
        { TYPE_APPEARING_MESSAGE.ComboAdded, new ParametersAppearingSprite("ComboAdd") },
        { TYPE_APPEARING_MESSAGE.SkillUsed, new ParametersAppearingSprite("SkillUsed") },
    };

    [SerializeField] private SpriteRenderer selfSprite;


    private class ParametersAppearingSprite
    {
        public string nameVisualisation; // эту штуку инициализируем в конструкторе класса при создании объекта ParametersAppearingSprite в словаре dictionaryNamesAnimations
        public RectTransform transformParentForSpawnPosition; // эту штуку инициализируем в Awake

        public ParametersAppearingSprite(string nameVisualisation)
        {
            this.nameVisualisation = nameVisualisation;
        }
    }
    

    private void Awake()
    {
        animator = GetComponent<Animator>();

        transformSpawnComboAdd = GameObject.Find("ComboAddSpawn").GetComponent<RectTransform>();
        transformSpawnSkillUsed = GameObject.Find("SkillUsedSpawn").GetComponent<RectTransform>();

        dictionaryNamesAnimations[TYPE_APPEARING_MESSAGE.ComboAdded].transformParentForSpawnPosition = transformSpawnComboAdd;
        dictionaryNamesAnimations[TYPE_APPEARING_MESSAGE.SkillUsed].transformParentForSpawnPosition = transformSpawnSkillUsed;

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
        string nameAnimation = dictionaryNamesAnimations[typeAppearingMessage].nameVisualisation;
        if (StaticClassForAdditionalFunctions.AnimationExists(nameAnimation, animator))
        {
            animator.Play(nameAnimation);
        }
        else
        {
            animator.enabled = false;
            fullPath = C.DK.PathFolderImagesForAppearingMessages + nameAnimation;
            sprite = Resources.Load<Sprite>(fullPath);
            selfSprite.sprite = sprite;
        }

        // Устанавливаем родительский Transform
        transform.SetParent(dictionaryNamesAnimations[typeAppearingMessage].transformParentForSpawnPosition);

        // Дополнительные настройки (необязательно)
        transform.localPosition = Vector3.zero; // Обнуляем локальную позицию
        transform.localRotation = Quaternion.identity; // Обнуляем локальный поворот

    }


    //IEnumerator StartAppearingMessage(AudioClip explosionSound)
    //{
    //    while (_audioMusicComponent.volume > 0.03f)
    //    {
    //        _audioMusicComponent.volume -= 0.015f;
    //        yield return new WaitForSeconds(_timeTickOfChangingVolumeBetweenMusic);
    //    }
    //    StartCoroutine(RiseCurrentMusicTick(explosionSound));
    //}
    //IEnumerator FadeCurrentMusicTick(AudioClip explosionSound)
    //{
    //    while (_audioMusicComponent.volume > 0.03f)
    //    {
    //        _audioMusicComponent.volume -= 0.015f;
    //        yield return new WaitForSeconds(_timeTickOfChangingVolumeBetweenMusic);
    //    }
    //}

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
