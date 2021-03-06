using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("Color")]
    public int chosenColor;
    public Color[] colors;
    public SpriteRenderer[] partsToColor;

    [Header("Stats")]
    public float speed;

    [Header("Jump Mechanics")]
    public float jumpForce;
    public float jumpTime;
    public Transform groundCheck;
    public LayerMask whatIsGround;
    public float checkRadius;
    public float coyoteTime;
    float coyoteTimeCounter;
    float jumpTimeCounter;

    // Components ---
    Rigidbody2D rb;
    Animator anim;
    // --------------

    // Checks ---
    bool isJumping = true;
    bool isGrounded = false;
    bool gravityIsReversed = false;
    bool canDoubleJump = false;
    // ----------

    private void Start() {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        SetColor();
    }

    void SetColor() {
        foreach (var part in partsToColor)
        {
            part.color = colors[chosenColor];
        }
    }

    private void FixedUpdate() {
        // MOVE
        rb.velocity = new Vector2(1 * speed, rb.velocity.y);

        // Check if grounded and animate accordingly
        bool _isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, whatIsGround);
        if (isGrounded && !_isGrounded) {
            // NOT GROUNDED
            canDoubleJump = true;
        }
        else if (!isGrounded && _isGrounded) {
            // JUST LANDED
            canDoubleJump = false;
            anim.SetBool("isJumping", false);
        }
        isGrounded = _isGrounded;

        // Coyote time stuff
        if (isGrounded) {
            coyoteTimeCounter = coyoteTime;
        } else {
            coyoteTimeCounter -= Time.deltaTime;
        }

        // If holding jump, go higher
        if (isJumping) {
            if (jumpTimeCounter > 0) {
                // rb.velocity = Vector2.up * jumpForce;
                rb.velocity = new Vector2(rb.velocity.x, (gravityIsReversed ? -1 : 1) * jumpForce);
                jumpTimeCounter -= Time.deltaTime;
            } else {
                // When jump time runs out, stop going higher
                isJumping = false;
                coyoteTimeCounter = 0;
            }
        }
    }

    public void Jump(InputAction.CallbackContext context) {
        if (context.started && (coyoteTimeCounter > 0 || canDoubleJump)) {
            if (!isGrounded) {
                canDoubleJump = false;
            }
            isJumping = true;
            jumpTimeCounter = jumpTime;
            anim.SetBool("isJumping", true);
        }

        if (context.canceled) { // If button released, stop jumping
            isJumping = false;
            coyoteTimeCounter = 0;
        }
    }

    public void ReverseGravity(InputAction.CallbackContext context) {
        if (context.started) {
            gravityIsReversed = !gravityIsReversed;
            rb.gravityScale *= -1;
            transform.Rotate(0, 180, 180);
        }
    }
}
