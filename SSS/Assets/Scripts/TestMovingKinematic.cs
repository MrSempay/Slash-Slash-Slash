using UnityEngine;

public class TestMovingKinematic : MonoBehaviour 
{
    private Rigidbody2D rb;
    private float speed = 1;
    private Vector2 targetPosition;
    private Vector2 targetPosition1;
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.AddForce(new Vector2(0, 10));
    }

    private void FixedUpdate()
    {
        targetPosition = rb.position + new Vector2(0, speed * Time.fixedDeltaTime);
        targetPosition1 = rb.position + new Vector2(speed * Time.fixedDeltaTime, 0);
        //rb.MovePosition(targetPosition);
        //rb.MovePosition(targetPosition1);
        
    }
}
