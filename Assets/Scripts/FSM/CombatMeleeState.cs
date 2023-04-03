using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatMeleeState : CombatBaseState
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
    float jumpAttackDamageMultiplier = 1f;
    float distance;
    Transform weaponPoint;
    Transform damagePoint;

    //Animation hashes
    int isAttackingHash;
    int attackAnimSpeedHash;
    int jumpAttackAnimSpeedHash;

    public override void EnterState(CombatStateManager manager)
    {
        //Log state change
        Debug.Log("Entered Melee State");

        //Get references
        movementController = manager.GetComponent<MovementController>();
        animator = manager.GetComponent<Animator>();
        playerInput = manager.GetComponent<PlayerInputs>();
        damagePoint = manager.transform.Find("DamagePoint");
        weaponPoint = manager.MeleeWeapon.transform.Find("WeaponPoint");

        //Activate weapon
        manager.MeleeWeapon.SetActive(true);

        //Get animation hashes
        isAttackingHash = Animator.StringToHash("isAttacking");
        attackAnimSpeedHash = Animator.StringToHash("attackAnimSpeed");
        jumpAttackAnimSpeedHash = Animator.StringToHash("jumpAttackAnimSpeed");

        //Set up animation times
        animator.SetFloat(attackAnimSpeedHash, manager.MeleeSettings.AttackAnimSpeed);
        animator.SetFloat(jumpAttackAnimSpeedHash, manager.MeleeSettings.JumpAttackAnimSpeed);
    }

    public override void UpdateState(CombatStateManager manager)
    {
        HandleAttack(manager);
        HandleJumpAttack(manager);
    }

    public override void ExitState(CombatStateManager manager)
    {
        //Deactivate weapon
        manager.MeleeWeapon.SetActive(false);
    }

    void HandleAttack(CombatStateManager manager)
    {
        if (playerInput.attack && !movementController.IsJumping && !movementController.IsDodging && !isAttacking && !isJumpAttacking)
        {
            //Start attack, set runtime animator controller to attack animation
            isAttacking = true;
            animator.runtimeAnimatorController = manager.MeleeSettings.AttacksList[attackIndex].animatorOverride;
            //Start attack coroutine
            manager.StartCoroutine(Attack(manager.MeleeSettings.AttacksList, attackIndex));
            //HandleDamage();
        }
        HandleDamageRaycast(manager);
    }

    void HandleJumpAttack(CombatStateManager manager)
    {
        if (playerInput.attack && movementController.IsJumping && !movementController.IsDodging && !isAttacking && !isJumpAttacking)
        {
            //Calculate jump attack damage multiplier
            distance = Physics.Raycast(manager.transform.position, Vector3.down, out RaycastHit hit, 100f, manager.MeleeSettings.GroundLayers) ? hit.distance : 0f;           
            if (distance <= manager.MeleeSettings.MultliplierThreshold1) jumpAttackDamageMultiplier = 1f;
            else if (distance < manager.MeleeSettings.MultliplierThreshold2) jumpAttackDamageMultiplier = 1.5f;
            else if (distance >= manager.MeleeSettings.MultliplierThreshold2) jumpAttackDamageMultiplier = 2f;

            //Start jump attack, set runtime animator controller to jump attack animation
            isJumpAttacking = true;
            animator.runtimeAnimatorController = manager.MeleeSettings.JumpAttacksList[jumpAttackIndex].animatorOverride;
            //Start jump attack coroutine
            manager.StartCoroutine(Attack(manager.MeleeSettings.JumpAttacksList, jumpAttackIndex, true));
        }
        HandleJumpDamage(manager);
    }

    //Old Code for damage handling, using OverlapSphere
    void HandleDamage(CombatStateManager manager)
    {
        if (isAttacking)
        {
            Collider[] hitEnemies = Physics.OverlapSphere(damagePoint.position, manager.MeleeSettings.DamageRadius, manager.MeleeSettings.EnemyLayers);
            foreach (Collider enemy in hitEnemies)
            {
                if (!damagedEnemies.Contains(enemy.gameObject) && !movementController.IsJumping)
                {
                    damagedEnemies.Add(enemy.gameObject);
                    enemy.GetComponent<EnemyAI>().TakeDamage(manager.MeleeSettings.AttacksList[attackIndex].damage);
                    attackIndex++;
                    if (attackIndex >= manager.MeleeSettings.AttacksList.Count) attackIndex = 0;
                }
            }
        }
    }

    //New Code for damage handling, using Raycast
    void HandleDamageRaycast(CombatStateManager manager)
    {
        if (isAttacking)
        {
            RaycastHit hit;

            //Check for enemies hit by raycast
            if (Physics.Raycast(weaponPoint.position, -weaponPoint.transform.up, out hit, manager.MeleeSettings.WeaponLength, manager.MeleeSettings.EnemyLayers))
            {
                if (!damagedEnemies.Contains(hit.transform.gameObject) && !movementController.IsJumping)
                {
                    //Damage enemy
                    damagedEnemies.Add(hit.transform.gameObject);
                    hit.transform.GetComponent<EnemyAI>().TakeDamage(manager.MeleeSettings.AttacksList[attackIndex].damage);
                    //Increment attack index
                    attackIndex++;
                    if (attackIndex >= manager.MeleeSettings.AttacksList.Count) attackIndex = 0;
                }
            }
        }
    }

    void HandleJumpDamage(CombatStateManager manager)
    {
        if (isJumpAttacking)
        {
            //Check for enemies in hit radius
            Collider[] hitEnemies = Physics.OverlapSphere(damagePoint.position, manager.MeleeSettings.JumpDamageRadius, manager.MeleeSettings.EnemyLayers);
            foreach (Collider enemy in hitEnemies)
            {
                if (!damagedEnemies.Contains(enemy.gameObject) && movementController.IsGrounded)
                {
                    //Damage enemy
                    damagedEnemies.Add(enemy.gameObject);
                    enemy.GetComponent<EnemyAI>().TakeDamage(manager.MeleeSettings.JumpAttacksList[jumpAttackIndex].damage * jumpAttackDamageMultiplier);
                    //Reset damage multiplier
                    jumpAttackDamageMultiplier = 1f;
                    //Increment jump attack index
                    jumpAttackIndex++;
                    if (jumpAttackIndex >= manager.MeleeSettings.JumpAttacksList.Count) jumpAttackIndex = 0;
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
            while (!movementController.IsGrounded)
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
}
