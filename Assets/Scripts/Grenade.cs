using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{

    [SerializeField] float radius = 5f;
    [SerializeField] float grenadeDmg = 20f;

    [Header("Effects")]
    [SerializeField] GameObject explosionEffect;

    bool isCollided;
    bool hasExploded = false;

    List<GameObject> damagedEnemies = new List<GameObject>();
    List<GameObject> damagedPlayers = new List<GameObject>();
    public LayerMask hitLayers;


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
   
        Collider[] hits = Physics.OverlapSphere(transform.position, radius, hitLayers);

        foreach(Collider h in hits)
        {
            if (!damagedEnemies.Contains(h.gameObject) && hasExploded)
            {
                //Damage enemy
                if (h.TryGetComponent(out EnemyAI enemy))
                {
                    damagedEnemies.Add(h.gameObject);
                    enemy.TakeDamage(grenadeDmg);
                }
            }
            if (!damagedPlayers.Contains(h.gameObject) && hasExploded)
            {
                //Damage player
                if (h.TryGetComponent(out PlayerStatsSystem player))
                {
                    damagedPlayers.Add(h.gameObject);
                    player.TakeDamage(grenadeDmg);
                }
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
