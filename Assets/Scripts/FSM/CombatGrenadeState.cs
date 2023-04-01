using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatGrenadeState : CombatBaseState
{
    //References 
    PlayerInputs playerInput;
    Animator animator;
    MovementController movementController;

    //Private Grenade varibles
    GameObject grenade;
    bool isAttacking = false;
    bool maxDistanceReached = false;

    //Animation hashes
    int isAimingHash;

    public override void EnterState(CombatStateManager manager)
    {
        //Log state change
        Debug.Log("Entered Grenade State");

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
            //Set up line renderer
            movementController.SetRotateOnMove(false);
            movementController.SetStrafing(true);
            Vector3 mouseWorldPosition = Vector3.zero;
            Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
            Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
            if(Physics.Raycast(ray, out RaycastHit raycastHit, 999f))
            {
                mouseWorldPosition = raycastHit.point;
            }

            //Activate aim camera
            manager.aimVirtualCamera.gameObject.SetActive(true);

            //Rotate player to face mouse
            Vector3 worldAimTarget = mouseWorldPosition;
            worldAimTarget.y = manager.transform.position.y;
            Vector3 aimDirection = (worldAimTarget - manager.transform.position).normalized;

            manager.transform.forward = Vector3.Lerp(manager.transform.forward, aimDirection, Time.deltaTime * 20f);

            //Set animation layer weight
            animator.SetLayerWeight(3, Mathf.Lerp(animator.GetLayerWeight(3), 1f, Time.deltaTime * 10f));

            DrawProjection(manager);

            if (playerInput.attack && playerInput.grenadeAim && !isAttacking && !maxDistanceReached)
                {   
                    //Set attack bool
                    isAttacking = true;

                    //Instantiate grenade
                    grenade = GameObject.Instantiate(manager.grenadePrefab, manager.LineRenderer.transform.position, manager.transform.rotation);

                    //Set velocity
                    Rigidbody grenadeRb = grenade.GetComponent<Rigidbody>();
                    grenadeRb.velocity = manager.throwForce * Camera.main.transform.forward;

                    playerInput.attack = false;

                    //Reset attack bool after delay
                    manager.StartCoroutine(ThrowGrenadeDelay(manager));
                } 

                
        }   
        if(!playerInput.grenadeAim)
        {
            //Reset movement, grenade, camera and line renderer
            movementController.SetRotateOnMove(true);
            movementController.SetStrafing(false);
            manager.LineRenderer.enabled = false;
            manager.aimVirtualCamera.gameObject.SetActive(false);

            //Set animation layer weight
            animator.SetLayerWeight(3, Mathf.Lerp(animator.GetLayerWeight(3), 0f, Time.deltaTime * 10f));
        }

    }

    private void DrawProjection(CombatStateManager manager)
    {
        manager.LineRenderer.enabled = true;
        manager.LineRenderer.positionCount = Mathf.CeilToInt(manager.linePoints / manager.timeBetweenPoints) + 1;
        Vector3 startPosition = manager.LineRenderer.transform.position;
        Vector3 startVelocity = manager.throwForce * Camera.main.transform.forward;
        int i = 0;
        manager.LineRenderer.SetPosition(i, startPosition);
        for (float time = 0; time < manager.linePoints; time += manager.timeBetweenPoints)
        {
            i++;
            Vector3 point = startPosition + time * startVelocity;
            point.y = startPosition.y + startVelocity.y * time + (Physics.gravity.y / 2f * time * time);

            manager.LineRenderer.SetPosition(i, point);

            //Max distance reached, change color of line renderer
            if (Vector3.Distance(startPosition, point) > manager.maxDistance)
            {
                manager.LineRenderer.material.SetColor("_EmissionColor", Color.red);
                maxDistanceReached = true;
            }
            else
            {
                manager.LineRenderer.material.SetColor("_EmissionColor", Color.white);
                maxDistanceReached = false;
            }
            

            Vector3 lastPosition = manager.LineRenderer.GetPosition(i - 1);

            if (Physics.Raycast(lastPosition, 
                (point - lastPosition).normalized, 
                out RaycastHit hit,
                (point - lastPosition).magnitude,
                manager.GrenadeCollisionMask))
            {
                manager.LineRenderer.SetPosition(i, hit.point);
                manager.LineRenderer.positionCount = i + 1;
                return;
            }
        }
    }

    private IEnumerator ThrowGrenadeDelay(CombatStateManager manager)
    {
        yield return new WaitForSeconds(manager.grenadeCooldown);
        isAttacking = false;
    }

    public override void ExitState(CombatStateManager manager)
    {
        manager.LineRenderer.enabled = false;
    }
}
