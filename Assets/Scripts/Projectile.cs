using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float damage;
    [SerializeField] float range = 100f;
    [SerializeField] float speed = 100f;

    Vector3 originPosition;
   
    void Start()
    {
        //Set origin position
        originPosition = transform.position;

        //Set velocity
        Rigidbody projectileRb = GetComponent<Rigidbody>();
        projectileRb.velocity = speed * transform.forward;
    }

    void Update()
    {
        float distance = Vector3.Distance(originPosition, transform.position);
         if(distance > range)
            {
                Destroy(gameObject);
            }
              
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<BasicEnemy>() != null)
        {
            Destroy(gameObject);
            other.GetComponent<BasicEnemy>().TakeDamage(damage);
        }
        else if(other.GetComponent<BasicEnemy>() == null)
        {
            Destroy(gameObject);
        }
    }
}
