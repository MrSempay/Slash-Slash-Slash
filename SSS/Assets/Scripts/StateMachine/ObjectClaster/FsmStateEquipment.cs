using System.IO;
using UnityEngine;
using UnityEngine.AI;

public class FsmStateEquipment : FsmState
{
    protected Equipment equipment;
    protected Vector3 baseLocalPositionInfoPanel;

    public FsmStateEquipment(Fsm fsm, GameObject gameObject) : base(fsm, gameObject)
    {
        equipment = gameObject.GetComponent<Equipment>();
        baseLocalPositionInfoPanel = equipment.transformPlaceInfoPanel.localPosition;

    }

    protected Equipment IsEquipmentPlaceOccupied(float x, float y)
    {
        if (!Player.isTransitingEquipment) // благодар€ этому, по идее, только одно снар€жение может быть в состо€нии Selected. »змен€етс€ только в том состо€нии
        {
            Vector3 mousePosition = new Vector3(x, y, 0);
            //—оздаем невидимый коллайдер в центре €чейки
            Collider2D[] hits = Physics2D.OverlapCircleAll(mousePosition - new Vector3(0, 0, 1), 0.5f); //радиус небольшой,  чтобы захватить только центр €чейки.


            //¬изуализаци€ круга обнаружени€ (отображаетс€ только в редакторе)
            DebugExtension.DebugCircle(mousePosition - new Vector3(0, 0, 2), Vector3.forward, Color.red, 0.5f, false, 0.5f);
            //≈сли коллайдер что-то обнаружил, значит €чейка зан€та

            foreach (Collider2D hit in hits)
            {
                ////Debug.Log("ќбнаружен коллайдер: " + hit.gameObject.name);

                GameObject someGameObject = hit.gameObject; // ѕолучаем GameObject
                if (someGameObject.CompareTag("Equipment"))
                {
                    if (equipment._fsm.StateCurrent.GetType() == typeof(FsmStateEquipmentSelected)) return someGameObject.GetComponent<Equipment>(); // возвращаем любое снар€жение

                    if (someGameObject.GetComponent<Equipment>() == equipment) return someGameObject.GetComponent<Equipment>(); // возвращаем только то снар€жение, которое равно экземпл€ру
                                                                                                                                // снар€жени€, из состо€ни€ которого мы вызываем метод
                }
            }
        }

        return null;
    }

    protected void SetPositionDescriptionPanel()
    {
        if (equipment.transform.position.x < Player.instance.transform.position.x) // если игрок справа от снар€жени€, то направо и перемещаем информационную панельку
        {
            equipment.transformPlaceInfoPanel.localPosition = baseLocalPositionInfoPanel;
        }
        else
        {
            equipment.transformPlaceInfoPanel.localPosition = new Vector3(baseLocalPositionInfoPanel.x * -1f, baseLocalPositionInfoPanel.y, baseLocalPositionInfoPanel.z);
        }
    }

 

}
