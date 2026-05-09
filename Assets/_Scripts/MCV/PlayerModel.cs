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

    [Header("Chỉ số Lướt (Dash)")]
    public float dashMultiplier = 3.5f;   // Lướt nhanh gấp 3.5 lần bình thường
    public float dashDuration = 0.2f;     // Thời gian lướt (0.2 giây)
    public float dashStaminaCost = 25f;   // Mỗi lần lướt tốn 25 Thể lực
    public float staminaRegenRate = 15f;  // Mỗi giây hồi 15 Thể lực

    [Header("Chỉ số Nhảy vách đá (Ledge Jump)")]
    public float jumpHeight = 1.5f;   // Độ nảy lên trên không trước khi rơi xuống

    void Start()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;
    }
}