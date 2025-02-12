using UnityEngine;

public class Move_Player : MonoBehaviour
{
    public float speed = 2f;       // Скорость перемещения
    public float jumpForce = 5f;  // Сила прыжка
    private Rigidbody2D rb;       // Rigidbody2D кубика
    private Transform transform;       // Rigidbody2D кубика
    private Vector3 startTouchPosition, endTouchPosition; // Для отслеживания свайпов
    private Vector3 startPositionPlayerBeforeMoving; // стартовая позиция игрока до того, как он начал движение
    private float differenceXBetweenStartAndEndPositions; // разница по координате х между началом свайпа и его окончанием
    public bool isGrounded = true; // Проверка, находится ли кубик на земле
    public Camera mainCamera; // Ссылка на камеру

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        transform = GetComponent<Transform>();
    }

    void Update()
    {
        transform.rotation = Quaternion.Euler(0, 0, 0);
        DebugInputTouch();
        IsAtSpecifiedPosition(); // узнаём, добрался ли игрок до конечной точки пути (по сути его позиция + длина свайпа. Если да, то сбрасываем мгновенную скорость по х в ноль.

        // Для мобильных устройств
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                startTouchPosition = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                endTouchPosition = touch.position;
                HandleSwipe(endTouchPosition - startTouchPosition);
            }
        }
        // Для ПК (с использованием мыши)
        if (Input.GetMouseButtonDown(0)) // Когда нажата левая кнопка мыши
        {
            startTouchPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition); // тут используем Vector3, поэтому координату z сбрасываем в ноль (для 2D)
            startTouchPosition.z = 0;
            startPositionPlayerBeforeMoving = transform.position; // запоминаем начальную позицию игрока, ибо уже от неё будет рассчитывать расстояние, которое нужно пройти
        }
        else if (Input.GetMouseButtonUp(0)) // Когда отпущена левая кнопка мыши
        {
            endTouchPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            endTouchPosition.z = 0;
            differenceXBetweenStartAndEndPositions = endTouchPosition.x - startTouchPosition.x;
            HandleSwipe(endTouchPosition - startTouchPosition);
        }
    }

    // Обрабатываем свайп
    void HandleSwipe(Vector3 swipe)
    {
        // Определяем направление свайпа
        if (Mathf.Abs(swipe.x) > Mathf.Abs(swipe.y))
        {
            // Свайп влево или вправо
            if (swipe.x > 0)
            {
                MoveRight();
            }
            else
            {
                
                MoveLeft();
            }
        }
        else
        {
            // Свайп вверх (прыжок)
            if (swipe.y > 0 && isGrounded)
            {
                Jump();
            }
        }
    }

    void MoveLeft()
    {
        //rb.AddForce(Vector2.left * speed, ForceMode2D.Impulse);
        rb.linearVelocity = new Vector3(differenceXBetweenStartAndEndPositions * speed, 0, 0);
    }

    void MoveRight()
    {
        //rb.AddForce(Vector2.right * speed, ForceMode2D.Impulse); // в теории можно сделать и импульсом перемещение, но тогда он будет накапливаться при многочисленных свайпах.
        rb.linearVelocity = new Vector3(differenceXBetweenStartAndEndPositions * speed, 0, 0);
    }

    void Jump()
    {
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        isGrounded = false; // Устанавливаем, что кубик в воздухе
    }

    // Исправленный метод для 2D физики
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Проверяем, столкнулся ли кубик с объектом с тегом "Ground"
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true; // Устанавливаем, что кубик снова на земле
        }
    }

    void DebugInputTouch()
    {
        // Проверяем количество касаний
        if (Input.touchCount > 0) // На мобильном устройстве
        {
            Debug.Log("Количество касаний: " + Input.touchCount);
        }
        else if (Input.GetMouseButton(0)) // Если нажата мышь
        {
            Debug.Log("Мышь нажата");
        }
        else
        {
            Debug.Log("Сейчас нет касаний и нажатий мыши");
        }
    }

    void IsAtSpecifiedPosition()
    {
        if (transform.position.x >= startPositionPlayerBeforeMoving.x + differenceXBetweenStartAndEndPositions && rb.linearVelocity.x > 0) // для правого движения
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocityY, 0);
        }
        if (transform.position.x <= startPositionPlayerBeforeMoving.x + differenceXBetweenStartAndEndPositions && rb.linearVelocity.x < 0) // для левого движения
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocityY, 0);
        }
    }
}