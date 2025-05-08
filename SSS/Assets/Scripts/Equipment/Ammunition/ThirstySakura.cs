using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class ThirstySakura : Ammunition
{

    public override void EnteredIntoInventory(Unit ownerInventory)
    {
        //Debug.Log("mda");


        // если хотим объединить в один счЄтчик сразу несколько предметов:

        /*
        CustomCombo scriptExistingKillCountCombo = Player.instance.rectTransformPlaceCustomCombos.GetComponentInChildren<CustomCombo>(); // чекаем, есть ли от предыдущей —акуры комбо уже

        if (scriptExistingKillCountCombo != null)
        {
            UnityAction<int> upCombo1 = scriptExistingKillCountCombo.UpCombo;

            EventBus.Instance.OnOneEnemyWasKilledByPlayer.AddListener(upCombo1); // если да, параллельно вешаем ещЄ один детектор на OnOneEnemyWasKilledByPlayer
            
            scriptExistingKillCountCombo.AddMethodListenerToDictionary(this, upCombo1);

            return;
        }*/

        CustomCombo scriptCustomCombo = Instantiate(GameManager.Instance.prefubCustomCombo, Player.instance.rectTransformPlaceCustomCombos);
        scriptCustomCombo.Initialize("KillCount", scriptCustomCombo.IncreaseDamageHeroeByTick, 0); // передаЄм базовый текст дл€ комбо (оно же будет именем объекта), ссылку на метод,
                                                                                                   // который будет срабатывать при изменении комбо, а также врем€ сбрасывани€ комбо (0 тут)

        UnityAction<int> upCombo = scriptCustomCombo.UpCombo; // ссылка на метод, который прив€зываем к событию убийства врага  
        EventBus.Instance.OnOneEnemyWasKilledByPlayer.AddListener(upCombo);

        scriptCustomCombo.AddMethodListenerToDictionary(this, upCombo); // теоретически можно перенести в Initialize. ’от€ нет, иногда нам нужно просто добавить новый объект дл€ инду
                                                                        // цировани€ изменени€ комбо при этом не создава€ его (например комбо одно, а его измен€ют несколько объектов)
    }
    public override void ExitedFromInventory(Unit ownerInventory)
    {
        //Debug.Log("mda");

        base.ExitedFromInventory(ownerInventory);

        foreach (RectTransform killCount in Player.instance.rectTransformPlaceCustomCombos)
        {
            CustomCombo scriptCustomCombo = killCount.GetComponent<CustomCombo>();

            if (scriptCustomCombo.DictionaryListenerMethods.ContainsKey(this))
            {
                scriptCustomCombo.CurrentCombo = 0;

                EventBus.Instance.OnOneEnemyWasKilledByPlayer.RemoveListener(scriptCustomCombo.DictionaryListenerMethods[this]);

                Destroy(scriptCustomCombo.gameObject);

                return;
            }
        }

        //CustomCombo scriptCustomCombo = Player.instance.rectTransformPlaceCustomCombos.Find("KillCount").GetComponent<CustomCombo>();
        //scriptCustomCombo.CurrentCombo = 0;


        // если есть несколько источников пополнени€ комбо, а само комбо должно быть одно:

        //if (scriptCustomCombo.DictionaryListenerMethods.Count == 1)
        //{
        //    Destroy(scriptCustomCombo.gameObject);
        //}
    }

}
