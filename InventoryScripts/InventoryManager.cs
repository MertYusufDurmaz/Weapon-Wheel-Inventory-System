using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }
    
    [Header("Inventory Settings")]
    public ItemData[] itemSlots = new ItemData[8];
    private ICollectable heldItem;
    public bool HasItemInHand => heldItem != null;

    [Header("References")]
    [SerializeField] private Transform playerHand;
    [SerializeField] private Transform itemPoolTransform;
    [SerializeField] private PlayerControllerHandler playerControllerHandler;
    public WeaponWheelController weaponWheelController;
    private InspectionHandler inspectionHandler;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
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
        // Eşya Bırakma (Sağ Tık)
        if (Input.GetMouseButtonDown(1) && heldItem != null)
        {
            if (inspectionHandler != null && inspectionHandler.IsInspecting) return;
            DropHeldItem();
        }
    }

    private void SetDefaultHandSlot()
    {
        itemSlots[0] = new ItemData { itemName = "Boş El", itemID = 0 };
    }

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

        FlashlightController flashlight = itemTransform.GetComponent<FlashlightController>();
        if (flashlight != null) flashlight.enabled = false;

        (heldItem as MonoBehaviour).enabled = false;
        heldItem = null;
    }

    public void EquipItemFromInventory(GameObject itemPrefab)
    {
        if (heldItem != null) ReturnHeldItemToInventory();

        if (itemPrefab == null)
        {
            if (playerControllerHandler != null) playerControllerHandler.EnablePlayerControls();
            return;
        }

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
            GameObject obj = selectedItemData.instantiatedObject;

            // 1. Günlük Kontrolü
            if (obj.GetComponent<Diary>() != null)
            {
                if (CanvasManager.Instance != null) CanvasManager.Instance.OpenCanvas("DiaryCanvas");
                return;
            }

            // 2. Anahtar Kontrolü
            if (obj.GetComponent<Key>() != null)
            {
                if (CanvasManager.Instance != null) CanvasManager.Instance.CloseAllCanvases();
                if (playerControllerHandler != null) playerControllerHandler.EnablePlayerControls();
                return;
            }

            // Normal Eşya Kuşanma İşlemi (Fener vb.)
            Transform itemTransform = obj.transform;
            itemTransform.SetParent(playerHand);
            itemTransform.localPosition = Vector3.zero;
            itemTransform.localRotation = Quaternion.identity;
            itemTransform.localScale = Vector3.one;
            itemTransform.gameObject.SetActive(true);

            Rigidbody rb = itemTransform.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;

            FlashlightController flashlight = itemTransform.GetComponentInChildren<FlashlightController>(true);
            if (flashlight != null)
            {
                flashlight.enabled = true;
                flashlight.SetupFlashlight(playerHand, Camera.main);
            }

            heldItem = itemTransform.GetComponent<ICollectable>();
            if (heldItem != null) heldItem.Collect(playerHand);

            if (playerControllerHandler != null) playerControllerHandler.EnablePlayerControls();
        }
    }

    // Anahtar metodları
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
