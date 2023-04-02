using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    float projectileDmg = 20f;
    float projectileRange = 100f;

    Vector3 originPosition;
   
    void Start()
    {
        gameObject.transform.Rotate(90, 0, 0);
        originPosition = transform.position;
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
