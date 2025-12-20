using System;
using UnityEngine;
using static C;

public class ArcLightning : Spell
{

    private readonly Vector3 _biasPositionLightningArea = new Vector3(2.9f, 0f, 0f);
    private readonly Vector3 _sizeArcLightningArea = new Vector2(5f, 3.5f);
    private GameObject _arcLightningArea;
    private int _directionArcLightningArea;
    private AttackArea _attackAreaScript;

    [NonSerialized] public int damage;

    public override void Awake()
    {
        base.Awake();
    }

    public override void Start()
    {
        base.Start();
    }

    public override void EnteredIntoUnitInventory(Unit ownerInventory) // хотя это ближе не к ownerInventory, а просто к inventory
    {
        base.EnteredIntoUnitInventory(ownerInventory);

        ownerUnit.OnDirectionViewWasChanged += ChangePositionArcLightningArea; 
    }
    public override void ExitedFromUnitInventory(Unit ownerInventory)
    {
        base.ExitedFromUnitInventory(ownerInventory); // тут вызывается Deactivate

        ownerUnit.OnDirectionViewWasChanged -= ChangePositionArcLightningArea; // думал перенсти это в Deactivate, но не при всяком Deactivate нам надо отписываться от прослушивания
                                                                               // события поворота юнита. Но при всяком покидании инвенторя юнита отписываться надо 
    }

    public override void Cast(Unit whoCastedSpell)
    {
        Activate(whoCastedSpell);
    }

    public override void Activate(Unit whoCastedSpell)
    {
        base.Activate(whoCastedSpell); // хотя я вот думаю, что логику базового метода можно было бы вывести просто в отдельную функцию и вызывать её при надобности 
                                       // в целом базовый Activate нужен только для подключения сигналов о конце анимации спела и достижении анимацией пикового её состояния

        if (!isActivated)
        {
            isActivated = true;
        }
    }
    public override void Deactivate(Unit whoCastedSpell)
    {
        base.Deactivate(whoCastedSpell); // хотя я вот думаю, что логику базового метода можно было бы вывести просто в отдельную функцию и вызывать её при надобности

        if (isActivated)
        {
            StartCallDown();

            isActivated = false;
            if (_attackAreaScript) // проблема тут может быть только в случае если нас застанят во время каста и при этом мы ещё не успеем заспавнить зону для цепной молнии, поэтому проверяем.
            {
                if (ownerUnit.CompareTag("Player") || ownerUnit.CompareTag("Allies"))
                {
                    _attackAreaScript.isEnemyInAttackArea -= MakeDamage;
                }
                if (ownerUnit.CompareTag("Enemy"))
                {
                    _attackAreaScript.isPlayerOrAlliesInAttackArea -= MakeDamage;
                }
            }
            Destroy(_arcLightningArea);
        }
    }

    public override void UnitCastAnimationFinishedForThisEquipment()
    {
        Deactivate(ownerUnit);
    }

    public override void UnitCastAnimationPeackedForThisEquipment()
    {
        ////Debug.Log("MMM");
        _arcLightningArea = new GameObject("LightningArea");
        _arcLightningArea.transform.SetParent(ownerUnit.transform, false);
        ////Debug.Log("Mda" + _directionArcLightningArea);
        _arcLightningArea.transform.localPosition = new Vector3(_directionArcLightningArea * _biasPositionLightningArea.x, _biasPositionLightningArea.y, _biasPositionLightningArea.z);
        //_arcLightningArea.transform.localPosition = Vector3.Scale(_biasPositionLightningArea, new Vector3(_directionArcLightningArea, 1, 1));
        _attackAreaScript = _arcLightningArea.AddComponent<AttackArea>();

        _arcLightningArea.layer = ownerUnit.gameObject.layer;

        if (ownerUnit.CompareTag("Player") || ownerUnit.CompareTag("Allies")) 
        {
            _attackAreaScript.isEnemyInAttackArea += MakeDamage;
        }
        if (ownerUnit.CompareTag("Enemy"))
        {
            _attackAreaScript.isPlayerOrAlliesInAttackArea += MakeDamage;
        }

        // Добавляем Collider2D (BoxCollider2D для прямоугольника)
        BoxCollider2D collider = _arcLightningArea.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;

        // Устанавливаем размер коллайдера (по умолчанию 1x1)
        collider.size = _sizeArcLightningArea; // Ширина 2, высота 1

        base.UnitCastAnimationPeackedForThisEquipment();
    }

    private void MakeDamage(bool isUnitInArea, Unit unit)
    {
        // так как урон можем наносить только во время свайпа, а иметь мгновенную скорость по оси Х также только во время свайпа, проверяем в условии скорость на неравенство нулю.
        if (isUnitInArea) { unit.GetDamage(damage, ownerUnit, false); }
    }

    private void ChangePositionArcLightningArea(bool lookingRight)
    {
        _directionArcLightningArea = lookingRight ? 1 : -1;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        if (ownerUnit)
        {
            if (_attackAreaScript)
            {
                if (ownerUnit.CompareTag("Player") || ownerUnit.CompareTag("Allies"))
                {
                    _attackAreaScript.isEnemyInAttackArea -= MakeDamage;
                }
                if (ownerUnit.CompareTag("Enemy"))
                {
                    _attackAreaScript.isPlayerOrAlliesInAttackArea -= MakeDamage;
                }
            }
            ownerUnit.OnDirectionViewWasChanged -= ChangePositionArcLightningArea;
        }
    }

}
