using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatsSystem : MonoBehaviour
{
    [Header("Player Stats")]
    //Stats
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float maxEnergy = 100f;
    [SerializeField] private float regenEnergyTimer = 2f;

    [Header("UI")]
    //UI
    [SerializeField] private StatsBar healthBar;
    [SerializeField] private StatsBar energyBar;

    //Private variables
    private bool isVunerable;
    private float timer;
    private float health;
    private float energy;

    // Start is called before the first frame update
    void Start()
    {
        //Set initial values
        health = maxHealth;
        energy = maxEnergy;
        timer = regenEnergyTimer;
        isVunerable = true;


        if (healthBar != null) healthBar.SetMaxStat(health);
        if (energyBar != null) energyBar.SetMaxStat(energy);
    }

    // Update is called once per frame
    void Update()
    {
        RegenEnergy();
    }

    public void TakeDamage(float damage)
    {
        if (!isVunerable) return;
        health -= damage;
        if (healthBar != null)
        {
            healthBar.SetStat(health);
        }
        if (health <= 0)
        {
            Die();
        }
    }

    public void TakeEnergy(float energy)
    {
        this.energy -= energy;
        if (energyBar != null)
        {
            energyBar.SetStat(this.energy);
        }
        timer = regenEnergyTimer;
    }

    public void RegenEnergy()
    {
        if (energy >= maxEnergy) return;
        timer -= Time.deltaTime;
        if (timer <= 0 && energy < maxEnergy)
        {
            energy += 5f * Time.deltaTime;
            if (energyBar != null)
            {
                energyBar.SetStat(energy);
            }
        }
    }

    void Die()
    {
        Debug.Log("Player died");
    }

    //Getters and Setters
    public float Health { get => health; set => health = value; }
    public float Energy { get => energy; set => energy = value; }
    public bool IsVunerable { get => isVunerable; set => isVunerable = value; }

}
