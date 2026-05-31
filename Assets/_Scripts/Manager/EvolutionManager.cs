using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class EvolutionRequirement
{
    public int lockLevel; 
    public CoreTier requiredTier; 
    [TextArea(2, 3)]
    public string customPrompt = "Cần Tiến Hóa để thăng cấp!"; 
}

public class EvolutionManager : MonoBehaviour
{
    [Header("=== LIÊN KẾT HỆ THỐNG ===")]
    public PlayerModel player;
    public InventoryManager inventory;

    [Header("=== DANH SÁCH TỰ THIẾT LẬP (INSPECTOR) ===")]
    public List<EvolutionRequirement> evolutionRequirements;

    [Header("=== GIAO DIỆN Ô TIẾN HÓA ===")]
    public Button btnEvolutionSlot;
    public TextMeshProUGUI txtEvoPrompt;

    [Header("=== BẢNG CHỌN TINH HẠCH ===")]
    public GameObject panelCoreSelection;
    public Transform coreContainer;
    public GameObject coreUIPrefab; 

    [Header("=== BẢNG XÁC NHẬN NGUYÊN TỐ (LV 20) ===")]
    public GameObject panelConfirm;
    public TextMeshProUGUI txtConfirmMessage;
    
    private ItemData selectedCore;
    private int lastRecordedLevel = -1;
    private bool lastEvolved1 = false;
    private bool lastEvolved2 = false;

    private void OnEnable()
    {
        if (player != null)
        {
            lastRecordedLevel = player.currentLevel;
            lastEvolved1 = player.hasEvolvedTier1;
            lastEvolved2 = player.hasEvolvedTier2;
        }
        CheckEvolutionStatus();
    }

    private void Update()
    {
        if (player != null)
        {
            if (player.currentLevel != lastRecordedLevel || player.hasEvolvedTier1 != lastEvolved1 || player.hasEvolvedTier2 != lastEvolved2)
            {
                lastRecordedLevel = player.currentLevel;
                lastEvolved1 = player.hasEvolvedTier1;
                lastEvolved2 = player.hasEvolvedTier2;
                CheckEvolutionStatus(); 
            }
        }
    }

    private EvolutionRequirement GetCurrentRequirement()
    {
        if (evolutionRequirements == null) return null;
        foreach (var req in evolutionRequirements)
        {
            if (player.currentLevel == req.lockLevel)
            {
                if (player.evolvedLevels.Contains(req.lockLevel)) continue;
                return req;
            }
        }
        return null;
    }

    public void CheckEvolutionStatus()
    {
        if (panelCoreSelection != null) panelCoreSelection.SetActive(false);
        if (panelConfirm != null) panelConfirm.SetActive(false);

        EvolutionRequirement currentReq = GetCurrentRequirement();

        if (currentReq != null)
        {
            if (btnEvolutionSlot != null) 
            {
                btnEvolutionSlot.gameObject.SetActive(true);
                btnEvolutionSlot.interactable = true; 
            }
            if (txtEvoPrompt != null)
            {
                txtEvoPrompt.gameObject.SetActive(true);
                txtEvoPrompt.text = currentReq.customPrompt;
            }
        }
        else
        {
            if (btnEvolutionSlot != null) btnEvolutionSlot.gameObject.SetActive(false);
            if (txtEvoPrompt != null) txtEvoPrompt.gameObject.SetActive(false);
        }
    }

    public void OpenCoreSelection()
    {
        EvolutionRequirement currentReq = GetCurrentRequirement();
        if (currentReq == null) return;

        if (panelCoreSelection != null) panelCoreSelection.SetActive(true);
        foreach (Transform child in coreContainer) Destroy(child.gameObject);

        List<ItemData> foundCores = new List<ItemData>();
        HashSet<string> uniqueElements = new HashSet<string>();
        
        foreach (InventorySlot slot in inventory.inventorySlots)
        {
            DraggableItem item = slot.GetComponentInChildren<DraggableItem>();
            if (item != null && item.itemData != null && item.itemData.category == ItemCategory.Core)
            {
                if (item.itemData.coreTier == currentReq.requiredTier)
                {
                    string elementKey = item.itemData.coreElement.ToString();
                    if (item.itemData.coreTier == CoreTier.Mutant) elementKey = item.itemData.itemName;

                    if (!uniqueElements.Contains(elementKey))
                    {
                        uniqueElements.Add(elementKey);
                        foundCores.Add(item.itemData);
                    }
                }
            }
        }

        if (foundCores.Count == 0)
        {
            if (panelCoreSelection != null) panelCoreSelection.SetActive(false);
            return;
        }

        foreach (ItemData core in foundCores)
        {
            GameObject coreBtnObj = Instantiate(coreUIPrefab, coreContainer);
            
            // ==========================================
            // BẢN VÁ: GỌI CORES LOT UI ĐỂ VẼ ICON
            // ==========================================
            CoreSlotUI slotUI = coreBtnObj.GetComponent<CoreSlotUI>();
            if (slotUI != null)
            {
                slotUI.Setup(core); // Tự động gán Icon và viền hệ
                if (slotUI.btn != null)
                {
                    slotUI.btn.onClick.AddListener(() => OnCoreSelected(core));
                }
            }
            else
            {
                Debug.LogWarning("Prefab của bạn bị thiếu script CoreSlotUI!");
            }

            // Gắn Tooltip hiển thị chỉ số
            ItemTooltipTrigger tooltip = coreBtnObj.GetComponent<ItemTooltipTrigger>();
            if (tooltip == null) tooltip = coreBtnObj.AddComponent<ItemTooltipTrigger>();
            tooltip.staticItemData = core; 
        }
    }

    public void OnCoreSelected(ItemData core)
    {
        selectedCore = core;
        if (panelCoreSelection != null) panelCoreSelection.SetActive(false);

        if (core.coreElement != CoreElement.None)
        {
            if (panelConfirm != null) panelConfirm.SetActive(true);
            if (txtConfirmMessage != null)
            {
                txtConfirmMessage.text = $"Chọn loại tinh hạch này sẽ ảnh hưởng đến hướng phát triển kỹ năng sau này! Bạn có chắc chắn muốn thức tỉnh hệ <color=orange>{core.itemName.ToUpper()}</color> không?";
            }
        }
        else
        {
            ExecuteEvolution(); 
        }
    }

    public void ExecuteEvolution()
    {
        if (selectedCore == null) return;
        inventory.ConsumeMaterial(selectedCore, 1);
        string element = selectedCore.coreElement.ToString();
        player.RegisterEvolution(player.currentLevel, element);
        
        if (panelConfirm != null) panelConfirm.SetActive(false);
        CheckEvolutionStatus();
    }

    public void CancelEvolution()
    {
        selectedCore = null;
        if (panelConfirm != null) panelConfirm.SetActive(false);
        if (panelCoreSelection != null) panelCoreSelection.SetActive(false);
    }
}