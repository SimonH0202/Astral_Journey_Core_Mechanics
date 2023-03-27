using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    
    //Editor Enemy Settings
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] public List<EnemyAI> fellowAI;
    [SerializeField] private Transform[] chaseSpots;

    [Header("Enemy Stats")]
    //Enemy stats
    [SerializeField] private float health = 100f;

    [Header("Enemy Attack Settings")]
    //Enemy attack settings
    
    //Private patrolling variables
    private int currentPoint;
    private bool patrolling;
    private bool attacking;

    //Private enemy variables
    private bool dead = false;

    //References
    private MovementController playerController;
    private NavMeshAgent agent;
    private GameObject player;
    private Animator animator;

    //Animation hashes
    int isWalkingHash;
    int isAttackingHash;
    int isDeadHash;

    private void Start()
    {
        //Get References
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player");
        playerController = player.GetComponent<MovementController>();
        animator = GetComponent<Animator>();

        //Set up patrolling variables
        patrolling = true;
        attacking = false;
        currentPoint = 0;

        //Set up start destination
        agent.destination = patrolPoints[currentPoint].position;

        //Set up animation hashes
        isWalkingHash = Animator.StringToHash("isWalking");
        isAttackingHash = Animator.StringToHash("isAttacking");
        isDeadHash = Animator.StringToHash("isDead");
    }

    private void Update()
    {
        //Check if enemy is dead
        if (!dead)
        {
            PatrolAndAttack();
            HandleWalkAnimation();
        }
    }

    public void PatrolAndAttack()
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
                agent.speed = 5f;
                agent.destination = patrolPoints[currentPoint].position;
            }
            //Go to next waypoint
            if(Vector3.Distance(this.transform.position, patrolPoints[currentPoint].position) <= 5f)
            {
                Iterate(); 
            }    
        }
        //Check if player is still in reach
        if(!patrolling)
        {
            if(CheckDistance()) FollowPlayer();
            else patrolling = true;

            //Player is in attack range
            if(Vector3.Distance(this.transform.position, player.transform.position) <= 2f)
            {
                if (!attacking)
                {
                    attacking = true;
                    agent.isStopped = true;
                    StartCoroutine(Attack());
                }
            }
        }       
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {   
        //Remove enemy from list
        fellowAI.Remove(this);

        for(int i = 0; i < fellowAI.Count; i++)
        {
            fellowAI[i].fellowAI.Remove(this);
        }

        //Disable enemy
        dead = true;
        Destroy(GetComponent<Collider>());

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

    void FollowPlayer()
    {
        int k = 0;
        for(int i = 0; i < fellowAI.Count; i++)
        {
            fellowAI[i].SetPatrolling(false);
            fellowAI[i].agent.speed = 7.5f;
            fellowAI[i].agent.destination = chaseSpots[k].position;
            k++;
            // out of bounds error handling, spare enemys share first spot
            if(k >= chaseSpots.Length)
            {
                k = 0;
            }
        }
    }

    public bool CheckDistance(){
        //Check if player is in reach of one of the enemys
        for(int i = 0; i < fellowAI.Count; i++)
        {
            if(Vector3.Distance(fellowAI[i].transform.position, player.transform.position) <= 10f)
            {
                return true;
            }
        }
        return false;
    }


    void Iterate()
    {
        //Iterate to next waypoint
        if(currentPoint < patrolPoints.Length-1)
        {
            currentPoint++;
        }
        else
        {
            currentPoint = 0;
        }
        agent.destination = patrolPoints[currentPoint].transform.position;
    }
    
    public bool GetPatrolling()
    {
        return patrolling;
    }

    public void SetPatrolling(bool b)
    {
        patrolling = b;
    }    
}
