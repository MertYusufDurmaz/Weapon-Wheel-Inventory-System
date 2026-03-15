using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; 

public class WeaponWheelController : MonoBehaviour
{
    public Animator anim;
    private bool weaponWheelSelected = false;

    public static int weaponID;

    [Header("Player Controllers")]
    public PlayerControllerHandler playerControllerHandler;
    public InventoryManager inventoryManager;
    private CanvasManager canvasManager;

    [Header("UI Slot References")]
    public Image[] slotIcons;

    [Header("UI Text References")]
    public TextMeshProUGUI itemNameText;

    void Start()
    {
        canvasManager = FindObjectOfType<CanvasManager>();
        if (canvasManager != null)
        {
            canvasManager.RegisterCanvas("WeaponWheelCanvas", gameObject);
            Debug.Log("WeaponWheelCanvas kaydedildi.");
        }
        weaponID = 0;
        if (slotIcons.Length != 8)
        {
            Debug.LogWarning("slotIcons array must have 8 elements! Current size: " + slotIcons.Length);
        }
        UpdateUI();
        if (itemNameText != null)
        {
            itemNameText.text = "";
        }

        gameObject.SetActive(true);
        if (anim != null) anim.SetBool("OpenWeaponWheel", false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            ToggleWeaponWheel();
        }
        if (weaponWheelSelected)
        {
            HandleHover();
        }
    }

    public void ToggleWeaponWheel()
    {
        weaponWheelSelected = !weaponWheelSelected;
        if (weaponWheelSelected)
        {
            CanvasManager.Instance.OpenCanvas("WeaponWheelCanvas");
            UpdateUI();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if (playerControllerHandler != null)
            {
                playerControllerHandler.DisablePlayerControls();
            }
            if (anim != null) anim.SetBool("OpenWeaponWheel", true);
        }
        else
        {
            if (anim != null) anim.SetBool("OpenWeaponWheel", false);
            if (itemNameText != null)
            {
                itemNameText.text = "";
            }
            if (canvasManager != null)
            {
                //canvasManager.SetPlayerState(true); // Crosshair'ý ve diðer kontrolleri geri aç
                CanvasManager.Instance.CloseAllCanvases();
                
                
            }
        }
    }

    private void HandleHover()
    {
        int hoverSlotID = -1;
        for (int i = 0; i < slotIcons.Length; i++)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(slotIcons[i].rectTransform, Input.mousePosition))
            {
                hoverSlotID = i;
                break;
            }
        }
        if (hoverSlotID != -1 && inventoryManager.itemSlots[hoverSlotID] != null)
        {
            itemNameText.text = inventoryManager.itemSlots[hoverSlotID].itemName;
        }
        else
        {
            itemNameText.text = "";
        }
    }

    public void UpdateUI()
    {
        if (InventoryManager.Instance == null)
        {
            Debug.LogError("InventoryManager.Instance is null!");
            return;
        }
        int maxLength = Mathf.Min(slotIcons.Length, InventoryManager.Instance.itemSlots.Length);
        for (int i = 0; i < maxLength; i++)
        {
            if (slotIcons[i] == null)
            {
                Debug.LogWarning($"slotIcons[{i}] is null, please assign the correct Image in the Inspector.");
                continue;
            }
            if (InventoryManager.Instance.itemSlots[i] != null && InventoryManager.Instance.itemSlots[i].itemIcon != null)
            {
                slotIcons[i].sprite = InventoryManager.Instance.itemSlots[i].itemIcon;
                slotIcons[i].color = new Color(1, 1, 1, 1);
            }
            else
            {
                slotIcons[i].sprite = null;
                slotIcons[i].color = new Color(1, 1, 1, 0);
            }
        }
    }

    public void EquipItem(int slotID)
    {
        ItemData item = InventoryManager.Instance.GetItemBySlot(slotID);
        bool shouldCloseWheel = true; // Tekerleðin kapanýp kapanmayacaðýný kontrol eden bir bayrak ekleyin


        if (item != null)
        {
            weaponID = slotID;
            Debug.Log("Equipped item with ID " + weaponID + ": " + item.itemName);
            CanvasManager.Instance.OpenCanvas("WeaponWheelCanvas");
            // Eðer günlükse, tekerleði kapatma
            if (item.instantiatedObject != null && item.instantiatedObject.GetComponent<Diary>() != null)
            {
                shouldCloseWheel = false;
            }

            inventoryManager.EquipItemFromInventory(item.itemPrefab);
        }
        else
        {
            weaponID = 0;
            Debug.Log("Clicked on an empty slot, equipped Empty Hand.");
            inventoryManager.EquipItemFromInventory(null);
        }

        // Sadece normal bir eþya donatýlmýþsa tekerleði kapatýn
        if (shouldCloseWheel)
        {
            ToggleWeaponWheel();
        }
    }
}