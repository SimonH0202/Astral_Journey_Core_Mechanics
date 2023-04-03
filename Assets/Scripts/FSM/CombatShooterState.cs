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

    //Private Shooter varibles
    GameObject projectile;
    bool isAttacking = false;

    //Animation hashes
    int isAimingHash;

    public float elapsedTime = 0.0f;

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
    }

    public override void UpdateState(CombatStateManager manager)
    {
        Aim(manager);
    }

    private void Aim(CombatStateManager manager)
    {
        if (playerInput.grenadeAim)
        {
            movementController.SetRotateOnMove(false);
            movementController.SetStrafing(true);

            //Charge up damage
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / manager.chargeUpTime);
            manager.currentDamage = (int)Mathf.Lerp(5, manager.targetDamage, t);

            //set color of crosshair to red
            if(manager.currentDamage == manager.targetDamage)
            {
                manager.crosshair.GetComponent<RawImage>().color = Color.green;
            }
            else
            {
                manager.crosshair.GetComponent<RawImage>().color = Color.white;
            }

            //Set up line renderer
            Vector3 mouseWorldPosition = Vector3.zero;
            Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
            Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
            if(Physics.Raycast(ray, out RaycastHit raycastHit, 999f))
            {
                mouseWorldPosition = raycastHit.point;
            }

            //Set weight of aim rig
            manager.aimRig.weight = Mathf.Lerp(manager.aimRig.weight, 1f, Time.deltaTime * 10f);

            //Aim arm towards mouseWorldPosition
            movementController.SetSensitiviy(manager.aimSenitivity);  
            manager.armAimPoint.position = mouseWorldPosition;

            //Activate aim camera
            manager.aimVirtualCamera.gameObject.SetActive(true);

            //Activate crosshair
            manager.crosshair.gameObject.SetActive(true);

            
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
        if(!playerInput.grenadeAim)
        {
            //Reset movement, projectile, crosshair, camera, line renderer, elapsed time and current damage
            movementController.SetRotateOnMove(true);
            movementController.SetStrafing(false);
            manager.aimVirtualCamera.gameObject.SetActive(false);
            manager.crosshair.gameObject.SetActive(false);
            elapsedTime = 0.0f;
            manager.currentDamage = 5f;

            //Set animation layer weight
            animator.SetLayerWeight(3, Mathf.Lerp(animator.GetLayerWeight(3), 0f, Time.deltaTime * 10f));

            //Set weight of aim rig
            manager.aimRig.weight = Mathf.Lerp(manager.aimRig.weight, 0f, Time.deltaTime * 10f);
        }
    }

    private void Fire(CombatStateManager manager, Vector3 mouseWorldPosition)
    {
        if (playerInput.attack && playerInput.grenadeAim && !isAttacking)
        {   
            //Set attack bool
            isAttacking = true;

            //Instantiate projectile
            Vector3 aimDir = (mouseWorldPosition - aimPoint.position).normalized;
            projectile = GameObject.Instantiate(manager.shooterSettings.projectilePrefab, aimPoint.transform.position, Quaternion.LookRotation(aimDir, Vector3.up)); 

            //Set projectile damage
            projectile.GetComponent<Projectile>().damage = manager.currentDamage;
            Debug.Log("Damage: " + manager.currentDamage);

            //Reset damage and timer
            manager.currentDamage = 5f;
            elapsedTime = 0.0f;

            //Set velocity
            Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();
            projectileRb.velocity = manager.shooterSettings.projectileSpeed * manager.transform.forward;

            playerInput.attack = false;
                 
            //Reset attack bool after delay
            manager.StartCoroutine(ShootDelay(manager));
        } 
    }

    private IEnumerator ShootDelay(CombatStateManager manager)
    {
        yield return new WaitForSeconds(manager.shooterSettings.shootingCooldown);
        isAttacking = false;
    }

    public override void ExitState(CombatStateManager manager)
    {
    }
}

