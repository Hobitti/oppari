using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Mirror.Build.shootterMP
{
    public class Players : NetworkBehaviour
    {

        [Header("Movement")]
        public float moveSpeed;

        public float groundDrag;


        //jump adributes
        public float jumpForce;
        public float jumpCooldown;
        public float airMultiplier;
        public float shootCooldown;
        bool readyToJump;
        bool readyToShoot;

        [HideInInspector] public float walkSpeed;
        [HideInInspector] public float sprintSpeed;
        float health = 3;

        private float crouchSpeed = 0.3f;

        //hardset jump as space
        [Header("Keybinds")]
        public KeyCode jumpKey = KeyCode.Space;
        public KeyCode shootKey = KeyCode.Mouse0;

        //ground and height vars
        [Header("Ground Check")]
        public float playerHeight;
        public float crouchHeight;
        public LayerMask whatIsGround;
        bool grounded;
        bool cantStand;
        bool _crouching;

        public Transform orientation;
        public Transform shootPos;
        public GameObject bullet;
        public GameObject ui;
        public TMP_Text hpAmmount;
        private GameObject ui_clone;

        float horizontalInput;
        float verticalInput;

        Vector3 moveDirection;

        Rigidbody rb;
        CapsuleCollider cc;


        // Start is called before the first frame update
        
        void Start()
        {
            if (isLocalPlayer)
            {
                rb = GetComponent<Rigidbody>();
                cc = GetComponent<CapsuleCollider>();
                ui_clone=Instantiate(ui);

                readyToJump = true;
                readyToShoot = true;
                GameObject.FindGameObjectWithTag("MainCamera").GetComponent<playerCamera>().respawn(gameObject);

                health = 3;
                hpAmmount = ui_clone.transform.GetChild(0).GetComponent<TMP_Text>();
                hpAmmount.text=health.ToString();


            }
        }

        // Update is called once per frame
        private void Update()
        {
            
            if (isLocalPlayer)
            {
                // ground check
                grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, whatIsGround);
                cantStand = Physics.Raycast(transform.position, Vector3.up, playerHeight * 1f + 0.3f, whatIsGround);
                MyInput();
                SpeedControl();

                // handle drag
                if (grounded)
                    rb.drag = groundDrag;
                else
                    rb.drag = 0;

                //crouch check
                if (!cantStand) _crouching = (Input.GetKey(KeyCode.C));
            }


        }

        private void FixedUpdate()
        {
            if (isLocalPlayer)
            {
                var desiredHeught = _crouching ? crouchHeight : playerHeight;
                if (cc.height != desiredHeught)
                {
                    adjustHeight(desiredHeught);
                }

                MovePlayer();
            }
        }

        private void adjustHeight(float desiredHeught)
        {
            float center = desiredHeught / 2;
            cc.height = Mathf.Lerp(cc.height, desiredHeught, crouchSpeed);
            transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(1, center, 1), crouchSpeed);

        }

        private void MyInput()
        {
            // get imput types from settings
            horizontalInput = Input.GetAxisRaw("Horizontal");
            verticalInput = Input.GetAxisRaw("Vertical");

            // when to jump
            if (Input.GetKey(jumpKey) && readyToJump && grounded)
            {
                readyToJump = false;

                Jump();

                Invoke(nameof(ResetJump), jumpCooldown);
            }
            if (Input.GetKeyDown(shootKey) && readyToShoot)
            {
                CmdFire();
            }

        }
        [Command]
        void CmdFire()
        {
                readyToShoot = false;
                Invoke(nameof(resetShoot), shootCooldown);
                GameObject projectile = Instantiate(bullet, shootPos.position, shootPos.rotation);
                NetworkServer.Spawn(projectile);

            
        }

        private void MovePlayer()
        {
            float speed;

            if (_crouching) speed = moveSpeed / 2;
            else if (Input.GetKey(KeyCode.LeftShift)) speed = moveSpeed * 2;
            else speed = moveSpeed;
            // calculate movement direction
            moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
            
          

            // on ground
            if (grounded)
                rb.AddForce(moveDirection.normalized * speed * 10f, ForceMode.Force);

            // in air
            else if (!grounded)
                rb.AddForce(moveDirection.normalized * speed * 10f * airMultiplier, ForceMode.Force);
        }

        private void SpeedControl()
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            // limit velocity if needed
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }

        private void Jump()
        {
            // reset y velocity
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        }
        [Command]
        private void ResetJump()
        {
            readyToJump = true;
        }
        [Command]
        private void resetShoot()
        {
            readyToShoot = true;
        }


        private void OnTriggerEnter(Collider other)
        {
            if (isLocalPlayer)
            {           
                if (other.CompareTag("bullet"))
                {
                    health--;
                    print(health);
                    hpAmmount.text = health.ToString();
                    if (health == 0)
                    {
                        Cursor.lockState = CursorLockMode.None;
                        Cursor.visible = true;
                        Destroy(ui_clone);
                        death();
                      
                    }
                    print("ouh");
                }
            }
        }
        [Command]
        private void death()
        {
           
            
            Destroy(gameObject);

        }
    }
}
