using UnityEngine;

public class UISceneManager : UIController
{
    private void Start()
    {

        Debug.Log("—ука " + GetInstanceID());
        SettingsMenu[] allObjects = Resources.FindObjectsOfTypeAll<SettingsMenu>();
        allObjects[0].Awake();
        SettingsMenu.Instance.Start(); 
    }


    private void OnDestroy()
    {
        GameManager.Instance.PauseGame(false); // если открыта панель настроек или меню и при этом сцена крашитс€ - следующа€, если запуститс€, то запуститс€ в паузе. ѕроблема
                                               // с диалоговой сценой - может крашнутс€, если нет нужного €зыка
    }
}
