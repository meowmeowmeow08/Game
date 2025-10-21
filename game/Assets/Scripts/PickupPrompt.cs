using UnityEngine;
using TMPro; // needed if using TextMeshProUGUI

public class PickupPrompt : MonoBehaviour
{
    [SerializeField] Camera playerCamera;
    [SerializeField] float interactDistance = 3f;
    [SerializeField] LayerMask weaponLayer; // optional filter
    [SerializeField] TextMeshProUGUI promptText;

    bool visible = false;

    void Start()
    {
        if (!playerCamera) playerCamera = Camera.main;
        if (promptText) promptText.gameObject.SetActive(false);
    }

    void Update()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
        {
            if (hit.collider.CompareTag("weapon"))
            {
                Show("Press E to pick up");
                return;
            }
        }
        Hide();
    }

    void Show(string message)
    {
        if (visible) return;
        visible = true;
        if (promptText)
        {
            promptText.text = message;
            promptText.gameObject.SetActive(true);
        }
    }

    void Hide()
    {
        if (!visible) return;
        visible = false;
        if (promptText)
            promptText.gameObject.SetActive(false);
    }
}
