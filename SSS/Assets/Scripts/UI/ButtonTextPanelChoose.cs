using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonTextPanelChoose : ButtonText, IPointerDownHandler, IPointerUpHandler
{
    public static Sprite spriteChangeSlotButton;
    public static Sprite spriteShowInfoButton;

    public Image imageComponent;
    public UnityAction OnPress;
    public UnityAction OnRelease;

    [NonSerialized] public PanelChoose panelChoose;

    public static void Initialize()
    {
        spriteChangeSlotButton = Resources.Load<Sprite>(C.Paths.ChangeSlotButton);
        spriteShowInfoButton = Resources.Load<Sprite>(C.Paths.ShowInfoButton);
    }

    public void DisablePanelChoose()
    {
        panelChoose.gameObject.SetActive(false);
    }


    public void OnPointerDown(PointerEventData eventData)
    {
        if (OnPress != null)
        {
            OnPress.Invoke();
            PlaySound();
            DisablePanelChoose();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        OnRelease?.Invoke();
    }

    private void Awake()
    {
        //base.OnEnable();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }
}
