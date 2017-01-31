using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    float groundAcceleration = 15;
    float maxSpeed = 7.5f;
    public float jumpForce = 750;
    public LayerMask whatIsGround;
    float moveX;
    bool facingRight = true;
    Rigidbody2D rb;
    Animator anim;
    bool grounded = true;
    float jumpingTime = 1;
    PlayerState myState;
    PlayerState nextState;
    bool stateEnded;
    GameObject duckingMario;
    /*Get the value for this variable using the editor!
     * No coding required! */ 
    public GameObject littleMario;
    GameObject superMario;
    bool super;
    bool little;

    //Awake is called before any Start function
    void Awake() {
        superMario = GameObject.Find("Super Mario");
    }

    // Use this for initialization
    void Start () {
        rb = this.transform.root.gameObject.GetComponent<Rigidbody2D>();
        anim = this.gameObject.GetComponent<Animator>();
        myState = new Grounded(this);
        if (gameObject.name == "Super Mario")
        {
            super = true;
            little = false;
            duckingMario = GameObject.Find("Ducking Mario");
            duckingMario.SetActive(false);
            superMario.SetActive(false);
        }
        else
        {
            super = false;
            little = true;
            duckingMario = null;
        }
    }

    // Update is called once per frame
    void Update () {
        moveX = Input.GetAxis("Horizontal");
        myState.Update();
        if (Input.anyKeyDown || stateEnded)
        {
            nextState = myState.HandleInput();
        }
    }

    /*This function is called every fixed framerate frame. Best practice 
     * to do physics updates in here. */
    void FixedUpdate() {
        anim.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
        anim.SetFloat("vSpeed", Mathf.Abs(rb.velocity.y));
        /* Make Mario switch directions!
         */
        if (facingRight && Input.GetKeyDown(KeyCode.A) == true) {
            Flip();
        } else if (!facingRight && Input.GetKeyDown(KeyCode.D) == true) {
            Flip();
        }
        myState.FixedUpdate();
        if (nextState != null)
        {
            stateEnded = false;
            myState.Exit();
            myState = nextState;
            nextState = null;
            myState.Enter();
        }
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = this.gameObject.transform.localScale;
        scale.x = scale.x * -1;
        this.gameObject.transform.localScale = scale;
        //Force to turn quicker.
        rb.AddForce(new Vector3(-25 * rb.velocity.x, 0));
    }

    bool CheckForGround() {
        /*Vector2 origin = new Vector2(rb.position.x + 0.5f, rb.position.y + 1);
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, 1.2f, whatIsGround);
        Debug.DrawRay(origin, Vector2.down * 1.2f);
        return hit.collider != null;*/
        SpriteRenderer mySprite = GetComponent<SpriteRenderer>();
        var castHeight = mySprite.sprite.bounds.size.y / 2 + 0.25f;
        Vector3 origin = new Vector3(transform.position.x, transform.position.y);
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector3.down, castHeight, whatIsGround);
        Debug.DrawRay(origin, Vector3.down * castHeight);
        return hit.collider != null;
    }

    void Duck()
    {
        duckingMario.SetActive(true);
        duckingMario.transform.position = new Vector3(rb.position.x, duckingMario.transform.position.y);
        rb.velocity = Vector3.zero;
        if (!facingRight)
        {
            Vector3 scale = duckingMario.transform.localScale;
            scale.x = scale.x * -1;
            duckingMario.transform.localScale = scale;
        }
        this.gameObject.SetActive(false);
    }

    /* Used in Project 1-2 */
    public void Grow() {
        //If littleMario turn into superMario.
        if (little) {
            superMario.SetActive(true);
            littleMario.SetActive(false);
        }
    }

    /* Used in Project 1-2 */
    public void Shrink() {
        //If littleMario then gameOver.
        //If superMario turn into littleMario.
        if (little)
        {
            /*Implemented in Project 1-3*/       
        }
        else if (super) {
            littleMario.SetActive(true);
            superMario.SetActive(false);
        }
    }

    private class Grounded : PlayerState
    {

        PlayerController controller;
        Rigidbody2D rb;
        Animator anim;
        float moveX;
        float moveJump;
        float maxSpeed;
        float groundAcceleration;
        bool jump;
        bool fall;
        
        public Grounded(PlayerController controller) {
            this.controller = controller;
            this.rb = controller.rb;
            this.anim = controller.anim;
            this.moveX = controller.moveX;
            this.maxSpeed = controller.maxSpeed;
            this.groundAcceleration = controller.groundAcceleration;
            
        }

        /* Enter is called immediately after a state transition. */
        public void Enter()
        {
            moveX = Input.GetAxis("Horizontal");
            moveJump = Input.GetAxis("Vertical");
            anim.SetBool("Grounded", true);
        }

        /* Update is called every frame by Player Controller. */
        public void Update()
        {
            moveX = Input.GetAxis("Horizontal");
            moveJump = Input.GetAxis("Vertical");
            if (Input.GetButton("Vertical") && controller.super)
            {
                controller.Duck();   
            }
        }

        /* Fixed update is called very fixed frame by Player Controller. */
        public void FixedUpdate()
        {
            if(Mathf.Abs(rb.velocity.magnitude) <= maxSpeed)
            {
                rb.AddForce(new Vector2(groundAcceleration * moveX, 0));
            }
            /* Check if falling. Move to "Jumping" state. */
            if (Mathf.Abs(rb.velocity.y) > 0)
            {
                fall = true;
                controller.stateEnded = true;
            }
        }

        /* Exit is called immediately before a state transition. */
        public void Exit()
        {
            if (jump)
            {
                anim.SetBool("Grounded", false);
            }
            else if (fall)
            {
                anim.enabled = false;
            }
        }

        /* HandleInput is called whenever Player Controller detects 
         * input or the 'stateEnded' variable is true. It should return 
         * the next state. */
        public PlayerState HandleInput()
        {
            if (Input.GetButton("Jump"))
            {
                jump = true;
                return new Jumping(controller);
            }
            else if (fall) {
                return new Jumping(controller);
            }
            return null;
        }
    }

    private class Jumping : PlayerState
    {

        PlayerController controller;
        Rigidbody2D rb;
        Animator anim;
        float moveX;
        float moveJump;
        float jumpForce;
        float jumpingTime = 1;
        float doubleJumpingTime = 1;
        float maxSpeed;
        float airHorizAcceleration = 15;
        float airJumpAcceleration = 13;

        public Jumping(PlayerController controller)
        {
            this.controller = controller;
            rb = controller.rb;
            anim = controller.anim;
            moveX = controller.moveX;
            jumpForce = controller.jumpForce;
            maxSpeed = controller.maxSpeed;
        }

        public void Enter()
        {
            moveJump = Input.GetAxis("Jump");
            moveX = Input.GetAxis("Horizontal");
            /* Add the preliminary jump force.
             */
            jumpForce = 100f;
            anim.SetBool("Jumping", true);
            
            
        }

        public void Update()
        {
            /* Get the two input variables. 
             * 
             * YOUR CODE HERE.
             * 
             */
            moveJump = Input.GetAxis("Jump");
            moveX = Input.GetAxis("Horizontal");    
        }

        public void FixedUpdate()
        {
            /* Jumping timer. When this reaches 0,
             * the player is not allowed to add any more
             * force to the jump. */
            jumpingTime -= Time.deltaTime;
            doubleJumpingTime -= Time.deltaTime;
            /* 1. Add horizontal control in the air. Make sure the player 
             * is not over the maximum speed. 
             * 2. Add extra vertical force while the player holds down
             * space bar. Make sure not to add force after jumpingTime 
             * has reached 0. 
             *
             * YOUR CODE HERE.
             * 
             */

            if (Input.GetButton("Jump") && jumpingTime > 0)
            {
                rb.AddForce(new Vector2(0, (jumpForce + 10) * moveJump));

                //Add additional double jump ability
                if (Input.GetButton("Jump") && doubleJumpingTime > 0)
                {
                    rb.AddForce(new Vector2(0, (jumpForce + 20) * moveJump));


                }

            }


            /* Continuously check that you haven't hit the ground. If
             * you have, then transition to the 'Grounded' state.*/
            if (controller.CheckForGround())
            {
                rb.gravityScale = 1f;
                controller.stateEnded = true;
            }
            /*Add horizontal control in the air. Make sure the player 
             * is not over the maximum speed.*/
            else
            {
                jumpForce = 1f;
                rb.gravityScale = 4f;
                if (Mathf.Abs(rb.velocity.magnitude) <= maxSpeed)
                {
                    rb.AddForce(new Vector2(airHorizAcceleration * moveX, 0));
                }
                //Add additional multiple jump and levitation ability
                /*if (Input.GetButton("Jump"))
                {
                    for (int i = 0; i < 5; i++)
                    {
                        rb.gravityScale = 0f;
                    }
                    rb.gravityScale = 4f;
                }*/
            }
        }

        public void Exit()
        {
            anim.enabled = true;
            anim.SetBool("Jumping", false);
        }

        public PlayerState HandleInput()
        {
            if (controller.stateEnded)
            {
                return new Grounded(controller);
            }
            else {
                return null;
            }
        }
    }

}
