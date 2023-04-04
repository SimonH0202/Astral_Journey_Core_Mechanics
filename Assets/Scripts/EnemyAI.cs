using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    
    //Editor Enemy Settings
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private List<EnemyAI> fellowAI;
    [SerializeField] private Transform[] chaseSpots;

    [Header("Enemy Stats")]
    //Enemy stats
    [SerializeField] private float health = 100f;

    [SerializeField] private StatsBar healthBar;

    [Header("Enemy Attack Settings")]
    //Enemy attack settings
    [SerializeField] private float attackDamage = 10f;

    [SerializeField] private float weaponLength = 2f;

    [SerializeField] private Transform weaponPoint;

    [SerializeField] private LayerMask playerLayers;

    //Private patrolling variables
    private int currentPoint;
    private bool patrolling;
    private bool attacking;
    private bool hit;


    private List<GameObject> damagedPlayers = new List<GameObject>();

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

        //Set up health bar
        healthBar.SetMaxStat(health);

        //Set up start destination
        agent.destination = patrolPoints[currentPoint].position;

        //Set up animation hashes
        isWalkingHash = Animator.StringToHash("isWalking");
        isAttackingHash = Animator.StringToHash("isAttacking");
        isDeadHash = Animator.StringToHash("isDead");
    }

    private void Update()
    {
        if (health <= 0) Die();
        if (!dead)
        {
            PatrolAndAttack();
            HandleWalkAnimation();
            AttackIfHit();
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
                HandleDamageRaycast();
            }
        }       
    }

    public void AttackIfHit()
    {
        if (hit && patrolling)
        {
            //Follow player if hit
            FollowPlayer();

            //Start timer to stop following
            StartCoroutine(StopFollowing());
        }
    }
    

    public void TakeDamage(float damage)
    {
            health -= damage;
            healthBar.SetStat(health);
            hit = true;
    }

    void Die()
    {   
        dead = true;

        //Remove enemy from list
        fellowAI.Remove(this);

        for(int i = 0; i < fellowAI.Count; i++)
        {
            //Set all fellow AI to follow player on death
            fellowAI[i].FollowPlayer();

            //Remove this enemy from fellow AI list
            fellowAI[i].fellowAI.Remove(this);
        }

        //Disable enemy
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

    //Stop following coroutine
    IEnumerator StopFollowing()
    {
        yield return new WaitForSeconds(5f);
        hit = false;
        patrolling = true;
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
            fellowAI[i].patrolling = false;
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
    
    public bool Patrolling { get; set; }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(weaponPoint.position, weaponPoint.position + weaponLength * -weaponPoint.transform.up);
    }
}
