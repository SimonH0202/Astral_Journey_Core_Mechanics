using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatShooterState : CombatBaseState
{
    //References 
    PlayerInputs playerInput;
    Animator animator;
    MovementController movementController;

    //Private Shooter varibles
    GameObject projectile;
    bool isAttacking = false;

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
    }

    public override void UpdateState(CombatStateManager manager)
    {
        if (playerInput.grenadeAim)
        {
            movementController.SetRotateOnMove(false);
            movementController.SetStrafing(true);

            //Set up line renderer
            Vector3 mouseWorldPosition = Vector3.zero;
            Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
            Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
            if(Physics.Raycast(ray, out RaycastHit raycastHit, 999f))
            {
                mouseWorldPosition = raycastHit.point;
            }

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


            if (playerInput.attack && playerInput.grenadeAim && !isAttacking)
                {   
                    //Set attack bool
                    isAttacking = true;

                    //Instantiate projectile
                    Vector3 aimDir = (mouseWorldPosition - manager.LineRenderer.transform.position).normalized;
                    projectile = GameObject.Instantiate(manager.projectilePrefab, manager.LineRenderer.transform.position, Quaternion.LookRotation(aimDir, Vector3.up)); 

                    //Set velocity
                    Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();
                    projectileRb.velocity = manager.projectileSpeed * Camera.main.transform.forward;

                    playerInput.attack = false;
                 
                    //Reset attack bool after delay
                    manager.StartCoroutine(ShootDelay(manager));
                } 
        }   
        if(!playerInput.grenadeAim)
        {
            //Reset movement, projectile,crosshair, camera and line renderer
            movementController.SetRotateOnMove(true);
            movementController.SetStrafing(false);
            manager.LineRenderer.enabled = false;
            manager.aimVirtualCamera.gameObject.SetActive(false);
            manager.crosshair.gameObject.SetActive(false);

            //Set animation layer weight
            animator.SetLayerWeight(3, Mathf.Lerp(animator.GetLayerWeight(3), 0f, Time.deltaTime * 10f));
        }
    }

    private IEnumerator ShootDelay(CombatStateManager manager)
    {
        yield return new WaitForSeconds(manager.shootingCooldown);
        isAttacking = false;
    }

    public override void ExitState(CombatStateManager manager)
    {
    }
}

