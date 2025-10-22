using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    Camera playerCam;
    Rigidbody rb;
    Ray jumpRay;
    Ray interactRay;
    RaycastHit interactHit;
    GameObject pickupObj;
    bool attackLocked = false;

    public PlayerInput input;
    public Transform weaponSlot;
    public Weapon currentWeapon;

    float inputX;
    float inputY;

    public int health = 3;
    public int maxHealth = 5;

    Vector3 externalVelocity = Vector3.zero;  // accumulates knockback, dashes, etc.
    [SerializeField] float externalDecay = 8f; // how fast knockback fades (units/sec)

    public float speed = 5f;
    public float jumpHeight = 2.5f;
    public float groundDetectionDistance = 1.1f;
    public float interactDistance = 1f;

    public bool attacking = false;

    PlayerInput _pi;

    private void Start()
    {
        input = GetComponent<PlayerInput>();
        interactRay = new Ray(transform.position, transform.forward);
        jumpRay = new Ray(transform.position, -transform.up);
        rb = GetComponent<Rigidbody>();
        playerCam = Camera.main;
        weaponSlot = playerCam.transform.GetChild(0);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        var act = _pi?.actions?.FindAction("Interact");
        if (act != null && act.WasPressedThisFrame())
            Debug.Log("Interact action performed this frame");
    }
    private void Update()
    {
        // Camera Handler
        Quaternion playerRotation = Quaternion.identity;
        playerRotation.y = playerCam.transform.rotation.y;
        playerRotation.w = playerCam.transform.rotation.w;
        transform.rotation = playerRotation;

        jumpRay.origin = transform.position;
        jumpRay.direction = -transform.up;

        interactRay.origin = playerCam.transform.position;
        interactRay.direction = playerCam.transform.forward;

        if (Physics.Raycast(interactRay, out interactHit, interactDistance))
        {

            if (interactHit.collider.tag == "weapon")
            {

                pickupObj = interactHit.collider.gameObject;
                // In Update(), after you set pickupObj:
                //if (pickupObj) Debug.Log("Pickup target: " + pickupObj.name);

                
            }
        }
        else
            pickupObj = null;

        if (currentWeapon)
            if (currentWeapon.holdToAttack && attacking)
            {
                // Block during pause or grace
                if (GameManager.Instance && (GameManager.Instance.isPaused || GameManager.Instance.unpauseGraceActive)) { }
                else if (!attackLocked)
                {
                    currentWeapon.fire();
                }
            }
        // Build planar velocity from input (W/S on forward axis, A/D on right axis)
        Vector3 planar = (transform.forward * inputY + transform.right * inputX) * speed;

        // Keep whatever vertical velocity you already have (gravity/jump)
        float vy = rb.linearVelocity.y;

        // Combine with external knockback (no vertical kick unless you purposely add it)
        Vector3 finalVel = planar + externalVelocity;
        finalVel.y = vy;

        // Apply ONCE
        rb.linearVelocity = finalVel;

        // Decay external forces smoothly
        externalVelocity = Vector3.MoveTowards(
            externalVelocity,
            Vector3.zero,
            externalDecay * Time.deltaTime
        );
    }
    public void Attack(InputAction.CallbackContext context)
    {
        // Don’t accept attack while paused or locked
        if ((GameManager.Instance && GameManager.Instance.isPaused) || attackLocked)
            return;

        if (currentWeapon)
        {
            if (currentWeapon.holdToAttack)
            {
                if (context.ReadValueAsButton())
                    attacking = true;
                else
                    attacking = false;
            }
            else if (context.ReadValueAsButton())
            {
                currentWeapon.fire();
            }
        }
    }

    public void ClearAttackState()
    {
        attacking = false;
        StartCoroutine(AttackLockoutOneFrame());
    }

    System.Collections.IEnumerator AttackLockoutOneFrame()
    {
        attackLocked = true;
        yield return null;             // skip one frame
        attackLocked = false;
    }

    public void Reload()
    {
        if (currentWeapon)
            if (!currentWeapon.reloading)
            currentWeapon.reload();
    }

    // Relay for Reload (calls your no-arg Reload)
    public void OnReload(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        Reload();
    }

    GameObject GetPickupTarget()
    {
        if (playerCam == null) playerCam = Camera.main;
        Ray ray = new Ray(playerCam.transform.position, playerCam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
            if (hit.collider.CompareTag("weapon"))
                return hit.collider.gameObject;
        return null;
    }

    // Keep your relay:
    public void OnInteract(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        Interact();
    }

    // Update Interact() to prefer a fresh raycast:
    public void Interact()
    {
        var target = GetPickupTarget();              // <- new
        if (target == null) target = pickupObj;      // fallback to cached

        if (target != null)
        {
            if (currentWeapon) DropWeapon();
            target.GetComponent<Weapon>().equip(this);
            return;
        }

        if (currentWeapon && !currentWeapon.reloading)
            currentWeapon.reload();
    }

    public void DropWeapon()
    {
        if (currentWeapon)
        {
            currentWeapon.GetComponent<Weapon>().unequip();
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        if (context.canceled) 
        {
            inputX = 0; inputY = 0; return; 
        }

        Vector2 InputAxis = context.ReadValue<Vector2>();

        inputX = InputAxis.x;
        inputY = InputAxis.y;
    }

    public void Jump()
    {
        if (Physics.Raycast(jumpRay, groundDetectionDistance))
            rb.AddForce(transform.up * jumpHeight, ForceMode.Impulse);
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        Jump();
    }

    public void OnPause(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        if (GameManager.Instance != null) GameManager.Instance.TogglePause();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "hazard")
            health--;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "killzone")
            health = 0;

        if (other.tag == "health" && health < maxHealth)
        {
            health++;
            Destroy(other.gameObject);
        }
    }

    // Call this from enemies to push the player
    public void ApplyKnockback(Vector3 sourcePosition, float force)
    {
        Vector3 dir = (transform.position - sourcePosition);
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) dir = -transform.forward; // fallback
        dir.Normalize();

        // accumulate so multiple hits stack a bit
        externalVelocity += dir * force;
    }
}
