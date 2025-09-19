using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System;

public class PanelChoose : MonoBehaviour
{
    public static GameObject InstanceTextButtonPanelChoose(PanelChoose panelChoose, string baseLocalizationKey, Sprite iconButton, UnityAction onClickFunc, UnityAction onPressFunc)
    {
        RectTransform rectTransformPanelChoose = (RectTransform)panelChoose.gameObject.transform;

        GameObject objectButton = Instantiate(GameManager.Instance.prefubTextButtonPanelChoose, rectTransformPanelChoose, false);

        ButtonTextPanelChoose buttonTextPanelChoose = objectButton.GetComponent<ButtonTextPanelChoose>();

        buttonTextPanelChoose.textButton.SetBaseText(baseLocalizationKey);
        buttonTextPanelChoose.buttonComponent.onClick.AddListener(onClickFunc);
        buttonTextPanelChoose.OnPress = onPressFunc;
        buttonTextPanelChoose.panelChoose = panelChoose;

        //buttonTextPanelChoose.imageComponent.sprite = iconButton ?? iconButton; // ну и дичь, так нельз€. »бо, видите ли, Sprite это Unity-тип

        if (iconButton != null)
        {
            buttonTextPanelChoose.imageComponent.sprite = iconButton;
        }


        return objectButton;
    }
}
