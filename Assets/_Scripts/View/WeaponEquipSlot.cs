using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class EquipSlotSync : MonoBehaviour, IDropHandler
{
    public PlayerModel model;
    public PlayerController controller;
    public GameObject itemUIPrefab;

    [Header("Cài đặt ô")]
    public int slotID; 

    private void Start()
    {
        if (InventoryManager.Instance != null) InventoryManager.Instance.RegisterEquipSlot(this);
        StartCoroutine(DelayedUpdateSlotUI());
    }

    private IEnumerator DelayedUpdateSlotUI() 
    {
        yield return new WaitForEndOfFrame();
        UpdateSlotUI();
    }

    public void UpdateSlotUI()
    {
        foreach (Transform child in transform) Destroy(child.gameObject);

        int index = slotID - 1;
        ItemData dataInModel = model.equippedWeapons[index];

        if (dataInModel != null)
        {
            GameObject newItem = Instantiate(itemUIPrefab, transform);
            DraggableItem dragScript = newItem.GetComponent<DraggableItem>();
            dragScript.itemData = dataInModel;
            dragScript.InitializeItem();
            newItem.transform.localPosition = Vector3.zero;

            UpdateSlotAmmoText(dragScript); 
        }
    }

    public void RefreshAmmoDisplayOnly()
    {
        DraggableItem dragScript = GetComponentInChildren<DraggableItem>();
        if (dragScript != null) UpdateSlotAmmoText(dragScript);
    }

    private void UpdateSlotAmmoText(DraggableItem dragScript)
    {
        int index = slotID - 1;
        if (dragScript.itemData == null) return;

        // 1. NẾU LÀ SÚNG BẮN XA -> HIỂN THỊ ĐẠN TRONG BĂNG (Ví dụ: 30/30)
        if (dragScript.itemData.category == ItemCategory.Weapon && dragScript.itemData.weaponType == WeaponType.Ranged)
        {
            dragScript.SetCustomCornerText($"{model.currentAmmoInMag[index]}/{dragScript.itemData.ammoAmount}");
        }
        // 2. NẾU LÀ VẬT PHẨM TIÊU HAO -> HIỂN THỊ ĐỐNG STACK SỐ LƯỢNG (Ví dụ: x5)
        else if (dragScript.itemData.category == ItemCategory.Consumable)
        {
            dragScript.SetCustomCornerText($"x{dragScript.itemData.ammoAmount}");
        }
        // 3. VŨ KHÍ CẬN CHIẾN TỰ ĐỘNG ẨN CHỮ
        else
        {
            if (dragScript.cornerText != null) dragScript.cornerText.gameObject.SetActive(false);
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        DraggableItem draggedItem = eventData.pointerDrag.GetComponent<DraggableItem>();
        if (draggedItem == null) return;

        // 🔒 CẬP NHẬT: Cho phép mang cả Vũ khí và Đồ tiêu hao lên Hotbar
        if (draggedItem.itemData.category != ItemCategory.Weapon && draggedItem.itemData.category != ItemCategory.Consumable)
        {
            Debug.LogWarning("Không thể đặt vật phẩm này lên khay phím tắt!");
            return; 
        }

        int targetIndex = slotID - 1;
        Transform sourceParent = draggedItem.parentAfterDrag;
        EquipSlotSync sourceEquipSlot = sourceParent.GetComponent<EquipSlotSync>();

        if (sourceEquipSlot != null)
        {
            // Đổi chỗ hai khay phím tắt
            int sourceIndex = sourceEquipSlot.slotID - 1;

            ItemData tempWeapon = model.equippedWeapons[targetIndex];
            model.equippedWeapons[targetIndex] = model.equippedWeapons[sourceIndex];
            model.equippedWeapons[sourceIndex] = tempWeapon;

            int tempAmmo = model.currentAmmoInMag[targetIndex];
            model.currentAmmoInMag[targetIndex] = model.currentAmmoInMag[sourceIndex];
            model.currentAmmoInMag[sourceIndex] = tempAmmo;
        }
        else
        {
            // Đưa từ balo lên
            DraggableItem targetItem = GetComponentInChildren<DraggableItem>();
            if (targetItem != null) 
            {
                targetItem.itemData = model.equippedWeapons[targetIndex];
                targetItem.InitializeItem();
                targetItem.transform.SetParent(sourceParent);
                targetItem.transform.localPosition = Vector3.zero;
            }

            model.equippedWeapons[targetIndex] = draggedItem.itemData;
            
            // Nếu đưa súng lên thì mới setup đạn đầy băng ban đầu
            if (draggedItem.itemData.category == ItemCategory.Weapon)
            {
                model.currentAmmoInMag[targetIndex] = draggedItem.itemData.ammoAmount;
            }
        }

        draggedItem.parentAfterDrag = transform;

        // Cập nhật súng trên tay nhân vật
        controller.SwitchWeapon(model.activeWeaponIndex);

        StartCoroutine(SyncAllTwinsNextFrame());
    }

    private IEnumerator SyncAllTwinsNextFrame()
    {
        yield return new WaitForEndOfFrame();
        EquipSlotSync[] allSlots = Resources.FindObjectsOfTypeAll<EquipSlotSync>();
        foreach (var slot in allSlots)
        {
            if (slot.gameObject.scene.rootCount != 0) slot.UpdateSlotUI();
        }
    }
}