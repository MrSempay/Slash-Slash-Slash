using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public RectTransform rtTopCenterPanel;
    public RectTransform rtTopRightPanel;
    public RectTransform rtBottomCenterPanel;
    public RectTransform rtBottomLeftPanel;
    public RectTransform rtTopLeftPanel;
    public enum TYPES_UI { Dialogue, MenuButton }

    [SerializeField] protected GameObject playMenu;
    [SerializeField] protected GameObject settingsMenu;
    [SerializeField] protected Camera cam;

    protected Vector2 bottomLeft = Vector2.zero;
    protected Vector2 bottomRight;
    protected Vector2 topLeft;
    protected Vector2 topRight;
    protected Vector2 topCenter;
    protected Vector2 bottomCenter;

    private List<RectTransform> _listRTPanels = new();
    public void OpenOrClosePlayMenu()
    {
        ////Debug.Log(settingsMenu.activeSelf);
        GameManager.Instance.PauseGame(!(playMenu.activeSelf || settingsMenu.activeSelf)); // если хотя бы одна менюшка активна, то значит нужна пауза, иначе снимаем её

        if (settingsMenu.activeSelf)
        {
            settingsMenu.SetActive(false);
            return;
        }
        playMenu.SetActive(!playMenu.activeSelf);
    }

    protected virtual void Awake()
    {
        if (cam != null)
        {
            // половина высоты и ширины
            float halfHeight = cam.orthographicSize;
            float halfWidth = halfHeight * cam.aspect;

            // центр камеры
            Vector3 c = cam.transform.position;

            // углы (в мировых координатах)
            bottomLeft = new Vector2(c.x - halfWidth, c.y - halfHeight);
            bottomRight = new Vector2(c.x + halfWidth, c.y - halfHeight);
            topLeft = new Vector2(c.x - halfWidth, c.y + halfHeight);
            topRight = new Vector2(c.x + halfWidth, c.y + halfHeight);
            topCenter = new Vector2(c.x, c.y + halfHeight);
            bottomCenter = new Vector2(c.x, c.y - halfHeight);

            // в рамках тестирования у нас есть несколько префабов Player, и у некоторых из них отсутствют эти элементы. Чтоб не ловить ошибки при тестировании, сделали так.
            if (rtTopLeftPanel != null)
            {
                rtTopLeftPanel.position = topLeft;
                _listRTPanels.Add(rtTopLeftPanel);
            }
            if (rtBottomLeftPanel != null)
            {
                rtBottomLeftPanel.position = bottomLeft;
                _listRTPanels.Add(rtBottomLeftPanel);
            }
            if (rtTopCenterPanel != null)
            {
                rtTopCenterPanel.position = topCenter;
                _listRTPanels.Add(rtTopCenterPanel);
            }
            if (rtBottomCenterPanel != null)
            {
                rtBottomCenterPanel.position = bottomCenter;
                _listRTPanels.Add(rtBottomCenterPanel);
            }
            if (rtTopRightPanel != null)
            {
                rtTopRightPanel.position = topRight;
                _listRTPanels.Add(rtTopRightPanel);
            }
        }
    }
    protected virtual void Start()
    {
    }

    public void HideAllUI()
    {
        foreach (RectTransform rtPanel in _listRTPanels)
        {
            foreach(Transform uiElement in rtPanel)
            {
                if (uiElement.GetComponent<NoHideUI>()) continue;
                uiElement.gameObject.SetActive(false);
            }
        }
    }
    public void ShowAllUI()
    {
        foreach (RectTransform rtPanel in _listRTPanels)
        {
            foreach (Transform uiElement in rtPanel)
            {
                uiElement.gameObject.SetActive(true);
            }
        }
    }
}
