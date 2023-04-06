using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MothEnemy : EnemyAI
{
    public override void Die()
    {
        base.Die();

        //Delete enemy
        Destroy(gameObject);
    }

    public override void FollowPlayer()
    {
        base.FollowPlayer();
        agent.speed = 15f;
        agent.destination = player.transform.position;
    }   


    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            player.GetComponent<PlayerStatsSystem>().TakeDamage(attackDamage);
            Die();
        }
    }
}
