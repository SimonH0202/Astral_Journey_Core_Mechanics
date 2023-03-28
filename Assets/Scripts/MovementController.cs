using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MovementController : MonoBehaviour
{
    //References
    PlayerInput playerInput;
    CharacterController characterController;
    Animator animator;
    Transform cameraTransform;
        
    //Private Movement variables
    Vector2 movementInput;
    Vector3 moveDirection;
    Vector3 movement;
    bool isMovementPressed;
    bool isRunningPressed;
    bool isMovementLocked = false;
    float turnSmoothVelocity;
    float targetSpeed;

    //Private Jump variables
    bool isJumpPressed = false;
    bool isJumping = false;
    bool isJumpAnimating = false;
    float initialJumpVelocity;

    [Header("Jump Settings")]
    //Public Jump variables
    public float maxJumpHeight = 2f;
    public float maxJumpTime = 0.5f;

    

    [Header("Movement Settings")]
    //Public Movement variables
    public float movementSpeed = 2.5f;
    public float runSpeed = 8f;
    public float rotationFactorPerFrame = 15.0f;
    [Space]
    [Header("Physics Settings")]
    //public Physics variables
    public float gravity = -9.81f;

    //Animation Hashes
    int isWalkingHash;
    int isRunningHash;
    int isJumpingHash;


    void Awake()
    {
        //Get References
        playerInput = new PlayerInput();
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        cameraTransform = Camera.main.transform;

        //Register Input Events
        playerInput.CharacterControls.Move.started += OnMovementInput;
        playerInput.CharacterControls.Move.performed += OnMovementInput;
        playerInput.CharacterControls.Move.canceled += OnMovementInput;
        playerInput.CharacterControls.Run.started += OnRunInput;
        playerInput.CharacterControls.Run.canceled += OnRunInput;
        playerInput.CharacterControls.Jump.started += OnJumpInput;
        playerInput.CharacterControls.Jump.canceled += OnJumpInput;
        

        //Get Animation Hashes
        isWalkingHash = Animator.StringToHash("isWalking");
        isRunningHash = Animator.StringToHash("isRunning");
        isJumpingHash = Animator.StringToHash("isJumping");

        //Setup Jump Variables
        SetupJumpVariables();
    }

    void Start()
    {
        //Lock cursor
        Cursor.lockState = CursorLockMode.Locked;

        //Set initial values
        targetSpeed = movementSpeed;
    }

    void OnMovementInput(InputAction.CallbackContext context)
    {
        if (!isMovementLocked)
        {
            //Get movement input 
            movementInput = context.ReadValue<Vector2>();
            movement.x = movementInput.x;
            movement.z = movementInput.y;
            isMovementPressed = movementInput.x != 0 || movementInput.y != 0;
        }
    }

    void OnRunInput(InputAction.CallbackContext context)
    {
        //Check run input
        isRunningPressed = context.ReadValueAsButton();
    }

    void OnJumpInput(InputAction.CallbackContext context)
    {
        //Check jump input
        if (!isMovementLocked) isJumpPressed = context.ReadValueAsButton();
    }
    

    void SetupJumpVariables()
    {
        //Calculate jump variables based on jump height and time
        float timeToApex = maxJumpTime / 2;
        gravity = (-2 * maxJumpHeight) / Mathf.Pow(timeToApex, 2);
        initialJumpVelocity = (2 * maxJumpHeight) / timeToApex;
    }

    void HandleRun() {
        //Set current movement speed
        if (isRunningPressed) targetSpeed = runSpeed;
        else targetSpeed = movementSpeed;
    }

    void HandleJump() {
        //Handle Jumping and Animation
        if (!isJumping && characterController.isGrounded && isJumpPressed) {
            animator.SetBool(isJumpingHash, true);
            isJumpAnimating = true;
            isJumping = true;
            moveDirection.y = initialJumpVelocity / 2;
        } else if (isJumping && !isJumpPressed && characterController.isGrounded) {
            isJumping = false;
        }
    }

    void HandleRotation()
    {
        Quaternion currentRotation = transform.rotation;

        if (isMovementPressed) {
            //Calculate and set rotation with camera and movement
            Quaternion targetRotation = Quaternion.Euler(0, Mathf.Atan2(movement.x, movement.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y, 0);
            transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, rotationFactorPerFrame * Time.deltaTime);

            //Set movement direction to where the player is facing
            moveDirection.x = transform.forward.x;
            moveDirection.z = transform.forward.z;
        } else {
            //Set movement direction to 0 if no movement input
            moveDirection.x = 0;
            moveDirection.z = 0;
        }
    }

    public void DisableMovement()
    {
        //Set movement to 0 and disable movement
        moveDirection.x = 0;
        moveDirection.z = 0;

        movementInput.x = 0;
        movementInput.y = 0;

        isMovementPressed = false;
        isMovementLocked = true;
    }

    public void EnableMovement()
    {
        //Enable movement and check for movement input
        isMovementLocked = false;
        //movementInput = playerInput.CharacterControls.Move.ReadValue<Vector2>();
        isMovementPressed = movementInput.x != 0 || movementInput.y != 0;
        HandleRotation();
    }

    public bool GetIsJumping() 
    {
        return isJumping;
    }

    public bool GetIsGrounded()
    {
        return characterController.isGrounded;
    }

    void HandleGravity()
    {
        //Set y direction to 0 if grounded
        if (characterController.isGrounded) {
            moveDirection.y = -0.5f;
            if (isJumpAnimating) {
                animator.SetBool(isJumpingHash, false);
                isJumpAnimating = false;
            }
        }
        //Handle Gravity with Verlet Integration
        else {
            float previousY = moveDirection.y;
            float newY = moveDirection.y + gravity * Time.deltaTime;
            float nextY = (previousY + newY) / 2;
            moveDirection.y = nextY;
        }
    }

    void HandleAnimation()
    {
        bool isWalking = animator.GetBool(isWalkingHash);
        bool isRunning = animator.GetBool(isRunningHash);

        //Handle Walking
        if (isMovementPressed && !isWalking) animator.SetBool(isWalkingHash, true);
        else if (!isMovementPressed && isWalking) animator.SetBool(isWalkingHash, false);

        //Handle Running
        if (isMovementPressed && isRunningPressed && !isRunning) animator.SetBool(isRunningHash, true);
        else if (!isMovementPressed || !isRunningPressed) animator.SetBool(isRunningHash, false);
    }

    // Update is called once per frame
    void Update()
    {
        HandleRotation();
        HandleRun();
        HandleAnimation();

        //Move CharacterController
        characterController.Move(new Vector3(moveDirection.x * targetSpeed, moveDirection.y, moveDirection.z * targetSpeed) * Time.deltaTime);
        
        HandleGravity();
        HandleJump();
    }

    void OnEnable()
    {
        //Enable Input
        playerInput.CharacterControls.Enable();
    }

    void OnDisable()
    {
        //Disable Input
        playerInput.CharacterControls.Disable();
    }
}