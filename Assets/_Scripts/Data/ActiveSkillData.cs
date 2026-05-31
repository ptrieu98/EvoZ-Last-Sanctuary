using UnityEngine;

public enum ActiveSkillType { Attack, Defense, Utility, Summon }

[CreateAssetMenu(fileName = "NewGlitchDNA", menuName = "EvoZ/Glitch DNA (Mã Gen Khuyết)")]
public class ActiveSkillData : ScriptableObject
{
    [Header("=== THÔNG TIN MÃ GEN ===")]
    public string skillName = "Tên Mã Gen Khuyết";
    [TextArea(3, 5)] 
    public string description = "Mô tả kỹ năng gốc...";
    public Sprite icon;
    public ActiveSkillType skillType;

    [Header("=== CHỈ SỐ HOẠT ĐỘNG ===")]
    public float cooldownTime = 15f; // Thời gian hồi chiêu
    public float staminaCost = 30f;  // Thể lực tiêu hao khi dùng chiêu
    public float baseDamage = 50f;   // Sát thương gốc
    public float actionDuration = 0.5f; // Thời gian đứng khựng tung chiêu
    public string animationTriggerName = "CastSpell"; // Tên Trigger Animation của Player

    [Header("=== TƯƠNG TÁC HỆ LỬA (FIRE) ===")]
    [TextArea(2, 3)] 
    public string fireSynergyDescription = "Mô tả hiệu ứng Lửa (VD: Để lại vũng dung nham)...";
    public GameObject fireVFX; // Hiệu ứng lửa

    [Header("=== TƯƠNG TÁC HỆ NƯỚC (WATER) ===")]
    [TextArea(2, 3)] 
    public string waterSynergyDescription = "Mô tả hiệu ứng Nước (VD: Đóng băng địch)...";
    public GameObject waterVFX; // Hiệu ứng băng/nước

    [Header("=== TƯƠNG TÁC HỆ ĐẤT (EARTH) ===")]
    [TextArea(2, 3)] 
    public string earthSynergyDescription = "Mô tả hiệu ứng Đất (VD: Hất tung, tạo khiên)...";
    public GameObject earthVFX; // Hiệu ứng đất/đá

    [Header("=== NGUYÊN BẢN (CHƯA THỨC TỈNH) ===")]
    public GameObject defaultVFX; // Dùng khi Player mang hệ "None" (Dưới cấp 20)
}