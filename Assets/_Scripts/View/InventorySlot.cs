using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        DraggableItem draggedItem = eventData.pointerDrag.GetComponent<DraggableItem>();
        if (draggedItem == null) return;
        
        DraggableItem targetItem = GetComponentInChildren<DraggableItem>();

        if (targetItem == null)
        {
            // Vứt súng từ Khay trang bị về lại Balo trống
            EquipSlotSync sourceWeaponSlot = draggedItem.parentAfterDrag.GetComponent<EquipSlotSync>();
            if (sourceWeaponSlot != null)
            {
                int index = sourceWeaponSlot.slotID - 1;
                sourceWeaponSlot.model.equippedWeapons[index] = null;
                sourceWeaponSlot.model.currentAmmoInMag[index] = 0;
                if (sourceWeaponSlot.model.activeWeaponIndex == index) sourceWeaponSlot.controller.SwitchWeapon(index);
            }
            draggedItem.parentAfterDrag = transform;
        }
        else
        {
            // Kiểm tra gộp đạn
            if (draggedItem.itemData.category == ItemCategory.Ammo && targetItem.itemData.category == ItemCategory.Ammo && draggedItem.itemData.ammoType == targetItem.itemData.ammoType)
            {
                if (!targetItem.itemData.name.Contains("(Clone)")) {
                    targetItem.itemData = ScriptableObject.Instantiate(targetItem.itemData);
                    targetItem.itemData.name += "(Clone)";
                }
                if (!draggedItem.itemData.name.Contains("(Clone)")) {
                    draggedItem.itemData = ScriptableObject.Instantiate(draggedItem.itemData);
                    draggedItem.itemData.name += "(Clone)";
                }

                int spaceLeft = 120 - targetItem.itemData.ammoAmount;
                if (spaceLeft > 0)
                {
                    int toTransfer = Mathf.Min(spaceLeft, draggedItem.itemData.ammoAmount);
                    targetItem.itemData.ammoAmount += toTransfer;
                    draggedItem.itemData.ammoAmount -= toTransfer;

                    targetItem.InitializeItem();
                    draggedItem.InitializeItem();

                    if (draggedItem.itemData.ammoAmount <= 0) {
                        Destroy(draggedItem.gameObject);
                        StartCoroutine(SyncAllNextFrame());
                        return; 
                    }
                }
            }
            else
            {
                // Đổi vị trí 2 đồ vật
                Transform sourceParent = draggedItem.parentAfterDrag;
                EquipSlotSync sourceEquipSlot = sourceParent.GetComponent<EquipSlotSync>();

                // Tìm đoạn check điều kiện này trong InventorySlot.cs của bạn và dán đè:
if (sourceEquipSlot != null && targetItem.itemData.category != ItemCategory.Weapon && targetItem.itemData.category != ItemCategory.Consumable)
{
    Debug.LogWarning("Không thể đẩy vật phẩm này lên Khay phím tắt!");
    return; 
}

                if (sourceEquipSlot != null)
                {
                    int index = sourceEquipSlot.slotID - 1;
                    sourceEquipSlot.model.equippedWeapons[index] = targetItem.itemData;
                    sourceEquipSlot.model.currentAmmoInMag[index] = targetItem.itemData.ammoAmount; 
                    if (sourceEquipSlot.model.activeWeaponIndex == index) sourceEquipSlot.controller.SwitchWeapon(index);
                }

                targetItem.transform.SetParent(sourceParent);
                targetItem.transform.localPosition = Vector3.zero;
                draggedItem.parentAfterDrag = transform;
            }
        }
        StartCoroutine(SyncAllNextFrame());
    }

    private IEnumerator SyncAllNextFrame()
    {
        yield return new WaitForEndOfFrame();
        EquipSlotSync[] allSlots = Resources.FindObjectsOfTypeAll<EquipSlotSync>();
        foreach (var slot in allSlots)
        {
            if (slot.gameObject.scene.rootCount != 0) slot.UpdateSlotUI();
        }
    }
}