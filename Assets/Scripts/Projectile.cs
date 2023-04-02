using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] float projectileDmg = 20f;
    [SerializeField] float projectileRange = 100f;

    [SerializeField] float projectileSpeed = 100f;

    Vector3 originPosition;
   
    void Start()
    {
        //Set origin position
        originPosition = transform.position;

        //Set velocity
        Rigidbody projectileRb = GetComponent<Rigidbody>();
        projectileRb.velocity = projectileSpeed * transform.forward;
    }

    void Update()
    {
        float distance = Vector3.Distance(originPosition, transform.position);
         if(distance > projectileRange)
            {
                Destroy(gameObject);
            }
              
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<EnemyAI>() != null)
        {
            Destroy(gameObject);
            other.GetComponent<EnemyAI>().TakeDamage(projectileDmg);
        }
        else if(other.GetComponent<EnemyAI>() == null)
        {
            Destroy(gameObject);
        }
    }
}
