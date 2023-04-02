using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatGrenadeState : CombatBaseState
{
    //References 
    PlayerInputs playerInput;
    Animator animator;
    MovementController movementController;
    LineRenderer lineRenderer;

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
        lineRenderer = manager.transform.Find("ReleasePosition").GetComponent<LineRenderer>();
    }

    public override void UpdateState(CombatStateManager manager)
    {
        Aim(manager);
    }

    private void Aim(CombatStateManager manager)
    {
        if (playerInput.grenadeAim)
        {
            //Set up line renderer
            lineRenderer.enabled = true;
            movementController.SetRotateOnMove(false);
            movementController.SetStrafing(true);
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

            //Rotate player to face mouse
            Vector3 worldAimTarget = mouseWorldPosition;
            worldAimTarget.y = manager.transform.position.y;
            Vector3 aimDirection = (worldAimTarget - manager.transform.position).normalized;

            manager.transform.forward = Vector3.Lerp(manager.transform.forward, aimDirection, Time.deltaTime * 20f);

            //Set animation layer weight
            animator.SetLayerWeight(3, Mathf.Lerp(animator.GetLayerWeight(3), 1f, Time.deltaTime * 10f));

            DrawProjection(manager);
            Fire(manager);
       
        }   
        if(!playerInput.grenadeAim)
        {
            //Reset movement, grenade, camera and line renderer
            movementController.SetRotateOnMove(true);
            movementController.SetStrafing(false);
            lineRenderer.enabled = false;
            manager.aimVirtualCamera.gameObject.SetActive(false);

            //Set animation layer weight
            animator.SetLayerWeight(3, Mathf.Lerp(animator.GetLayerWeight(3), 0f, Time.deltaTime * 10f));

            //Set weight of aim rig
            manager.aimRig.weight = Mathf.Lerp(manager.aimRig.weight, 0f, Time.deltaTime * 10f);
        }
    }

    private void Fire(CombatStateManager manager)
    {
        if (playerInput.attack && playerInput.grenadeAim && !isAttacking && !maxDistanceReached)
        {   
            //Set attack bool
            isAttacking = true;

            //Instantiate grenade
            grenade = GameObject.Instantiate(manager.grenadeSettings.grenadePrefab, lineRenderer.transform.position, manager.transform.rotation);

            //Set velocity
            Rigidbody grenadeRb = grenade.GetComponent<Rigidbody>();
            grenadeRb.velocity = manager.grenadeSettings.throwForce * Camera.main.transform.forward;

            playerInput.attack = false;

            //Reset attack bool after delay
            manager.StartCoroutine(ThrowGrenadeDelay(manager));
        }  
    }

    private void DrawProjection(CombatStateManager manager)
    {
        lineRenderer.enabled = true;
        lineRenderer.positionCount = Mathf.CeilToInt(manager.grenadeSettings.linePoints / manager.grenadeSettings.timeBetweenPoints) + 1;
        Vector3 startPosition = lineRenderer.transform.position;
        Vector3 startVelocity = manager.grenadeSettings.throwForce * Camera.main.transform.forward;
        int i = 0;
        lineRenderer.SetPosition(i, startPosition);
        for (float time = 0; time < manager.grenadeSettings.linePoints; time += manager.grenadeSettings.timeBetweenPoints)
        {
            i++;
            Vector3 point = startPosition + time * startVelocity;
            point.y = startPosition.y + startVelocity.y * time + (Physics.gravity.y / 2f * time * time);

            lineRenderer.SetPosition(i, point);

            //Max distance reached, change color of line renderer
            if (Vector3.Distance(startPosition, point) > manager.grenadeSettings.maxDistance)
            {
                lineRenderer.material.SetColor("_EmissionColor", Color.red);
                maxDistanceReached = true;
            }
            else if (isAttacking)
            {
                lineRenderer.material.SetColor("_EmissionColor", Color.yellow);
            }
            else
            {
                lineRenderer.material.SetColor("_EmissionColor", Color.white);
                maxDistanceReached = false;
            }
            

            Vector3 lastPosition = lineRenderer.GetPosition(i - 1);

            if (Physics.Raycast(lastPosition, 
                (point - lastPosition).normalized, 
                out RaycastHit hit,
                (point - lastPosition).magnitude,
                manager.grenadeSettings.GrenadeCollisionMask))
            {
                lineRenderer.SetPosition(i, hit.point);
                lineRenderer.positionCount = i + 1;
                return;
            }
        }
    }

    private IEnumerator ThrowGrenadeDelay(CombatStateManager manager)
    {
        yield return new WaitForSeconds(manager.grenadeSettings.grenadeCooldown);
        isAttacking = false;
    }

    public override void ExitState(CombatStateManager manager)
    {
        lineRenderer.enabled = false;
    }
}
