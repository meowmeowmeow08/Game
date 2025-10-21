using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Runtime Config (set by Weapon)")]
    public int damage = 1;
    public float lifespan = 3f;
    public string ownerTag = "Player";

    Rigidbody rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // Self-cleanup in case we miss everything
        Destroy(gameObject, lifespan);
    }

    // Handle non-trigger collisions
    private void OnCollisionEnter(Collision collision)
    {
        TryDamage(collision.collider);
    }

    // Handle trigger volumes if projectile/targets use triggers
    private void OnTriggerEnter(Collider other)
    {
        TryDamage(other);
    }

    void TryDamage(Collider hit)
    {
        // Ignore our owner and other projectiles/weapons
        if (hit.CompareTag(ownerTag) || hit.CompareTag("weapon") || hit.CompareTag("proj"))
            return;

        // Find a Health component on the thing we struck (or its parents)
        Health h = hit.GetComponent<Health>();
        if (!h)
            h = hit.GetComponentInParent<Health>();

        if (h)
        {
            h.TakeDamage(damage);
            // Destroy the projectile on a successful hit
            Destroy(gameObject);
        }
        else
        {
            // Optional: destroy on any surface hit
            Destroy(gameObject);
        }
    }
}
