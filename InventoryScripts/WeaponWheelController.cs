using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponWheelController : MonoBehaviour
{
    [Header("Controls")]
    public KeyCode toggleKey = KeyCode.T;

    [Header("UI Components")]
    public Animator anim;
    public Image[] slotIcons;
    public TextMeshProUGUI itemNameText;

    [Header("Manager References")]
    public PlayerControllerHandler playerControllerHandler;
    
    private bool weaponWheelSelected = false;
    public static int weaponID = 0;

    void Start()
    {
        if (CanvasManager.Instance != null)
        {
            CanvasManager.Instance.RegisterCanvas("WeaponWheelCanvas", gameObject);
        }

        if (slotIcons.Length != 8)
        {
            Debug.LogWarning("WeaponWheelController: slotIcons dizisi 8 elemanlı olmalıdır!");
        }

        SetHoverText("");
        gameObject.SetActive(true);
        if (anim != null) anim.SetBool("OpenWeaponWheel", false);
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleWeaponWheel();
        }
    }

    // Butonlar tarafından çağrılacak yeni hafif metod
    public void SetHoverText(string text)
    {
        if (itemNameText != null)
        {
            itemNameText.text = text;
        }
    }

    public void ToggleWeaponWheel()
    {
        weaponWheelSelected = !weaponWheelSelected;
        
        if (weaponWheelSelected)
        {
            if (CanvasManager.Instance != null) CanvasManager.Instance.OpenCanvas("WeaponWheelCanvas");
            UpdateUI();
            
            if (playerControllerHandler != null) playerControllerHandler.DisablePlayerControls();
            if (anim != null) anim.SetBool("OpenWeaponWheel", true);
        }
        else
        {
            if (anim != null) anim.SetBool("OpenWeaponWheel", false);
            SetHoverText("");
            
            if (CanvasManager.Instance != null) CanvasManager.Instance.CloseAllCanvases();
        }
    }

    public void UpdateUI()
    {
        if (InventoryManager.Instance == null) return;

        int maxLength = Mathf.Min(slotIcons.Length, InventoryManager.Instance.itemSlots.Length);
        for (int i = 0; i < maxLength; i++)
        {
            if (slotIcons[i] == null) continue;

            ItemData slotData = InventoryManager.Instance.itemSlots[i];

            if (slotData != null && slotData.itemIcon != null)
            {
                slotIcons[i].sprite = slotData.itemIcon;
                slotIcons[i].color = Color.white;
            }
            else
            {
                slotIcons[i].sprite = null;
                slotIcons[i].color = Color.clear; // (1, 1, 1, 0) yerine
            }
        }
    }

    public void EquipItem(int slotID)
    {
        ItemData item = InventoryManager.Instance.GetItemBySlot(slotID);
        bool shouldCloseWheel = true;

        if (item != null)
        {
            weaponID = slotID;
            if (CanvasManager.Instance != null) CanvasManager.Instance.OpenCanvas("WeaponWheelCanvas");

            // Not: İleride IEquippable arayüzü ile bu kontrolü kaldırmak daha iyi olur.
            if (item.instantiatedObject != null && item.instantiatedObject.GetComponent<Diary>() != null)
            {
                shouldCloseWheel = false;
            }

            InventoryManager.Instance.EquipItemFromInventory(item.itemPrefab);
        }
        else
        {
            weaponID = 0;
            InventoryManager.Instance.EquipItemFromInventory(null);
        }

        if (shouldCloseWheel)
        {
            ToggleWeaponWheel();
        }
    }
}
