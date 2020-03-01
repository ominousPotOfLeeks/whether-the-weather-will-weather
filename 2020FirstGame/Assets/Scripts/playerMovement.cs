using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerMovement : MonoBehaviour
{

    private float m_MovementSmoothing = .02f;

    public float runSpeed;

    private Rigidbody2D m_Rigidbody2D;
    private Vector3 m_Velocity = Vector3.zero;

    float horizontalInputValue = 0f;
    float verticalInputValue = 0f;

    void Awake()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        horizontalInputValue = Input.GetAxisRaw("Horizontal");
        verticalInputValue = Input.GetAxisRaw("Vertical");
    }

    void FixedUpdate ()
    {
        float horizontalMove = horizontalInputValue * runSpeed * Time.fixedDeltaTime;
        float verticalMove = verticalInputValue * runSpeed * Time.fixedDeltaTime;
        Vector3 targetVelocity = new Vector2(horizontalMove, verticalMove);
        m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);
    }
}
