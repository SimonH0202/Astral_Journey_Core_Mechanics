using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    float projectileDmg = 20f;
   
    void Start()
    {
        gameObject.transform.Rotate(90, 0, 0);
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<EnemyAI>() != null)
        {
            Destroy(gameObject);
            other.GetComponent<EnemyAI>().TakeDamage(projectileDmg);
        }
    }
}
