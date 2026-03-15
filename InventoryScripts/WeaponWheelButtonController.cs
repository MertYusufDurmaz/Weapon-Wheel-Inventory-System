using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

// IPointerEnterHandler ve IPointerExitHandler eklendi
public class WeaponWheelButtonController : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Item Properties")]
    public int ID;
    public string itemName;
    public Sprite icon;
    public int slotID;

    [Header("UI References")]
    public TextMeshProUGUI itemText;
    public Image selectedItem;

    private Animator anim;
    public bool selected = false;
    private WeaponWheelController wheelController;

    void Start()
    {
        anim = GetComponent<Animator>();
        wheelController = GetComponentInParent<WeaponWheelController>();
        
        if (anim == null) Debug.LogWarning($"Animator eksik: {gameObject.name}");
        if (itemText == null) Debug.LogWarning($"itemText eksik: {gameObject.name}");
        if (selectedItem == null) Debug.LogWarning($"selectedItem eksik: {gameObject.name}");
    }

    // Unity Event'i: Fare objenin üzerine geldiğinde otomatik çalışır
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (anim != null) anim.SetBool("Hover", true);
        if (itemText != null) itemText.text = itemName;
        
        // Tekerlek yöneticisine ismimizi gönderiyoruz
        if (wheelController != null) wheelController.SetHoverText(itemName);
    }

    // Unity Event'i: Fare objeden ayrıldığında otomatik çalışır
    public void OnPointerExit(PointerEventData eventData)
    {
        if (anim != null) anim.SetBool("Hover", false);
        if (itemText != null) itemText.text = "";
        
        if (wheelController != null) wheelController.SetHoverText("");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (wheelController != null)
        {
            wheelController.EquipItem(slotID);
        }
    }
}
