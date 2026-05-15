using UnityEngine;

// Định nghĩa các loại vũ khí
public enum WeaponType 
{ 
    Melee,  // Cận chiến (Gậy, Kiếm)
    Ranged  // Tầm xa (Súng)
}

[CreateAssetMenu(fileName = "New Item", menuName = "EvoZ/Item")]
public class ItemData : ScriptableObject
{
    [Header("=== THÔNG TIN CƠ BẢN ===")]
    public string itemName;
    public Sprite icon; 

    [Header("=== PHÂN LOẠI VŨ KHÍ ===")]
    public WeaponType weaponType; // Chọn loại vũ khí ở đây

    [Header("=== CHỈ SỐ VŨ KHÍ ===")]
    public int damage;
    public float attackRange = 1.5f; // Tầm xa (Gậy để 1.5, Súng để 50)
    public GameObject weaponPrefab; 
}