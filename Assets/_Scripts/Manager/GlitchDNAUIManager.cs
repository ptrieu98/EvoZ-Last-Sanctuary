using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class GlitchDNAUIManager : MonoBehaviour
{
    public static GlitchDNAUIManager Instance;

    [Header("=== KẾT NỐI HỆ THỐNG ===")]
    public PlayerModel playerModel;

    [Header("=== CỘT TRÁI: DANH SÁCH GEN (VERTICAL LIST) ===")]
    public Transform geneListContent;       
    public GameObject geneRowPrefab;      // Prefab hàng ngang (Có Icon + Tên)
    public GameObject itemUIPrefab;       // Prefab cục kéo thả DraggableItem

    [Header("=== KHE CẤY GHÉP (SLOTS) ===")]
    public Button slot1Btn;
    public TextMeshProUGUI slot1LockText; 
    public Button slot2Btn;
    public TextMeshProUGUI slot2LockText; 
    
    // Biến nhớ xem đang chọn soi thông tin Khe 1 hay Khe 2
    private int currentSelectedSlotIndex = 1; 

    [Header("=== TRẠM TẨY LUYỆN (BÊN PHẢI) ===")]
    public GameObject rerollStationPanel; 
    public TextMeshProUGUI genNameText;
    
    [Header("Dòng 1")]
    public Image line1StatIcon;
    public TextMeshProUGUI line1NameText;  
    public Slider line1Slider;             // MỚI: Thanh trượt
    public TextMeshProUGUI line1ValueText; 
    public Button line1LockButton;
    public Image line1LockIcon; 
    
    [Header("Dòng 2")]
    public Image line2StatIcon;
    public TextMeshProUGUI line2NameText;
    public Slider line2Slider;             // MỚI: Thanh trượt
    public TextMeshProUGUI line2ValueText;
    public Button line2LockButton;
    public Image line2LockIcon;

    [Header("Nút Tẩy Luyện")]
    public Button rerollTypeBtn;   
    public TextMeshProUGUI rerollTypeCostText;
    public Button rerollValueBtn;  
    public TextMeshProUGUI rerollValueCostText;

    [Header("=== TÀI NGUYÊN (RESOURCES) ===")]
    public Sprite lockOpenSprite;
    public Sprite lockClosedSprite;
    public Sprite lockPermanentSprite; 
    
    [Header("Icon Chỉ Số")]
    public Sprite iconDamage;
    public Sprite iconHealth;
    public Sprite iconArmor;
    public Sprite iconCrit;
    public Sprite iconCooldown;

    private GlitchDNAInstance currentSelectedGen;

    private void Awake() { if (Instance == null) Instance = this; }

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
        RefreshGeneList(); 
        
        // Mặc định bật lên thì soi Slot 1
        SelectGraftSlot(1);
    }

    // --- CỘT TRÁI: ĐẺ DANH SÁCH DỌC ---
    public void RefreshGeneList()
    {
        if (geneListContent == null || geneRowPrefab == null || itemUIPrefab == null || InventoryManager.Instance == null) return;

        // Dọn sạch rác cũ
        foreach (Transform child in geneListContent) Destroy(child.gameObject);

        foreach (GlitchDNAInstance dna in InventoryManager.Instance.ownedGenes)
        {
            // Đẻ 1 cái Row (Hàng dọc)
            GameObject rowObj = Instantiate(geneRowPrefab, geneListContent, false);
            
            // SỬ DỤNG HÀM QUÉT ĐỆ QUY XỊN ĐỂ TÌM Ô CHỨA BẤT CHẤP NÓ NẰM Ở ĐÂU
            Transform iconSlot = FindChildRecursive(rowObj.transform, "IconSlot"); 
            
            if (iconSlot != null)
            {
                // Tìm thấy ổ -> Bắt đầu đẻ cục itemUIPrefab vào
                GameObject itemVisual = Instantiate(itemUIPrefab, iconSlot, false);
                DraggableItem newDrag = itemVisual.GetComponent<DraggableItem>();
                if (newDrag != null)
                {
                    newDrag.glitchDNAInstance = dna;
                    newDrag.InitializeItem();
                }
            }
            else
            {
                // Báo lỗi đỏ chót nếu bạn lỡ đặt sai tên
                Debug.LogError($"<color=red>LỖI CỰC MẠNH: Không tìm thấy GameObject nào tên là 'IconSlot' bên trong cái GeneRowPrefab!</color>");
            }

            // Tìm thẻ chữ để ghi tên
            Transform nameTextTrans = FindChildRecursive(rowObj.transform, "NameText");
            if (nameTextTrans != null)
            {
                TextMeshProUGUI nameTxt = nameTextTrans.GetComponent<TextMeshProUGUI>();
                string tierStr = dna.tier == GenTier.Mutant ? "<color=red>[DỊ BIẾN]</color>" : $"[T{(int)dna.tier + 1}]";
                if (nameTxt != null) nameTxt.text = $"{tierStr} {dna.genName}";
            }
        }
    }

    // ==========================================
    // HÀM PHỤ TRỢ: LỤC SOÁT MỌI NGÓC NGÁCH ĐỂ TÌM OBJECT
    // ==========================================
    private Transform FindChildRecursive(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName) return child;
            
            // Nếu thằng con này có chứa con nhỏ hơn, mò tiếp vào trong
            Transform result = FindChildRecursive(child, childName);
            if (result != null) return result;
        }
        return null;
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

    // --- CHỌN KHE ĐỂ SOI THÔNG TIN ---
    public void SelectGraftSlot(int slotIndex)
    {
        currentSelectedSlotIndex = slotIndex;

        // 1. Kiểm tra xem có bị mất kết nối với Nhân vật không
        if (playerModel == null)
        {
            Debug.LogError("LỖI: Chưa gắn PlayerModel vào Glitch DNA UI Manager!");
            return;
        }
        
        // 2. Nhận diện xem Khe đó đang có Gen hay rỗng
        if (slotIndex == 1) currentSelectedGen = playerModel.equippedGen1;
        else if (slotIndex == 2) currentSelectedGen = playerModel.equippedGen2;

        // 3. Xử lý bật/tắt cái bảng bên phải
        if (rerollStationPanel != null) 
        {
            bool hasGen = (currentSelectedGen != null);
            rerollStationPanel.SetActive(hasGen); // Cú chốt tắt/bật
            
            // In báo cáo ra Console để bạn dễ theo dõi
            Debug.Log($"<color=cyan>Hệ thống: Vừa check Khe {slotIndex}. Có Gen bên trong không? {hasGen}. Đã {(hasGen ? "BẬT" : "TẮT")} bảng Reroll!</color>");
        }
        else
        {
            Debug.LogError("LỖI: Bạn chưa kéo Panel Reroll Station vào ô Inspector!");
        }

        // 4. Chỉ vẽ lại thông số nếu có Gen
        if (currentSelectedGen != null) 
        {
            RefreshRerollStationUI(false); 
        }
    }

    private void RefreshRerollStationUI(bool playAnimation)
    {
        if (currentSelectedGen == null) return;
        if (genNameText != null) genNameText.text = currentSelectedGen.genName;

        UpdateStatLineUI(currentSelectedGen.line1, line1StatIcon, line1NameText, line1Slider, line1ValueText, line1LockButton, line1LockIcon, playAnimation);
        UpdateStatLineUI(currentSelectedGen.line2, line2StatIcon, line2NameText, line2Slider, line2ValueText, line2LockButton, line2LockIcon, playAnimation);

        UpdateCostUI();
    }

    private void UpdateStatLineUI(GenStatLine line, Image iconImg, TextMeshProUGUI nameText, Slider slider, TextMeshProUGUI valueText, Button lockBtn, Image lockIcon, bool playAnim)
    {
        if (line == null || nameText == null || valueText == null || slider == null) return;

        nameText.text = GetStatName(line.statType);
        if (iconImg != null) iconImg.sprite = GetStatIcon(line.statType);

        // HIỆU ỨNG SLIDER
        if (playAnim)
        {
            StopAllCoroutines(); // Dừng các hiệu ứng đang chạy dở
            StartCoroutine(AnimateSlider(slider, valueText, line.currentValue, line.maxValue));
        }
        else
        {
            slider.maxValue = line.maxValue;
            slider.value = line.currentValue;
            valueText.text = $"<color=#00FF00>{line.currentValue}</color> <size=80%><color=#808080>(Max {line.maxValue})</color></size>";
        }

        if (line.isCoreStat)
        {
            lockBtn.interactable = false;
            if (lockIcon != null) lockIcon.sprite = lockPermanentSprite;
        }
        else
        {
            lockBtn.interactable = true;
            if (lockIcon != null) lockIcon.sprite = line.isLocked ? lockClosedSprite : lockOpenSprite;
        }
    }

    // COROUTINE: TẠO HIỆU ỨNG THANH TRƯỢT CHẠY MƯỢT MÀ KHI ROLL
    private IEnumerator AnimateSlider(Slider slider, TextMeshProUGUI text, float targetValue, float maxValue)
    {
        slider.maxValue = maxValue;
        float startValue = slider.value;
        float time = 0;
        float duration = 0.4f; // Trượt trong 0.4 giây

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            t = Mathf.Sin(t * Mathf.PI * 0.5f); // Hiệu ứng trượt nhanh rồi chậm dần ở cuối (Ease-out)
            
            float val = Mathf.Lerp(startValue, targetValue, t);
            slider.value = val;
            text.text = $"<color=yellow>{val:F1}</color> <size=80%><color=#808080>(Max {maxValue})</color></size>"; // Đang chạy thì hiện số vàng
            yield return null;
        }
        
        // Kết thúc animation -> Chốt sổ màu xanh
        slider.value = targetValue;
        text.text = $"<color=#00FF00>{targetValue}</color> <size=80%><color=#808080>(Max {maxValue})</color></size>";
    }

    private void ToggleLock(int lineIndex)
    {
        if (currentSelectedGen == null) return;
        if (lineIndex == 1 && !currentSelectedGen.line1.isCoreStat) currentSelectedGen.line1.isLocked = !currentSelectedGen.line1.isLocked;
        else if (lineIndex == 2 && !currentSelectedGen.line2.isCoreStat) currentSelectedGen.line2.isLocked = !currentSelectedGen.line2.isLocked;
        RefreshRerollStationUI(false); 
    }

    private void UpdateCostUI()
    {
        if (currentSelectedGen == null || InventoryManager.Instance == null) return;
        int cost = (currentSelectedGen.line1.isLocked ? 1 : 0) + (currentSelectedGen.line2.isLocked ? 1 : 0) + 1;
        int curT3 = InventoryManager.Instance.GetCoreCount(CoreTier.Tier3); 
        int curT2 = InventoryManager.Instance.GetCoreCount(CoreTier.Tier2); 

        if (rerollTypeCostText != null) rerollTypeCostText.text = $"Cost: {cost} TH3\n<color={(curT3 >= cost ? "#FFFFFF" : "#FF0000")}>(Có: {curT3})</color>";
        if (rerollValueCostText != null) rerollValueCostText.text = $"Cost: {cost} TH2\n<color={(curT2 >= cost ? "#FFFFFF" : "#FF0000")}>(Có: {curT2})</color>";

        rerollTypeBtn.interactable = (!currentSelectedGen.line1.isLocked && !currentSelectedGen.line1.isCoreStat) || (!currentSelectedGen.line2.isLocked && !currentSelectedGen.line2.isCoreStat);
    }

    private void OnRerollTypeClicked()
    {
        int cost = (currentSelectedGen.line1.isLocked ? 1 : 0) + (currentSelectedGen.line2.isLocked ? 1 : 0) + 1;
        if (!InventoryManager.Instance.ConsumeCore(CoreTier.Tier3, cost)) return;
        currentSelectedGen.RerollStatTypes();
        RefreshRerollStationUI(true); // true = Bật hiệu ứng trượt slider!
        playerModel.RecalculateStats(); // Đang cấy trên người nên roll xong ép nhảy sức mạnh luôn
    }

    private void OnRerollValueClicked()
    {
        int cost = (currentSelectedGen.line1.isLocked ? 1 : 0) + (currentSelectedGen.line2.isLocked ? 1 : 0) + 1;
        if (!InventoryManager.Instance.ConsumeCore(CoreTier.Tier2, cost)) return;
        currentSelectedGen.RerollStatValues();
        RefreshRerollStationUI(true);
        playerModel.RecalculateStats();
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
            default: return "Chưa rõ";
        }
    }

    private Sprite GetStatIcon(GenStatType type)
    {
        switch (type)
        {
            case GenStatType.Damage: return iconDamage;
            case GenStatType.MaxHealth: return iconHealth;
            case GenStatType.Armor: return iconArmor;
            case GenStatType.CritChance: return iconCrit;
            case GenStatType.CooldownReduction: return iconCooldown;
            default: return null;
        }
    }
}