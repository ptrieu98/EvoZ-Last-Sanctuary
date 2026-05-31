using UnityEngine;
using UnityEngine.UI;

public class CraftingRowUI : MonoBehaviour
{
    [Header("=== CÔNG THỨC DÒNG NÀY ===")]
    public CraftingRecipe recipe;
    public ItemData zombieCoreRequired; 
    
    [Tooltip("Phẩm chất tinh hạch truyền vào lò đúc (Ảnh hưởng tỉ lệ ra sao. VD: 1, 2, 3)")]
    public int coreQuality = 1;

    [Header("=== GIAO DIỆN HÀNG NGANG ===")]
    public Image targetEquipIcon;                
    public Transform materialsContainer;         
    public GameObject materialRequirementPrefab; 
    public Button craftButton;                   

    private void Start()
    {
        SetupRow();
    }

    private void OnEnable()
    {
        // Khắc phục lỗi Race Condition: Chỉ chạy Setup khi InventoryManager đã sẵn sàng
        if (InventoryManager.Instance != null)
        {
            SetupRow();
        }
    }

    public void SetupRow()
    {
        // Kiểm tra an toàn: Nếu chưa kéo Công thức hoặc InventoryManager chưa chạy thì bỏ qua
        if (recipe == null || InventoryManager.Instance == null) return;

        // 1. GẮN ẢNH ICON CHO SẢN PHẨM ĐẦU DÒNG (Mũ/Áo/Quần...)
        if (targetEquipIcon != null && recipe.targetEquipment != null)
        {
            targetEquipIcon.sprite = recipe.targetEquipment.icon;

            // TỰ ĐỘNG HÓA TOOLTIP: Tự tìm hoặc tự thêm cấu hình hiển thị thông tin khi di chuột vào sản phẩm
            ItemTooltipTrigger targetTooltip = targetEquipIcon.GetComponent<ItemTooltipTrigger>();
            if (targetTooltip == null) targetTooltip = targetEquipIcon.gameObject.AddComponent<ItemTooltipTrigger>();
            targetTooltip.staticItemData = recipe.targetEquipment;
        }

        // 2. DỌN DẸP AN TOÀN: Xóa các ô nguyên liệu cũ bằng vòng lặp ngược để tránh lỗi UI xếp đè lên nhau
        for (int i = materialsContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(materialsContainer.GetChild(i).gameObject);
        }

        // Biến đánh dấu xem người chơi có đủ toàn bộ nguyên liệu để chế tạo không
        bool canCraft = true;

        // 3. ĐẺ RA CÁC Ô NGUYÊN LIỆU CƠ BẢN (Sắt, Vải, Gỗ...) Ở GIỮA HÀNG
        foreach (ResourceCost cost in recipe.requiredMaterials)
        {
            if (cost.material == null) continue;

            // Tạo bản sao từ Prefab ô nguyên liệu
            GameObject reqObj = Instantiate(materialRequirementPrefab, materialsContainer);
            MaterialRequirementUI reqUI = reqObj.GetComponent<MaterialRequirementUI>();
            
            // Quét túi đồ đếm xem đang có bao nhiêu cục nguyên liệu này
            int currentAmount = InventoryManager.Instance.GetMaterialCount(cost.material);
            
            // Đẩy vào hàm Setup để hiển thị số lượng (Ví dụ: 5 / 10) và tự gán Tooltip nguyên liệu
            reqUI.Setup(cost.material, currentAmount, cost.amount);

            // Nếu chỉ cần thiếu 1 loại nguyên liệu, khóa lệnh chế tạo ngay
            if (currentAmount < cost.amount) 
            {
                canCraft = false;
            }
        }

        // 4. ĐẺ RA Ô YÊU CẦU TINH HẠCH ZOMBIE (Nếu công thức này bắt buộc có)
        if (zombieCoreRequired != null)
        {
            GameObject reqObj = Instantiate(materialRequirementPrefab, materialsContainer);
            MaterialRequirementUI reqUI = reqObj.GetComponent<MaterialRequirementUI>();
            
            int coreAmount = InventoryManager.Instance.GetMaterialCount(zombieCoreRequired);
            reqUI.Setup(zombieCoreRequired, coreAmount, 1); // Yêu cầu luôn luôn là 1 viên tinh hạch
            
            if (coreAmount < 1) 
            {
                canCraft = false;
            }
        }

        // 5. ĐIỀU KHIỂN NÚT CHẾ TẠO Ở CUỐI HÀNG
        if (craftButton != null)
        {
            craftButton.interactable = canCraft; // Đủ đồ thì cho bấm, thiếu đồ thì khóa nút
            
            // Tự động thêm CanvasGroup để làm mờ hẳn nút đi khi thiếu đồ (Nhìn rất chuyên nghiệp)
            CanvasGroup cg = craftButton.GetComponent<CanvasGroup>();
            if (cg == null) cg = craftButton.gameObject.AddComponent<CanvasGroup>();
            
            cg.alpha = canCraft ? 1f : 0.3f; // Sáng rõ 100% nếu đủ đồ, mờ còn 30% nếu thiếu đồ
        }
    }

    // Gắn hàm này vào sự kiện OnClick() của nút craftButton
    public void OnCraftButtonClicked()
    {
        if (recipe == null || InventoryManager.Instance == null) return;

        // ==========================================
        // BẢO VỆ 2: KIỂM TRA CHỖ TRỐNG TRONG TÚI ĐỒ (Quan trọng nhất)
        // ==========================================
        bool hasSpace = false;
        foreach (InventorySlot slot in InventoryManager.Instance.inventorySlots)
        {
            if (slot.transform.childCount == 0)
            {
                hasSpace = true;
                break;
            }
        }

        if (!hasSpace)
        {
            Debug.LogWarning("Túi đồ đã đầy! Hủy bỏ lệnh chế tạo để tránh mất nguyên liệu.");
            if (TooltipManager.Instance != null) 
                TooltipManager.Instance.ShowTooltip("Cảnh báo", "<color=red>Túi đồ đã đầy! Cần dọn dẹp trước.</color>");
            return; 
        }

        // ==========================================
        // BẢO VỆ 3: KIỂM TRA LẠI NGUYÊN LIỆU (Chống Spam Click hack đồ)
        // ==========================================
        foreach (ResourceCost cost in recipe.requiredMaterials)
        {
            if (InventoryManager.Instance.GetMaterialCount(cost.material) < cost.amount) return;
        }
        if (zombieCoreRequired != null && InventoryManager.Instance.GetMaterialCount(zombieCoreRequired) < 1) return;

        // THỰC THI CHẾ TẠO
        // 1. Trừ nguyên liệu
        foreach (ResourceCost cost in recipe.requiredMaterials)
        {
            InventoryManager.Instance.ConsumeMaterial(cost.material, cost.amount);
        }
        if (zombieCoreRequired != null) InventoryManager.Instance.ConsumeMaterial(zombieCoreRequired, 1);

        // 2. Sinh đồ và nhét vào túi 
        // 2. Sinh đồ dựa trên tỉ lệ % được cấu hình trong Công thức
EquipmentInstance newEquip = EquipmentGenerator.GenerateRandomEquipment(recipe.targetEquipment, recipe.GetStarRates());
        InventoryManager.Instance.AddEquipment(newEquip);

        // 3. ÉP TOÀN BỘ BẢNG CHẾ TẠO CẬP NHẬT LẠI
        // (Ví dụ bạn có 5 Sắt, chế Áo hết 5 Sắt thì dòng chế Mũ cũng phải lập tức tắt nút đi)
        CraftingRowUI[] allRows = FindObjectsOfType<CraftingRowUI>();
        foreach(CraftingRowUI row in allRows)
        {
            row.SetupRow();
        }
    }
}