using System.Collections; // <-- needed for coroutines
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // Scene refs (cached per scene)
    PlayerController player;

    [Header("Optional wired in inspector; otherwise auto-found by tag")]
    [SerializeField] GameObject gameOverUI; // Tag fallback: "gameover"
    [SerializeField] GameObject pauseUI;    // Tag fallback: "ui_pause"

    // UI (auto-found by tag if present in scene)
    Image healthBar;               // Tag: "ui_health"
    TextMeshProUGUI ammoCounter;   // Tag: "ui_ammo"
    TextMeshProUGUI clipCounter;   // Tag: "ui_clip"

    public bool isPaused = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Rebind scene refs whenever a new scene loads
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CacheSceneRefs();
        // Ensure time runs and UI hidden appropriately when entering a new scene
        if (isPaused) TogglePause(); // unpause if we were paused
        if (gameOverUI != null) gameOverUI.SetActive(false);
        if (pauseUI != null) pauseUI.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1f;
    }

    void CacheSceneRefs()
    {
        // Player
        var playerGO = GameObject.FindGameObjectWithTag("Player");
        player = playerGO ? playerGO.GetComponent<PlayerController>() : null;

        // Pause UI: prefer serialized, else tag
        if (pauseUI == null)
            pauseUI = GameObject.FindGameObjectWithTag("ui_pause");

        // GameOver UI: prefer serialized, else tag
        if (gameOverUI == null)
            gameOverUI = GameObject.FindGameObjectWithTag("gameover");

        // HUD
        var hb = GameObject.FindGameObjectWithTag("ui_health");
        healthBar = hb ? hb.GetComponent<Image>() : null;

        var ammo = GameObject.FindGameObjectWithTag("ui_ammo");
        ammoCounter = ammo ? ammo.GetComponent<TextMeshProUGUI>() : null;

        var clip = GameObject.FindGameObjectWithTag("ui_clip");
        clipCounter = clip ? clip.GetComponent<TextMeshProUGUI>() : null;

        // Hide panels at start if found
        if (pauseUI) pauseUI.SetActive(false);
        if (gameOverUI) gameOverUI.SetActive(false);
    }

    public void StartGame()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SceneManager.LoadScene("Level1");
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;

        var panel = pauseUI;
        if (panel == null)
            panel = GameObject.FindGameObjectWithTag("ui_pause");

        if (panel) panel.SetActive(isPaused);

        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isPaused;
    }

    public void GameOver()
    {
        Time.timeScale = 0f;

        var panel = gameOverUI;
        if (panel == null)
            panel = GameObject.FindGameObjectWithTag("gameover");

        if (panel) panel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Retry()
    {
        Time.timeScale = 1f;
        var i = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(i);
    }

    public void ReturnToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void LevelComplete()
    {
        StartCoroutine(LoadNext());
    }

    IEnumerator LoadNext()
    {
        yield return new WaitForSeconds(1f);
        int i = SceneManager.GetActiveScene().buildIndex;
        if (i + 1 < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(i + 1);
        else
            SceneManager.LoadScene("MainMenu");
    }

    void Update()
    {
        // Guard if a scene (like Main Menu) doesn’t have these yet
        if (player && healthBar)
            healthBar.fillAmount = Mathf.Infinity != player.maxHealth ? (float)player.health / (float)player.maxHealth : 1f;

        if (player && player.currentWeapon != null)
        {
            if (ammoCounter) ammoCounter.text = "Ammo: " + player.currentWeapon.ammo;
            if (clipCounter) clipCounter.text = "Clip: " + player.currentWeapon.clip + " / " + player.currentWeapon.clipSize;
        }
    }
}
