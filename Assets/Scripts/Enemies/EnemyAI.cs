using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class EnemyAI : MonoBehaviour
{ 
    //Editor Enemy Settings
    [Header("Enemy Settings")]
    [Space(10)]
    [SerializeField] protected List<EnemyAI> fellowAI;
  
    //Enemy stats
    [SerializeField] protected float health = 100f;
    [SerializeField] protected StatsBar healthBar;

    //Enemy attack settings
    [SerializeField] protected float attackDamage = 10f;

    //Enemy patrol settings
    [SerializeField] protected float patrolRange = 10f;

    //Private patrolling variables
    protected Vector3 startPoint;
    protected Vector3 nextPoint;
    protected bool patrolling;
    protected bool attacking;
    protected bool hit;

    //Private enemy variables
    protected bool dead = false;

    //References
    private MovementController playerController;
    protected NavMeshAgent agent;
    protected GameObject player;
    protected Animator animator;

    //Animation hashes
    protected int isWalkingHash;
    protected int isAttackingHash;
    protected int isDeadHash;

    [SerializeField] protected LayerMask playerLayers;

    protected void Start()
    {
        //Get References
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player");
        playerController = player.GetComponent<MovementController>();
        animator = GetComponent<Animator>();

        //Set up patrolling variables
        patrolling = true;
        attacking = false;
        startPoint = transform.position;
        nextPoint = RandomPoint();

        //Set up health bar
        if (healthBar != null) healthBar.SetMaxStat(health);

        //Set up start destination
        agent.destination = nextPoint;

        isWalkingHash = Animator.StringToHash("isWalking");
        isAttackingHash = Animator.StringToHash("isAttacking");
        isDeadHash = Animator.StringToHash("isDead");

    }

    protected virtual void Update()
    {
        if (!dead)
        {
            PatrolAndAttack();
        }
    }

    public virtual void PatrolAndAttack()
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
        }
    }

    public virtual void FollowPlayer()
    {
        for(int i = 0; i < fellowAI.Count; i++)
        {  
            EnemyAI EnemyAI = fellowAI[i] as EnemyAI;
            EnemyAI.patrolling = false;
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
            if (healthBar != null) healthBar.SetStat(health);
            hit = true;
            if (health <= 0) Die();
    }

    public virtual void Die()
    {
        //Remove enemy from list
        fellowAI.Remove(this);

        for(int i = 0; i < fellowAI.Count; i++)
        {
            EnemyAI EnemyAI = fellowAI[i] as EnemyAI;
            //Set all fellow AI to follow player on death
            EnemyAI.FollowPlayer();

            //Remove this enemy from fellow AI list
            EnemyAI.fellowAI.Remove(this);
        }

        //Disable enemy
        dead = true;
        Destroy(GetComponent<Collider>());

        //Disable navmesh agent movement
        agent.SetDestination(transform.position);

        //Disable health bar
        if (healthBar != null) healthBar.gameObject.SetActive(false);
    }

    //Stop following coroutine
    IEnumerator StopFollowing()
    {
        yield return new WaitForSeconds(5f);
        hit = false;
        patrolling = true;
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

    protected Vector3 RandomPoint()
    {
        //Get random point in circle
        Vector3 randomPoint = startPoint + Random.insideUnitSphere * patrolRange;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
        {
            Debug.DrawRay(hit.position, Vector3.up, Color.red, 1f);
            //Set destination to random point
            if (Vector3.Distance(hit.position, transform.position) > 1f) return hit.position;
            else RandomPoint();
        }
        return startPoint;
    }
    
    public bool Patrolling { get; set; }

  
}