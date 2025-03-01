using UnityEngine;

public class Spell : Equipment
{
    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        StaticClassForAdditionalFunctions.AssignParametersAndProperties(AdjustEquipmentParameters.spellParameters, this, equipmentName); // нужно вызвать до входа в первое состояние, поэтому до base
        base.Start();
    }

    public void SomeSpell1()
    {
        // Получаем массив всех объектов на сцене, которые являются экземплярами класса Enemy или его подклассов
        Enemy[] allEnemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);

        // Перебираем массив и делаем что-то с каждым врагом
        foreach (Enemy enemy in allEnemies)
        {
            Debug.Log("Найден враг: " + enemy.gameObject.name);
            enemy.Die();
        }
    }

}
