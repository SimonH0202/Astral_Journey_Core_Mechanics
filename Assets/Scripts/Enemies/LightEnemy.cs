using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightEnemy : EnemyAI
{
    [Space(10)]
    [Header("Light Enemy Settings")]
    [Space(10)]
    [SerializeField] private Transform[] chaseSpots;
    [SerializeField] private float weaponLength = 1.4f;
    [SerializeField] private Transform weaponPoint;
    [SerializeField] private LayerMask playerLayers;

    //Private variables
    private List<GameObject> damagedPlayers = new List<GameObject>();

  
    private void Update()
    {
        if (!dead)
        {
            PatrolAndAttack();
            HandleWalkAnimation();
            AttackIfHit();
        }
    }

    public override void PatrolAndAttack()
    {
        if(patrolling){
            //Player is in sight
             if(Vector3.Distance(this.transform.position, player.transform.position) <= 10f)
            {
                FollowPlayer();
            }
            //Player is out of sight
            if(Vector3.Distance(this.transform.position, player.transform.position) > 10f)
            {   
                agent.speed = 2.5f;
                agent.destination = nextPoint;          
            }
            //Go to next waypoint
            if(Vector3.Distance(this.transform.position, nextPoint) <= 5f)
            {
                nextPoint = RandomPoint();
                agent.destination = nextPoint; 
            }    
        }
        //Check if player is still in reach
        if(!patrolling)
        {
            if(CheckDistance()) FollowPlayer();
            else patrolling = true;

            //Player is in attack range
            if(Vector3.Distance(this.transform.position, player.transform.position) <= attackRange)
            {
                if (!attacking)
                {
                    attacking = true;
                    agent.isStopped = true;
                    StartCoroutine(Attack());
                }
                HandleDamageRaycast();
            }
        } 
    }

    public override void Die()
    {
        //Remove enemy from list
        fellowAI.Remove(this);

        for(int i = 0; i < fellowAI.Count; i++)
        {
            LightEnemy lightEnemy = fellowAI[i] as LightEnemy;
            //Set all fellow AI to follow player on death
            lightEnemy.FollowPlayer();

            //Remove this enemy from fellow AI list
            lightEnemy.fellowAI.Remove(this);
        }

        //Disable enemy
        dead = true;
        Destroy(GetComponent<Collider>());

        //Disable navmesh agent movement
        agent.SetDestination(transform.position);

        //Disable health bar
        healthBar.gameObject.SetActive(false);

        //Set dead animation
        animator.SetBool(isDeadHash, true);
        animator.SetBool(isWalkingHash, false);
        animator.SetBool(isAttackingHash, false);

        //Delete enemy after 10 seconds
        Destroy(gameObject, 10f);
    }

      //Attack coroutine
    IEnumerator Attack()
    {
        //Play attack animation
        animator.SetBool(isAttackingHash, true);

        //Wait for animation to finish
        yield return new WaitForSeconds(1f);
        animator.SetBool(isAttackingHash, false);

        //Wait for a bit and reset attacking
        yield return new WaitForSeconds(Random.Range(0.5f, 1f));
        attacking = false;
        agent.isStopped = false;
        damagedPlayers.Clear();
    }

    void HandleWalkAnimation()
    {
        //Handle walk animation and rotation
        if (agent.velocity.magnitude > 0.5f)
        {
            animator.SetBool(isWalkingHash, true);
            if (patrolling) transform.rotation = Quaternion.LookRotation(agent.velocity.normalized);
            else transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(player.transform.position - transform.position), 5f * Time.deltaTime);
        }
        else
        {
            animator.SetBool(isWalkingHash, false);
            if (!patrolling) transform.LookAt(new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z));
        }       
    }

    public override void FollowPlayer()
    {
        int k = 0;
        for(int i = 0; i < fellowAI.Count; i++)
        {
            LightEnemy lightEnemy = fellowAI[i] as LightEnemy;
            lightEnemy.patrolling = false;
            lightEnemy.agent.speed = 7.5f;
            lightEnemy.agent.destination = chaseSpots[k].position;
            k++;
            // out of bounds error handling, spare enemys share first spot
            if(k >= chaseSpots.Length)
            {
                k = 0;
            }
        }
    }

    void HandleDamageRaycast()
    {
        if (attacking)
        {
            RaycastHit hit;

            //Check for enemies hit by raycast
            if (Physics.Raycast(weaponPoint.position, -weaponPoint.transform.up, out hit, weaponLength, playerLayers))
            {
                if (!damagedPlayers.Contains(hit.transform.gameObject))
                {
                    //Damage enemy
                    damagedPlayers.Add(hit.transform.gameObject);
                    hit.transform.GetComponent<PlayerStatsSystem>().TakeDamage(attackDamage);
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(weaponPoint.position, weaponPoint.position + weaponLength * -weaponPoint.transform.up);
    }

}
