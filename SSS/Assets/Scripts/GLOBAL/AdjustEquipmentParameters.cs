using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public static class AdjustEquipmentParameters : object
{
    public static readonly Dictionary<string, Dictionary<string, object>> spellParameters = new Dictionary<string, Dictionary<string, object>>()
    {
        // Инициализируем словарь при объявлении
        { C.DK.ProtectiveField, new Dictionary<string, object>() {
            { C.DK.equipmentName, C.DK.ProtectiveField },
            { C.DK.cost, 70 },
            { C.DK.timeCallDown, 15f },
            { C.DK.amountUpCombo, 10 },
            { C.DK.amountBlockingAttackMax, 4 },
            { C.DK.shouldBeCastedAtStartUnitAnimation, false }, // Вроде как разобрался: этот параметр отвечает за то, будет ли сразу при старте каста (анимации) активирована функция Cast и,
                                                                // как следствие, Activate. Если true, то дальнейшее управление ходом сотворения заклинания (реакции на анимацию) передаётся
                                                                // самому Spell, именно там в функции Activate устанавливается прослушка сигналов о пике и окочании анимации. Если же false,
                                                                // то функция Cast (и Activate) будет вызвана только после окончания анимации, до этого момента сам Spell не может ни на 
                                                                // что реагировать. Вообщем данный параметр определяет, кому и на каком этапе будет передано управление сотворения каста.
            { C.DK.durationActiveState, 8f },
        } },
        { C.DK.ArcLightning, new Dictionary<string, object>() {
            { C.DK.equipmentName, C.DK.ArcLightning },
            { C.DK.cost, 140 },
            { C.DK.damage, 120 },
            { C.DK.timeCallDown, 8f },
            { C.DK.amountUpCombo, 10 },
            { C.DK.shouldBeCastedAtStartUnitAnimation, true },
        } },
        { C.DK.Berserker, new Dictionary<string, object>() { 
            { C.DK.equipmentName, C.DK.Berserker },
            { C.DK.cost, 70 },
            { C.DK.timeCallDown, 20f },
            { C.DK.amountUpCombo, 10 },
            { C.DK.shouldBeCastedAtStartUnitAnimation, false },
            { C.DK.durationActiveState, 7f },
            { C.DK.increasingUnitParametersByAmmunitionPercentageByCast, new Dictionary<string, float>() { { C.DK.damage, 50f } } },
        } },
        { "SomeSpell2", new Dictionary<string, object>() {
            { C.DK.equipmentName, "SomeSpell2" },
            { C.DK.cost, 0 },
            { C.DK.timeCallDown, 5 },
            { C.DK.shouldBeCastedAtStartUnitAnimation, true },
            { C.DK.amountUpCombo, 10 },
        } },
        { C.DK.Healing, new Dictionary<string, object>() {
            { C.DK.equipmentName, C.DK.Healing },
            { C.DK.cost, 50 },
            { C.DK.timeCallDown, 20 },
            { C.DK.healthHealAmount, 20f },
            { C.DK.shouldBeCastedAtStartUnitAnimation, true },
            { C.DK.amountUpCombo, 10 },
        } }
    };

    /*public static readonly Dictionary<string, Dictionary<string, object>> ammunitionParameters = new Dictionary<string, Dictionary<string, object>>()
    {
        // Инициализируем словарь при объявлении
        { "Ammunition1", new Dictionary<string, object>() {
            { C.DK.equipmentName, "Ammunition1" },
            { C.DK.cost, 15 },
        } },
        { "Ammunition2", new Dictionary<string, object>() {
            { C.DK.equipmentName, "Ammunition2" },
            { C.DK.cost, 45 },
        } },
        { "Ammunition3", new Dictionary<string, object>() {
            { C.DK.equipmentName, "Ammunition3" },
            { C.DK.cost, 70 },
        } }
    };*/



    public struct EquipmentChance // структура, описывающая какие предметы в каких разрезах выпадают с какой вероятностью
    {
        public string equipmentCategory; // например Weapon, Armor и т.п
        public string equipmentRarityType; // например Standart, Rare и т.п
        public float chance;    // Вероятность выпадения (в процентах).
    }

    public static List<EquipmentChance> allEquipmentTypesAndCategoriesChance = new List<EquipmentChance>() { 
        new() { equipmentCategory = C.DK.Weapon, equipmentRarityType = C.DK.Standart, chance = 25f } ,
        new() { equipmentCategory = C.DK.Weapon, equipmentRarityType = C.DK.Rare, chance = 8.93f } ,
        new() { equipmentCategory = C.DK.Weapon, equipmentRarityType = C.DK.Legendary, chance = 1.79f } ,
        new() { equipmentCategory = C.DK.Armor, equipmentRarityType = C.DK.Standart, chance = 25f } ,
        new() { equipmentCategory = C.DK.Armor, equipmentRarityType = C.DK.Rare, chance = 8.93f } ,
        new() { equipmentCategory = C.DK.Armor, equipmentRarityType = C.DK.Legendary, chance = 1.79f } ,
        new() { equipmentCategory = C.DK.Accessories, equipmentRarityType = C.DK.Standart, chance = 20f } ,
        new() { equipmentCategory = C.DK.Accessories, equipmentRarityType = C.DK.Rare, chance = 7.14f } ,
        new() { equipmentCategory = C.DK.Accessories, equipmentRarityType = C.DK.Legendary, chance = 1.43f } ,   
    };

    // increasingUnitParametersByAmmunitionAbsolute - увеличиваем на абсолютное значение параметры в словаре 
    // increasingUnitParametersByAmmunitionPercentage - увеличиваем на процент значение параметров в словаре
    // Все процентные параметры юнитов увеличиваются на абсолютное значение процента, а не долю от базового. Увеличение уклонения/шанса стана на 5% добавит просто 5% к текущему показателю

    public static readonly Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, object>>>> ammunitionParameters =
        new Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, object>>>>()
        {
            {
                C.DK.Weapon, new Dictionary<string, Dictionary<string, Dictionary<string, object>>>()
                {
                    {
                        C.DK.Standart, new Dictionary<string, Dictionary<string, object>>()
                        {
                            {
                                C.DK.Spear, new Dictionary<string, object>()
                                {
                                    { C.DK.increasingUnitParametersByAmmunitionPercentage, new Dictionary<string, float>() { { C.DK.damage, 20f } } },
                                    { C.DK.increasingUnitParametersByAmmunitionAbsolute, new Dictionary<string, float>() { { C.DK.CurrentIncreasingStamina, -15f }, { C.DK.jumpForce, 5 } } }, // CurrentIncreasingStamina - уменьшаем/увеличиваем макс. стамину на вот этот процент!!!!
                                    { C.DK.cost, 80 } } },
                            {
                                C.DK.Knife, new Dictionary<string, object>()
                                {
                                    { C.DK.increasingUnitParametersByAmmunitionPercentage, new Dictionary<string, float>() { { C.DK.damage, 10f } } },
                                    { C.DK.increasingUnitParametersByAmmunitionAbsolute, new Dictionary<string, float>() { { C.DK.CurrentIncreasingStamina, 30f } } }, // CurrentIncreasingStamina - уменьшаем/увеличиваем макс. стамину на вот этот процент!!!!
                                    { C.DK.cost, 80 } } },
                            {
                                C.DK.Axe, new Dictionary<string, object>()
                                {
                                    { C.DK.increasingUnitParametersByAmmunitionPercentage, new Dictionary<string, float>() { { C.DK.damage, 25f } } },
                                    { C.DK.increasingUnitParametersByAmmunitionAbsolute, new Dictionary<string, float>() { { C.DK.stuneChanceByStandardAttackPercentage, 5f }, { C.DK.CurrentIncreasingStamina, -20f } } }, // CurrentIncreasingStamina - уменьшаем/увеличиваем макс. стамину на вот этот процент!!!!
                                    { C.DK.cost, 80 } } },
                            {
                                C.DK.Sword, new Dictionary<string, object>()
                                {
                                    { C.DK.increasingUnitParametersByAmmunitionPercentage, new Dictionary<string, float>() { { C.DK.damage, 15f } } },
                                    { C.DK.cost, 80 } } } } },
                    {
                        C.DK.Rare, new Dictionary<string, Dictionary<string, object>>()
                        {
                            {
                                C.DK.ThunderAxe, new Dictionary<string, object>()
                                {
                                    { C.DK.increasingUnitParametersByAmmunitionPercentage, new Dictionary<string, float>() { { C.DK.damage, 28f } } },
                                    { C.DK.increasingUnitParametersByAmmunitionAbsolute, new Dictionary<string, float>() { { C.DK.stuneChanceByStandardAttackPercentage, 17f }, { C.DK.CurrentIncreasingStamina, -20f } } },
                                    { C.DK.cost, 180 } } },
                            {
                                C.DK.FireSword, new Dictionary<string, object>()
                                {
                                    { C.DK.increasingUnitParametersByAmmunitionPercentage, new Dictionary<string, float>() { { C.DK.damage, 35f } } },
                                    { C.DK.cost, 180 } } } } },
                    {
                        C.DK.Legendary, new Dictionary<string, Dictionary<string, object>>()
                        {
                            {
                                C.DK.ThirstySakura, new Dictionary<string, object>()
                                {
                                    { C.DK.increasingUnitParametersByAmmunitionPercentage, new Dictionary<string, float>() {   } },
                                    { C.DK.cost, 340 } } } } }, } },
            {
                C.DK.Armor, new Dictionary<string, Dictionary<string, Dictionary<string, object>>>()
                {
                    {
                        C.DK.Standart, new Dictionary<string, Dictionary<string, object>>()
                        {
                            {
                                C.DK.LeatherArmor, new Dictionary<string, object>()
                                {
                                    { C.DK.increasingUnitParametersByAmmunitionPercentage, new Dictionary<string, float>() {  } },
                                    { C.DK.increasingUnitParametersByAmmunitionAbsolute, new Dictionary<string, float>() { { C.DK.DamageReductionPercentage, 10f }, { C.DK.CurrentIncreasingStamina, 10f } } },
                                    { C.DK.cost, 80 } } },
                            {
                                C.DK.PlateArmor, new Dictionary<string, object>()
                                {
                                    { C.DK.increasingUnitParametersByAmmunitionPercentage, new Dictionary<string, float>() {  } },
                                    { C.DK.increasingUnitParametersByAmmunitionAbsolute, new Dictionary<string, float>() { { C.DK.DamageReductionPercentage, 30f }, { C.DK.CurrentIncreasingStamina, -20f } } },
                                    { C.DK.cost, 120 } } },
                            {
                                C.DK.ChainMail, new Dictionary<string, object>()
                                {
                                    { C.DK.increasingUnitParametersByAmmunitionAbsolute, new Dictionary<string, float>() { { C.DK.DamageReductionPercentage, 10f } } },
                                    { C.DK.cost, 160 } } } } },
                    {
                        C.DK.Rare, new Dictionary<string, Dictionary<string, object>>()
                        {
                            {
                                C.DK.ThunderArmor, new Dictionary<string, object>()
                                {
                                    { C.DK.increasingUnitParametersByAmmunitionPercentage, new Dictionary<string, float>() {  } },
                                    { C.DK.increasingUnitParametersByAmmunitionAbsolute, new Dictionary<string, float>() { { C.DK.stuneChanceByStandardAttackPercentage, 5f }, { C.DK.DamageReductionPercentage, 35f }, { C.DK.CurrentIncreasingStamina, -15f } } },
                                    { C.DK.cost, 200 } } },
                            {
                                C.DK.DragonArmor, new Dictionary<string, object>()
                                {
                                    { C.DK.increasingUnitParametersByAmmunitionPercentage, new Dictionary<string, float>() {  } },
                                    { C.DK.increasingUnitParametersByAmmunitionAbsolute, new Dictionary<string, float>() { { C.DK.DamageReductionPercentage, 45f }, { C.DK.CurrentIncreasingStamina, -20f } } },
                                    { C.DK.cost, 200 } } } } },
                    {
                        C.DK.Legendary, new Dictionary<string, Dictionary<string, object>>()
                        { } 
                    
                    } } },
            {
                C.DK.Accessories, new Dictionary<string, Dictionary<string, Dictionary<string, object>>>()
                {
                    {
                        C.DK.Standart, new Dictionary<string, Dictionary<string, object>>()
                        {
                            {
                                C.DK.DexterityBracelet, new Dictionary<string, object>()
                                {
                                    { C.DK.increasingUnitParametersByAmmunitionAbsolute, new Dictionary<string, float>() { { C.DK.CurrentIncreasingStamina, 25f } } },
                                    { C.DK.cost, 150 } } },
                            {
                                C.DK.RingWarrior, new Dictionary<string, object>()
                                {
                                    { C.DK.increasingUnitParametersByAmmunitionPercentage, new Dictionary<string, float>() { { C.DK.damage, 7f } } },
                                    { C.DK.increasingUnitParametersByAmmunitionAbsolute, new Dictionary<string, float>() { { C.DK.DamageReductionPercentage, 5f } } },
                                    { C.DK.cost, 150 } } },
                            {
                                C.DK.MedallionOfLife, new Dictionary<string, object>()
                                {
                                    { C.DK.increasingUnitParametersByAmmunitionPercentage, new Dictionary<string, float>() { { C.DK.healthMax, 20f } } },
                                    { C.DK.cost, 150 } } } } },
                    {
                        C.DK.Rare, new Dictionary<string, Dictionary<string, object>>()
                        {
                            {
                                C.DK.RingBerserker, new Dictionary<string, object>()
                                {
                                    { C.DK.increasingUnitParametersByAmmunitionPercentage, new Dictionary<string, float>() { { C.DK.damage, 30f } } },
                                    { C.DK.increasingUnitParametersByAmmunitionAbsolute, new Dictionary<string, float>() { { C.DK.DamageReductionPercentage, -20f } } },
                                    { C.DK.cost, 240 } } },
                            {
                                C.DK.RingForesight, new Dictionary<string, object>()
                                {
                                    { C.DK.increasingUnitParametersByAmmunitionAbsolute, new Dictionary<string, float>() { { C.DK.evasionPercentage, 10f } } },
                                    { C.DK.cost, 260 } } } } },
                    {
                        C.DK.Legendary, new Dictionary<string, Dictionary<string, object>>()
                        {
                            {
                                C.DK.Tragicomedy, new Dictionary<string, object>()
                                {
                                    { C.DK.increasingUnitParametersByAmmunitionPercentageByCast, new Dictionary<string, float>() { { C.DK.damage, 200f }, { C.DK.DamageReductionPercentage, -100f } } },
                                    { C.DK.timeCallDown, 5 },
                                    { C.DK.durationActiveState, 5 }, // можно настраивать длительность эффекта. Влияет на то, когда активный эффект сбросится и активка снаряжения уйдёт на КД
                                    { C.DK.shouldBeCastedAtStartUnitAnimation, true },
                                    { C.DK.cost, 320 } } }
                        } 
                    
                    } } }


        };

    // получаем параметр из словаря по названию юнита и параметра
    public static object GetParameter(string spellName, string parameterName)
    {
        if (spellParameters.ContainsKey(spellName) && spellParameters[spellName].ContainsKey(parameterName))
        {
            return spellParameters[spellName][parameterName];
        }
        return null;
    }


    // Метод для получения случайного ключа из словаря spellParameters
    private static List<string> spellsForTest = new() { "SomeSpell2" };
    public static string GetRandomSpellName()
    {
        List<string> spellNames = new List<string>(spellParameters.Keys);
        if (spellNames.Count == 0)
        {
            //Debug.LogError("No spell names available in unitParameters!");
            return null; // Или какое-то значение по умолчанию
        }
        foreach (string spellForTestName in spellsForTest)
        {
            if (spellNames.Contains(spellForTestName))
            {
                spellNames.Remove(spellForTestName);
            }
        }
        int randomIndex = UnityEngine.Random.Range(0, spellNames.Count);
        return spellNames[randomIndex];
    }

    public static string GetRandomAmmunitionName(EquipmentChance randomCategoryAndRarityTypesOfEquipment = default)
    {
        List<string> ammunitionNames = new List<string>();
        if (randomCategoryAndRarityTypesOfEquipment.equipmentCategory != null) // Проверяем, что предмет был выбран (randomCategoryAndRarityTypesOfEquipment не остался default)
        {
            foreach (var equipment in ammunitionParameters[randomCategoryAndRarityTypesOfEquipment.equipmentCategory][randomCategoryAndRarityTypesOfEquipment.equipmentRarityType])
            {
                ammunitionNames.Add(equipment.Key);
            }
        }
        else
        {
            foreach (var equipmentCategory in ammunitionParameters)
            {
                foreach (var equipmentRarityType in equipmentCategory.Value)
                {
                    foreach (var equipment in equipmentRarityType.Value)
                    {
                        ammunitionNames.Add(equipment.Key);
                    }
                }
            }
        }
        
        if (ammunitionNames.Count == 0)
        {
            ////Debug.LogError("No ammunition names available in equipmentParameters!");
            ////Debug.Log(randomCategoryAndRarityTypesOfEquipment.equipmentCategory);
            ////Debug.Log(randomCategoryAndRarityTypesOfEquipment.chance);
            ////Debug.Log(randomCategoryAndRarityTypesOfEquipment.equipmentRarityType);
            ////Debug.Log("No ammunition names available in equipmentParameters!");

            //EquipmentChance someChance = GenerateItems(1)[0];
            //string nameItem = GetRandomAmmunitionName(someChance);
            //if (nameItem != null)
            //{
            //    return nameItem;
            //}
            //else
            //{
            //    return GetRandomAmmunitionName(someChance);
            //}

            return null;
        }
        
        int randomIndex = UnityEngine.Random.Range(0, ammunitionNames.Count);
        return ammunitionNames[randomIndex];                
    }


    public static List<EquipmentChance> GenerateItems(int numberOfItems)
    {
        List<EquipmentChance> possibleItems = new List<EquipmentChance>(allEquipmentTypesAndCategoriesChance);

        List<EquipmentChance> generatedItems = new List<EquipmentChance>();

        // 1. Нормализуем вероятности (если они не нормализованы)
        float totalChance = 0;
        foreach (var item in possibleItems)
        {
            totalChance += item.chance;
        }

        if (Math.Abs(totalChance - 100) > 0.01f) // Проверяем, что сумма близка к 100
        {
            //Debug.LogWarning("Сумма вероятностей не равна 100%. Нормализуем...");
            // Нормализуем
            float normalizationFactor = 100f / totalChance;
            for (int i = 0; i < possibleItems.Count; i++)
            {
                EquipmentChance item = possibleItems[i];
                item.chance *= normalizationFactor;
                possibleItems[i] = item;
            }
        }

        // 2. Генерируем предметы
        for (int i = 0; i < numberOfItems; i++)
        {
            float randomValue = UnityEngine.Random.Range(0f, 100f);
            float cumulativeChance = 0f;
            EquipmentChance selectedItem = default;

            foreach (var item in possibleItems)
            {
                ////Debug.Log(item.chance);
                cumulativeChance += item.chance;
                if (randomValue <= cumulativeChance)
                {
                    selectedItem = item;
                    break; // Выбираем первый подходящий предмет
                }
            }

            if (selectedItem.equipmentCategory != null) // Проверяем, что предмет был выбран (selectedItem не остался default)
            {
                generatedItems.Add(selectedItem);
            }
            else
            {
                //Debug.LogError("Не удалось выбрать предмет! Проверьте вероятности.");
            }
        }

        return generatedItems;
    }





    // вызывается для установки пассивных бонусов при попадании снаряжения в инвентарь. Требуется явно передать имя вызываемой функции (на более высоком уровне управления выбираем 
    // префикс Activate/Deactivate. Переделали, теперь вызывается при любом взаимодействии спела с миром, хоть при нажатии, хоть при пассивных бонусах. Имя функции активации передаётся
    // через параметр nameActivationFunction. Если ничего не было передано (null), значит, по умолчанию имя функции активации равно имени снаряжения (scriptEquipment.equipmentName)
    public static void CallActionFunctionByName(Equipment scriptEquipment, int amountUpCombo, Unit whoCallAction, string nameActivationFunction = null)
    {
        // Получаем тип текущего объекта (MyClass)
        Type type = scriptEquipment.GetType();

        string nameOfSpell = nameActivationFunction;
        if (nameOfSpell == null)
        {
            nameOfSpell = scriptEquipment.equipmentName;
        }
        //Debug.Log("чё за параща " + type);
        // Получаем информацию о методе с указанным именем
        MethodInfo methodInfo = type.GetMethod(nameOfSpell, BindingFlags.Public | BindingFlags.Instance);

        // Проверяем, что метод существует
        if (methodInfo != null)
        {
            // Вызываем метод
            ScoreManager.Instance.UpCombo((int)(amountUpCombo * scriptEquipment.multiplierFreshness));
            ScoreManager.Instance.UpActionCombo(1, nameOfSpell);
            //ScoreManager.Instance.InvokeAppearingSprite(ScoreManager.TYPE_APPEARING_MESSAGE.SkillUsed); // вызовется как при прожатии скила, так и при прожатии активки аммуниции
            ScoreManager.Instance.InvokeAppearingText(ScoreManager.TYPE_APPEARING_MESSAGE.SkillUsed); // вызовется как при прожатии скила, так и при прожатии активки аммуниции

            if (scriptEquipment.isEquipmentASpell) // только для спелов мы ищем комбо Master Of Skills
            {
                ScoreManager.Instance.AchivementMasterOfSkills();
            }
            
            scriptEquipment.CurrentFreshnessCount++;

            object[] parameters =  new object[] { whoCallAction }; // по идее можно было бы в самом скрипте спела в функции спела ссылаться на игрока, но предполагаем, что в будущем (???)
                                                                    // кастовать спелы сможет не только игрок                                                                        

            methodInfo.Invoke(scriptEquipment, parameters); // this - экземпляр объекта, null - аргументы (если есть)
        }
        else
        {
            //Debug.Log($"Функция с именем '{nameOfSpell}' не найдена.");
            return;
        }
    }
    public static void CallActionFunctionByLink(Equipment scriptEquipment, int amountUpCombo, Unit whoCallAction, Action<Unit> linkActionFunction)
    {

        if (!scriptEquipment.isActivated) // чтоб, если снаряжение уже активное (то есть было прожато в недавнем будущем), второй раз комбо не прибавлять
                                          // (второй раз эффект активации всё равно не применить). Логика данного контроля также реализована в FsmStateEquipmentAtUnit
        {
            ScoreManager.Instance.UpActionCombo(1, scriptEquipment.equipmentName);
            //ScoreManager.Instance.InvokeAppearingSprite(ScoreManager.TYPE_APPEARING_MESSAGE.SkillUsed); // вызовется как при прожатии скила, так и при прожатии активки аммуниции
            ScoreManager.Instance.InvokeAppearingText(ScoreManager.TYPE_APPEARING_MESSAGE.SkillUsed); // вызовется как при прожатии скила, так и при прожатии активки аммуниции

            if (scriptEquipment.isEquipmentASpell) // только для спелов мы ищем комбо Master Of Skills
            {
                ScoreManager.Instance.AchivementMasterOfSkills();
            }

            if (amountUpCombo > 0) // иначе комбо увеличится на ноль, но соответствующая логика всё равно применится (сигнализирующий спрайт появится, КД на обнуление сбросится и т.п)
            {
                ScoreManager.Instance.UpCombo((int)(amountUpCombo * scriptEquipment.multiplierFreshness));
            }
        }
            
        scriptEquipment.CurrentFreshnessCount++;

        linkActionFunction.Invoke(whoCallAction);

    }

}