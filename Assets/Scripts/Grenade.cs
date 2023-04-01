using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{

    public float radius = 5f;

    public GameObject explosionEffect;

    float grenadeDmg = 20f;
    bool isCollided;
    bool hasExploded = false;

    List<GameObject> damagedEnemies = new List<GameObject>();
    public LayerMask enemyLayers;


    void Update()
    {
        if(isCollided && !hasExploded)
        {
            Destroy(gameObject);
            hasExploded = true;
            Explode();
        }
    }

    void Explode()
    {
        Instantiate(explosionEffect, transform.position, transform.rotation);
   
        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, radius, enemyLayers);

        foreach(Collider enemy in hitEnemies)
        {
             if (!damagedEnemies.Contains(enemy.gameObject) && hasExploded)
                {
                    //Damage enemy
                    damagedEnemies.Add(enemy.gameObject);
                    enemy.GetComponent<EnemyAI>().TakeDamage(grenadeDmg);
                }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag != "Player")
        {
            isCollided = true;
        }
    }
}
