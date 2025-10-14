using UnityEngine;

public class UISceneManager : UIController
{
    protected override void Start()
    {
        base.Start();

        SettingsMenu[] allObjects = Resources.FindObjectsOfTypeAll<SettingsMenu>();
        allObjects[0].Awake();
        SettingsMenu.Instance.Start(); 
    }


    private void OnDestroy()
    {
        GameManager.Instance.PauseGame(false); // если открыта панель настроек или меню и при этом сцена крашится - следующая, если запустится, то запустится в паузе. Проблема
                                               // с диалоговой сценой - может крашнутся, если нет нужного языка
    }
}
