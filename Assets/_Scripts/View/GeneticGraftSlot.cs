using UnityEngine;
using UnityEngine.EventSystems;

public class GeneticGraftSlot : MonoBehaviour, IDropHandler, IPointerClickHandler
{
    public PlayerModel playerModel;
    [Tooltip("Điền 1 cho phím Q, điền 2 cho phím E")]
    public int slotIndex; 

    // CLICK ĐỂ SOI THÔNG TIN
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (GlitchDNAUIManager.Instance != null)
                GlitchDNAUIManager.Instance.SelectGraftSlot(slotIndex);
        }
    }

    // NHẬN DIỆN THẢ GEN VÀO
    public void OnDrop(PointerEventData eventData)
    {
        GameObject dropped = eventData.pointerDrag;
        if (dropped == null) return;

        DraggableItem dragItem = dropped.GetComponent<DraggableItem>();
        
        if (dragItem != null && dragItem.glitchDNAInstance != null)
        {
            if (slotIndex == 1 && playerModel.currentLevel < 40) { Debug.LogWarning("Chưa đạt Cấp 40!"); return; }
            if (slotIndex == 2 && playerModel.currentLevel < 45) { Debug.LogWarning("Chưa đạt Cấp 45!"); return; }

            // --- SỬA LỖI 3 (MẤT GEN): Tìm chính xác cục Gen cũ đang nằm trong ô để thu hồi ---
            DraggableItem[] existingItems = transform.GetComponentsInChildren<DraggableItem>();
            foreach (DraggableItem oldItem in existingItems)
            {
                if (oldItem != dragItem && oldItem.glitchDNAInstance != null)
                {
                    InventoryManager.Instance.ownedGenes.Add(oldItem.glitchDNAInstance);
                    Destroy(oldItem.gameObject); 
                }
            }

            // Gỡ Gen mới khỏi túi và cắm vào Khe
            InventoryManager.Instance.ownedGenes.Remove(dragItem.glitchDNAInstance);
            dragItem.parentAfterDrag = transform;
            dragItem.transform.SetParent(transform); 
            
            if (GlitchDNAUIManager.Instance != null)
                GlitchDNAUIManager.Instance.RefreshGeneList();
        }
    }

    // BỘ NÃO TỰ ĐỘNG CẢM NHẬN (ĐEO / THÁO) VÀ ẨN HIỆN UI
    private void OnTransformChildrenChanged()
    {
        if (playerModel == null) return;

        // --- SỬA LỖI 1 (CHỈ SỐ BẰNG 0): Ép tìm đúng cục DraggableItem, phớt lờ hình nền trang trí ---
        DraggableItem itemInside = GetComponentInChildren<DraggableItem>();

        if (itemInside == null)
        {
            // Ô BỊ TRỐNG
            if (slotIndex == 1) { playerModel.equippedGen1 = null; playerModel.activeSkill1 = null; }
            else if (slotIndex == 2) { playerModel.equippedGen2 = null; playerModel.activeSkill2 = null; }
        }
        else if (itemInside.glitchDNAInstance != null)
        {
            // Ô CÓ ĐỒ
            if (slotIndex == 1) { playerModel.equippedGen1 = itemInside.glitchDNAInstance; playerModel.activeSkill1 = itemInside.glitchDNAInstance.activeSkill; }
            else if (slotIndex == 2) { playerModel.equippedGen2 = itemInside.glitchDNAInstance; playerModel.activeSkill2 = itemInside.glitchDNAInstance.activeSkill; }
        }

        playerModel.RecalculateStats();

        if (GlitchDNAUIManager.Instance != null && GlitchDNAUIManager.Instance.gameObject.activeInHierarchy) 
        {
            GlitchDNAUIManager.Instance.SelectGraftSlot(slotIndex);
        }
    }
}