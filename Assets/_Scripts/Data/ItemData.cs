using UnityEngine;

// ==========================================
// CÁC ENUM PHÂN LOẠI HỆ THỐNG
// ==========================================
public enum ItemCategory { Weapon, Ammo, Consumable, Equipment, Material, Core } 
public enum WeaponType { Melee, Ranged }
public enum AmmoType { None, Pistol, Rifle, Shotgun, Sniper } 
public enum ConsumableType { None, Bandage, Medkit, EnergyDrink } 
public enum EquipmentType { Helmet, Chest, Pants, Shoes }

// MỚI: Định nghĩa cho hệ thống Tinh hạch
public enum CoreTier { Tier1, Tier2, Tier3, Mutant } 
public enum CoreElement { None, Fire, Water, Earth }

[CreateAssetMenu(fileName = "New Item", menuName = "EvoZ/Item")]
public class ItemData : ScriptableObject
{
    [Header("=== THÔNG TIN CƠ BẢN ===")]
    public string itemName;
    public Sprite icon; 
    public ItemCategory category; 

    [Header("=== HỆ THỐNG TINH HẠCH (CORES) ===")]
    public CoreTier coreTier;
    public CoreElement coreElement;
    
    [Tooltip("Hiệu ứng đặc biệt khi dùng Tinh hạch Biến dị (Nếu có)")]
    [TextArea(2, 4)]
    public string mutantEffectDescription;

    [Header("=== PHÂN LOẠI TRANG BỊ (Dành riêng cho Equipment) ===")]
    public EquipmentType equipmentType;

    [Header("=== PHÂN LOẠI VŨ KHÍ ===")]
    public WeaponType weaponType; 

    [Header("=== CHỈ SỐ VŨ KHÍ ===")]
    public int damage;
    public float attackRange = 1.5f; 
    public GameObject weaponPrefab; 

    [Tooltip("Độ nặng của vũ khí. 1 = Bình thường, 0.7 = Đi chậm lại")]
    public float moveSpeedMultiplier = 1f;
    
    [Tooltip("Mã dáng cầm Animation (VD: 1=Cận chiến, 2=Súng thường, 3=Súng 6 nòng)")]
    public int animationStance = 2; 

    [Header("=== HIỆU ỨNG VŨ KHÍ (VFX & SFX) ===")]
    public AudioClip attackSound;       
    public GameObject muzzleFlashVFX;   
    public GameObject hitEnemyVFX;       
    public GameObject hitEnvironmentVFX; 

    [Header("=== CHỈ SỐ BẮN LIÊN THANH ===")]
    public bool isAutomatic = false;    
    public float fireRate = 0.1f;       

    [Header("=== HỆ THỐNG ĐẠN DỰ TRỮ ===")]
    public AmmoType ammoType;

    [Header("=== CHỈ SỐ SỐ LƯỢNG / SỨC CHỨA ===")]
    [Tooltip("Súng: Băng đạn tối đa | Vật phẩm/Nguyên liệu/Tinh hạch: Số lượng 1 ô (Stack)")]
    public int ammoAmount = 15; 
    
    [Tooltip("Thời gian nạp đạn (Giây)")]
    public float reloadTime = 2f;      
    public AudioClip reloadSound;      
    public AudioClip emptyClickSound;  

    [Header("=== HỆ THỐNG TIÊU HAO (CONSUMABLES) ===")]
    public ConsumableType consumableType;
    public float healAmount;             
    public float speedBoostMultiplier = 1.4f; 
    public float buffDuration = 8f;      
    public float useTime = 2f;           
    public AudioClip useSound;           
}