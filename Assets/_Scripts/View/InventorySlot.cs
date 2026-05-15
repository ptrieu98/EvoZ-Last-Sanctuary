using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        GameObject droppedObj = eventData.pointerDrag;
        DraggableItem draggedItem = droppedObj.GetComponent<DraggableItem>();

        if (draggedItem != null)
        {
            EquipSlotSync thisEquipSlot = GetComponent<EquipSlotSync>();

            // --- XỬ LÝ ĐỔI CHỖ (SWAP) ---
            if (transform.childCount > 0)
            {
                Transform existingItem = transform.GetChild(0);
                Transform destination = draggedItem.parentAfterDrag; 
                
                existingItem.SetParent(destination);
                existingItem.localPosition = Vector3.zero;

                EquipSlotSync destSync = destination.GetComponent<EquipSlotSync>();
                DraggableItem existingDrag = existingItem.GetComponent<DraggableItem>();
                if (destSync != null && existingDrag != null)
                {
                    int destIndex = destSync.slotID - 1;
                    destSync.model.equippedWeapons[destIndex] = existingDrag.itemData;
                    
                    // 🔒 CHỐNG LỖI ĐỔI CHỖ MÀ 3D KHÔNG ĐỔI:
                    if (destSync.model.activeWeaponIndex == destIndex)
                    {
                        destSync.controller.SwitchWeapon(destIndex);
                    }
                }
            }

            draggedItem.transform.SetParent(transform);
            draggedItem.parentAfterDrag = transform;

            if (thisEquipSlot != null)
            {
                thisEquipSlot.OnItemDroppedInSlot(draggedItem.itemData);
            }
            else
            {
                InventoryManager.Instance.RefreshAllEquipSlots();
            }
        }
    }
}