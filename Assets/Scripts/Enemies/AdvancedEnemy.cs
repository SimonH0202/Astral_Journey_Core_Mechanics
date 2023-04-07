using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AdvancedEnemy : BasicEnemy
{
    [Space(10)]
    [Header("Advanced Enemy Settings")]
    [Space(10)]
    [SerializeField] protected Transform[] chaseSpots;
    [SerializeField] protected Transform weaponPoint;
    [SerializeField] protected float weaponLength = 2f;
    [SerializeField] protected List<EnemyAttacksSO> attacks;
    [SerializeField] protected float attackRange = 3f;
    [Space(10)]
    [Header("Special Attack Settings")]
    [Space(10)]
    [SerializeField] protected KnightSpecialAttackSO specialAttack;
    [SerializeField] protected float specialAttackRange = 7.5f;

    //Private variables
    protected List<GameObject> damagedPlayers = new List<GameObject>();
    protected List<EnemyAttacksSO> availableAttacks = new List<EnemyAttacksSO>();
    protected int randomAttack;  
    protected bool canDealDamage = false;
    protected bool specialAttacking = false;

    //Private animator variables
    protected RuntimeAnimatorController defaultAnimator;

    protected override void Start()
    {
        base.Start();
        defaultAnimator = animator.runtimeAnimatorController;
    }

    protected override void Update()
    {
        base.Update();
        if (!dead)
        {
            HandleWalkAnimation();
            HandleDamage();
            AttackIfHit();
            CooldownTimer();
            SpecialAttack();
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
                RandomAttack();
            }
            //Player is in special attack range
            else if (Vector3.Distance(this.transform.position, player.transform.position) > attackRange && Vector3.Distance(this.transform.position, player.transform.position) <= specialAttackRange)
            {
                if (!specialAttack.AttackOnCooldown && !attacking) specialAttacking = true;            
            }
        }
    }

    protected abstract void SpecialAttack();

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
    protected void RandomAttack()
    {
        if (!attacking)
        {
            //Choose random attack
            ChooseRandomAttack();

            //Stop agent and start attack
            attacking = true;
            agent.isStopped = true;
            StartCoroutine(AttackCorountine());
        }
    }

    public override void Die()
    {
        base.Die();

        //Set animator runtime controller back to default
        animator.runtimeAnimatorController = defaultAnimator;

        //Set dead animation
        animator.SetBool(isDeadHash, true);
        animator.SetBool(isWalkingHash, false);
        animator.SetBool(isAttackingHash, false);

        //Delete enemy after 10 seconds
        Destroy(gameObject, 10f);
    }

    protected virtual void HandleWalkAnimation()
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

    protected virtual void HandleDamageRaycast()
    {
        if (attacking && canDealDamage)
        {
            RaycastHit hit;

            //Check for enemies hit by raycast
            if (Physics.Raycast(weaponPoint.position, -weaponPoint.transform.up, out hit, weaponLength, playerLayers))
            {
                if (!damagedPlayers.Contains(hit.transform.gameObject))
                {
                    //Damage enemy
                    damagedPlayers.Add(hit.transform.gameObject);
                    hit.transform.GetComponent<PlayerStatsSystem>().TakeDamage(availableAttacks[randomAttack].Damage);
                }
            }
        }
    }

    protected virtual void HandleDamageAoE()
    {
        if (attacking && canDealDamage)
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, availableAttacks[randomAttack].AoERadius, playerLayers);

            //Check for enemies hit by AoE
            foreach (Collider hitCollider in hitColliders)
            {
                if (!damagedPlayers.Contains(hitCollider.gameObject))
                {
                    //Damage enemy
                    damagedPlayers.Add(hitCollider.gameObject);
                    hitCollider.GetComponent<PlayerStatsSystem>().TakeDamage(availableAttacks[randomAttack].Damage);
                }
            }
        }
    }

    protected virtual void HandleDamage()
    {
        if (attacking)
        {
            if (availableAttacks[randomAttack].AttackType == EnemyAttacksSO.AttackTypes.Raycast) HandleDamageRaycast();
            else if (availableAttacks[randomAttack].AttackType == EnemyAttacksSO.AttackTypes.AOE) HandleDamageAoE();
        }
    }

    //Attack coroutine
    protected virtual IEnumerator AttackCorountine()
    {
        //Play attack animation
        animator.SetBool(isAttackingHash, true);
        //Wait to start damage raycast
        yield return new WaitForSeconds(availableAttacks[randomAttack].DamageDelay);
        canDealDamage = true;

        //Wait for animation to finish
        yield return new WaitForSeconds(availableAttacks[randomAttack].AttackTime);
        animator.SetBool(isAttackingHash, false);
        canDealDamage = false;

        //Wait for a bit and reset attacking
        yield return new WaitForSeconds(Random.Range(0.25f, 0.5f));
        attacking = false;
        agent.isStopped = false;
        damagedPlayers.Clear();

        //Set attack on cooldown
        availableAttacks[randomAttack].AttackOnCooldown = true;
    }

    protected void CooldownTimer()
    {
        //Check if attack is on cooldown
        foreach (EnemyAttacksSO attack in attacks)
        {
            if (attack.AttackOnCooldown)
            {
                attack.AttackTimer += Time.deltaTime;
                if (attack.AttackTimer >= attack.Cooldown)
                {
                    attack.AttackOnCooldown = false;
                    attack.AttackTimer = 0;
                }
            }
        }
        //Check if special attack is on cooldown
        if (specialAttack.AttackOnCooldown)
        {
            specialAttack.AttackTimer += Time.deltaTime;
            if (specialAttack.AttackTimer >= specialAttack.Cooldown)
            {
                specialAttack.AttackOnCooldown = false;
                specialAttack.AttackTimer = 0;
            }
        }
    }
    protected void CheckAvailableAttacks()
    {
        //Clear available attacks
        availableAttacks.Clear();

        //Add all attacks that are not on cooldown
        foreach (EnemyAttacksSO attack in attacks)
        {
            if (!attack.AttackOnCooldown) availableAttacks.Add(attack);
        }              

        //Check if there are any attacks available
        if (availableAttacks.Count == 0)
        {
            //Add all attacks to available attacks
            foreach (EnemyAttacksSO attack in attacks)
            {
                availableAttacks.Add(attack);
            }

            //Reset all attacks cooldowns
            foreach (EnemyAttacksSO attack in availableAttacks)
            {
                attack.AttackOnCooldown = false;
            }
        }
    }

    private void ChooseRandomAttack()
    {
        //Handle available attacks
        CheckAvailableAttacks();
        //Choose random attack
        randomAttack = Random.Range(0, availableAttacks.Count);

        //Set attack animation
        animator.runtimeAnimatorController = availableAttacks[randomAttack].AnimatorOverride;
        animator.speed = availableAttacks[randomAttack].AnimationSpeed;
    }

}
