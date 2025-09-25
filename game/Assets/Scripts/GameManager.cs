using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    PlayerController player;

    GameObject pauseMenu;

    Image healthBar;
    TextMeshProUGUI ammoCounter;
    TextMeshProUGUI clip;

    public bool isPaused = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();

        pauseMenu = GameObject.FindGameObjectWithTag("ui_pause");

        pauseMenu.SetActive(false);

        healthBar = GameObject.FindGameObjectWithTag("ui_health").GetComponent<Image>();
        ammoCounter = GameObject.FindGameObjectWithTag("ui_ammo").GetComponent<TextMeshProUGUI>();
        clip = GameObject.FindGameObjectWithTag("ui_clip").GetComponent<TextMeshProUGUI>();

    }

    // Update is called once per frame
    void Update()
    {
        healthBar.fillAmount = (float)player.health / (float)player.maxHealth;

        if (player.currentWeapon != null)
        {
            ammoCounter.text = "Ammo: " + player.currentWeapon.ammo;
            clip.text = "Clip: " + player.currentWeapon.clip + " / " + player.currentWeapon.clipSize;
        }
    }

    public void Pause()
    {
        if (!isPaused)
        isPaused = true;

        pauseMenu.SetActive(true);

        Time.timeScale = 0;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
}
