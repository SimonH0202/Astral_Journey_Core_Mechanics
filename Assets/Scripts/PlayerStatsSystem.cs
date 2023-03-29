using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatsSystem : MonoBehaviour
{
    [Header("Player Stats")]
    //Stats
    [SerializeField] private float health = 100f;
    [SerializeField] private StatsBar healthBar;

    //Private variables
    private bool isVunerable = true;

    // Start is called before the first frame update
    void Start()
    {
        healthBar.SetMaxStat(health);
    }

    public void TakeDamage(float damage)
    {
        if (!isVunerable) return;
        health -= damage;
        healthBar.SetStat(health);
        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Player died");
    }

    //Getters and Setters
    public float GetHealth()
    {
        return health;
    }

    public void SetHealth(float newHealth)
    {
        health = newHealth;
    }

    public bool GetIsVunerable()
    {
        return isVunerable;
    }

    public void SetIsVunerable(bool newIsVunerable)
    {
        isVunerable = newIsVunerable;
    }
}
