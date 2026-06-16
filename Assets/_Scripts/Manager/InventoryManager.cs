using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance; 
    public static bool isUIOpen = false;

    [Header("=== GIAO DIỆN TỔNG (BẤM TAB ĐỂ MỞ) ===")]
    public GameObject mainUIPanel; 
    public GameObject hotbarPanel;    

    [Header("=== CÁC TRANG BÊN TRONG (TABS) ===")]
    public GameObject statsPanel;     
    public GameObject inventoryPanel; 
    
    [Header("=== NÚT BẤM ĐỂ ĐỔI MÀU ===")]
    public Image statsTabImage;       
    public Image inventoryTabImage;   
    public Color activeColor = new Color(1f, 0.8f, 0f, 1f);       
    public Color inactiveColor = new Color(0.8f, 0.8f, 0.8f, 1f); 

    [Header("=== CÀI ĐẶT TÚI ĐỒ CHÍNH ===")]
    public InventorySlot[] inventorySlots; 

    // --- MỚI THÊM: QUẢN LÝ BALO PHỤ (DẠNG LIST ĐỘNG) ---
    [Header("=== KHO CHỨA GEN ===")]
    public List<GlitchDNAInstance> ownedGenes = new List<GlitchDNAInstance>(); 

    [Header("=== PREFAB ===")]
    public GameObject itemUIPrefab; 

    [HideInInspector] 
    public List<EquipSlotSync> allEquipSlots = new List<EquipSlotSync>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (mainUIPanel != null) mainUIPanel.SetActive(false);
        if (hotbarPanel != null) hotbarPanel.SetActive(true);
    }

    public void ToggleMainUI()
    {
        bool isOpening = !mainUIPanel.activeSelf; 
        isUIOpen = isOpening;
        
        if (mainUIPanel != null) mainUIPanel.SetActive(isOpening);
        if (hotbarPanel != null) hotbarPanel.SetActive(!isOpening);

        if (TooltipManager.Instance != null) TooltipManager.Instance.HideTooltip();

        if (isOpening) OpenStatsTab();
    }

    public void CloseMainUI()
    {
        if (mainUIPanel != null && mainUIPanel.activeSelf) ToggleMainUI();
    }

    public void OpenStatsTab()
    {
        if (statsPanel != null) statsPanel.SetActive(true);
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
        if (statsTabImage != null) statsTabImage.color = activeColor;
        if (inventoryTabImage != null) inventoryTabImage.color = inactiveColor;
    }

    public void OpenInventoryTab()
    {
        if (statsPanel != null) statsPanel.SetActive(false);
        if (inventoryPanel != null) inventoryPanel.SetActive(true);
        if (statsTabImage != null) statsTabImage.color = inactiveColor;
        if (inventoryTabImage != null) inventoryTabImage.color = activeColor;
    }

    public bool AddItem(ItemData newItemData)
    {
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            InventorySlot slot = inventorySlots[i];
            if (slot.transform.childCount == 0)
            {
                GameObject itemObj = Instantiate(itemUIPrefab, slot.transform);
                DraggableItem draggableItem = itemObj.GetComponent<DraggableItem>();
                if (draggableItem != null)
                {
                    draggableItem.itemData = newItemData;
                    draggableItem.equipInstance = null; 
                    draggableItem.InitializeItem(); 
                }
                return true; 
            }
        }
        Debug.LogWarning("Túi đồ chính đã đầy!");
        return false; 
    }

    public bool AddEquipment(EquipmentInstance newEquip)
    {
        foreach (InventorySlot slot in inventorySlots)
        {
            if (slot.transform.childCount == 0)
            {
                GameObject itemObj = Instantiate(itemUIPrefab, slot.transform);
                DraggableItem draggableItem = itemObj.GetComponent<DraggableItem>();
                
                draggableItem.equipInstance = newEquip; 
                draggableItem.InitializeItem(); 
                
                return true; 
            }
        }
        Debug.LogWarning("Túi đồ chính đã đầy!");
        return false; 
    }

    // ==========================================
    // NHẶT MÃ GEN KHUYẾT VÀO DANH SÁCH (MỚI CẬP NHẬT)
    // ==========================================
    public bool AddGlitchDNA(GlitchDNAInstance newDNA)
    {
        // Nhặt là quăng thẳng vào Danh sách động (Không bị giới hạn 30 ô nữa)
        ownedGenes.Add(newDNA);
        
        // Cập nhật lại UI Danh sách dọc nếu bảng Tiến Hóa Gen đang mở
        if (GlitchDNAUIManager.Instance != null && GlitchDNAUIManager.Instance.gameObject.activeInHierarchy)
        {
            GlitchDNAUIManager.Instance.RefreshGeneList();
        }
        
        return true; 
    }

    // ==========================================
    // HỆ THỐNG VẬT LIỆU (MATERIAL)
    // ==========================================
    public int GetMaterialCount(ItemData materialToFind)
    {
        int total = 0;
        foreach (InventorySlot slot in inventorySlots)
        {
            DraggableItem item = slot.GetComponentInChildren<DraggableItem>();
            if (item != null && item.itemData != null && item.itemData.itemName == materialToFind.itemName)
                total += item.itemData.ammoAmount; 
        }
        return total;
    }

    public void ConsumeMaterial(ItemData materialToConsume, int amountToConsume)
    {
        int remaining = amountToConsume;
        foreach (InventorySlot slot in inventorySlots)
        {
            DraggableItem item = slot.GetComponentInChildren<DraggableItem>();
            if (item != null && item.itemData != null && item.itemData.itemName == materialToConsume.itemName)
            {
                if (!item.itemData.name.Contains("(Clone)"))
                {
                    item.itemData = ScriptableObject.Instantiate(item.itemData);
                    item.itemData.name += "(Clone)";
                }

                if (item.itemData.ammoAmount >= remaining)
                {
                    item.itemData.ammoAmount -= remaining;
                    remaining = 0;
                    item.InitializeItem(); 
                    if (item.itemData.ammoAmount <= 0) Destroy(item.gameObject);
                    break; 
                }
                else
                {
                    remaining -= item.itemData.ammoAmount;
                    Destroy(item.gameObject); 
                }
            }
        }
    }

    // ==========================================
    // XỬ LÝ TINH HẠCH CHO TRẠM TẨY LUYỆN
    // ==========================================
    public int GetCoreCount(CoreTier tierRequired)
    {
        int total = 0;
        foreach (InventorySlot slot in inventorySlots)
        {
            DraggableItem item = slot.GetComponentInChildren<DraggableItem>();
            if (item != null && item.itemData != null && item.itemData.category == ItemCategory.Core && item.itemData.coreTier == tierRequired)
                total += item.itemData.ammoAmount;
        }
        return total;
    }

    public bool ConsumeCore(CoreTier tierRequired, int amountToConsume)
    {
        if (GetCoreCount(tierRequired) < amountToConsume) return false;

        int remaining = amountToConsume;
        foreach (InventorySlot slot in inventorySlots)
        {
            DraggableItem item = slot.GetComponentInChildren<DraggableItem>();
            if (item != null && item.itemData != null && item.itemData.category == ItemCategory.Core && item.itemData.coreTier == tierRequired)
            {
                if (!item.itemData.name.Contains("(Clone)"))
                {
                    item.itemData = ScriptableObject.Instantiate(item.itemData);
                    item.itemData.name += "(Clone)";
                }

                if (item.itemData.ammoAmount >= remaining)
                {
                    item.itemData.ammoAmount -= remaining;
                    remaining = 0;
                    item.InitializeItem(); 
                    if (item.itemData.ammoAmount <= 0) Destroy(item.gameObject);
                    break; 
                }
                else
                {
                    remaining -= item.itemData.ammoAmount;
                    Destroy(item.gameObject); 
                }
            }
        }
        return true; 
    }

    // ==========================================
    // ĐỒNG BỘ TRANG BỊ
    // ==========================================
    public void RegisterEquipSlot(EquipSlotSync slot)
    {
        if (!allEquipSlots.Contains(slot)) allEquipSlots.Add(slot);
    }

    public void RefreshAllEquipSlots()
    {
        foreach (EquipSlotSync slot in allEquipSlots)
        {
            if (slot != null) slot.UpdateSlotUI();
        }
    }
}