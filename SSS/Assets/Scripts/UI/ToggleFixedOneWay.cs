using UnityEngine;

public class ToggleFixedOneWay : ToggleFixed
{
    float mudaDAYIOOO = 15;
    public override void OnMouseClicked()
    {
        //Debug.Log("shit");
        if (!IsToggled) // если тумблер не прожат, то мы можем его прожать. В обратную сторону отжать нельзя, оттого и OneWay
        {
            IsToggled = true;
        }
    }


}
