using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance; 

    [Header("=== GIAO DIỆN UI ===")]
    public GameObject inventoryPanel; 
    public GameObject hotbarPanel;    
    public InventorySlot[] inventorySlots; 

    [Header("=== PREFAB ===")]
    public GameObject itemUIPrefab; 

    // DANH SÁCH LƯU TRỮ TOÀN BỘ 8 Ô TRANG BỊ (4 TÚI + 4 HOTBAR)
    [HideInInspector] 
    public List<EquipSlotSync> allEquipSlots = new List<EquipSlotSync>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
        if (hotbarPanel != null) hotbarPanel.SetActive(true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            bool isOpening = !inventoryPanel.activeSelf; 
            if (inventoryPanel != null) inventoryPanel.SetActive(isOpening);
            if (hotbarPanel != null) hotbarPanel.SetActive(!isOpening);
        }
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
                    draggableItem.InitializeItem(); 
                }
                return true; 
            }
        }
        Debug.LogWarning("Túi đồ đã đầy, không thể nhặt thêm!");
        return false; 
    }

    // --- HÀM CHO PHÉP CÁC Ô TỰ BÁO DANH ---
    public void RegisterEquipSlot(EquipSlotSync slot)
    {
        if (!allEquipSlots.Contains(slot)) 
        {
            allEquipSlots.Add(slot);
        }
    }

    // --- ĐỒNG BỘ CHÍNH XÁC ---
    public void RefreshAllEquipSlots()
    {
        foreach (EquipSlotSync slot in allEquipSlots)
        {
            if (slot != null) slot.UpdateSlotUI();
        }
    }
}