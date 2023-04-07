using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnightEnemy : AdvancedEnemy
{
    [SerializeField] private LineRenderer energyBeam;

    //Private special attack variables
    private Transform laserOrigin;
    private Vector3 previousPlayerDirection;

    protected override void Start()
    {
        base.Start();
        //Set laser origin
        laserOrigin = energyBeam.transform;
    }
    
    protected override void SpecialAttack()
    {
        if (specialAttacking)
        {

            //Stop agent and start attack
            agent.isStopped = true;

            //Set special attack animation
            animator.runtimeAnimatorController = specialAttack.AnimatorOverride;
            animator.speed = specialAttack.AnimationSpeed;
            animator.SetBool(isAttackingHash, true);

            float playerDistance = Vector3.Distance(player.transform.position, laserOrigin.position);
            float directionMultiplier = CheckPlayerDirection();
            
            specialAttack.LaserTarget = Vector3.Lerp(specialAttack.LaserTarget, player.transform.position + Vector3.up, Time.deltaTime * specialAttack.LaserSpeed * directionMultiplier * 1.25f);

            // Fire laser for 5 seconds
            if (specialAttack.LaserTimer < 5f)
            {
                // Fire laser
                FireLaser();
                specialAttack.LaserTimer += Time.deltaTime;
            }

            // Reduce laser timer
            if (specialAttack.LaserTimer > 5f)
            {
                specialAttacking = false;
                animator.SetBool(isAttackingHash, false);
                agent.isStopped = false;
                specialAttack.LaserTimer = 0f;
                energyBeam.enabled = false;

                //Reset special attack variables
                specialAttack.LaserDistanceIncrease = 1f;
                specialAttack.SameDirectionTimer = 0f;

                //Set special attack on cooldown
                specialAttack.AttackOnCooldown = true;
            }
        }
    }

    void FireLaser()
    {
        //Enable laser
        energyBeam.enabled = true;

        RaycastHit hit;
        if (Physics.Raycast(laserOrigin.position, specialAttack.LaserTarget - laserOrigin.position, out hit, specialAttack.LaserRange, playerLayers))
        {
            //Draw Linerenderer and set color to red
            energyBeam.SetPosition(0, laserOrigin.position);
            energyBeam.SetPosition(1, hit.point);
            energyBeam.material.SetColor("_EmissionColor", Color.red);
            
            //Damage enemy
            hit.transform.GetComponent<PlayerStatsSystem>().TakeDamage(specialAttack.Damage * Time.deltaTime);
        }
        else
        {
            //Draw Linerenderer and set color to blue
            energyBeam.SetPosition(0, laserOrigin.position);
            energyBeam.SetPosition(1, laserOrigin.position + (specialAttack.LaserTarget - laserOrigin.position).normalized * specialAttack.LaserRange);
            energyBeam.material.SetColor("_EmissionColor", Color.blue);
        }
    }

    float CheckPlayerDirection()
    {
        float directionMultiplier = 1f;

        // Check if player is running in the same direction as before
        if (Vector3.Dot(player.transform.forward, previousPlayerDirection) > 0.9f)
        {
            // Increase same direction timer
            specialAttack.SameDirectionTimer += Time.deltaTime;

            // If same direction timer exceeds threshold, increase laser distance multiplier
            if (specialAttack.SameDirectionTimer > specialAttack.SameDirectionThreshold)
            {
                directionMultiplier =+ specialAttack.SameDirectionTimer - specialAttack.SameDirectionThreshold;
            }
        }
        else
        {
            // Reset same direction timer and direction multiplier
            specialAttack.SameDirectionTimer = 0f;
            directionMultiplier = 1f;
        }

        previousPlayerDirection = player.transform.forward;

        return directionMultiplier;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(weaponPoint.position, weaponPoint.position + weaponLength * -weaponPoint.transform.up);
        Gizmos.DrawWireSphere(transform.position, 7);
    }

}
