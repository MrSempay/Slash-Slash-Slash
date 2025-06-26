using UnityEngine;

public interface IInventoryPlayer : IInventoryUnit // убого, хотелось бы наследоваться от IInventoryUnit, но тогда, какого-то чёрта, надо и его свойство Self явно определять в классе реализаторе.
                                                   // Не, переделали. Класс Unit реализует IInventoryUnit и там же реализует Unit Self, то есть у Player этот Unit Self уже есть, он его просто переопределяет
{
    new Player Self { get; }
    void SomeEquipmentShouldBeActivate(Equipment equipment);
}
