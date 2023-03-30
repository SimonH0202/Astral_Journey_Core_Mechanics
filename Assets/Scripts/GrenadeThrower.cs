using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

public class GrenadeThrower : MonoBehaviour
{

    public float throwForce = 40f;
    public GameObject grenadePrefab;
    public Transform player;

    GameObject grenade;
   

    [SerializeField]
    private CinemachineVirtualCamera aimVirtualCamera;


    PlayerInputs playerInput;
    

    [SerializeField]
    private Transform ReleasePosition;

    [SerializeField]
    private LineRenderer LineRenderer;

    [Header("Display Controls")]
    [SerializeField]
    [Range(10, 100)]
    private int LinePoints = 25;
    [SerializeField]
    [Range(0.01f, 0.25f)]
    private float TimeBetweenPoints = 0.1f;

    private LayerMask GrenadeCollisionMask;

    [SerializeField]
    private Transform debugTransform;

    private MovementController movementController;

    
    void Awake()
    {
        playerInput = GetComponent<PlayerInputs>();
        movementController = GetComponent<MovementController>();
    }

    // Update is called once per frame
    void Update()
    {
         if (playerInput.grenadeAim)
        {
            movementController.SetRotateOnMove(false);
            Vector3 mouseWorldPosition = Vector3.zero;
            Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
            Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
            if(Physics.Raycast(ray, out RaycastHit raycastHit, 999f))
            {
                mouseWorldPosition = raycastHit.point;
            }

            aimVirtualCamera.gameObject.SetActive(true);


            Vector3 worldAimTarget = mouseWorldPosition;
            worldAimTarget.y = transform.position.y;
            Vector3 aimDirection = (worldAimTarget - transform.position).normalized;

            transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * 20f);

            DrawProjection();

            if (playerInput.grenadeShoot)
                {   
                    grenade = Instantiate(grenadePrefab, ReleasePosition.position, transform.rotation);
                    grenade.transform.parent = player;
                    Rigidbody grenadeRb = grenade.GetComponent<Rigidbody>();
                    grenadeRb.velocity = throwForce * Camera.main.transform.forward;
                    grenade.transform.parent = null;
                    grenade = null;
                    playerInput.grenadeShoot = false;
                    Destroy(grenade);
                } 

                
        }   
        if(!playerInput.grenadeAim)
        {
            movementController.SetRotateOnMove(true);
            LineRenderer.enabled = false;
            aimVirtualCamera.gameObject.SetActive(false);
            Destroy(grenade);
        }

    }

    private void DrawProjection()
    {
        LineRenderer.enabled = true;
        LineRenderer.positionCount = Mathf.CeilToInt(LinePoints / TimeBetweenPoints) + 1;
        Vector3 startPosition = ReleasePosition.position;
        Vector3 startVelocity = throwForce * Camera.main.transform.forward;
        int i = 0;
        LineRenderer.SetPosition(i, startPosition);
        for (float time = 0; time < LinePoints; time += TimeBetweenPoints)
        {
            i++;
            Vector3 point = startPosition + time * startVelocity;
            point.y = startPosition.y + startVelocity.y * time + (Physics.gravity.y / 2f * time * time);

            LineRenderer.SetPosition(i, point);

            Vector3 lastPosition = LineRenderer.GetPosition(i - 1);

            if (Physics.Raycast(lastPosition, 
                (point - lastPosition).normalized, 
                out RaycastHit hit,
                (point - lastPosition).magnitude,
                GrenadeCollisionMask))
            {
                LineRenderer.SetPosition(i, hit.point);
                LineRenderer.positionCount = i + 1;
                return;
            }
        }
    } 
}
