using UnityEngine;
using Fusion;

[RequireComponent(typeof(Collider))]
public class HealthController : NetworkBehaviour
{
    public int maxHealth = 100;

    [Networked]
    public int currentHealth { get; set; }
    public int baseDamage = 10;


    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            currentHealth = maxHealth;
        }

        Debug.Log($"[{Object.Id}] {gameObject.name} 已生成，初始 HP: {currentHealth}");
    }
    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            TakeDamage(baseDamage);
            Debug.Log("sdfkljf");
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (!Object.HasStateAuthority) return;

        currentHealth -= damageAmount;

        Debug.Log($"[{Object.Id}] 被擊中！扣除傷害: {damageAmount}. 剩餘 HP: {currentHealth}");


    }



}

