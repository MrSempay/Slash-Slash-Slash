using UnityEngine;
using static FsmStatePlayer;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class FsmStateEquipmentSelected : FsmStateEquipment
{
    public FsmStateEquipmentSelected(Fsm fsm, GameObject gameObject) : base(fsm, gameObject)
    {

    }


    public override void Enter()
    {
        Debug.Log("Equipment selected state [ENTER]");
        equipment.player.IsTranslatingEquipment = true;
        equipment.selfSprite.sortingOrder = 25; // чтоб поверх вообще всех UI было. Кроме, возможно, диалогового окна (там 25 тоже)
    }

    public override void Exit()
    {
        Debug.Log("Equipment selected state [EXIT]");
        equipment.player.IsTranslatingEquipment = false;
        equipment.selfCollider.enabled = true;
        equipment.selfSprite.sortingOrder = 11; // это значение, скорее всего, будет изменять в методе Enter других состояний
    }

    public override void Update()
    {
        base.Update();
        //Получаем текущую позицию курсора в мировых координатах
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //Задаем новую позицию объекта
        mousePosition = new Vector3(mousePosition.x, mousePosition.y, 0) - new Vector3(0, 0, 3) - new Vector3(0, 0, 0);
        equipment.transform.position = mousePosition;

        if (Input.GetMouseButtonUp(0)) // Когда отпущена левая кнопка мыши
        {
            // получаем ссылку на компонент RectTransform места для снаряжения, либо null, если такового места не нашлось
            RectTransform rectTransformPlace = IsEquipmentPlaceThere(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
            if (rectTransformPlace) // если место для снаряжения найдено
            {
                if (rectTransformPlace.parent == equipment.rectTransformTargetEquipmentPanelPlayer)
                { // проверяем, что это мы пытаемся на место снаряжения в панели игрока установить наше снаряжение, а не, условно, магазинное. Перемещать можно только в рамках или на целевую панель

                    // НУЖНО ДЛЯ ДЕТЕКЦИИ, НА ПРОДАЖУ ЛИ ДАННЫЙ ЭКЗЕМПЛЯР СНАРЯЖЕНИЯ. Если нет (уже было продано), то ничего не делаем, если да, то проверяем, хватает ли денег, если нет, возвращае
                    // в состояние FsmStateEquipmentInsideShop
                    if (equipment.WasSold == false)
                    {
                        if (equipment.BuildingWhereEquipmentIs.HasTargetEnoughMoneyForBuy(equipment.player, equipment)) // спелы стоят 0 злата, так что по идее на них всегда будет хватать
                        {
                            if (equipment.isEquipmentASpell) // если то, что мы продаём - спел
                            {
                                if (equipment.BuildingWhereEquipmentIs.HasAccessToUpLevelInSchool(equipment.player))
                                {
                                    equipment.BuildingWhereEquipmentIs.Sell(equipment.player, equipment);
                                    equipment.BuildingWhereEquipmentIs.TeachByUpLevel(equipment.player, equipment);
                                }
                                else { fsm.SetState<FsmStateEquipmentInsideShop>(); return; }
                            }
                            else equipment.BuildingWhereEquipmentIs.Sell(equipment.player, equipment); // если то, что мы продаём - не спел (аммуниция)
                        }
                        else { fsm.SetState<FsmStateEquipmentInsideShop>(); return; }
                    }
                    equipment.SetEquipmentToPlaceIfNotNull(rectTransformPlace); // устанавливаем снаряжение на это место либо вернёт false если null и ничего не сделает

                    // НУЖНО ДЛЯ ОБМЕНА МЕСТАМИ СНАРЯЖЕНИЯ В ЗДАНИИ И У ИГРОКА. Получаем ссылку на снаряжение, которое находится в интересующем нас месте у игрока либо null
                    equipment.selfCollider.enabled = false;
                    Equipment isAtPlaceEquipment = IsEquipmentPlaceOccupied(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
                    if (isAtPlaceEquipment) // если таковое снаряжение на заданном месте было найдено
                    {
                        isAtPlaceEquipment.transform.SetParent(equipment.transformCurrentEquipmentPlace, false);
                        isAtPlaceEquipment.transformCurrentEquipmentPlace = equipment.transformCurrentEquipmentPlace;
                        isAtPlaceEquipment.transformCurrentEquipmentPlace.gameObject.GetComponent<PlaceForEquipment>().Equipment = isAtPlaceEquipment; // устанавливаем для места снаряжения
                        isAtPlaceEquipment.startLocalPosition = equipment.startLocalPosition;
                        isAtPlaceEquipment.transform.localPosition = equipment.startLocalPosition;
                        isAtPlaceEquipment.BuildingWhereEquipmentIs = equipment.BuildingWhereEquipmentIs;
                                                                                                                                                       // другое снаряжения в его скрипте

                    }
                    equipment.transformCurrentEquipmentPlace = rectTransformPlace; // устанавливаем текущую позицию для снаряжения в виде целевой, на которую снаряжение только что переместили
                    fsm.SetState<FsmStateEquipmentAtPlayer>();
                    return;
                 }
            }
            // короче, проверяем тут, равен ли родитель текущего места для снаряжения целевой панели снаряжения для данного экземпляра снаряжения. Если да, то возвращаем это снаряжение
            // в состояние FsmStateEquipmentAtPlayer, если нет, то подразумевается, что оно было в магазине и мы возвращаем его туда FsmStateEquipmentInsideShop
            if (equipment.transformCurrentEquipmentPlace.parent == equipment.rectTransformTargetEquipmentPanelPlayer) fsm.SetState<FsmStateEquipmentAtPlayer>();
            else fsm.SetState<FsmStateEquipmentInsideShop>();
        }

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Ended)
            {
                // получаем ссылку на компонент RectTransform места для снаряжения, либо null, если такового места не нашлось
                RectTransform rectTransformPlace = IsEquipmentPlaceThere(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
                if (rectTransformPlace) // если место для снаряжения найдено
                {
                    if (rectTransformPlace.parent == equipment.rectTransformTargetEquipmentPanelPlayer)
                    { // проверяем, что это мы пытаемся на место снаряжения в панели игрока установить наше снаряжение, а не, условно, магазинное. Перемещать можно только в рамках или на целевую панель

                        // НУЖНО ДЛЯ ДЕТЕКЦИИ, НА ПРОДАЖУ ЛИ ДАННЫЙ ЭКЗЕМПЛЯР СНАРЯЖЕНИЯ. Если нет (уже было продано), то ничего не делаем, если да, то проверяем, хватает ли денег, если нет, возвращае
                        // в состояние FsmStateEquipmentInsideShop
                        if (equipment.WasSold == false)
                        {
                            if (equipment.BuildingWhereEquipmentIs.HasTargetEnoughMoneyForBuy(equipment.player, equipment)) // спелы стоят 0 злата, так что по идее на них всегда будет хватать
                            {
                                if (equipment.isEquipmentASpell) // если то, что мы продаём - спел
                                {
                                    if (equipment.BuildingWhereEquipmentIs.HasAccessToUpLevelInSchool(equipment.player))
                                    {
                                        equipment.BuildingWhereEquipmentIs.Sell(equipment.player, equipment);
                                        equipment.BuildingWhereEquipmentIs.TeachByUpLevel(equipment.player, equipment);
                                    }
                                    else { fsm.SetState<FsmStateEquipmentInsideShop>(); return; }
                                }
                                else equipment.BuildingWhereEquipmentIs.Sell(equipment.player, equipment); // если то, что мы продаём - не спел (аммуниция)
                            }
                            else { fsm.SetState<FsmStateEquipmentInsideShop>(); return; }
                        }
                        equipment.SetEquipmentToPlaceIfNotNull(rectTransformPlace); // устанавливаем снаряжение на это место либо вернёт false если null и ничего не сделает

                        // НУЖНО ДЛЯ ОБМЕНА МЕСТАМИ СНАРЯЖЕНИЯ В ЗДАНИИ И У ИГРОКА. Получаем ссылку на снаряжение, которое находится в интересующем нас месте у игрока либо null
                        equipment.selfCollider.enabled = false;
                        Equipment isAtPlaceEquipment = IsEquipmentPlaceOccupied(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
                        if (isAtPlaceEquipment) // если таковое снаряжение на заданном месте было найдено
                        {
                            isAtPlaceEquipment.transform.SetParent(equipment.transformCurrentEquipmentPlace, false);
                            isAtPlaceEquipment.transformCurrentEquipmentPlace = equipment.transformCurrentEquipmentPlace;
                            isAtPlaceEquipment.transformCurrentEquipmentPlace.gameObject.GetComponent<PlaceForEquipment>().Equipment = isAtPlaceEquipment; // устанавливаем для места снаряжения
                            isAtPlaceEquipment.startLocalPosition = equipment.startLocalPosition;
                            isAtPlaceEquipment.transform.localPosition = equipment.startLocalPosition;
                            isAtPlaceEquipment.BuildingWhereEquipmentIs = equipment.BuildingWhereEquipmentIs;
                            // другое снаряжения в его скрипте

                        }
                        equipment.transformCurrentEquipmentPlace = rectTransformPlace; // устанавливаем текущую позицию для снаряжения в виде целевой, на которую снаряжение только что переместили
                        fsm.SetState<FsmStateEquipmentAtPlayer>();
                        return;
                    }
                }
                // короче, проверяем тут, равен ли родитель текущего места для снаряжения целевой панели снаряжения для данного экземпляра снаряжения. Если да, то возвращаем это снаряжение
                // в состояние FsmStateEquipmentAtPlayer, если нет, то подразумевается, что оно было в магазине и мы возвращаем его туда FsmStateEquipmentInsideShop
                if (equipment.transformCurrentEquipmentPlace.parent == equipment.rectTransformTargetEquipmentPanelPlayer) fsm.SetState<FsmStateEquipmentAtPlayer>();
                else fsm.SetState<FsmStateEquipmentInsideShop>();
            }

        }



    }

    // подразумевается, что эта функция вызывается только из состояния FsmStateEquipmentSelected... ну и бред
    private RectTransform IsEquipmentPlaceThere(float x, float y)
    {
        Vector3 mousePosition = new Vector3(x, y, 0);

        // Получаем массив всех коллайдеров, попавших в круг
        Collider2D[] hits = Physics2D.OverlapCircleAll(mousePosition - new Vector3(0, 0, 1), 0.05f);

        //Визуализация круга обнаружения (отображается только в редакторе)
        DebugExtension.DebugCircle(mousePosition - new Vector3(0, 0, 1), Vector3.forward, Color.red, 0.5f, false, 0.5f);

        // Перебираем все найденные коллайдеры
        foreach (Collider2D hit in hits)
        {
            //Debug.Log("Обнаружен коллайдер: " + hit.gameObject.name);

            GameObject placeEquipment = hit.gameObject; // Получаем GameObject
            if (placeEquipment.CompareTag("PlaceForEquipment"))
            {
                //Debug.Log("Обнаружена ячейка для оборудования: " + placeEquipment.name);
                return placeEquipment.GetComponent<RectTransform>(); // Возвращаем RectTransform, если нашли
            }
        }

        // Если ничего не нашли, возвращаем null
        return null;
    }

}
