using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeThrower : MonoBehaviour
{

    public float throwForce = 40f;
    public GameObject grenadePrefab;

    private Vector3 grenadeLaunchPos;

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown("g"))
        {
            ThrowGrenade();
        }
    }

    void ThrowGrenade()
    {
        grenadeLaunchPos = new Vector3(transform.position.x, 2.0f, transform.position.z);
        GameObject grenade = Instantiate(grenadePrefab, grenadeLaunchPos, transform.rotation);
        Rigidbody rb = grenade.GetComponent<Rigidbody>();
        rb.AddForce(transform.forward * throwForce, ForceMode.VelocityChange);
    }
}
