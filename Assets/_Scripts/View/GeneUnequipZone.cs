using UnityEngine;
using UnityEngine.EventSystems;

public class GeneUnequipZone : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        GameObject dropped = eventData.pointerDrag;
        if (dropped == null) return;

        DraggableItem dragItem = dropped.GetComponent<DraggableItem>();
        
        if (dragItem != null && dragItem.glitchDNAInstance != null)
        {
            // Kiểm tra xem cục Gen này có phải đang được tháo từ Khe cấy ghép ra không
            GeneticGraftSlot oldSlot = dragItem.parentAfterDrag.GetComponent<GeneticGraftSlot>();
            
            if (oldSlot != null)
            {
                // Hoàn trả Gen về Balo danh sách
                InventoryManager.Instance.ownedGenes.Add(dragItem.glitchDNAInstance);
                Destroy(dragItem.gameObject); // Hủy cục UI cầm trên tay
                
                // Cập nhật giao diện
                if (GlitchDNAUIManager.Instance != null)
                    GlitchDNAUIManager.Instance.RefreshGeneList();
            }
        }
    }
}