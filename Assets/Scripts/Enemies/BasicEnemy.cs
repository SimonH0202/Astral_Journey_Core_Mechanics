using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class BasicEnemy : MonoBehaviour
{ 
    //Editor Enemy Settings
    [Header("Enemy Settings")]
    [Space(10)]
    [SerializeField] protected List<BasicEnemy> fellowAI;
  
    //Enemy stats
    [SerializeField] protected StatsBar healthBar;
    [SerializeField] protected float health = 100f;


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
    protected int attackAnimSpeedHash; 

    [SerializeField] protected LayerMask playerLayers;

    protected virtual void Start()
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
             if(Vector3.Distance(this.transform.position, player.transform.position) <= patrolRange)
            {
                FollowPlayer();
            }
            //Player is out of sight
            if(Vector3.Distance(this.transform.position, player.transform.position) > patrolRange)
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
            if (fellowAI[i] is AdvancedEnemy)
            {
                AdvancedEnemy advancedEnemy = fellowAI[i] as AdvancedEnemy;
                advancedEnemy.patrolling = false;
            }
            else if (fellowAI[i] is BasicEnemy)
            {
                BasicEnemy enemyAI = fellowAI[i] as BasicEnemy;
                enemyAI.patrolling = false;
            }
        }
    }

    public void AttackIfHit()
    {
        if (hit && patrolling)
        {
            for(int i = 0; i < fellowAI.Count; i++)
            {  
                if (fellowAI[i] is AdvancedEnemy)
                {
                    AdvancedEnemy advancedEnemy = fellowAI[i] as AdvancedEnemy;
                    advancedEnemy.FollowPlayer();
                }
                else if (fellowAI[i] is BasicEnemy)
                {
                    BasicEnemy basicEnemy = fellowAI[i] as BasicEnemy;
                    basicEnemy.FollowPlayer();
                }
            }

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
            BasicEnemy basicEnemy = fellowAI[i] as BasicEnemy;
            //Set all fellow AI to follow player on death
            basicEnemy.FollowPlayer();

            //Remove this enemy from fellow AI list
            basicEnemy.fellowAI.Remove(this);
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

    public bool CheckDistance()
    {
        //Check if there are any enemies in the scene
        if (fellowAI == null || fellowAI.Count == 0)
        {
            return false;
        }

        //Check if player is in reach of one of the enemys
        for(int i = 0; i < fellowAI.Count; i++)
        {
            if (fellowAI[i] is AdvancedEnemy)
            {
                AdvancedEnemy advancedEnemy = fellowAI[i] as AdvancedEnemy;
                if(Vector3.Distance(advancedEnemy.transform.position, player.transform.position) <= 10f)
                {
                    return true;
                }
            }
            else if (fellowAI[i] is BasicEnemy)
            {
                BasicEnemy basicEnemy = fellowAI[i] as BasicEnemy;
                if(Vector3.Distance(basicEnemy.transform.position, player.transform.position) <= 10f)
                {
                    return true;
                }
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

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override bool Equals(object other)
    {
        return base.Equals(other);
    }

    public override string ToString()
    {
        return base.ToString();
    }

    public bool Patrolling { get; set; }

  
}