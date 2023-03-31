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

    public override void EnterState(CombatStateManager manager)
    {
        //Log state change
        Debug.Log("Entered Grenade State");

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
            Vector3 mouseWorldPosition = Vector3.zero;
            Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
            Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
            if(Physics.Raycast(ray, out RaycastHit raycastHit, 999f))
            {
                mouseWorldPosition = raycastHit.point;
            }

            manager.aimVirtualCamera.gameObject.SetActive(true);


            Vector3 worldAimTarget = mouseWorldPosition;
            worldAimTarget.y = manager.transform.position.y;
            Vector3 aimDirection = (worldAimTarget - manager.transform.position).normalized;

            manager.transform.forward = Vector3.Lerp(manager.transform.forward, aimDirection, Time.deltaTime * 20f);

            DrawProjection(manager);

            if (playerInput.attack)
                {   
                    //Instantiate grenade
                    grenade = GameObject.Instantiate(manager.grenadePrefab, manager.LineRenderer.transform.position, manager.transform.rotation);
                    grenade.transform.parent = manager.transform;

                    //Set velocity
                    Rigidbody grenadeRb = grenade.GetComponent<Rigidbody>();
                    grenadeRb.velocity = manager.throwForce * Camera.main.transform.forward;
                    grenade.transform.parent = null;

                    //Destroy grenade
                    grenade = null;
                    playerInput.attack = false;
                    GameObject.Destroy(grenade);
                } 

                
        }   
        if(!playerInput.grenadeAim)
        {
            //Reset movement, grenade, camera and line renderer
            movementController.SetRotateOnMove(true);
            manager.LineRenderer.enabled = false;
            manager.aimVirtualCamera.gameObject.SetActive(false);
            GameObject.Destroy(grenade);
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

    public override void ExitState(CombatStateManager manager)
    {
        manager.LineRenderer.enabled = false;
    }
}
