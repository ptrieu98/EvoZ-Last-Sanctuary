using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GlitchDNASlotElement : MonoBehaviour
{
    public TextMeshProUGUI nameAndTierText; // Hiển thị: [T3] Gen Hỏa Long
    public Image iconImage;                 // Icon kỹ năng
    public Button selectButton;             

    private GlitchDNAInstance containedGen;
    private GlitchDNAUIManager uiManager;

    public void Setup(GlitchDNAInstance gen, GlitchDNAUIManager manager)
    {
        containedGen = gen;
        uiManager = manager;

        string tierStr = gen.tier == GenTier.Mutant ? "MUTANT" : $"T{(int)gen.tier + 1}";
        if (nameAndTierText != null) nameAndTierText.text = $"[{tierStr}] {gen.genName}";

        if (iconImage != null && gen.activeSkill != null)
        {
            iconImage.sprite = gen.activeSkill.icon;
            iconImage.color = Color.white;
        }

        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(OnSlotClicked);
        }
    }

    private void OnSlotClicked()
    {
        if (uiManager != null && containedGen != null)
        {
            // Bấm vào thì đưa cục Gen này qua Trạm tẩy luyện bên phải
            uiManager.SelectGenForReroll(containedGen);
        }
    }
}