using UnityEngine;

public enum SkillTier { Tier1, Tier2, Tier3, Tier4 }
public enum SkillCategory { Survival, Fire, Water, Earth } 

public enum SkillEffectType 
{ 
    None, 
    // --- Chỉ Số Sinh Tồn ---
    IncreaseMaxHealth, IncreaseMaxStamina, IncreaseBaseDamage, IncreaseBaseArmor, 
    IncreaseMoveSpeed, IncreaseStaminaRegen, IncreaseCritChance, IncreaseLifesteal, 
    
    // --- Chỉ Số Đột Phá ---
    IncreaseCritDamage, IncreaseArmorPenetration, IncreaseDodgeChance, IncreaseAccuracy, IncreaseAntiCrit, 
    
    // --- Hệ Lửa ---
    UnlockCorpseExplosion, UnlockIgnite, UnlockMelt, UnlockHellfireTrail, UnlockPhoenix, UnlockMeteor,
    
    // --- Hệ Nước ---
    UnlockFrostbite, UnlockBloodShield, UnlockMist, UnlockBlizzard, UnlockBubbleShield, UnlockIllusion,

    // --- Hệ Đất ---
    UnlockThorns, UnlockTremor, UnlockStoneSkin, UnlockQuake, UnlockTombstone, UnlockTitanGrasp
}

[CreateAssetMenu(fileName = "NewSkill", menuName = "EvoZ/SkillData")]
public class SkillData : ScriptableObject
{
    [Header("=== THÔNG TIN CƠ BẢN ===")]
    public string skillName;
    [TextArea(3, 5)] public string description;
    public Sprite icon;

    [Header("=== PHÂN LOẠI & GIỚI HẠN ===")]
    public SkillCategory category; 
    public SkillTier tier;
    public int maxLevel = 5;
    
    [Header("=== HIỆU ỨNG KÍCH HOẠT ===")]
    public SkillEffectType effectType; 
    public float valuePerLevel; 

    [Header("=== HIỆU ỨNG HÌNH ẢNH (VFX) ===")]
    [Tooltip("Kéo thả Prefab vụ nổ, vệt lửa, sao băng... vào đây nếu là chiêu đặc biệt")]
    public GameObject skillVFX; 
}