using UnityEngine;

// Phân loại thế lực chính
public enum EnemyCategory { Basic, Mutant, WaveBoss, RegionalBoss }

// Phân cấp cho nhóm Zombie cơ bản
public enum BasicEnemyTier { Normal, Medium, High }

// Thuộc tính nguyên tố (Dành cho Boss và Mutant)
public enum EnemyElement { None, Fire, Water, Earth }

[CreateAssetMenu(fileName = "New Enemy Data", menuName = "EvoZ/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("=== THÔNG TIN CHUNG ===")]
    public string enemyName;
    public EnemyCategory category;
    public GameObject prefab;

    [Header("=== PHÂN LOẠI CHI TIẾT ===")]
    public BasicEnemyTier basicTier;
    public EnemyElement element;

    [Header("=== CHỈ SỐ CƠ BẢN ===")]
    public float maxHealth = 100f;
    public float moveSpeed = 2.5f;
    public float damage = 10f;
    public float attackRange = 1.5f;
    public float attackCooldown = 1.5f; 

    [Header("=== CƠ CHẾ AI ĐẶC BIỆT ===")]
    [Range(0f, 1f)]
    public float fleeHealthThreshold = 0f; 
    public float fleeSpeedMultiplier = 1.5f;

    [Header("=== HIỆU ỨNG (VFX & SFX) ===")]
    public AudioClip aggroSound; 
    public AudioClip attackSound; 
    public AudioClip deathSound; 
    public GameObject hitVFX;    

    [Header("=== PHẦN THƯỞNG (LOOT) ===")]
    public int expReward = 10;
    
    // ĐÂY LÀ BẢNG LOOT TABLE CHUYÊN NGHIỆP
    public LootDrop[] lootTable; 
}

// ==========================================
// ĐỊNH NGHĨA CẤU TRÚC 1 MÓN ĐỒ TRONG BẢNG LOOT
// ==========================================
[System.Serializable]
public class LootDrop
{
    [Tooltip("Mô hình 3D rơi ra môi trường")]
    public GameObject dropPrefab; 
    
    [Tooltip("Dữ liệu vật phẩm (Linh hồn)")]
    public ItemData itemData;     
    
    [Tooltip("Tỉ lệ rớt (0% đến 100%)")]
    [Range(0f, 100f)]
    public float dropChance = 100f; 
    
    [Tooltip("Số lượng ít nhất")]
    public int minAmount = 1;     
    
    [Tooltip("Số lượng tối đa")]
    public int maxAmount = 1;     
}