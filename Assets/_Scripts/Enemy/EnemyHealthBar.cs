using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("=== THÀNH PHẦN UI ===")]
    public Image healthFill;    
    public Image borderFrame;   

    [Header("=== KHUNG VIỀN PHÂN LOẠI ===")]
    public Sprite normalFrame;  
    public Sprite mediumFrame;  
    public Sprite bossFrame;    

    private EnemyModel model;
    private Camera mainCam;

    public void Setup(EnemyModel enemyModel)
    {
        model = enemyModel;
        mainCam = Camera.main;

        if (model != null && model.data != null)
        {
            switch (model.data.category)
            {
                case EnemyCategory.Basic:
                    if (borderFrame != null && normalFrame != null) borderFrame.sprite = normalFrame;
                    break;
                // Nếu quái gốc vốn là loại Medium thì load khung Medium
                // case EnemyCategory.Medium:
                //     if (borderFrame != null && mediumFrame != null) borderFrame.sprite = mediumFrame;
                //     break;
            }
        }
    }

    // ĐÃ THÊM: Hàm ép nâng cấp khung viền (Gọi khi bị ếch ký sinh)
    public void UpgradeTier()
    {
        if (borderFrame == null) return;

        // Nếu đang là viền thường -> Nâng lên viền Trung
        if (borderFrame.sprite == normalFrame && mediumFrame != null)
        {
            borderFrame.sprite = mediumFrame;
        }
        // Nếu đang là viền Trung -> Nâng lên viền Boss
        else if (borderFrame.sprite == mediumFrame && bossFrame != null)
        {
            borderFrame.sprite = bossFrame;
        }
    }

    void LateUpdate()
    {
        if (model == null || model.isDead)
        {
            Destroy(gameObject); 
            return;
        }

        if (model.data != null && healthFill != null)
        {
            healthFill.fillAmount = model.currentHealth / model.data.maxHealth;
        }

        if (mainCam != null)
        {
            transform.LookAt(transform.position + mainCam.transform.forward);
        }
    }
}