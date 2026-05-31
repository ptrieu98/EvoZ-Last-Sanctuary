using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MaterialRequirementUI : MonoBehaviour
{
    public Image materialIcon;
    public TextMeshProUGUI amountText;

    public void Setup(ItemData material, int currentAmount, int requiredAmount)
    {
        if (materialIcon != null && material.icon != null)
        {
            materialIcon.sprite = material.icon;
        }

        // ==========================================
        // THÊM ĐOẠN NÀY: Tự động nạp dữ liệu vật phẩm vào Tooltip
        // ==========================================
        ItemTooltipTrigger tooltip = GetComponent<ItemTooltipTrigger>();
        if (tooltip == null) tooltip = gameObject.AddComponent<ItemTooltipTrigger>();
        tooltip.staticItemData = material;
        // ==========================================

        // Logic đổi màu chữ giữ nguyên hoàn toàn
        if (currentAmount >= requiredAmount)
        {
            amountText.text = $"{currentAmount} / {requiredAmount}";
            amountText.color = Color.white; 
        }
        else
        {
            amountText.text = $"<color=red>{currentAmount}</color> / {requiredAmount}";
        }
    }
}