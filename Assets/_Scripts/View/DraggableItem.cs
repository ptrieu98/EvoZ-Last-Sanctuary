using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("=== DỮ LIỆU VẬT PHẨM ===")]
    public ItemData itemData; // Dùng cho vũ khí/đạn/tiêu hao/nguyên liệu/TINH HẠCH
    
    [Tooltip("Dùng cho trang bị ARPG đã qua đúc/Roll chỉ số")]
    public EquipmentInstance equipInstance; 

    // --- MỚI THÊM: CHỨA DỮ LIỆU MÃ GEN ĐÃ ĐƯỢC RANDOM CHỈ SỐ ---
    [Tooltip("Dùng cho hệ thống Mã Gen Khuyết (Glitch DNA)")]
    public GlitchDNAInstance glitchDNAInstance;

    [Header("=== GIAO DIỆN UI ===")]
    [Tooltip("Kéo Image chứa Icon của Prefab vào đây để tránh bắt nhầm Background")]
    public Image iconImage; 
    public TextMeshProUGUI cornerText;
    
    [HideInInspector] public Transform parentAfterDrag;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void InitializeItem()
    {
        if (iconImage == null)
        {
            Debug.LogError("LỖI 1: Bạn chưa kéo Image vào ô 'Icon Image' trong Prefab!");
            return;
        }

        iconImage.color = new Color(1f, 1f, 1f, 0f); 

        // 1. KIỂM TRA NẾU ĐÂY LÀ MÃ GEN KHUYẾT
        if (glitchDNAInstance != null && glitchDNAInstance.activeSkill != null)
        {
            if (glitchDNAInstance.activeSkill.icon != null)
            {
                iconImage.sprite = glitchDNAInstance.activeSkill.icon;
                iconImage.color = Color.white;
            }
            
            // Hiện chữ T1, T2, T3 hoặc Dị Biến ở góc
            string tierStr = "T1";
            if (glitchDNAInstance.tier == GenTier.Tier2) tierStr = "T2";
            else if (glitchDNAInstance.tier == GenTier.Tier3) tierStr = "T3";
            else if (glitchDNAInstance.tier == GenTier.Mutant) tierStr = "MUTANT";
            
            SetCustomCornerText(tierStr);
        }
        // 2. KIỂM TRA NẾU ĐÂY LÀ TRANG BỊ GACHA
        else if (equipInstance != null && equipInstance.baseTemplate != null)
        {
            if (equipInstance.baseTemplate.icon != null)
            {
                iconImage.sprite = equipInstance.baseTemplate.icon;
                iconImage.color = Color.white; 
            }
            SetCustomCornerText($"{equipInstance.starLevel} Sao");
        }
        // 3. ĐÂY LÀ VẬT PHẨM BÌNH THƯỜNG (Súng, Tinh hạch, v.v...)
        else if (itemData != null)
        {
            if (itemData.icon != null)
            {
                iconImage.sprite = itemData.icon;
                iconImage.color = Color.white;
            }
            UpdateStaticCornerText();
        }
        else 
        {
            Debug.LogError("LỖI KHÔNG CÓ DỮ LIỆU!");
        }
    }

    public void UpdateStaticCornerText()
    {
        if (cornerText == null) return;

        // --- CẬP NHẬT: Hiển thị số lượng cho cả TINH HẠCH (Category = Core) ---
        if (itemData != null && (itemData.category == ItemCategory.Ammo || itemData.category == ItemCategory.Consumable || itemData.category == ItemCategory.Material || itemData.category == ItemCategory.Core))
        {
            cornerText.gameObject.SetActive(true);
            cornerText.text = itemData.ammoAmount.ToString(); 
        }
        else
        {
            cornerText.gameObject.SetActive(false);
        }
    }

    public void SetCustomCornerText(string text)
    {
        if (cornerText == null) return;
        cornerText.gameObject.SetActive(true);
        cornerText.text = text;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        parentAfterDrag = transform.parent; 
        transform.SetParent(transform.root); 
        transform.SetAsLastSibling();
        
        if (canvasGroup != null) canvasGroup.blocksRaycasts = false; 
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition; 
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(parentAfterDrag);
        transform.localPosition = Vector3.zero;
        
        if (canvasGroup != null) canvasGroup.blocksRaycasts = true; 
        InitializeItem();

        PlayerController pc = FindObjectOfType<PlayerController>();
        if (pc != null) pc.UpdateAmmoDisplay();
    }
}