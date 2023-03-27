using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPlaceholderScript : MonoBehaviour
{
    float health = 100f;
    public void TakeDamage(float damage)
    {
        Debug.Log("Enemy took " + damage + " damage");
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }
}
