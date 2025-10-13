using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class FsmStatePlayer : FsmStateUnit
{

    public delegate void SwipeEnded();
    public event SwipeEnded OnSwipeEnded;
    public delegate void SwipeStarted();
    public event SwipeStarted OnSwipeStarted;
        
    protected Fsm fsmPlayer;
    protected Player player;


    private const int MOUSE_FAKE_FINGER_ID = -999;

    // --- Настраиваемые параметры ---
    private const float MIN_SWIPE_PIXELS = 10f;        // минимальный размер свайпа в пикселях
    private const float MIN_SWIPE_WORLD = 0.033f;        // минимальный размер свайпа в мировых координатах (страховка)
    private const float MOVE_VELOCITY_MULTIPLIER = 10f;// как у вас было speed * 10
    private const float STOP_EPSILON = 0.05f;          // допуск при остановке (в мировых координатах)
    Coroutine _coroutineRemoveTouchAfterFrame;


    public FsmStatePlayer(Fsm fsm, GameObject gameObject) : base(fsm, gameObject)
    {
        fsmPlayer = fsm;
        player = (Player)unit;
    }

    // ----------------- Unity-циклы -----------------
    public override void Update()
    {
        // Обработка тачей и мыши в Update (интерфейс/ввод — в Update)
        ProcessTouches();
        ProcessMouse();

        // Обновление вида (если скорость поменялась вне движения)
        if (Mathf.Abs(player.rb.linearVelocityX) > 0.01f)
        {
            // Меняем вид только если действительно движемся и направление отличается
            bool shouldLookRight = player.rb.linearVelocityX > 0f;
            if (player.lookingRight != shouldLookRight)
            {
                ChangeDirectionView(shouldLookRight);
            }
        }

    }

    public override void FixedUpdate()
    {
        MoveTarget();        
    }

    protected internal void SetStateIdleCallback() => fsmPlayer.SetState<FsmStateIdle>();

    // ----------------- Обработка ввода -----------------
    private void ProcessTouches()
    {
        if (Input.touchCount == 0) return;



        foreach (Touch touch in Input.touches)
        {
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    RegisterPointerBegan(touch.fingerId, touch.position);
                    break;

                case TouchPhase.Moved:
                    //if (player.isSwipingNow)
                    //{
                    //    RegisterPointerBegan(touch.fingerId, touch.position);
                    //}
                    //break;
                case TouchPhase.Stationary:
                    // Ничего не делаем здесь; удержание не важно для логики свайпа
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    RegisterPointerEnded(touch.fingerId, touch.position);
                    break;
            }
        }
    }

    private void ProcessMouse()
    {
        if (Application.isMobilePlatform) return;

        // Мышь имитируется как отдельный "палец"
        if (Input.GetMouseButtonDown(0))
        {
            RegisterPointerBegan(MOUSE_FAKE_FINGER_ID, Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            RegisterPointerEnded(MOUSE_FAKE_FINGER_ID, Input.mousePosition);
        }
    }

    private void RegisterPointerBegan(int fingerId, Vector2 screenPos)
    {
        bool overUi = IsPointerOverUIAtPosition(screenPos);

        var info = new Player.TouchInfo
        {
            startScreen = screenPos,
            startWorld = ScreenToWorldAtZ0(player.mainCamera, screenPos),
            startedOverUI = overUi
        };

        // Сохраняем, сколько мировых единиц = 1 пикселю на момент начала свайпа
        float camDepth = Mathf.Abs(player.mainCamera.transform.position.z); // расстояние до Z=0
        info.worldUnitsPerPixelAtStart = ComputeWorldUnitsPerPixel(player.mainCamera, camDepth);

        player.activeTouches[fingerId] = info;
        player.isSwipingNow = true;
    }

    private void RegisterPointerEnded(int fingerId, Vector2 endScreenPos)
    {
        if (player.processedTouches.Contains(fingerId))
            return;

        player.processedTouches.Add(fingerId);
        _coroutineRemoveTouchAfterFrame = CoroutineManager.Instance.StartManagedCoroutine(gameObject, RemoveTouchAfterFrame(fingerId));
        player.isSwipingNow = false;

        if (!player.activeTouches.ContainsKey(fingerId)) return;

        Player.TouchInfo info = player.activeTouches[fingerId];
        player.activeTouches.Remove(fingerId);

        if (info.startedOverUI) return;

        Vector2 deltaScreen = endScreenPos - info.startScreen;
        bool horizontalSwipe = Mathf.Abs(deltaScreen.x) >= Mathf.Abs(deltaScreen.y);

        if (deltaScreen.magnitude < MIN_SWIPE_PIXELS) return;
        if (player.CurrentStamina <= 0 && horizontalSwipe) return;


        // Конвертируем экранную дельту в мировую дельту, используя коэффициент, сохранённый в начале свайпа
        float dxWorld = deltaScreen.x * info.worldUnitsPerPixelAtStart;
        float dyWorld = deltaScreen.y * info.worldUnitsPerPixelAtStart;
        Vector3 swipeWorld = new Vector3(dxWorld, dyWorld, 0f);


        // Проверка "свайп в стену" — делаем на основе screen-space (надежнее при движ. камеры)
        if (player.isTouchingWall && horizontalSwipe)
        {
            // swipe left -> deltaScreen.x < 0
            if (player.wallOnLeft && deltaScreen.x < 0f)
            {
                Debug.Log("Свайп в стену (слева) отменён");
                return;
            }
            // swipe right -> deltaScreen.x > 0
            else if (!player.wallOnLeft && deltaScreen.x > 0f)
            {
                Debug.Log("Свайп в стену (справа) отменён");
                return;
            }
        }

        // Если мировой дельта слишком мала — игнорируем
        if (Mathf.Abs(swipeWorld.x) < MIN_SWIPE_WORLD && Mathf.Abs(swipeWorld.y) < MIN_SWIPE_WORLD) return;

        // Формируем endWorld относительно сохранённого startWorld (чтобы интерфейс не зависел от перемещения камеры)
        Vector3 endWorld = info.startWorld + swipeWorld;

        HandleSwipeDecision(deltaScreen, swipeWorld, info.startWorld, endWorld, horizontalSwipe);
    }


    private float ComputeWorldUnitsPerPixel(Camera cam, float depth)
    {
        if (cam == null) return 0f;

        if (cam.orthographic)
        {
            // высота мира в юнитах, видимая камерой на данном расстоянии (ортографич.)
            return (cam.orthographicSize * 2f) / cam.pixelHeight;
        }
        else
        {
            // приближённая конвертация для перспективной камеры:
            // высота фрустума на заданной глубине
            float frustumHeight = 2f * depth * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
            return frustumHeight / cam.pixelHeight;
        }
    }

    private IEnumerator RemoveTouchAfterFrame(int fingerId)
    {
        yield return null; // ждём один кадр
        player.processedTouches.Remove(fingerId);
    }

    // ----------------- Обработка жеста -----------------
    private void HandleSwipeDecision(Vector2 swipeScreenDelta, Vector3 swipeWorldDelta, Vector3 startWorld, Vector3 endWorld, bool horizontalSwipe)
    {
        player.wasEnemyDamagedByLastSwipe = false;
        player.OnMove();
        ScoreManager.Instance.UpActionCombo();

        // Сравниваем по абсолютной величине экранных компонентов, чтобы UX был стабильнее на разных камерах/масштабах
        if (horizontalSwipe)
        {
            // Горизонтальный свайп — движение в targetX
            float dxWorld = swipeWorldDelta.x;
            Debug.Log(swipeWorldDelta.x);
            if (Mathf.Abs(dxWorld) >= MIN_SWIPE_WORLD)
            {
                StartHorizontalMove(dxWorld);
                player.CurrentStamina--;
            }
        }
        else
        {
            // Вертикальный свайп — прыжок
            if (swipeScreenDelta.y > 0f && player.isGrounded)
            {
                Jump();
                //player.CurrentStamina--;
            }
        }
    }

    private void StartHorizontalMove(float dxWorld)
    {
        // Вычисляем целевую позицию в мировых координатах
        player.targetX = player.transform.position.x + dxWorld;

        // Желаемая скорость по направлению свайпа
        player.desiredVelocityX = Mathf.Sign(dxWorld) * player.speed * MOVE_VELOCITY_MULTIPLIER;

        // Включаем движение — будет применено в FixedUpdate
        //Debug.Log(player.movingToTarget);
        player.movingToTarget = true;
        //Debug.Log(player.movingToTarget);

        // Немедленное обновление направления вида (чтобы спрайт сразу повернулся)
        ChangeDirectionView(player.desiredVelocityX > 0f);

        // Вызов сигнала начала свайпа/его окончания для внешних listeners
        //Debug.Log("Ьвф,,");
        //Debug.Log(player.desiredVelocityX);
        OnSwipeStarted?.Invoke();

        // Смена состояния FSM: если рядом враг — WalkAndAttack, иначе Walk
        if (player.isEnemyNear)
            fsmPlayer.SetState<FsmStateWalkAndAttack>();
        else
            fsmPlayer.SetState<FsmStateWalk>();
    }

    protected internal void StopHorizontalMovement() // в Idle вызываем
    {
        //Debug.Log("Да что за хуета");
        player.movingToTarget = false;
        player.desiredVelocityX = 0f;

        // Обнуляем горизонтальную скорость — используем способ, совместимый с вашей оболочкой rb 
        player.rb.linearVelocity = new Vector3(0f, player.rb.linearVelocityY, 0f);

        OnSwipeEnded?.Invoke();
    }

    public void HandleSwipe(Vector3 swipe) // оставлен для совместимости, вызывает тот же путь
    {
        HandleSwipeDecision(new Vector2(swipe.x, swipe.y), swipe, player.startTouchPosition, player.startTouchPosition + swipe, swipe.x > swipe.y);
    }

    // ----------------- Вспомогательные методы -----------------
    private Vector3 ScreenToWorldAtZ0(Camera cam, Vector2 screenPosition)
    {
        if (cam == null)
        {
            Debug.LogWarning("FsmStatePlayer_Reworked: main camera is null in ScreenToWorldAtZ0.");
            return Vector3.zero;
        }

        // расстояние камеры до мирового Z = 0
        float dist = Mathf.Abs(cam.transform.position.z);
        Vector3 screenPoint = new Vector3(screenPosition.x, screenPosition.y, dist);
        Vector3 world = cam.ScreenToWorldPoint(screenPoint);
        world.z = 0f;
        return world;
    }

    private bool IsPointerOverUIAtPosition(Vector2 screenPosition)
    {
        if (EventSystem.current == null) return false;

        PointerEventData ped = new PointerEventData(EventSystem.current)
        {
            position = screenPosition
        };
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(ped, results);
        return results.Count > 0;
    }

    private void MoveTarget()
    {
        // Применяем физику в FixedUpdate — изменение velocity/linearVelocity
        //Debug.Log(player.movingToTarget);
        if (player.movingToTarget)
        {
            // Применяем желаемую скорость
            player.rb.linearVelocityX = player.desiredVelocityX;

            // Проверяем, не прошли ли цель (overshoot)
            float posX = player.transform.position.x;
            //Debug.Log(player.desiredVelocityX);
            //Debug.Log(player.targetX);
            //Debug.Log(STOP_EPSILON);
            if (player.desiredVelocityX > 0f)
            {
                if (posX >= player.targetX - STOP_EPSILON)
                {
                    StopHorizontalMovement();
                }
            }
            else if (player.desiredVelocityX < 0f)
            {
                if (posX <= player.targetX + STOP_EPSILON)
                {
                    StopHorizontalMovement();
                }
            }
        }
    }

    // ----------------- Физические / логические действия -----------------
    void Jump()
    {
        fsmPlayer.SetState<FsmStateJump>();
    }

    // Подписки/отписки (оставлены как раньше, но рекомендую вызывать Unsubscribe на выходе из состояния)
    public void SubscribeForSignalActivationSomeEquipment()
    {
        player.OnSomeEquipmentShouldBeActivate += SomeEquipmentShouldBeActivate;
    }

    public void UnsubscribeForSignalActivationSomeEquipment()
    {
        player.OnSomeEquipmentShouldBeActivate -= SomeEquipmentShouldBeActivate;
    }

    private void SomeEquipmentShouldBeActivate(Equipment equipment)
    {
        player._fsm.SetState<FsmStateCastUnit>(new Dictionary<string, object> { { "equipmentWhatWasPressed", equipment } });
    }

    public void SomeTranslateEquipment(bool isTranslating)
    {
        if (isTranslating) player._fsm.SetState<FsmStateTranslatingEquipment>();
        else player._fsm.SetState<FsmStateIdle>();
    }

    // ----------------- Отображение / направление -----------------
    // lookingRight может быть null в базовой версии, но мы сдаём bool явно
    public override void ChangeDirectionView(bool? lookingRight)
    {
        // Если передано null — используем текущую скорость для вычисления направления
        bool newLookingRight;
        if (lookingRight.HasValue)
            newLookingRight = lookingRight.Value;
        else
            newLookingRight = player.rb.linearVelocityX > 0f;

        // Если уже смотрим в нужную сторону — ничего не делаем
        if (player.lookingRight == newLookingRight) return;

        player.lookingRight = newLookingRight;
        base.ChangeDirectionView(player.lookingRight);

        // Устанавливаем flipX однозначно (не инвертируем)
        // Предполагается, что при lookingRight == true спрайт не перевёрнут (адаптируйте если у вас иначе)
        player.selfSprite.flipX = !player.lookingRight;

        // Устанавливаем позицию attackAreaTransform по знаку X (идемпотентно)
        var lp = player.attackAreaTransform.localPosition;
        lp.x = Mathf.Abs(lp.x) * (player.lookingRight ? 1f : -1f);
        player.attackAreaTransform.localPosition = lp;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        CoroutineManager.Instance.StopAllCoroutinesFor(gameObject);
    }

    // ----------------- Доп. рекомендации / защита -----------------
    // Рекомендуется: при входе/выходе из состояния явно вызывать Subscribe/Unsubscribe, чтобы не было утечек.
}
