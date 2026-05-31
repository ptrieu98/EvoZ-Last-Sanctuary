using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GlitchDNAUIManager : MonoBehaviour
{
    // Cấp quyền kết nối toàn cục
    public static GlitchDNAUIManager Instance;

    [Header("=== KẾT NỐI HỆ THỐNG ===")]
    public PlayerModel playerModel;

    [Header("=== KHE CẤY GHÉP (SLOTS) ===")]
    public Button slot1Btn;
    public TextMeshProUGUI slot1LockText; 
    
    public Button slot2Btn;
    public TextMeshProUGUI slot2LockText; 

    [Header("=== TRẠM TẨY LUYỆN (REROLL STATION) ===")]
    public GameObject rerollStationPanel; 
    public TextMeshProUGUI genNameText;
    
    public TextMeshProUGUI line1NameText;  
    public TextMeshProUGUI line1ValueText; 
    public Button line1LockButton;
    public Image line1LockIcon; 
    
    public TextMeshProUGUI line2NameText;
    public TextMeshProUGUI line2ValueText;
    public Button line2LockButton;
    public Image line2LockIcon;

    [Header("=== NÚT TẨY LUYỆN ===")]
    public Button rerollTypeBtn;   
    public TextMeshProUGUI rerollTypeCostText;
    
    public Button rerollValueBtn;  
    public TextMeshProUGUI rerollValueCostText;

    [Header("=== TÀI NGUYÊN (RESOURCES) ===")]
    public Sprite lockOpenSprite;
    public Sprite lockClosedSprite;
    public Sprite lockPermanentSprite; 

    private GlitchDNAInstance currentSelectedGen;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        if (line1LockButton != null) line1LockButton.onClick.AddListener(() => ToggleLock(1));
        if (line2LockButton != null) line2LockButton.onClick.AddListener(() => ToggleLock(2));
        
        if (rerollTypeBtn != null) rerollTypeBtn.onClick.AddListener(OnRerollTypeClicked);
        if (rerollValueBtn != null) rerollValueBtn.onClick.AddListener(OnRerollValueClicked);
    }

    public void OpenPanel()
    {
        gameObject.SetActive(true);
        CheckSlotUnlocks();
        currentSelectedGen = null;
        if (rerollStationPanel != null) rerollStationPanel.SetActive(false); 
    }

    private void CheckSlotUnlocks()
    {
        if (playerModel == null) return;

        bool isSlot1Unlocked = playerModel.currentLevel >= 40;
        slot1Btn.interactable = isSlot1Unlocked;
        if (slot1LockText != null) slot1LockText.gameObject.SetActive(!isSlot1Unlocked);

        bool isSlot2Unlocked = playerModel.currentLevel >= 45;
        slot2Btn.interactable = isSlot2Unlocked;
        if (slot2LockText != null) slot2LockText.gameObject.SetActive(!isSlot2Unlocked);
    }

    public void SelectGenForReroll(GlitchDNAInstance gen)
    {
        currentSelectedGen = gen;
        
        if (rerollStationPanel != null) 
        {
            // NẾU GEN CÓ TỒN TẠI THÌ BẬT (TRUE), NẾU GEN BỊ THÁO RA (NULL) THÌ TẮT (FALSE)
            rerollStationPanel.SetActive(gen != null); 
        }
        
        if (gen != null) RefreshRerollStationUI();
    }

    private void RefreshRerollStationUI()
    {
        if (currentSelectedGen == null) return;

        if (genNameText != null) genNameText.text = currentSelectedGen.genName;

        UpdateStatLineUI(currentSelectedGen.line1, line1NameText, line1ValueText, line1LockButton, line1LockIcon);
        UpdateStatLineUI(currentSelectedGen.line2, line2NameText, line2ValueText, line2LockButton, line2LockIcon);

        UpdateCostUI();
    }

    private void UpdateStatLineUI(GenStatLine line, TextMeshProUGUI nameText, TextMeshProUGUI valueText, Button lockBtn, Image lockIcon)
    {
        if (line == null || nameText == null || valueText == null) return;

        nameText.text = GetStatName(line.statType);
        
        // --- ĐÃ SỬA: Thay chữ 'gray' bằng mã màu '#808080' ---
        valueText.text = $"<color=#00FF00>{line.currentValue}</color> <size=80%><color=#808080>(Max {line.maxValue})</color></size>";

        if (line.isCoreStat)
        {
            lockBtn.interactable = false;
            if (lockIcon != null) lockIcon.sprite = lockPermanentSprite;
            nameText.alpha = 1f; valueText.alpha = 1f;
        }
        else
        {
            lockBtn.interactable = true;
            if (lockIcon != null) lockIcon.sprite = line.isLocked ? lockClosedSprite : lockOpenSprite;
            
            float alphaAlpha = line.isLocked ? 0.5f : 1f; 
            nameText.alpha = alphaAlpha; valueText.alpha = alphaAlpha;
        }
    }

    private void ToggleLock(int lineIndex)
    {
        if (currentSelectedGen == null) return;

        if (lineIndex == 1 && !currentSelectedGen.line1.isCoreStat) currentSelectedGen.line1.isLocked = !currentSelectedGen.line1.isLocked;
        else if (lineIndex == 2 && !currentSelectedGen.line2.isCoreStat) currentSelectedGen.line2.isLocked = !currentSelectedGen.line2.isLocked;

        RefreshRerollStationUI(); 
    }

    private int CalculateCost()
    {
        if (currentSelectedGen == null) return 0;
        int lockedCount = 0;
        if (currentSelectedGen.line1.isLocked) lockedCount++;
        if (currentSelectedGen.line2.isLocked) lockedCount++;
        return 1 + lockedCount;
    }

    private void UpdateCostUI()
    {
        if (currentSelectedGen == null || InventoryManager.Instance == null) return;

        int cost = CalculateCost(); 
        int currentTier3 = InventoryManager.Instance.GetCoreCount(CoreTier.Tier3); 
        int currentTier2 = InventoryManager.Instance.GetCoreCount(CoreTier.Tier2); 

        string colorT3 = currentTier3 >= cost ? "#FFFFFF" : "#FF0000";
        string colorT2 = currentTier2 >= cost ? "#FFFFFF" : "#FF0000";

        if (rerollTypeCostText != null) rerollTypeCostText.text = $"Cost: {cost} TH3\n<color={colorT3}>(Có: {currentTier3})</color>";
        if (rerollValueCostText != null) rerollValueCostText.text = $"Cost: {cost} TH2\n<color={colorT2}>(Có: {currentTier2})</color>";

        bool canRerollType = (!currentSelectedGen.line1.isLocked && !currentSelectedGen.line1.isCoreStat) || 
                             (!currentSelectedGen.line2.isLocked && !currentSelectedGen.line2.isCoreStat);
        rerollTypeBtn.interactable = canRerollType;
    }

    private void OnRerollTypeClicked()
    {
        if (currentSelectedGen == null || InventoryManager.Instance == null) return;
        int cost = CalculateCost();
        if (!InventoryManager.Instance.ConsumeCore(CoreTier.Tier3, cost)) return;

        currentSelectedGen.RerollStatTypes();
        RefreshRerollStationUI();
    }

    private void OnRerollValueClicked()
    {
        if (currentSelectedGen == null || InventoryManager.Instance == null) return;
        int cost = CalculateCost();
        if (!InventoryManager.Instance.ConsumeCore(CoreTier.Tier2, cost)) return;

        currentSelectedGen.RerollStatValues();
        RefreshRerollStationUI();
    }

    private string GetStatName(GenStatType type)
    {
        switch (type)
        {
            case GenStatType.Damage: return "Sát Thương";
            case GenStatType.MaxHealth: return "Máu Tối Đa";
            case GenStatType.Armor: return "Giáp";
            case GenStatType.CritChance: return "Chí Mạng (%)";
            case GenStatType.CooldownReduction: return "Giảm Hồi Chiêu (%)";
            default: return "Không xác định";
        }
    }
}

// Bắt click chuột đẩy dữ liệu lên Trạm Tẩy
public class GlitchDNAClickHandler : MonoBehaviour, IPointerClickHandler
{
    public GlitchDNAInstance genData;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (GlitchDNAUIManager.Instance != null && genData != null)
            {
                GlitchDNAUIManager.Instance.SelectGenForReroll(genData);
            }
        }
    }
}