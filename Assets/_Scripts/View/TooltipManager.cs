using UnityEngine;
using TMPro;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance;
    
    [Header("=== GIAO DIỆN TOOLTIP ===")]
    public GameObject tooltipWindow;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText; // Hiện mô tả hoặc thông số Máu/Giáp

    private void Awake()
    {
        Instance = this;
        tooltipWindow.SetActive(false);
    }

    private void Update()
    {
        // Ép cái bảng đi theo chuột
        if (tooltipWindow.activeSelf)
        {
            tooltipWindow.transform.position = Input.mousePosition;
        }
    }

    public void ShowTooltip(string title, string content)
    {
        titleText.text = title;
        descriptionText.text = content;
        tooltipWindow.SetActive(true);
    }

    public void HideTooltip()
    {
        tooltipWindow.SetActive(false);
    }
}