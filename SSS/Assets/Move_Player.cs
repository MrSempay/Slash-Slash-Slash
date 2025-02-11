using UnityEngine;

public class CubeController : MonoBehaviour
{
    public float speed = 5f;       // Скорость перемещения
    public float jumpForce = 5f;  // Сила прыжка
    private Rigidbody2D rb;       // Rigidbody2D кубика
    private Vector2 startTouchPosition, endTouchPosition; // Для отслеживания свайпов
    public bool isGrounded = true; // Проверка, находится ли кубик на земле

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        DebugInputTouch();

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
        else if (Input.GetMouseButtonDown(0)) // Когда нажата левая кнопка мыши
        {
            startTouchPosition = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0)) // Когда отпущена левая кнопка мыши
        {
            endTouchPosition = Input.mousePosition;
            HandleSwipe(endTouchPosition - startTouchPosition);
        }
    }

    // Обрабатываем свайп
    void HandleSwipe(Vector2 swipe)
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
        rb.AddForce(Vector2.left * speed, ForceMode2D.Impulse);
    }

    void MoveRight()
    {
        rb.AddForce(Vector2.right * speed, ForceMode2D.Impulse);
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
}