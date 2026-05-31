using UnityEngine;
using TMPro;

public class StatDisplayUI : MonoBehaviour
{
    public static StatDisplayUI Instance; // MỚI: Trạm phát sóng để gọi cập nhật thời gian thực

    public PlayerModel playerModel;

    [Header("=== NHÓM CHỈ SỐ CƠ BẢN ===")]
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI damageText;
    public TextMeshProUGUI armorText;
    public TextMeshProUGUI speedText;
    
    [Header("=== NHÓM CHỈ SỐ ĐẶC BIỆT ===")]
    public TextMeshProUGUI critText;
    public TextMeshProUGUI lifestealText;

    [Header("=== NHÓM CHỈ SỐ MỚI (ĐỘT PHÁ) ===")]
    public TextMeshProUGUI critDamageText;    
    public TextMeshProUGUI armorPenText;      
    public TextMeshProUGUI dodgeChanceText;   
    public TextMeshProUGUI accuracyText;      
    
    [Header("=== CHỈ SỐ MÃ GEN (TỰ CHỌN) ===")]
    public TextMeshProUGUI cooldownText; // Bạn có thể tạo thêm Text này trên UI và kéo vào đây

    private void Awake()
    {
        // Khởi tạo Trạm phát sóng
        if (Instance == null) Instance = this;
    }

    private void OnEnable()
    {
        UpdateStatPanel();
    }

    public void UpdateStatPanel()
    {
        if (playerModel == null) return;

        // 1. In Nhóm Sinh tồn & Cơ bản
        if (hpText != null) hpText.text = $"Máu Tối Đa: {Mathf.RoundToInt(playerModel.maxHealth)}";
        if (damageText != null) damageText.text = $"Sát Thương: {Mathf.RoundToInt(playerModel.baseDamage)} (+{Mathf.RoundToInt(playerModel.bonusDamage)})";
        if (armorText != null) armorText.text = $"Giáp Phòng Thủ: {Mathf.RoundToInt(playerModel.armor)}";
        if (speedText != null) speedText.text = $"Tốc Độ Chạy: {playerModel.moveSpeed:F1}";
        
        // 2. In Nhóm Hiệu ứng Đặc biệt (Cũ)
        if (critText != null) critText.text = $"Tỉ Lệ Chí Mạng: {playerModel.critChance:F1}%";
        if (lifestealText != null) lifestealText.text = $"Hút Máu: {playerModel.lifestealPercent:F1}%";

        // 3. In Nhóm Hiệu ứng Đặc biệt (MỚI)
        if (critDamageText != null) critDamageText.text = $"Sát Thương CM: {(playerModel.critDamageMultiplier * 100f):F0}%";
        if (armorPenText != null) armorPenText.text = $"Xuyên Giáp: {playerModel.armorPenetration:F0}";
        if (dodgeChanceText != null) dodgeChanceText.text = $"Né Tránh: {playerModel.dodgeChance:F1}%";
        if (accuracyText != null) accuracyText.text = $"Chuẩn Xác: {playerModel.accuracy:F0}";

        // 4. In Chỉ số Giảm Hồi Chiêu từ Gen
        if (cooldownText != null) cooldownText.text = $"Giảm Hồi Chiêu: {playerModel.cooldownReduction:F1}%";
    }
}