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
        Debug.Log("Hit player");

        if (!other.CompareTag("Player")) return;
        if (Time.time - lastHitTime < hitCooldown) return;

        var hp = other.GetComponent<Health>();
        if (hp == null) return;

        hp.TakeDamage(damage);
        lastHitTime = Time.time;

        // Knockback (optional but feels good)
        var rb = other.attachedRigidbody ?? other.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 dir = (other.transform.position - transform.position).normalized;
            if (faceOnlyHorizontal) dir.y = 0f;
            dir = dir.sqrMagnitude > 0.001f ? dir.normalized : transform.forward;
            rb.AddForce(dir * knockbackForce, ForceMode.Impulse);
        }
    }
}
