using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using static StaticClassForAdditionalFunctions;

public class AppearingNotification : AppearingText
{
    [SerializeField] private Image _background;
    [SerializeField] private TextMeshProUGUI _text;

    private float _timeRisingFading = 0.5f; // 0.5 секунда 

    public void SetProperlyPositionAndType(string text,
                                           TYPE_NOTIFICATION typeNotification,
                                           float liveTime,
                                           bool shouldBeOnlyOneTextInGroup,
                                           bool shouldBeSpecifyControlPositionTextInGroup = false)
    {
        switch (typeNotification)
        {
            case TYPE_NOTIFICATION.Success:
                _text.color = Color.green;
                break;

            case TYPE_NOTIFICATION.Warning:
                _text.color = Color.yellow;
                break;

            case TYPE_NOTIFICATION.Failure:
                _text.color = Color.red;
                break;
        }
        StartCoroutine(StartRisingOrFadingNotification(true));
        textMessage.SetBaseText(text);

        // ƒополнительные настройки (необ€зательно)
        transform.localPosition = Vector3.zero; // ќбнул€ем локальную позицию
        transform.localRotation = Quaternion.identity; // ќбнул€ем локальный поворот

        if (!shouldBeOnlyOneTextInGroup) // если должно быть больше одного дочернего спрайта у заданного transformParent
        {
            if (!shouldBeSpecifyControlPositionTextInGroup)
            {
                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(transform);
                float size = transform.rect.height;

                float height = size;

                BiasAllAnotherAppearingTextInGroupDown(height, GameManager.Instance.notificationPlacement);
            }
            else // дл€ специфического контрол€ позиции текстов в группе тут пока что ничего не делаем, логику определ€ет управление свыше
            {

            }
        }
        else
        {
            foreach (Transform transformChildSprite in GameManager.Instance.notificationPlacement)
            {
                if (transformChildSprite != transform)
                {
                    Destroy(transformChildSprite.gameObject);
                }
            }
        }
        if (liveTime != -1f)
        {
            StartCoroutine(StartDisappearingMessageTimer(liveTime - _timeRisingFading));
        }
    }


    protected override void BiasAllAnotherAppearingTextInGroupDown(float height, Transform transformParent)
    {
        int currentNumberOfChild = 0;
        if (transformParent.childCount > 1)
        {
            foreach (RectTransform childTransform in transformParent) 
            {
                currentNumberOfChild++;
                if (currentNumberOfChild == transformParent.childCount) // по сути пропускаем псоледний дочерний элемент в группе, ибо это как раз тот, который мы только что заспавнили. ќпускаем все относительно его.
                {
                    return;
                }
                childTransform.localPosition = new Vector3(childTransform.localPosition.x, childTransform.localPosition.y - (height/2 + childTransform.rect.height/2) * childTransform.localScale.y, 0);
            }
        }
    }


    protected override IEnumerator StartDisappearingMessageTimer(float timeTimer)
    {
        yield return new WaitForSecondsRealtime(timeTimer);
        StartCoroutine(StartRisingOrFadingNotification(false));
    }

    private IEnumerator StartRisingOrFadingNotification(bool isRising)
    {
        float alpha = isRising? 0f : 1f;
        float tickTimeAndAdditionalAlpha = 1f / (_timeRisingFading / Time.fixedDeltaTime);
        _background.color = new Color(
                            _background.color.r,
                            _background.color.g,
                            _background.color.b,
                            alpha
                            );
        _text.color = new Color(
                            _text.color.r,
                            _text.color.g,
                            _text.color.b,
                            alpha
                            );

        Color colorAlphaBackground;
        Color colorAlphaText;
        while (true)
        {
            yield return new WaitForSecondsRealtime(Time.fixedDeltaTime); 
            alpha += isRising? tickTimeAndAdditionalAlpha : -tickTimeAndAdditionalAlpha;

            colorAlphaBackground = new Color(
                                _background.color.r,
                                _background.color.g,
                                _background.color.b,
                                alpha
                             );
            colorAlphaText = new Color(
                                _text.color.r,
                                _text.color.g,
                                _text.color.b,
                                alpha
                             );



            if (isRising)
            { 
                if (alpha > 1f)
                {
                    alpha = 1f;

                    colorAlphaBackground = new Color(
                    _background.color.r,
                    _background.color.g,
                    _background.color.b,
                    alpha
                 );
                    colorAlphaText = new Color(
                                        _text.color.r,
                                        _text.color.g,
                                        _text.color.b,
                                        alpha
                                     );
                    break;
                }
            }
            else
            {
                if (alpha < 0f)
                {
                    Destroy(gameObject);
                    break;
                }
            }

            _background.color = colorAlphaBackground;
            _text.color = colorAlphaText;
        }

        if (isRising)
        {
            alpha = 1f;

            colorAlphaBackground = new Color(
                                _background.color.r,
                                _background.color.g,
                                _background.color.b,
                                alpha
                                );
            colorAlphaText = new Color(
                                _text.color.r,
                                _text.color.g,
                                _text.color.b,
                                alpha
                                );
            _background.color = colorAlphaBackground;
            _text.color = colorAlphaText;
        }
        else
        {
            Destroy(gameObject);
        }

    }

}
