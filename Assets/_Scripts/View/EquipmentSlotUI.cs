using UnityEngine;
using UnityEngine.EventSystems;

public class EquipmentSlotUI : MonoBehaviour, IDropHandler
{
    [Header("=== CÀI ĐẶT Ô TRANG BỊ ===")]
    public EquipmentType targetSlotType; // Định danh ô này là Helmet, Chest, Pants, hay Shoes
    public PlayerModel playerModel;      // Liên kết tới Model người chơi để cập nhật chỉ số

    public void OnDrop(PointerEventData eventData)
    {
        // 1. LẤY THÔNG TIN VẬT PHẨM ĐANG ĐƯỢC KÉO THẢ
        DraggableItem draggedItem = eventData.pointerDrag.GetComponent<DraggableItem>();
        if (draggedItem == null) return;

        // BẢO VỆ 1: Ô này chỉ chấp nhận trang bị có chỉ số thực thể (đã qua đúc/roll gacha)
        if (draggedItem.equipInstance == null)
        {
            Debug.LogWarning("<color=orange>Hệ thống: Ô này chỉ chấp nhận trang bị cá nhân hóa có cấp sao!</color>");
            return;
        }

        // BẢO VỆ 2: Kiểm tra trùng khớp phân loại (Mũ phải vào ô Mũ, Áo phải vào ô Áo)
        if (draggedItem.equipInstance.baseTemplate.equipmentType != this.targetSlotType)
        {
            Debug.LogWarning($"<color=red>Sai vị trí! Bạn không thể mặc trang bị loại [{draggedItem.equipInstance.baseTemplate.equipmentType}] vào ô [{this.targetSlotType}]</color>");
            return; // Từ chối nhận đồ, vật phẩm tự động dội về vị trí cũ
        }

        // 2. TIẾN HÀNH XỬ LÝ DỮ LIỆU & LOGIC ĐỔI ĐỒ
        int slotIndex = (int)targetSlotType; // Chuyển đổi Enum thành chỉ số mảng [0 đến 3]
        
        DraggableItem currentItemInSlot = GetComponentInChildren<DraggableItem>();
        Transform sourceParent = draggedItem.parentAfterDrag; // Nơi món đồ vừa được kéo đi từ đó
        EquipmentSlotUI sourceEquipSlot = sourceParent.GetComponent<EquipmentSlotUI>();

        // TRƯỜNG HỢP A: Ô trang bị hiện tại ĐÃ CÓ ĐỒ MẶC SẴN (Tiến hành hoán đổi)
        if (currentItemInSlot != null)
        {
            // Trả vật phẩm UI cũ về lại vị trí cũ của vật phẩm mới
            currentItemInSlot.transform.SetParent(sourceParent);
            currentItemInSlot.transform.localPosition = Vector3.zero;
            currentItemInSlot.InitializeItem();

            // Nếu đổi đồ giữa 2 Ô trang bị với nhau, cập nhật lại ô dữ liệu cũ
            if (sourceEquipSlot != null)
            {
                playerModel.equippedGear[(int)sourceEquipSlot.targetSlotType] = currentItemInSlot.equipInstance;
            }
        }
        // TRƯỜNG HỢP B: Ô trang bị hiện tại ĐANG TRỐNG
        else
        {
            // Nếu vật phẩm mới được kéo đi từ một Ô trang bị khác, hãy dọn sạch ô cũ đó
            if (sourceEquipSlot != null)
            {
                playerModel.equippedGear[(int)sourceEquipSlot.targetSlotType] = null;
            }
        }

        // 3. ĐỒNG BỘ VẬT PHẨM MỚI VÀO Ô
        playerModel.equippedGear[slotIndex] = draggedItem.equipInstance;
        draggedItem.parentAfterDrag = transform; // Đổi mục tiêu neo giữ UI khi kết thúc Drag

        // 4. CẬP NHẬT CHỈ SỐ LÕI & GIAO DIỆN HÌNH ẢNH
        // Gọi hàm toán học tính toán lại chỉ số ARPG (Máu, Giáp, Chí mạng, Hút máu...)
        playerModel.RecalculateStats();

        // Ra lệnh làm mới bảng chữ số hiển thị trên màn hình
        StatDisplayUI statUI = FindObjectOfType<StatDisplayUI>();
        if (statUI != null)
        {
            statUI.UpdateStatPanel();
        }

        Debug.Log($"<color=green>Đã trang bị thành công: {draggedItem.equipInstance.baseTemplate.itemName} vào ô {targetSlotType}</color>");
    }
}