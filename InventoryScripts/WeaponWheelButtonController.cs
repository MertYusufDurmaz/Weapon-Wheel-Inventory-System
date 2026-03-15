using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class WeaponWheelButtonController : MonoBehaviour, IPointerClickHandler
{
    [Header("Item Properties")]
    public int ID;
    public string itemName;
    public Sprite icon;
    public int slotID;

    [Header("UI References")]
    public TextMeshProUGUI itemText;
    public Image selectedItem;

    // Bu referanslarý ve metotlarý siliyoruz
    // public MouseLook mouseLook;
    // public RaycastProcess raycastProcess;
    // public void Selected()
    // public void Deselected()

    private Animator anim;
    public bool selected = false;

    void Start()
    {
        anim = GetComponent<Animator>();
        if (anim == null)
        {
            Debug.LogError("Animator bileþeni eksik: " + gameObject.name);
        }
        if (itemText == null)
        {
            Debug.LogError("itemText atanmamýþ: " + gameObject.name);
        }
        if (selectedItem == null)
        {
            Debug.LogError("selectedItem atanmamýþ: " + gameObject.name);
        }
    }

    // Hover metotlarý kalabilir
    public void HoverEnter()
    {
        if (anim != null) anim.SetBool("Hover", true);
        if (itemText != null) itemText.text = itemName;
        Debug.Log("Fare üzerine geldi: " + itemName);
    }

    public void HoverExit()
    {
        if (anim != null) anim.SetBool("Hover", false);
        if (itemText != null) itemText.text = "";
        Debug.Log("Fare üzerinden ayrýldý: " + itemName);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Yalnýzca parent'a týklama olayýný bildirir
        GetComponentInParent<WeaponWheelController>().EquipItem(slotID);
    }
}