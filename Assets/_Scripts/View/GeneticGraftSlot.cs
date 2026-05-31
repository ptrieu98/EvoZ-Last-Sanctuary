using UnityEngine;
using UnityEngine.EventSystems;

public class GeneticGraftSlot : MonoBehaviour, IDropHandler
{
    public PlayerModel playerModel;
    
    [Tooltip("Điền 1 cho phím Q, điền 2 cho phím E")]
    public int slotIndex; 

    public void OnDrop(PointerEventData eventData)
    {
        GameObject dropped = eventData.pointerDrag;
        if (dropped == null) return;

        DraggableItem dragItem = dropped.GetComponent<DraggableItem>();
        
        if (dragItem != null && dragItem.glitchDNAInstance != null)
        {
            if (slotIndex == 1 && playerModel.currentLevel < 40)
            {
                Debug.LogWarning("Chưa đạt Cấp 40 để cấy ghép Slot 1!");
                return;
            }
            if (slotIndex == 2 && playerModel.currentLevel < 45)
            {
                Debug.LogWarning("Chưa đạt Cấp 45 để cấy ghép Slot 2!");
                return;
            }

            if (transform.childCount > 0)
            {
                Transform oldItemTransform = transform.GetChild(0);
                DraggableItem oldDragItem = oldItemTransform.GetComponent<DraggableItem>();

                bool isReturnedToBag = false;

                if (InventoryManager.Instance != null && InventoryManager.Instance.geneSlots != null)
                {
                    foreach (InventorySlot slot in InventoryManager.Instance.geneSlots)
                    {
                        if (slot.transform.childCount == 0) 
                        {
                            oldItemTransform.SetParent(slot.transform);
                            if (oldDragItem != null) oldDragItem.parentAfterDrag = slot.transform;
                            isReturnedToBag = true;
                            break;
                        }
                    }
                }

                if (!isReturnedToBag)
                {
                    Debug.LogWarning("Túi chứa Gen đã đầy, không thể tháo Gen cũ xuống!");
                    return;
                }
            }

            dragItem.parentAfterDrag = transform;

            if (slotIndex == 1)
            {
                playerModel.equippedGen1 = dragItem.glitchDNAInstance;
                playerModel.activeSkill1 = dragItem.glitchDNAInstance.activeSkill;
            }
            else if (slotIndex == 2)
            {
                playerModel.equippedGen2 = dragItem.glitchDNAInstance;
                playerModel.activeSkill2 = dragItem.glitchDNAInstance.activeSkill;
            }

            playerModel.RecalculateStats();

            if (GlitchDNAUIManager.Instance != null)
            {
                GlitchDNAUIManager.Instance.SelectGenForReroll(dragItem.glitchDNAInstance);
            }
        }
    }

    // ==========================================
    // MỚI: BẪY TỰ ĐỘNG PHÁT HIỆN BỊ RÚT ĐỒ 
    // ==========================================
    private void OnTransformChildrenChanged()
    {
        // Nếu số lượng con bằng 0 (Nghĩa là cục Gen vừa bị kéo ra khỏi ô cấy ghép)
        if (transform.childCount == 0 && playerModel != null)
        {
            // Trả lại các khe cắm về trạng thái trống
            if (slotIndex == 1)
            {
                playerModel.equippedGen1 = null;
                playerModel.activeSkill1 = null;
            }
            else if (slotIndex == 2)
            {
                playerModel.equippedGen2 = null;
                playerModel.activeSkill2 = null;
            }

            // Ép nhân vật tính toán lại chỉ số (Trừ đi sức mạnh của Gen vừa tháo)
            playerModel.RecalculateStats();

            // Báo cho UI Trạm Tẩy Luyện biết là "Tôi không có Gen nào cả, hãy ẩn bảng đi"
            if (GlitchDNAUIManager.Instance != null)
            {
                GlitchDNAUIManager.Instance.SelectGenForReroll(null);
            }
        }
    }
}