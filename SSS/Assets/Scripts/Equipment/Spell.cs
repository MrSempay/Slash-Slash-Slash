using UnityEngine;

public class Spell : Equipment
{
    public override void Awake()
    {
        base.Awake();
    }

    public override void Start()
    {
        StaticClassForAdditionalFunctions.AssignParametersAndProperties(AdjustEquipmentParameters.spellParameters, this, equipmentName); // нужно вызвать до входа в первое состояние, поэтому до base
        base.Start();
    }

    public virtual void ProtectiveField(Unit whoCastedSpell)
    {
        // Получаем массив всех объектов на сцене, которые являются экземплярами класса Enemy или его подклассов 
        Enemy[] allEnemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);

        // Перебираем массив и делаем что-то с каждым врагом 
        foreach (Enemy enemy in allEnemies)
        {
            Debug.Log("Найден враг: " + enemy.gameObject.name);
            enemy.Die(whoCastedSpell);
        }
        StartCallDown();
    }
    public void SomeSpell2(Unit whoCastedSpell)
    {
        StartCallDown();
    }
    public void SomeSpell3(Unit whoCastedSpell)
    {
        StartCallDown();
    }

}
