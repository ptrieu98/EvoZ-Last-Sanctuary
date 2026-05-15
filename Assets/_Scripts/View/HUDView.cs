using UnityEngine;
using UnityEngine.UI;

public class HUDView : MonoBehaviour
{
    [Header("=== DỮ LIỆU TỪ NHÂN VẬT ===")]
    public PlayerModel playerModel;

    [Header("=== GIAO DIỆN UI ===")]
    public Slider healthSlider;
    public Slider virusSlider;
    
    [Tooltip("Kéo cục Stamina_Ring (Image) vào đây")]
    public Image staminaRadialImage; 

    void Start()
    {
        if (playerModel != null)
        {
            if (healthSlider != null) healthSlider.maxValue = playerModel.maxHealth;
            if (virusSlider != null) virusSlider.maxValue = playerModel.maxVirus;
        }
    }

    void Update()
    {
        if (playerModel == null) return;

        // 1. Cập nhật thanh Máu
        if (healthSlider != null) 
            healthSlider.value = playerModel.currentHealth;

        // 2. Cập nhật thanh Virus
        if (virusSlider != null) 
            virusSlider.value = playerModel.currentVirus;

        // 3. Cập nhật Vòng tròn Thể lực & Logic Ẩn/Hiện
        if (staminaRadialImage != null)
        {
            // Cập nhật giá trị vòng xoay
            staminaRadialImage.fillAmount = playerModel.currentStamina / playerModel.maxStamina;

            // KIỂM TRA: Nếu thể lực hiện tại nhỏ hơn thể lực tối đa thì hiện, ngược lại thì ẩn
            if (playerModel.currentStamina < playerModel.maxStamina)
            {
                // Bật GameObject chứa hình ảnh thể lực
                staminaRadialImage.gameObject.SetActive(true);
            }
            else
            {
                // Tắt GameObject khi thể lực đã đầy 100%
                staminaRadialImage.gameObject.SetActive(false);
            }
        }
    }
}