using System.Collections.Generic;
using System;
using UnityEngine;
using static ScoreManager;
using System.Collections;

public class AppearingText : MonoBehaviour
{
    protected float _timeBeforeDisappearing = 2f;
    protected new RectTransform transform;
    private static Dictionary<TYPE_APPEARING_MESSAGE, ParametersAppearingText> dictionaryPropertiesSprites;

    [SerializeField] protected TextEdit textMessage;

    private class ParametersAppearingText
    {
        public string text; // эту штуку инициализируем в конструкторе класса при создании объекта ParametersAppearingSprite в словаре dictionaryNamesAnimations
        public RectTransform transformParentForSpawnPosition; // эту штуку инициализируем в Initialize

        public ParametersAppearingText(string nameVisualisation, RectTransform transformParentForSpawnPosition)
        {
            this.text = nameVisualisation;
            this.transformParentForSpawnPosition = transformParentForSpawnPosition;
        }
    }

    public static void Initialize()
    {
        dictionaryPropertiesSprites = new Dictionary<TYPE_APPEARING_MESSAGE, ParametersAppearingText>()
        {
            { TYPE_APPEARING_MESSAGE.ComboAdded, new ParametersAppearingText("ComboAdded", GameObject.Find("ComboAddSpawn").GetComponent<RectTransform>()) },
            { TYPE_APPEARING_MESSAGE.SkillUsed, new ParametersAppearingText("SkillUsed", GameObject.Find("SkillUsedSpawn").GetComponent<RectTransform>()) },
            { TYPE_APPEARING_MESSAGE.ComboMultyKill, new ParametersAppearingText("ComboMultyKill", GameObject.Find("ComboMultyKillSpawn").GetComponent<RectTransform>()) },
            { TYPE_APPEARING_MESSAGE.RankImproved, new ParametersAppearingText("RankImproved", GameObject.Find("RankImprovedSpawn").GetComponent<RectTransform>()) },
            { TYPE_APPEARING_MESSAGE.SkillCombo, new ParametersAppearingText("SkillCombo", GameObject.Find("SkillComboSpawn").GetComponent<RectTransform>()) },
            { TYPE_APPEARING_MESSAGE.MasterOfSkills, new ParametersAppearingText("MasterOfSkills", GameObject.Find("MasterOfSkillsSpawn").GetComponent<RectTransform>()) },
        };
    }

    private void Awake()
    {
        transform = GetComponent<RectTransform>();



    }



    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    // пока что может вызываться только из ScoreManager, но с публичным доступом
    public void SetProperlyPosition(TYPE_APPEARING_MESSAGE typeAppearingMessage)
    {
        //Debug.Log(typeAppearingMessage.ToString());
        RectTransform rectTranformParentGroup = dictionaryPropertiesSprites[typeAppearingMessage].transformParentForSpawnPosition;

        textMessage.SetBaseText(typeAppearingMessage.ToString());

        // Устанавливаем родительский Transform
        transform.SetParent(rectTranformParentGroup);

        // Дополнительные настройки (необязательно)
        transform.localPosition = Vector3.zero; // Обнуляем локальную позицию
        transform.localRotation = Quaternion.identity; // Обнуляем локальный поворот

        Vector2 size = transform.sizeDelta;

        float height = size.y;

        BiasAllAnotherAppearingTextInGroupDown(height, rectTranformParentGroup);

        StartCoroutine(StartDisappearingMessageTimer(_timeBeforeDisappearing));
    }

    protected virtual void BiasAllAnotherAppearingTextInGroupDown(float height, Transform transformParent)
    {
        if (transformParent.childCount > 1)
        {
            foreach (Transform childTransform in transformParent)
            {
                childTransform.localPosition = new Vector3(childTransform.localPosition.x, childTransform.localPosition.y - height/2, 0);
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
    public void SetProperlyPosition(string text,
                                    Transform transformParent,
                                    float timeDisappearing,
                                    bool shouldBeOnlyOneTextInGroup,
                                    bool shouldBeSpecifyControlPositionTextInGroup = false)
    {

        textMessage.SetBaseText(text);

        // Устанавливаем родительский Transform
        transform.SetParent(transformParent);

        // Дополнительные настройки (необязательно)
        transform.localPosition = Vector3.zero; // Обнуляем локальную позицию
        transform.localRotation = Quaternion.identity; // Обнуляем локальный поворот

        if (!shouldBeOnlyOneTextInGroup) // если должно быть больше одного дочернего спрайта у заданного transformParent
        {
            if (!shouldBeSpecifyControlPositionTextInGroup)
            {
                Vector2 size = transform.sizeDelta;

                float height = size.y;

                BiasAllAnotherAppearingTextInGroupDown(height, transformParent);
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





    protected virtual IEnumerator StartDisappearingMessageTimer(float timeTimer)
    {
        yield return new WaitForSeconds(timeTimer);

        Destroy(gameObject);
    }


    // ---------------------- ФУНКЦИИ, СИГНАЛИЗИРУЮЩИЕ О ТОМ, ЧТО КАКАЯ-ТО АНИМАЦИЯ ЗАВЕРШИЛАСЬ (ну и бред, к чёрту идёт инкапсулированость, видимо) ---------------------//




    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}
