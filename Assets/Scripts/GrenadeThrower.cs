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

    private Vector3 grenadeLaunchPos;
    GameObject grenade;
   

    [SerializeField]
    private CinemachineVirtualCamera aimVirtualCamera;

 

    // Update is called once per frame
    void Update()
    {
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            grenadeLaunchPos = new Vector3(transform.position.x, 2.0f, transform.position.z);
            grenade = Instantiate(grenadePrefab, grenadeLaunchPos, transform.rotation);
            grenade.transform.parent = player;
        }
        
        if (Mouse.current.rightButton.isPressed)
        {
            aimVirtualCamera.gameObject.SetActive(true);

            if (Keyboard.current.gKey.wasPressedThisFrame && grenade != null)
                {   
                    Rigidbody rb = grenade.GetComponent<Rigidbody>();
                    rb.useGravity = true;
                    rb.AddForce(transform.forward * throwForce, ForceMode.VelocityChange);
                    grenade.transform.parent = null;
                    grenade = null;
                }    

        }
        else
        {
            aimVirtualCamera.gameObject.SetActive(false);
        }
    }

 

    void SimulateTrajectory(Grenade grenadePrefab, Vector3 pos)
    {

    }
}
