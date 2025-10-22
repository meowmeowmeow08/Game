using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TouchDamage : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] int damage = 1;
    [SerializeField] float hitCooldown = 0.5f;

    [Header("Feedback")]
    [SerializeField] float knockbackForce = 6f;         // small shove
    [SerializeField] bool faceOnlyHorizontal = true;    // prevent vertical launch

    float lastHitTime = -999f;

    void Reset()
    {
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true; // we use OnTriggerEnter
    }

    void OnTriggerEnter(Collider other)
    {
        TryHurt(other);
    }

    void OnTriggerStay(Collider other)
    {
        // In case the player stays overlapping; cooldown controls frequency
        TryHurt(other);
    }

    void TryHurt(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (Time.time - lastHitTime < hitCooldown) return;

        var hp = other.GetComponent<Health>()
              ?? other.GetComponentInParent<Health>()
              ?? other.GetComponentInChildren<Health>();
        if (hp == null) return;

        hp.TakeDamage(damage);
        lastHitTime = Time.time;

        // NEW: push the player via their controller, not physics
        var pc = other.GetComponent<PlayerController>()
              ?? other.GetComponentInParent<PlayerController>()
              ?? other.GetComponentInChildren<PlayerController>();
        if (pc != null)
        {
            pc.ApplyKnockback(transform.position, knockbackForce); // e.g., 6–12 feels good
        }
    }

}
