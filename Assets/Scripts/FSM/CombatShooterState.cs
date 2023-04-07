using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CombatShooterState : CombatBaseState
{
    //References 
    PlayerInputs playerInput;
    Animator animator;
    MovementController movementController;
    Transform aimPoint;
    Transform cameraTransform;

    //Private Shooter varibles
    GameObject projectile;
    bool isAttacking = false;
    float currentDamage;
    float elapsedTime = 0f;
    float animResetTimer = 0f;

    //Animation hashes
    int isAimingHash;



    public override void EnterState(CombatStateManager manager)
    {
        //Log state change
        Debug.Log("Entered Shooter State");

        //Get animation hashes
        isAimingHash = Animator.StringToHash("isAiming");

        //Get references
        playerInput = manager.GetComponent<PlayerInputs>();
        movementController = manager.GetComponent<MovementController>();
        animator = manager.GetComponent<Animator>();
        aimPoint = manager.transform.Find("ReleasePosition");
        cameraTransform = Camera.main.transform;

        //Set initial values
        currentDamage = manager.ShooterSettings.StartDamage;
    }

    public override void UpdateState(CombatStateManager manager)
    {
        Aim(manager);
        HipFire(manager);
    }

    private void Aim(CombatStateManager manager)
    {
        if (playerInput.aim)
        {
            movementController.RotateOnMove = false;
            movementController.IsStrafing = true;

            //Charge up damage
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / manager.ShooterSettings.ChargeUpTime);
            currentDamage = (int)Mathf.Lerp(5, manager.ShooterSettings.TargetDamage, t);

            //lerp color of crosshair
            Color lerpedColor = Color.Lerp(Color.white, Color.green, t);
            manager.Crosshair.GetComponent<RawImage>().color = lerpedColor;

            //Set up line renderer
            Vector3 mouseWorldPosition = Vector3.zero;
            Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
            Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
            if(Physics.Raycast(ray, out RaycastHit raycastHit, 999f))
            {
                mouseWorldPosition = raycastHit.point;
            }

            //Set weight of aim rig
            manager.AimRig.weight = Mathf.Lerp(manager.AimRig.weight, 1f, Time.deltaTime * 10f);

            //Aim arm towards mouseWorldPosition
            movementController.Sensitivity = manager.AimSenitivity;  
            manager.ArmAimPoint.position = mouseWorldPosition;

            //Activate aim camera
            manager.AimVirtualCamera.gameObject.SetActive(true);

            //Activate crosshair
            manager.Crosshair.gameObject.SetActive(true);

            
            //Rotate player to face mouse
            Vector3 worldAimTarget = mouseWorldPosition;
            worldAimTarget.y = manager.transform.position.y;
            Vector3 aimDirection = (worldAimTarget - manager.transform.position).normalized;

            manager.transform.forward = Vector3.Lerp(manager.transform.forward, aimDirection, Time.deltaTime * 20f);

            //Set animation layer weight
            animator.SetLayerWeight(3, Mathf.Lerp(animator.GetLayerWeight(3), 1f, Time.deltaTime * 10f));

            //Fire projectile
            Fire(manager, mouseWorldPosition);
        }   
        if(!playerInput.aim)
        {
            //Reset, projectile, crosshair, camera, line renderer, elapsed time and current damage

            manager.AimVirtualCamera.gameObject.SetActive(false);
            manager.Crosshair.gameObject.SetActive(false);
            elapsedTime = 0.0f;
            currentDamage = manager.ShooterSettings.StartDamage;

            //Reset animation layer weight and aim rig weight
            animator.SetLayerWeight(3, Mathf.Lerp(animator.GetLayerWeight(3), 0f, Time.deltaTime * 10f));
            manager.AimRig.weight = Mathf.Lerp(manager.AimRig.weight, 0f, Time.deltaTime * 10f);

            //Reset movement
            movementController.RotateOnMove = true;
            movementController.IsStrafing = false;
        }
    }

    private void Fire(CombatStateManager manager, Vector3 mouseWorldPosition)
    {
        if (playerInput.attack && playerInput.aim && !isAttacking)
        {   
            //Set attack bool
            isAttacking = true;

            //Instantiate projectile
            Vector3 aimDir = (mouseWorldPosition - aimPoint.position).normalized;
            projectile = GameObject.Instantiate(manager.ShooterSettings.ProjectilePrefab, aimPoint.transform.position, Quaternion.LookRotation(aimDir, Vector3.up)); 

            //Set projectile damage
            projectile.GetComponent<Projectile>().damage = currentDamage;

            //Reset damage and timer
            currentDamage = manager.ShooterSettings.StartDamage;
            elapsedTime = 0.0f;

            //Set velocity
            Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();
            projectileRb.velocity = manager.ShooterSettings.ProjectileSpeed * manager.transform.forward;

            playerInput.attack = false;
                 
            //Reset attack bool after delay
            manager.StartCoroutine(ShootDelay(manager.ShooterSettings.ShootingCooldown));
        } 
    }

    private void HipFire(CombatStateManager manager)
    {
        //Set target
        Transform target = SetTarget(manager);

        //If target.postion is not manager.Armaimpoint.position, lerp to target
        if (target != null && target.position != manager.ArmAimPoint.position)
        {
            manager.ArmAimPoint.position = Vector3.Lerp(manager.ArmAimPoint.position, target.position, Time.deltaTime * 10f);
        }

        //Lerp rig and animation layer weight to 1 if is attacking
        if (isAttacking)
        {
            manager.HipFireRig.weight = Mathf.Lerp(manager.HipFireRig.weight, 1f, Time.deltaTime * 10f);
            animator.SetLayerWeight(4, Mathf.Lerp(animator.GetLayerWeight(4), 1f, Time.deltaTime * 10f));
        }


        if (playerInput.attack && !playerInput.aim && !isAttacking && target != null)
        {
            //If not sufficient energy, return
            if (manager.PlayerStatsSystem.Energy < manager.ShooterSettings.HipFireEnergyDrain)
            {
                playerInput.attack = false;
                return;
            }

            //Drain Energy
            manager.PlayerStatsSystem.TakeEnergy(manager.ShooterSettings.HipFireEnergyDrain);

            //Set bools
            isAttacking = true;
            movementController.CanDodge = false;

            //Damage target if it has an enemyAI component
            if (target.TryGetComponent(out BasicEnemy enemy)) enemy.TakeDamage(manager.ShooterSettings.HipFireDamage);    

            playerInput.attack = false;

            //Reset timer and attack bool after delay
            animResetTimer = 0f;
            manager.StartCoroutine(ShootDelay(manager.ShooterSettings.HipFireCooldown));
        }

        animResetTimer += Time.deltaTime;

        //After 1 seconds, reset animation layer weight and hipfire rig weight if not attacking
        if (animResetTimer >= 1f && !isAttacking)
        {
            manager.HipFireRig.weight = Mathf.Lerp(manager.HipFireRig.weight, 0f, Time.deltaTime * 20f);
            animator.SetLayerWeight(4, Mathf.Lerp(animator.GetLayerWeight(4), 0f, Time.deltaTime * 20f));

            //Reset bool
            movementController.CanDodge = true;
        }
    }

    private Transform SetTarget(CombatStateManager manager)
    {
        
        var forward = cameraTransform.forward;
        var right = cameraTransform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 inputDirection = forward + right;
        inputDirection = inputDirection.normalized;

        RaycastHit info;

        if (Physics.SphereCast(manager.transform.position, 3f, inputDirection, out info, 15, manager.MeleeSettings.EnemyLayers)) return info.collider.transform;
        else return null;
    }

    private IEnumerator ShootDelay(float delay = 0.5f) 
    {
        yield return new WaitForSeconds(delay);
        isAttacking = false;
    }

    public override void ExitState(CombatStateManager manager)
    {
        //Reset movement, crosshair, camera, elapsed time and current damage
        movementController.RotateOnMove = true;
        movementController.IsStrafing = false;
        manager.AimVirtualCamera.gameObject.SetActive(false);
        manager.Crosshair.gameObject.SetActive(false);
        elapsedTime = 0.0f;
        currentDamage = manager.ShooterSettings.StartDamage;

        //Set animation layer weight
        animator.SetLayerWeight(3, 0f);
        animator.SetLayerWeight(4, 0f);

        //Set weight of aim rig and hipfire rig to 0
        manager.AimRig.weight = 0;
        manager.HipFireRig.weight = 0;
    }
}

