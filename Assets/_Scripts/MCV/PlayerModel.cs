using UnityEngine;

public class PlayerModel : MonoBehaviour
{
    [Header("Chỉ số Di chuyển")]
    public float moveSpeed = 6f;

    [Header("Chỉ số Sinh tồn")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float maxStamina = 100f;
    public float currentStamina;
    public float maxVirus = 100f;
    public float currentVirus = 0f; // Bắt đầu game chưa bị nhiễm

    [Header("Chỉ số Lướt (Dash)")]
    public float dashMultiplier = 3.5f;   // Lướt nhanh gấp 3.5 lần bình thường
    public float dashDuration = 0.2f;     // Thời gian lướt (0.2 giây)
    public float dashStaminaCost = 25f;   // Mỗi lần lướt tốn 25 Thể lực
    public float staminaRegenRate = 15f;  // Mỗi giây hồi 15 Thể lực

    [Header("Chỉ số Nhảy vách đá (Ledge Jump)")]
    public float jumpHeight = 1.5f;   // Độ nảy lên trên không trước khi rơi xuống

    [Header("=== CHIẾN ĐẤU ===")]
    // Mảng lưu trữ 4 vũ khí trang bị
    public ItemData[] equippedWeapons = new ItemData[4]; 
    public int activeWeaponIndex = 0; // Ô đang chọn (0 đến 3)
    
    public bool isAttacking = false;
    public float attackDuration = 0.6f; // Tổng thời gian của 1 đòn chém

    // Tiện ích để Controller luôn lấy đúng vũ khí đang được chọn trên tay
    public ItemData CurrentWeapon => equippedWeapons[activeWeaponIndex];

    void Start()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;
    }
}