using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private float damage;
    private Vector3 direction;
    private float speed;
    private bool isPiercing;
    private int maxPierceCount = 3;
    private int currentPierceCount = 0;
    
    private Rigidbody2D rb;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    
    public void Initialize(float damage, Vector3 direction, float speed, float lifetime, bool isPiercing)
    {
        this.damage = damage;
        this.direction = direction;
        this.speed = speed;
        this.isPiercing = isPiercing;
        
        // Set velocity
        if (rb != null)
        {
            rb.linearVelocity = direction * speed;
        }
        
        // Set rotation to match direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        
        // Destroy after lifetime
        Destroy(gameObject, lifetime);
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if we hit an enemy
        if (other.CompareTag("Enemy"))
        {
            // Apply damage to enemy
            Monster monster = other.GetComponent<Monster>();
            if (monster != null)
            {
                monster.TakeDamage(damage);
            }
            
            // Handle piercing
            if (isPiercing)
            {
                currentPierceCount++;
                if (currentPierceCount >= maxPierceCount)
                {
                    Destroy(gameObject);
                }
            }
            else
            {
                // Destroy projectile if not piercing
                Destroy(gameObject);
            }
        }
        // Check if we hit a solid object like a wall
        else if (other.CompareTag("Obstacle"))
        {
            Destroy(gameObject);
        }
    }
} 