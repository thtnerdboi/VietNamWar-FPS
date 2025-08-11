
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float sprintMultiplier = 1.5f;
    public float gravity = -20f;
    public float jumpHeight = 1.2f;

    private CharacterController cc;
    private Vector3 velocity;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
    }

    void Update()
    {
        bool sprint = Input.GetKey(KeyCode.LeftShift);
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 move = transform.right * h + transform.forward * v;
        float speed = moveSpeed * (sprint ? sprintMultiplier : 1f);
        cc.Move(move * speed * Time.deltaTime);

        bool isGrounded = cc.isGrounded;
        if (isGrounded && velocity.y < 0f) velocity.y = -2f;

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            velocity.y = Mathf.Sqrt(-2f * gravity * jumpHeight);

        velocity.y += gravity * Time.deltaTime;
        cc.Move(velocity * Time.deltaTime);
    }
}
