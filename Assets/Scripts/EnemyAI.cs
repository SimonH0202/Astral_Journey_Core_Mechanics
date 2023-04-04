using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class EnemyAI : MonoBehaviour
{ 
    //Editor Enemy Settings
    [SerializeField] protected Transform[] patrolPoints;
    [SerializeField] protected List<EnemyAI> fellowAI;
  
    //Enemy stats
    [SerializeField] protected float health = 100f;

    [SerializeField] protected StatsBar healthBar;


    //Enemy attack settings
    [SerializeField] protected float attackDamage = 10f;


    //Private patrolling variables
    protected int currentPoint;
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
        currentPoint = 0;

        //Set up health bar
        healthBar.SetMaxStat(health);

        //Set up start destination
        agent.destination = patrolPoints[currentPoint].position;

        isWalkingHash = Animator.StringToHash("isWalking");
        isAttackingHash = Animator.StringToHash("isAttacking");
        isDeadHash = Animator.StringToHash("isDead");

    }
      

    public abstract void PatrolAndAttack();
    
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

    public abstract void Die();

    //Stop following coroutine
    IEnumerator StopFollowing()
    {
        yield return new WaitForSeconds(5f);
        hit = false;
        patrolling = true;
    }


    public abstract void FollowPlayer();
   

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

    protected void Iterate()
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

  
}