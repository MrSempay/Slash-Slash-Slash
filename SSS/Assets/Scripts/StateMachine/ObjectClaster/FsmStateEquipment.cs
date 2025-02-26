using System.IO;
using UnityEngine;
using UnityEngine.AI;

public class FsmStateEquipment : FsmState
{
    protected Equipment equipment;

    public FsmStateEquipment(Fsm fsm, GameObject gameObject) : base(fsm, gameObject)
    {
        equipment = gameObject.GetComponent<Equipment>();

    }

    protected Equipment IsEquipmentPlaceOccupied(float x, float y)
    {
        Vector3 mousePosition = new Vector3(x, y, 0);
        //Debug.Log(mousePosition);
        //—оздаем невидимый коллайдер в центре €чейки
        Collider2D[] hits = Physics2D.OverlapCircleAll(mousePosition - new Vector3(0, 0, 1), 0.05f); //радиус небольшой,  чтобы захватить только центр €чейки.


        //¬изуализаци€ круга обнаружени€ (отображаетс€ только в редакторе)
        DebugExtension.DebugCircle(mousePosition - new Vector3(0, 0, 2), Vector3.forward, Color.red, 0.5f, false, 0.5f);
        //≈сли коллайдер что-то обнаружил, значит €чейка зан€та

        foreach (Collider2D hit in hits)
        {
            //Debug.Log("ќбнаружен коллайдер: " + hit.gameObject.name);

            GameObject someGameObject = hit.gameObject; // ѕолучаем GameObject
            if (someGameObject.CompareTag("Equipment"))
            {
                if (equipment._fsm.StateCurrent.GetType() == typeof(FsmStateEquipmentSelected)) return someGameObject.GetComponent<Equipment>(); // возвращаем любое снар€жение

                if (someGameObject.GetComponent<Equipment>() == equipment) return someGameObject.GetComponent<Equipment>(); // возвращаем только то снар€жение, которое равно экземпл€ру
                                                                                                                            // снар€жени€, из состо€ни€ которого мы вызываем метод
            }
        }

        return null;
    }

 

}
