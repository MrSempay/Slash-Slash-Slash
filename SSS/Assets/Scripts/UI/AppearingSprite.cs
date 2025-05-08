using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using static ScoreManager;

public class AppearingSprite : MonoBehaviour
{

    private string fullPath;
    private float _timeBeforeDisappearing = 2f;
    private new RectTransform transform;
    private static Dictionary<TYPE_APPEARING_MESSAGE, ParametersAppearingSprite> dictionaryPropertiesSprites;

    [NonSerialized] public Animator animator;
    [NonSerialized] public Sprite sprite;
    [NonSerialized] public Action<string> OnSomeAnimationWasFninished;

    public SpriteRenderer selfSprite;

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

    // пока что может вызываться только из ScoreManager, но с публичным доступом
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
            fullPath = C.Paths.PathFolderImagesForAppearingSprites + nameAnimation;
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

        StartCoroutine(StartDisappearingMessageTimer(_timeBeforeDisappearing));
    }

    private void BiasAllAnotherAppearingSpritesInGroupDown(float height, Transform transformParent)
    {
        if (transformParent.childCount > 1)
        {
            foreach (Transform childTransform in transformParent)
            {
                childTransform.localPosition = new Vector3(childTransform.localPosition.x, childTransform.localPosition.y - height, 0);
            }
        }
    }

    // вызывается через GameManager. Глобальный доступ. timeDisappearing = -1 для бесконечного спрайта!
    // Разберём малость подробнее: nameAnimation - имя анимации для спрайта, которая будет проигрываться. transformParent - родительский трансформ, которому появляющийся спрайт будет
    // назанчен как дочерний. timeDisappearing - время жизни спрайта (не путать со временем анимации для исчезания спрайта! Таковой по умолчанию тут вообще не предусмотрено!), -1 для
    // бесконечного спрайта. shouldBeOnlyOneSpriteInGroup - у заданного transformParent будет только один дочерний спрайт, каждый предыдущий будет удаляться при добавлении следующего.
    // shouldBeSpecifyControlPositionSpritesInGroup - по сути будет работать только когда shouldBeOnlyOneSpriteInGroup = false. Логика в том, что если данный параметр будет равен false,
    // то по умолчанию появляющиеся спрайты будут сдвигаться вниз, образуя столбик. Если параметр равен true, то контроль за положением спрайтов ложится на более высокую ступень управления,
    // на ту, из которой данный спрайт и создавался (в общем случае). На данный момент эта логика нужна для того, чтоб появляющиеся спрайты у заданного transformParent не сдвигались вниз,
    // а скапливались, по сути, друг на друге. Но от того, что спрайты у нас занимают только небольшую область сами по себе (и не перекрываются), будет создаваться впечатления, что они
    // находятся в разных местах. Подход пока что примитивный, но в будущем можно будет улучить
    public void SetProperlyAnimationAndPosition(string nameAnimation,
                                                Transform transformParent,
                                                float timeDisappearing,
                                                bool shouldBeOnlyOneSpriteInGroup,
                                                bool shouldBeSpecifyControlPositionSpritesInGroup = false)
    {

        if (StaticClassForAdditionalFunctions.AnimationExists(nameAnimation, animator))
        {
            animator.Play(nameAnimation);
        }
        else
        {
            animator.enabled = false;
            fullPath = C.Paths.PathFolderImagesForAppearingSprites + nameAnimation;
            sprite = Resources.Load<Sprite>(fullPath);
            if (sprite != null)
                selfSprite.sprite = sprite;
        }

        // Устанавливаем родительский Transform
        transform.SetParent(transformParent);

        // Дополнительные настройки (необязательно)
        transform.localPosition = Vector3.zero; // Обнуляем локальную позицию
        transform.localRotation = Quaternion.identity; // Обнуляем локальный поворот

        if (!shouldBeOnlyOneSpriteInGroup) // если должно быть больше одного дочернего спрайта у заданного transformParent
        {
            if (!shouldBeSpecifyControlPositionSpritesInGroup) 
            {
                Vector2 size = transform.sizeDelta;

                float height = size.y;

                BiasAllAnotherAppearingSpritesInGroupDown(height, transformParent);
            }
            else // для специфического контроля позиции спрайтов в группе тут пока что ничего не делаем, логику определяет управление свыше
            {

            }
        }
        else
        {
            foreach (Transform transformChildSprite in transformParent)
            {
                if (transformChildSprite != transform)
                {
                    Destroy(transformChildSprite.gameObject);
                }
            }
        }
        if (timeDisappearing != -1f)
        {
            StartCoroutine(StartDisappearingMessageTimer(timeDisappearing));
        }
    }





    IEnumerator StartDisappearingMessageTimer(float timeTimer)
    {
        yield return new WaitForSeconds(timeTimer);

        Destroy(gameObject);
    }


    // ---------------------- ФУНКЦИИ, СИГНАЛИЗИРУЮЩИЕ О ТОМ, ЧТО КАКАЯ-ТО АНИМАЦИЯ ЗАВЕРШИЛАСЬ (ну и бред, к чёрту идёт инкапсулированость, видимо) ---------------------//

    
    public void ProtectiveFieldAppearFinished()
    {
        OnSomeAnimationWasFninished.Invoke("ProtectiveFieldAppear");
    }
       
    public void ProtectiveFieldDisappearFinished()
    {
        OnSomeAnimationWasFninished.Invoke("ProtectiveFieldDisappear");
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

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

}
