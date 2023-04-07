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
    bool rotateOnMove = true;
    bool isStrafing = false;
    float turnSmoothVelocity;
    float targetSpeed;

    //Private Jump variables
    bool isJumpPressed = false;
    bool isJumping = false;
    bool isJumpAnimating = false;
    float initialJumpVelocity;

    //Private Dodge variables
    bool isDodging = false;
    bool canDodge = true;

    //Private Cinemachine variables
    private float cinemachineTargetYaw;
    private float cinemachineTargetPitch;
    private float sensitivity = 1.0f;
    private bool isCurrentDeviceMouse;

    [Header("Jump Settings")]
    //Public Jump variables
    [SerializeField] private float maxJumpHeight= 2f;

    [SerializeField] private float maxJumpTime = 0.5f;

    [Header("Cinemachine")]
    [SerializeField] private GameObject cinemachineCameraTarget;  
    [SerializeField] private float topClamp = 70.0f;
    [SerializeField] private float bottomClamp = -30.0f;
    [SerializeField] private float cameraAngleOverride = 0.0f;
    [SerializeField] private bool lockCameraPosition = false;

    [Header("Movement Settings")]
    //Public Movement variables
    [SerializeField] private float movementSpeed = 2.5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float strafeSpeed = 4f;
    [SerializeField] private float rotationFactorPerFrame = 15.0f;

    [Header("Dodge Settings")]
    //Public Dodge variables
    [SerializeField] private float dodgeDistance = 10f;
    [SerializeField] private float dodgeTimer = 0.5f;

    [Space]
    [Header("Physics Settings")]
    //public Physics variables
    [SerializeField] private float gravity = -9.81f;

    //Animation Hashes
    int isWalkingHash;
    int isRunningHash;
    int isJumpingHash;
    int isDodgingHash;
    int isHorizontalMovementHash;
    int isVerticalMovementHash;


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
        isHorizontalMovementHash = Animator.StringToHash("horizontalMovement");
        isVerticalMovementHash = Animator.StringToHash("verticalMovement");

        //Setup Jump Variables
        SetupJumpVariables();
    }

    void Start()
    {
        input = GetComponent<PlayerInput>();

        //Set initial cinemachine values
        cinemachineTargetYaw = cinemachineCameraTarget.transform.rotation.eulerAngles.y;

        //Set initial values
        targetSpeed = movementSpeed;
    }

    void Update()
    {
        //Check for mouse and keyboard or controller
        isCurrentDeviceMouse = input.currentControlScheme == "MouseKeyboard";

        HandleRotation();
        HandleStrafeMovement();
        HandleRun();
        HandleDodge();
        HandleAnimation();
        
        //Move CharacterController
        if (!isMovementLocked) characterController.Move(new Vector3(moveDirection.x * targetSpeed, moveDirection.y, moveDirection.z * targetSpeed) * Time.deltaTime);

        HandleGravity();
        HandleJump();
    }

    void LateUpdate()
    {
        HandleCameraRotation();
    }

    //Handle Methods
    private void HandleCameraRotation()
    {
        //Handle Camera Rotation
        if (playerInput.look.sqrMagnitude >= 0.01f && !lockCameraPosition)
        {
            //Calculate our rotation multiplier based on if we are using a mouse or controller
            float deltaTimeMultiplier = isCurrentDeviceMouse ? 1f : Time.deltaTime;

            cinemachineTargetYaw += playerInput.look.x * deltaTimeMultiplier * sensitivity;
            cinemachineTargetPitch += playerInput.look.y * deltaTimeMultiplier * sensitivity;
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
        if (playerInput.sprint && !isStrafing)
        {
            targetSpeed = runSpeed;
        }
        else targetSpeed = movementSpeed;
    }

    void HandleJump()
    {
        //Handle Jumping and Animation
        if (!isJumping && characterController.isGrounded && playerInput.jump && !isStrafing && !isMovementLocked) {
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
        if (playerInput.dodge && !isJumping && !isDodging && !isMovementLocked && !isStrafing && canDodge)
        {
            //If not sufficient energy, return
            if (playerStatsSystem.Energy < 10)
            {
                playerInput.dodge = false;
                return;
            }

            isDodging = true;
            StartCoroutine(Dodge());
            playerStatsSystem.TakeEnergy(10);
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

        if (playerInput.move.magnitude >= 0.1f) {
            //Calculate and set rotation with camera and movement
            Quaternion targetRotation = Quaternion.Euler(0, Mathf.Atan2(movement.x, movement.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y, 0);
            if(rotateOnMove && !isMovementLocked && !isStrafing)
            {
            transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, rotationFactorPerFrame * Time.deltaTime);
            }
            //Set movement direction to where the player is facing
            moveDirection.x = transform.forward.x;
            moveDirection.z = transform.forward.z;
        } else {
            //Set movement direction to 0 if no movement input
            moveDirection.x = 0;
            moveDirection.z = 0;
        }
    }

    void HandleStrafeMovement()
    {
        if (isStrafing)
        {
            //Set animation layer weight to 1
			animator.SetLayerWeight(1, 1);

			Vector3 dir = CalculateInputVector();
            moveDirection.x = dir.x;
            moveDirection.z = dir.z;

            //Normalize movement input
            Vector2 strafeInput = new Vector2(playerInput.move.x, playerInput.move.y).normalized;


            //Set animation values
			animator.SetFloat(isHorizontalMovementHash, strafeInput.x, 0.1f, Time.deltaTime);
			animator.SetFloat(isVerticalMovementHash, strafeInput.y, 0.1f, Time.deltaTime);

            //Set strafe speed
            targetSpeed = strafeSpeed;
        }
        else
        {
            //Set animation layer weight to 0
            animator.SetLayerWeight(1, 0);

            //Reset animation values
            animator.SetFloat(isHorizontalMovementHash, 0);
            animator.SetFloat(isVerticalMovementHash, 0);

            //Set movement speed to normal
            targetSpeed = movementSpeed;
        }    
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
        if (playerInput.move.magnitude >= 0.1f && !isWalking) animator.SetBool(isWalkingHash, true);
        else if (!(playerInput.move.magnitude >= 0.1f) && isWalking) animator.SetBool(isWalkingHash, false);

        //Handle Running
        if (playerInput.move.magnitude >= 0.1f && playerInput.sprint && !isRunning) animator.SetBool(isRunningHash, true);
        else if (!(playerInput.move.magnitude >= 0.1f) || !playerInput.sprint) animator.SetBool(isRunningHash, false);
    }

    //Coroutine for dodging
    IEnumerator Dodge()
    {
        //Disable movement
        DisableMovement();

        //Set invulnerability
        playerStatsSystem.IsVunerable = false;

        //Set dodge animation to true
        animator.SetBool(isDodgingHash, true);

        float timer = 0f;

        Vector3 dodgeDirection = CalculateInputVector() + (Vector3.up * moveDirection.y);

        //Set rotation to dodge direction
        transform.rotation= Quaternion.Euler(0, Mathf.Atan2(playerInput.move.x, playerInput.move.y) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y, 0);

        while (timer < dodgeTimer)
        {
            characterController.Move(dodgeDirection * dodgeDistance * Time.deltaTime);
            timer += Time.deltaTime;

            //After 0.05 seconds, set vulnerable
            if (timer > 0.05f) playerStatsSystem.IsVunerable = true;

            yield return null;
        }
        //Set dodge animation to false and enable movement
        animator.SetBool(isDodgingHash, false);
        isDodging = false;
        EnableMovement();

        //Reset input
        playerInput.dodge = false;
    }

    //General Methods
    void SetupJumpVariables()
    {
        //Calculate jump variables based on jump height and time
        float timeToApex = maxJumpTime / 2;
        gravity = (-2 * maxJumpHeight) / Mathf.Pow(timeToApex, 2);
        initialJumpVelocity = (2 * maxJumpHeight) / timeToApex;
    }

    public void DisableMovement()
    {
        //Set movement to 0 and disable movement
        moveDirection.x = 0;
        moveDirection.z = 0;

        //Set movementInput to 0
        movementInput = Vector2.zero;

        isMovementLocked = true;
    }

    public void EnableMovement()
    {
        //Enable movement
        isMovementLocked = false;
        HandleRotation();
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    private Vector3 CalculateInputVector()
    {
        //Get camera forward and right vectors
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        //Set movement direction to where the player is facing
        Vector3 dir = (forward * playerInput.move.y + right * playerInput.move.x).normalized;
        return dir;
    }

    //Setter
    public bool RotateOnMove { set => rotateOnMove = value; }
    public bool IsMovementLocked { set => isMovementLocked = value; }
    public float Sensitivity { set => sensitivity = value; }
    public bool IsStrafing { set => isStrafing = value; }
    public float StrafeSpeed { set => strafeSpeed = value; }
    public bool CanDodge { set => canDodge = value; }

    //Getter
    public bool IsJumping { get => isJumping; }
    public bool IsDodging { get => isDodging; }
    public bool IsGrounded { get => characterController.isGrounded; }
}