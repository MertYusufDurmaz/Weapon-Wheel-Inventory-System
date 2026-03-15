using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }
    private ICollectable heldItem;
    [SerializeField] private Transform playerHand;
    [SerializeField] private Transform itemPoolTransform;
    [SerializeField] private PlayerControllerHandler playerControllerHandler;
    public GameObject weaponWheel;
    private Diary Diary;
    private InspectionHandler inspectionHandler;

    public bool HasItemInHand => heldItem != null;
    public ItemData[] itemSlots = new ItemData[8];
    public WeaponWheelController weaponWheelController;

    private void Awake()
    {
        Diary = FindObjectOfType<Diary>();
        if (Instance == null)
        {
            Instance = this;
            itemSlots = new ItemData[8];
            SetDefaultHandSlot();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        inspectionHandler = FindObjectOfType<InspectionHandler>();
        if (playerHand == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null) playerHand = player.transform.Find("Hand");
        }
        if (itemPoolTransform == null)
        {
            GameObject itemPoolObject = new GameObject("ItemPool");
            itemPoolTransform = itemPoolObject.transform;
            itemPoolTransform.SetParent(this.transform);
        }
        if (weaponWheelController != null) weaponWheelController.UpdateUI();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1) && heldItem != null)
        {
            if (inspectionHandler != null && inspectionHandler.IsInspecting) return;
            DropHeldItem();
        }
    }

    private void SetDefaultHandSlot()
    {
        itemSlots[0] = new ItemData { itemName = "Boþ El", itemID = 0 };
    }

    // Load Game için Envanteri temizleme
    public void ClearInventoryForLoading()
    {
        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (itemSlots[i] != null && itemSlots[i].instantiatedObject != null)
            {
                Destroy(itemSlots[i].instantiatedObject);
            }
            itemSlots[i] = null;
        }
        SetDefaultHandSlot();
        if (weaponWheelController != null) weaponWheelController.UpdateUI();
    }

    public void AddItem(ItemData itemToAdd, ICollectable collectableReference)
    {
        for (int i = 1; i < itemSlots.Length; i++)
        {
            if (itemSlots[i] == null || string.IsNullOrEmpty(itemSlots[i].itemName))
            {
                itemSlots[i] = itemToAdd;
                GameObject itemObject = (collectableReference as MonoBehaviour).gameObject;
                itemSlots[i].instantiatedObject = itemObject;

                Transform itemTransform = itemObject.transform;
                itemTransform.SetParent(itemPoolTransform);
                itemTransform.localPosition = Vector3.zero;
                itemTransform.localRotation = Quaternion.identity;
                itemTransform.gameObject.SetActive(false);
                (collectableReference as MonoBehaviour).enabled = false;

                if (weaponWheelController != null) weaponWheelController.UpdateUI();
                return;
            }
        }
        Debug.LogWarning("Envanter dolu!");
    }

    public ItemData GetItemBySlot(int slotID)
    {
        if (slotID >= 0 && slotID < itemSlots.Length) return itemSlots[slotID];
        return null;
    }

    public void Collect(ICollectable itemToCollect)
    {
        if (heldItem != null) ReturnHeldItemToInventory();
        heldItem = itemToCollect;
        heldItem.Collect(playerHand);
    }

    public void DropHeldItem()
    {
        if (heldItem == null) return;
        for (int i = 1; i < itemSlots.Length; i++)
        {
            if (itemSlots[i] != null && itemSlots[i].instantiatedObject == (heldItem as MonoBehaviour).gameObject)
            {
                itemSlots[i].instantiatedObject = null;
                itemSlots[i] = null;
                break;
            }
        }
        Vector3 dropPosition = Camera.main.transform.position + Camera.main.transform.forward * 1.5f;
        Quaternion dropRotation = (heldItem as MonoBehaviour).transform.rotation;
        heldItem.Drop(dropPosition, dropRotation);
        heldItem = null;
        if (weaponWheelController != null) weaponWheelController.UpdateUI();
    }

    public void ReturnHeldItemToInventory()
    {
        if (heldItem == null) return;
        Transform itemTransform = (heldItem as MonoBehaviour).transform;
        itemTransform.SetParent(itemPoolTransform);
        itemTransform.localPosition = Vector3.zero;
        itemTransform.localRotation = Quaternion.identity;
        itemTransform.gameObject.SetActive(false);

        FlashlightController flashlightController = itemTransform.GetComponent<FlashlightController>();
        if (flashlightController != null) flashlightController.enabled = false;

        (heldItem as MonoBehaviour).enabled = false;
        heldItem = null;
    }

    public void EquipItemFromInventory(GameObject itemPrefab)
    {
        // Not: Eðer anahtara týklandýysa eldeki eþyayý býrakmaya gerek yok, 
        // ama yine de temizlik açýsýndan býrakmasý sorun olmaz.
        if (heldItem != null) ReturnHeldItemToInventory();

        if (itemPrefab != null)
        {
            ItemData selectedItemData = null;
            foreach (var slot in itemSlots)
            {
                if (slot != null && slot.itemPrefab == itemPrefab)
                {
                    selectedItemData = slot;
                    break;
                }
            }

            if (selectedItemData != null && selectedItemData.instantiatedObject != null)
            {
                // Günlük Kontrolü
                if (selectedItemData.instantiatedObject.GetComponent<Diary>() != null)
                {
                    CanvasManager.Instance.OpenCanvas("DiaryCanvas");
                    if (weaponWheel != null) weaponWheel.SetActive(true);
                    return;
                }

                // --- [YENÝ EKLENEN KISIM] ANAHTAR KONTROLÜ ---
                // Eðer seçilen obje bir Anahtar (Key) ise:
                if (selectedItemData.instantiatedObject.GetComponent<Key>() != null)
                {
                    // 1. Envanteri/Weapon Wheel'i kapat (Ýstediðin davranýþ)
                    if (CanvasManager.Instance != null)
                    {
                        CanvasManager.Instance.CloseAllCanvases();
                    }

                    // 2. Oyuncu kontrolünü geri ver (Hareket edebilsin)
                    if (playerControllerHandler != null)
                    {
                        playerControllerHandler.EnablePlayerControls();
                    }

                    // 3. Fonksiyondan çýk. Böylece aþaðýdaki eline alma kodlarý çalýþmaz.
                    return;
                }
                // ---------------------------------------------

                Transform itemTransform = selectedItemData.instantiatedObject.transform;
                itemTransform.SetParent(playerHand);
                itemTransform.localPosition = Vector3.zero;
                itemTransform.localRotation = Quaternion.identity;

                // --- CILIZ IÞIK ÇÖZÜMÜ ---
                itemTransform.localScale = Vector3.one;
                // -------------------------

                itemTransform.gameObject.SetActive(true);

                Rigidbody rb = itemTransform.GetComponent<Rigidbody>();
                if (rb != null) rb.isKinematic = true;

                FlashlightController flashlight = itemTransform.GetComponentInChildren<FlashlightController>(true);
                if (flashlight != null)
                {
                    flashlight.enabled = true;
                    flashlight.SetupFlashlight(playerHand, Camera.main);

                    if (flashlight.batteryHealth <= 0.1f && flashlight.batteryCount <= 0)
                    {
                        //flashlight.batteryHealth = 10f;
                    }
                }

                heldItem = itemTransform.GetComponent<ICollectable>();
                if (heldItem != null) heldItem.Collect(playerHand);

                if (playerControllerHandler != null) playerControllerHandler.EnablePlayerControls();
            }
        }
        else
        {
            if (playerControllerHandler != null) playerControllerHandler.EnablePlayerControls();
        }
    }

    // Anahtar metodlarý
    public bool HasAnyKey()
    {
        foreach (var slot in itemSlots)
        {
            if (slot != null && slot.instantiatedObject != null)
            {
                if (slot.instantiatedObject.GetComponent<Key>() != null) return true;
            }
        }
        return false;
    }

    public bool HasCorrectKey()
    {
        foreach (var slot in itemSlots)
        {
            if (slot != null && slot.instantiatedObject != null)
            {
                Key key = slot.instantiatedObject.GetComponent<Key>();
                if (key != null && key.IsCorrectKey) return true;
            }
        }
        return false;
    }
}