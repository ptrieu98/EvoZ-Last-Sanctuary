using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems; 

public class SkillNodeUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public SkillData data;
    [HideInInspector] public int currentLevel = 0;
    
    [Header("=== KẾT NỐI UI CHÍNH NÓ ===")]
    public Image iconImage;
    public TextMeshProUGUI levelText;
    public GameObject lockOverlay; 
    public Button btn;

    // 3 Biến trạng thái để báo cho Tooltip biết tại sao nút bị khóa
    private bool isLockedByPoints = false;
    private bool isLockedByExclusivity = false;
    private bool isPreviewOnly = false;

    public void SetupUI()
    {
        if (data != null && iconImage != null) iconImage.sprite = data.icon;
        UpdateUI(false, false, false);
    }

    // Hàm này đồng bộ chuẩn xác với SkillTreeManager mới
    public void UpdateUI(bool lockedPoints, bool lockedExclusivity, bool preview)
    {
        isLockedByPoints = lockedPoints;
        isLockedByExclusivity = lockedExclusivity;
        isPreviewOnly = preview;
        
        if (data == null) return;
        
        if (levelText != null)
        {
            levelText.text = currentLevel >= data.maxLevel ? "<color=#FFD700>MAX</color>" : $"{currentLevel}/{data.maxLevel}";
        }
        
        bool anyLock = isLockedByPoints || isLockedByExclusivity || isPreviewOnly;
        
        if (lockOverlay != null) lockOverlay.SetActive(anyLock);
        if (btn != null) btn.interactable = (!anyLock && currentLevel < data.maxLevel);
    }

    // ==========================================
    // XỬ LÝ TOOLTIP THÔNG MINH CHO CẢ 2 DẠNG KỸ NĂNG
    // ==========================================
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (data == null || TooltipManager.Instance == null) return;

        string title = $"<b><color=#FFD700>{data.skillName}</color></b> (Cấp {currentLevel}/{data.maxLevel})";
        string content = $"{data.description}\n\n";

        // --- TÍNH TOÁN ĐIỂM TIÊU HAO ĐỂ HIỂN THỊ ---
        int cost = 1;
        if (data.tier == SkillTier.Tier2) cost = 2;
        else if (data.tier == SkillTier.Tier3) cost = 3;

        if (isPreviewOnly)
        {
            content += "<color=orange>⚠️ Chế độ xem trước (Chưa thức tỉnh hệ này)!</color>";
        }
        else if (isLockedByExclusivity)
        {
            content += "<color=red>⚠️ Đã bị khóa do bạn nâng cấp nhánh đối nghịch!</color>";
        }
        else if (isLockedByPoints)
        {
            content += "<color=red>⚠️ Yêu cầu đủ điểm ở Tầng trên để mở khóa!</color>";
        }
        else
        {
            if (data.effectType.ToString().StartsWith("Unlock"))
            {
                if (currentLevel > 0) content += "<color=#00BFFF>⭐ Đã kích hoạt hiệu ứng bị động!</color>";
                else 
                {
                    content += "<color=gray>Chưa mở khóa hiệu ứng này.</color>\n";
                    content += $"Tiêu hao: <color=orange>{cost} Điểm</color>"; // Hiển thị điểm
                }
            }
            else
            {
                float currentValue = currentLevel * data.valuePerLevel;
                float nextValue = (currentLevel + 1) * data.valuePerLevel;

                if (currentLevel > 0) content += $"Hiệu ứng hiện tại: <color=green>+{currentValue}</color>\n";
                if (currentLevel < data.maxLevel) 
                {
                    content += $"Cấp tiếp theo: <color=#00BFFF>+{nextValue}</color>\n";
                    content += $"Tiêu hao: <color=orange>{cost} Điểm</color>"; // Hiển thị điểm
                }
                else content += "<color=#FFD700>⭐ Đã đạt cấp tối đa!</color>";
            }
        }

        TooltipManager.Instance.ShowTooltip(title, content);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (TooltipManager.Instance != null) TooltipManager.Instance.HideTooltip();
    }

    private void OnDisable()
    {
        if (TooltipManager.Instance != null) TooltipManager.Instance.HideTooltip();
    }
}