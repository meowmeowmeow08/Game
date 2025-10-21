using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class Weapon : MonoBehaviour
{
    PlayerController player;


    public GameObject projectile;
    public AudioSource weaponSpeaker;
    public Transform firePoint;
    public Camera firingDirection;

    [Header("Meta Attributes")]
    public bool canFire = true;
    public bool holdToAttack = true;
    public bool reloading = false;
    public int weaponID;
    public string weaponName;

    [Header("Weapon Stats")]
    public float projLifespan;
    public float projVelocity;
    public float reloadCooldown;
    public float rof;
    public int fireModes;
    public int currentFireMode;
    public int clip;
    public int clipSize;
    public int damage = 1;

    [Header("Ammo Stats")]
    public int ammo;
    public int maxAmmo;
    public int ammoRefill;


    void Start()
    {
        weaponSpeaker = GetComponent<AudioSource>();
        firePoint = transform.GetChild(0);

        // Sanity check: warn if projectile is missing or wrong type
        if (projectile == null || projectile.GetComponent<Projectile>() == null)
            Debug.LogError($"{name}: 'projectile' field is not set to a valid Projectile prefab.");
    }

    public void fire()
    {
        // Block firing unless the weapon is actually equipped by a player
        if (player == null) return;

        if (canFire && clip > 0 && weaponID > -1)
        {
            weaponSpeaker.Play();
            GameObject p = Instantiate(projectile, firePoint.position, firePoint.rotation);

            Projectile proj = p.GetComponent<Projectile>();
            if (proj)
            {
                proj.damage = damage;
                proj.lifespan = projLifespan;

                // If this weapon is held by the player, ownerTag will be "Player".
                // If you later mount it on enemies, set their tag to "Enemy" and this will still work.
                proj.ownerTag = (player != null && player.CompareTag("Player")) ? "Player" : gameObject.tag;
            }

            p.GetComponent<Rigidbody>().AddForce(firingDirection.transform.forward * projVelocity);
            Destroy(p, projLifespan);
            clip--;
            canFire = false;
            StartCoroutine("cooldownFire");
        }
    }

    public void reload()
    {
        if (clip >= clipSize)
            return;

        else
        {
            int reloadCount = clipSize - clip;

            if (ammo < reloadCount)
            {
                clip += ammo;
                ammo = 0;
            }

            else
            {
                clip += reloadCount;
                ammo -= reloadCount;
            }

            reloading = true;
            canFire = false;
            StartCoroutine("cooldownFire", reloadCooldown);
            return;
        }
    }

    public void equip(PlayerController player)
    {
        player.currentWeapon = this;

        transform.SetPositionAndRotation(player.weaponSlot.position, player.weaponSlot.rotation);
        transform.SetParent(player.weaponSlot);

        GetComponent<Rigidbody>().isKinematic = true;
        GetComponent<Collider>().isTrigger = true;

        firingDirection = Camera.main;
        this.player = player;
    }

    public void unequip()
    {
        player.currentWeapon = null;
        
        transform.SetParent(null);

        GetComponent<Rigidbody>().isKinematic = false;
        GetComponent<Collider>().isTrigger = false;

        firingDirection = null;
        this.player = null;
    }

    IEnumerator cooldownFire()
    {
        yield return new WaitForSeconds(rof);
        
        if(clip > 0)
            canFire = true;
    }

    IEnumerator reloadingCooldown()
    {
        yield return new WaitForSeconds(reloadCooldown);

        reloading = false;
        canFire = true;
    }
}
