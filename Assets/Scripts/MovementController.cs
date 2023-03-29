using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MovementController : MonoBehaviour
{
    //References
    PlayerInputs playerInput;
    PlayerInput input;
    CharacterController characterController;
    Animator animator;
    Transform cameraTransform;
    PlayerStatsSystem playerStatsSystem;
        
    //Private Movement variables
    Vector2 movementInput;
    Vector3 moveDirection;
    Vector3 movement;
    bool isMovementPressed;
    bool isMovementLocked = false;
    float turnSmoothVelocity;
    float targetSpeed;

    //Private Jump variables
    bool isJumpPressed = false;
    bool isJumping = false;
    bool isJumpAnimating = false;
    float initialJumpVelocity;

    //Private Dodge variables
    bool isDodging = false;

    //Private Cinemachine variables
    private float cinemachineTargetYaw;
    private float cinemachineTargetPitch;
    private bool isCurrentDeviceMouse;


    [Header("Jump Settings")]
    //Public Jump variables
    public float maxJumpHeight = 2f;
    public float maxJumpTime = 0.5f;

    [Header("Cinemachine")]
    public GameObject cinemachineCameraTarget;  
    public float topClamp = 70.0f;
    public float bottomClamp = -30.0f;
    public float cameraAngleOverride = 0.0f;
    public bool lockCameraPosition = false;

    [Header("Movement Settings")]
    //Public Movement variables
    public float movementSpeed = 2.5f;
    public float runSpeed = 8f;
    public float rotationFactorPerFrame = 15.0f;

    [Header("Dodge Settings")]
    //Public Dodge variables
    public float dodgeDistance = 10f;
    public float dodgeTimer = 0.5f;

    [Space]
    [Header("Physics Settings")]
    //public Physics variables
    public float gravity = -9.81f;

    //Animation Hashes
    int isWalkingHash;
    int isRunningHash;
    int isJumpingHash;
    int isDodgingHash;


    void Awake()
    {
        //Get References
        playerInput = GetComponent<PlayerInputs>();
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        cameraTransform = Camera.main.transform;
        playerStatsSystem = GetComponent<PlayerStatsSystem>();
        
        //Get Animation Hashes
        isWalkingHash = Animator.StringToHash("isWalking");
        isRunningHash = Animator.StringToHash("isRunning");
        isJumpingHash = Animator.StringToHash("isJumping");
        isDodgingHash = Animator.StringToHash("isDodging");

        //Setup Jump Variables
        SetupJumpVariables();
    }

    void Start()
    {
        input = GetComponent<PlayerInput>();

        //Set initial cinemachine values
        cinemachineTargetYaw = cinemachineCameraTarget.transform.rotation.eulerAngles.y;

        //Lock cursor
        Cursor.lockState = CursorLockMode.Locked;

        //Set initial values
        targetSpeed = movementSpeed;
    }

    void SetupJumpVariables()
    {
        //Calculate jump variables based on jump height and time
        float timeToApex = maxJumpTime / 2;
        gravity = (-2 * maxJumpHeight) / Mathf.Pow(timeToApex, 2);
        initialJumpVelocity = (2 * maxJumpHeight) / timeToApex;
    }

    private void HandleCameraRotation()
    {
        //Handle Camera Rotation
        if (playerInput.look.sqrMagnitude >= 0.01f && !lockCameraPosition)
        {
            //Calculate our rotation multiplier based on if we are using a mouse or controller
            float deltaTimeMultiplier = isCurrentDeviceMouse ? 1f : Time.deltaTime;

            cinemachineTargetYaw += playerInput.look.x * deltaTimeMultiplier;
            cinemachineTargetPitch += playerInput.look.y * deltaTimeMultiplier;
        }

        //Clamp rotations to limit values tp 360 degrees
        cinemachineTargetYaw = ClampAngle(cinemachineTargetYaw, float.MinValue, float.MaxValue);
        cinemachineTargetPitch = ClampAngle(cinemachineTargetPitch, bottomClamp, topClamp);

        //Cinemachine will follow this target
        cinemachineCameraTarget.transform.rotation = Quaternion.Euler(cinemachineTargetPitch + cameraAngleOverride,
        cinemachineTargetYaw, 0.0f);
        }

    void HandleRun()
    {
        //Set current movement speed
        if (playerInput.sprint)
        {
            targetSpeed = runSpeed;
        }
        else targetSpeed = movementSpeed;
    }

    void HandleJump()
    {
        //Handle Jumping and Animation
        if (!isJumping && characterController.isGrounded && playerInput.jump) {
            animator.SetBool(isJumpingHash, true);
            isJumpAnimating = true;
            isJumping = true;
            moveDirection.y = initialJumpVelocity / 2;
        } else if (isJumping && !isJumpPressed && characterController.isGrounded) {
            isJumping = false;
            playerInput.jump = false;
        }
    }

    void HandleDodge()
    {
        if (playerInput.dodge && !isJumping && !isDodging && !isMovementLocked)
        {
            isDodging = true;
            StartCoroutine(Dodge());
        }
    }

    void HandleRotation()
    {
        Quaternion currentRotation = transform.rotation;

        if (!isMovementLocked)
        {
            //Get movement input 
            movementInput = playerInput.move;
            movement.x = movementInput.x;
            movement.z = movementInput.y;
        }

        if (playerInput.move.magnitude >= 0.1f && !isMovementLocked) {
            //Calculate and set rotation with camera and movement
            Quaternion targetRotation = Quaternion.Euler(0, Mathf.Atan2(movement.x, movement.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y, 0);
            transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, rotationFactorPerFrame * Time.deltaTime);

            //Set movement direction to where the player is facing
            moveDirection.x = transform.forward.x;
            moveDirection.z = transform.forward.z;

            //Move CharacterController
            characterController.Move(new Vector3(moveDirection.x * targetSpeed, moveDirection.y, moveDirection.z * targetSpeed) * Time.deltaTime);
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

        isMovementLocked = true;
    }

    public void EnableMovement()
    {
        //Enable movement and check for movement input
        isMovementLocked = false;
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

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
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

    IEnumerator Dodge()
    {
        //Disable movement and set dodge animation
        DisableMovement();
        animator.SetBool(isDodgingHash, true);

        //Set invulnerability
        playerStatsSystem.SetIsVunerable(false);

        float timer = 0f;

        //Move player forward
        while (timer < dodgeTimer)
        {
            timer += Time.deltaTime;
            Vector3 dodgeDirection = (transform.forward * dodgeDistance) + (Vector3.up * moveDirection.y);
            characterController.Move(dodgeDirection * Time.deltaTime);
            yield return null;
        }
        //Set dodge animation to false and enable movement
        animator.SetBool(isDodgingHash, false);
        isDodging = false;
        EnableMovement();

        //Set invulnerability to false
        playerStatsSystem.SetIsVunerable(true);

    }

    void HandleAnimation()
    {
        bool isWalking = animator.GetBool(isWalkingHash);
        bool isRunning = animator.GetBool(isRunningHash);

        //Handle Walking
        if (playerInput.move.magnitude >= 0.1f && !isWalking) animator.SetBool(isWalkingHash, true);
        else if (!(playerInput.move.magnitude >= 0.1f) && isWalking) animator.SetBool(isWalkingHash, false);

        //Handle Running
        if (playerInput.move.magnitude >= 0.1f && playerInput.sprint && !isRunning) animator.SetBool(isRunningHash, true);
        else if (!(playerInput.move.magnitude >= 0.1f) || !playerInput.sprint) animator.SetBool(isRunningHash, false);
    }

    void Update()
    {
        //Check for mouse and keyboard or controller
        isCurrentDeviceMouse = input.currentControlScheme == "MouseKeyboard";

        HandleRotation();
        HandleRun();
        HandleDodge();
        HandleAnimation();
        HandleGravity();
        HandleJump();
    }

    void LateUpdate()
    {
        HandleCameraRotation();
    }
}