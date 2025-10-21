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

    public PlayerInput input;
    public Transform weaponSlot;
    public Weapon currentWeapon;

    float inputX;
    float inputY;

    public int health = 3;
    public int maxHealth = 5;

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
                currentWeapon.fire();

        // Movement System
        Vector3 tempMove = rb.linearVelocity;

        tempMove.x = inputY * speed;
        tempMove.z = inputX * speed;

        rb.linearVelocity = (tempMove.x * transform.forward) +
                            (tempMove.y * transform.up) +
                            (tempMove.z * transform.right);
    }
    public void Attack(InputAction.CallbackContext context)
    {
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
                currentWeapon.fire();
        }
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

    public void Interact()
    {
        if (pickupObj)
        {
            if (pickupObj.tag == "weapon")
            {
                if (currentWeapon)
                    DropWeapon();
                // In Interact(), right before equip:
                Debug.Log("Interact pressed; pickupObj = " + (pickupObj ? pickupObj.name : "null"));

                pickupObj.GetComponent<Weapon>().equip(this);
            }
            pickupObj = null;
        }
        else
            Reload();
    }

    // Relay for Interact (calls your no-arg Interact)
    public void OnInteract(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        Interact();
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
}
