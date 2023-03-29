using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MeleeAttack : MonoBehaviour
{
    //References
    PlayerInputs playerInput;
    Animator animator;
    MovementController movementController;

    //Private Attack variables
    bool isAttacking = false;
    bool isJumpAttacking = false;
    int attackIndex = 0;
    int jumpAttackIndex = 0;

    //Private Damage variables
    List<GameObject> damagedEnemies = new List<GameObject>();
    Transform damagePoint;
    float jumpAttackDamageMultiplier = 1f;
    float distance;


    //Public Animator Override Controllers
    public List<AttackSO> Attacks;
    public List<AttackSO> JumpAttacks;

    [Header("Attack Animation Settings")]
    //Public Attack Animation variables
    public float attackAnimSpeed = 1.5f;
    public float jumpAttackAnimSpeed = 2f;

    [Header("Damage Settings")]
    //Public Damage variables
    public float damageRadius = 1f;
    public LayerMask enemyLayers;
    public float weaponLength;
    public Transform weaponPoint;

    [Header("Jump Damage Settings")]
    //Public Jump Damage variables
    public float jumpDamageRadius = 1.5f;
    public LayerMask groundLayers;
    public float multliplierThreshold1 = 2f;
    public float multliplierThreshold2 = 2.5f;

    //Animation hashes
    int isAttackingHash;
    int attackAnimSpeedHash;
    int jumpAttackAnimSpeedHash;

    void Awake()
    {
        //Get references
        playerInput = GetComponent<PlayerInputs>();
        animator = GetComponent<Animator>();
        movementController = GetComponent<MovementController>();
        damagePoint = transform.Find("DamagePoint");

        //Get animation hashes
        isAttackingHash = Animator.StringToHash("isAttacking");
        attackAnimSpeedHash = Animator.StringToHash("attackAnimSpeed");
        jumpAttackAnimSpeedHash = Animator.StringToHash("jumpAttackAnimSpeed");

    }

    void Start()
    {
        //Set up animation times
        animator.SetFloat(attackAnimSpeedHash, attackAnimSpeed);
        animator.SetFloat(jumpAttackAnimSpeedHash, jumpAttackAnimSpeed);
    }

    /*
    void OnAttackInput(InputAction.CallbackContext context)
    {
        isAttackPressed = context.ReadValueAsButton();
    }
    */

    void HandleAttack()
    {
        if (playerInput.attack && !movementController.GetIsJumping() && !isAttacking && !isJumpAttacking)
        {
            //Start attack, set runtime animator controller to attack animation
            isAttacking = true;
            animator.runtimeAnimatorController = Attacks[attackIndex].animatorOverride;
            //Start attack coroutine
            StartCoroutine(Attack(Attacks, attackIndex));
            //HandleDamage();
        }
        HandleDamageRaycast();
    }

    void HandleJumpAttack()
    {
        if (playerInput.attack && movementController.GetIsJumping() && !isAttacking && !isJumpAttacking)
        {
            //Calculate jump attack damage multiplier
            distance = Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 100f, groundLayers) ? hit.distance : 0f;           
            if (distance <= multliplierThreshold1) jumpAttackDamageMultiplier = 1f;
            else if (distance < multliplierThreshold2) jumpAttackDamageMultiplier = 1.5f;
            else if (distance >= multliplierThreshold2) jumpAttackDamageMultiplier = 2f;

            //Start jump attack, set runtime animator controller to jump attack animation
            isJumpAttacking = true;
            animator.runtimeAnimatorController = JumpAttacks[jumpAttackIndex].animatorOverride;
            //Start jump attack coroutine
            StartCoroutine(Attack(JumpAttacks, jumpAttackIndex, true));
        }
        HandleJumpDamage();
    }

    //Old Code for damage handling, using OverlapSphere
    void HandleDamage()
    {
        if (isAttacking)
        {
            Collider[] hitEnemies = Physics.OverlapSphere(damagePoint.position, damageRadius, enemyLayers);
            foreach (Collider enemy in hitEnemies)
            {
                if (!damagedEnemies.Contains(enemy.gameObject) && !movementController.GetIsJumping())
                {
                    damagedEnemies.Add(enemy.gameObject);
                    enemy.GetComponent<EnemyAI>().TakeDamage(Attacks[attackIndex].damage);
                    attackIndex++;
                    if (attackIndex >= Attacks.Count) attackIndex = 0;
                }
            }
        }
    }

    //New Code for damage handling, using Raycast
    void HandleDamageRaycast()
    {
        if (isAttacking)
        {
            RaycastHit hit;

            //Check for enemies hit by raycast
            if (Physics.Raycast(weaponPoint.position, -weaponPoint.transform.up, out hit, weaponLength, enemyLayers))
            {
                if (!damagedEnemies.Contains(hit.transform.gameObject) && !movementController.GetIsJumping())
                {
                    //Damage enemy
                    damagedEnemies.Add(hit.transform.gameObject);
                    hit.transform.GetComponent<EnemyAI>().TakeDamage(Attacks[attackIndex].damage);
                    //Increment attack index
                    attackIndex++;
                    if (attackIndex >= Attacks.Count) attackIndex = 0;
                }
            }
        }
    }

    void HandleJumpDamage()
    {
        if (isJumpAttacking)
        {
            //Check for enemies in hit radius
            Collider[] hitEnemies = Physics.OverlapSphere(damagePoint.position, jumpDamageRadius, enemyLayers);
            foreach (Collider enemy in hitEnemies)
            {
                if (!damagedEnemies.Contains(enemy.gameObject) && movementController.GetIsGrounded())
                {
                    //Damage enemy
                    damagedEnemies.Add(enemy.gameObject);
                    enemy.GetComponent<EnemyAI>().TakeDamage(JumpAttacks[jumpAttackIndex].damage * jumpAttackDamageMultiplier);
                    //Reset damage multiplier
                    jumpAttackDamageMultiplier = 1f;
                    //Increment jump attack index
                    jumpAttackIndex++;
                    if (jumpAttackIndex >= JumpAttacks.Count) jumpAttackIndex = 0;
                    //Reset attack variables
                    isJumpAttacking = false;
                }
            }
        }
    }

    //Attack coroutine
    IEnumerator Attack(List<AttackSO> attackType, int attackIndex, bool isJumpAttack = false)
    {
        //Disable movement and play attack animation
        movementController.DisableMovement();
        animator.SetBool(isAttackingHash, true);

        //Wait for attack animation to finish
        if (!isJumpAttack) yield return new WaitForSeconds(attackType[attackIndex].attackSpeed);
        else
        {
            //Wait until player is grounded
            while (!movementController.GetIsGrounded())
            {
                yield return null;
            }
            //Jump attack delay
            yield return new WaitForSeconds(attackType[attackIndex].attackSpeed);
        }
        animator.SetBool(isAttackingHash, false);

        //Wait for a bit and re-enable movement
        yield return new WaitForSeconds(0.25f);
        movementController.EnableMovement();

        //Reset attack variables
        isAttacking = false;
        isJumpAttacking = false;
        damagedEnemies.Clear();
    }

    //Update is called once per frame
    void Update()
    {
        HandleAttack();
        HandleJumpAttack();
    }

    void OnDrawGizmos()
    {
        if (damagePoint == null) damagePoint = transform.Find("DamagePoint");
        Gizmos.DrawWireSphere(damagePoint.position, damageRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(damagePoint.position, jumpDamageRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + distance * Vector3.down);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(weaponPoint.position, weaponPoint.position + weaponLength * -weaponPoint.transform.up);
    }
}
