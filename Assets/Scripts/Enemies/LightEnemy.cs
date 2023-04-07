using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightEnemy : BasicEnemy
{
    [Space(10)]
    [Header("Light Enemy Settings")]
    [Space(10)]
    [SerializeField] private Transform[] chaseSpots;
    [SerializeField] private Transform weaponPoint;
    [SerializeField] private float weaponLength = 1.4f;
    [SerializeField] private float attackRange = 2f;

    private List<GameObject> damagedPlayers = new List<GameObject>();

    protected override void Update()
    {
        base.Update();
        if (!dead)
        {
            HandleWalkAnimation();
            AttackIfHit();
            HandleDamageRaycast();
        }
    }
    
    public override void PatrolAndAttack()
    {
        base.PatrolAndAttack();
        //Check if player is still in reach
        if(!patrolling)
        {
            //Player is in attack range
            if(Vector3.Distance(this.transform.position, player.transform.position) <= attackRange)
            {
                if (!attacking)
                {
                    attacking = true;
                    agent.isStopped = true;
                    StartCoroutine(AttackCorountine());
                }    
            }
        }
    }

    public override void Die()
    {
        base.Die();

        //Set dead animation
        animator.SetBool(isDeadHash, true);
        animator.SetBool(isWalkingHash, false);
        animator.SetBool(isAttackingHash, false);

        //Delete enemy after 10 seconds
        Destroy(gameObject, 10f);
    }

      //Attack coroutine
    IEnumerator AttackCorountine()
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

    public override void FollowPlayer()
    {
        base.FollowPlayer();
        int k = 0;
        agent.speed = 7.5f;
        agent.destination = chaseSpots[k].position;
        k++;
        // out of bounds error handling, spare enemys share first spot
        if(k >= chaseSpots.Length)
        {
            k = 0;
        }    
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
