using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonTextPanelChoose : ButtonText, IPointerDownHandler, IPointerUpHandler
{
    public Image imageComponent;
    public UnityAction OnPress;
    public UnityAction OnRelease;

    [NonSerialized] public PanelChoose panelChoose;

    public void DisablePanelChoose()
    {
        panelChoose.gameObject.SetActive(false);
    }


    public void OnPointerDown(PointerEventData eventData)
    {
        if (OnPress != null)
        {
            OnPress.Invoke();
            DisablePanelChoose();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        OnRelease?.Invoke();
    }
}
