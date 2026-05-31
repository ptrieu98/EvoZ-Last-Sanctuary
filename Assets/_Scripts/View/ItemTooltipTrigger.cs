using UnityEngine;
using UnityEngine.EventSystems;

public class ItemTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Dữ liệu tự truyền vào (Dùng cho ô chế tạo tĩnh)")]
    public ItemData staticItemData;
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        DraggableItem dragItem = GetComponent<DraggableItem>();
        if (dragItem != null)
        {
            // 1. Nếu là Trang bị Gacha (Mũ, Áo, Quần...)
            if (dragItem.equipInstance != null && dragItem.equipInstance.baseTemplate != null)
            {
                ShowEquipmentTooltip(dragItem.equipInstance);
                return;
            }
            // 2. MỚI: Nếu là Mã Gen Khuyết (Glitch DNA)
            if (dragItem.glitchDNAInstance != null && dragItem.glitchDNAInstance.activeSkill != null)
            {
                ShowGlitchDNATooltip(dragItem.glitchDNAInstance);
                return;
            }
            // 3. Nếu là Đồ vật thường (Súng, Đạn, Máu, Sắt, Tinh Hạch...)
            if (dragItem.itemData != null)
            {
                ShowBasicItemTooltip(dragItem.itemData);
                return;
            }
        }

        if (staticItemData != null)
        {
            ShowBasicItemTooltip(staticItemData);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (TooltipManager.Instance != null) TooltipManager.Instance.HideTooltip();
    }

    // ==========================================
    // ĐẠI TỪ ĐIỂN DỊCH CHỈ SỐ SANG TIẾNG VIỆT
    // ==========================================
    private string DichChiSo(string tenChiSoGoc)
    {
        switch (tenChiSoGoc)
        {
            case "FlatHealth": case "Health": case "MaxHealth": return "Máu Tối Đa";
            case "HealthPct": return "% Máu Tối Đa";
            case "FlatStamina": case "Stamina": return "Thể Lực";
            case "StaminaPct": return "% Thể Lực";
            case "FlatArmor": case "Armor": return "Giáp Bảo Vệ";
            case "ArmorPct": return "% Giáp Bảo Vệ";
            case "FlatDamage": case "Damage": return "Sát Thương";
            case "DamagePct": return "% Sát Thương";
            case "FlatMoveSpeed": case "MoveSpeed": return "Tốc Độ Di Chuyển";
            case "MoveSpeedPct": return "% Tốc Độ Di Chuyển";
            case "FlatAttackSpeed": case "AttackSpeed": return "Tốc Độ Đánh";
            case "AttackSpeedPct": return "% Tốc Độ Đánh";
            case "FlatCritRate": case "CritRate": case "CritChance": return "Tỉ Lệ Chí Mạng";
            case "CritRatePct": case "CritChancePct": return "% Tỉ Lệ Chí Mạng";
            case "FlatCritDamage": case "CritDamage": return "Sát Thương Chí Mạng";
            case "CritDamagePct": return "% Sát Thương Chí Mạng";
            case "FlatLifesteal": case "Lifesteal": return "Hút Máu";
            case "LifestealPct": return "% Hút Máu";
            case "FlatReloadSpeed": case "ReloadSpeed": return "Tốc Độ Nạp Đạn";
            case "ReloadSpeedPct": return "% Tốc Độ Nạp Đạn";
            case "FlatFireRate": case "FireRate": return "Tốc Độ Bắn";
            case "FireRatePct": return "% Tốc Độ Bắn";
            case "FlatAccuracy": case "Accuracy": return "Độ Chính Xác";
            case "AccuracyPct": return "% Độ Chính Xác";
            case "FlatMagSize": case "MagSize": return "Cỡ Băng Đạn";
            case "MagSizePct": return "% Cỡ Băng Đạn";
            
            // TỪ KHÓA MỚI CỦA GEN
            case "CooldownReduction": return "Giảm Hồi Chiêu (%)";

            default: return tenChiSoGoc; 
        }
    }

    // ==========================================
    // CÁC HÀM HIỂN THỊ GIAO DIỆN
    // ==========================================
    private void ShowBasicItemTooltip(ItemData data)
    {
        string title = $"<b>{data.itemName}</b>";
        string content = "";

        if (data.category == ItemCategory.Weapon) content = $"Sát thương: {data.damage}\nTầm đánh: {data.attackRange}";
        else if (data.category == ItemCategory.Material) content = "Nguyên liệu chế tạo";
        else if (data.category == ItemCategory.Consumable) content = "Vật phẩm tiêu hao";
        else if (data.category == ItemCategory.Ammo) content = "Đạn dược";
        else if (data.category == ItemCategory.Core)
        {
            if (data.coreTier == CoreTier.Mutant)
            {
                content = $"<color=#FF00FF><b>[ TINH HẠCH BIẾN DỊ ]</b></color>\n<color=white>{data.mutantEffectDescription}</color>";
            }
            else
            {
                string tierName = data.coreTier.ToString().Replace("Tier", "Bậc ");
                string elementColor = "#FFFFFF"; string elementName = "Vô Hệ";
                switch (data.coreElement)
                {
                    case CoreElement.Fire: elementColor = "#FF4500"; elementName = "Hệ Lửa"; break;
                    case CoreElement.Water: elementColor = "#00BFFF"; elementName = "Hệ Nước"; break;
                    case CoreElement.Earth: elementColor = "#8B4513"; elementName = "Hệ Đất"; break;
                }
                content = $"Đẳng cấp: <color=yellow>{tierName}</color>\nThuộc tính: <color={elementColor}>{elementName}</color>";
            }
        }

        if (TooltipManager.Instance != null) TooltipManager.Instance.ShowTooltip(title, content);
    }

    private void ShowEquipmentTooltip(EquipmentInstance equip)
    {
        string title = $"<b><color=#FFD700>{equip.baseTemplate.itemName}</color></b> [{equip.starLevel} Sao]";
        string content = "";

        foreach (StatModifier stat in equip.basicStats) content += $"+{stat.value:F0} {DichChiSo(stat.statType.ToString())}\n";
        foreach (StatModifier stat in equip.specialStats) content += $"+{stat.value:F1}% {DichChiSo(stat.statType.ToString())}\n";

        if (TooltipManager.Instance != null) TooltipManager.Instance.ShowTooltip(title, content);
    }

    // --- MỚI THÊM: HÀM HIỂN THỊ TOOLTIP DÀNH RIÊNG CHO MÃ GEN ---
    private void ShowGlitchDNATooltip(GlitchDNAInstance dna)
    {
        string tierStr = dna.tier == GenTier.Mutant ? "<color=red>Dị Biến</color>" : $"Bậc {(int)dna.tier + 1}";
        string title = $"<b><color=#8A2BE2>{dna.genName}</color></b> [{tierStr}]";
        
        string content = "Loại: Kỹ năng Chủ động\n";
        
        string coreTag1 = dna.line1.isCoreStat ? "<color=red>(Cốt Lõi)</color>" : "";
        content += $"- {DichChiSo(dna.line1.statType.ToString())}: {dna.line1.currentValue} {coreTag1}\n";
        
        string coreTag2 = dna.line2.isCoreStat ? "<color=red>(Cốt Lõi)</color>" : "";
        content += $"- {DichChiSo(dna.line2.statType.ToString())}: {dna.line2.currentValue} {coreTag2}";

        if (TooltipManager.Instance != null) TooltipManager.Instance.ShowTooltip(title, content);
    }
}