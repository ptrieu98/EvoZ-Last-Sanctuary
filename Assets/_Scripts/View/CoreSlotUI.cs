using UnityEngine;
using UnityEngine.UI;

public class CoreSlotUI : MonoBehaviour
{
    [Header("=== THÀNH PHẦN GIAO DIỆN ===")]
    public Image coreIcon;
    public Image frameBorder; 
    public Button btn;

    public void Setup(ItemData data)
    {
        if (coreIcon != null && data != null)
        {
            coreIcon.sprite = data.icon;
            // BẢN VÁ: Ép buộc Unity reset lại màu trắng (tránh bị đen hoặc tàng hình)
            coreIcon.color = Color.white; 
            coreIcon.gameObject.SetActive(true);
        }

        if (frameBorder != null && data != null)
        {
            switch (data.coreElement)
            {
                case CoreElement.Fire:
                    ColorUtility.TryParseHtmlString("#FF4500", out Color fireColor);
                    frameBorder.color = fireColor;
                    break;
                case CoreElement.Water:
                    ColorUtility.TryParseHtmlString("#00BFFF", out Color waterColor);
                    frameBorder.color = waterColor;
                    break;
                case CoreElement.Earth:
                    ColorUtility.TryParseHtmlString("#8B4513", out Color earthColor);
                    frameBorder.color = earthColor;
                    break;
                default:
                    frameBorder.color = Color.white;
                    break;
            }
        }
    }
}