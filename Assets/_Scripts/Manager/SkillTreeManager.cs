using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class SkillTreeBranch
{
    public string branchID;          
    public string branchDisplayName; 
    public bool isAwakeningBranch;   
    
    [Header("=== GIAO DIỆN NHÁNH ===")]
    public GameObject pagePanel;     
    public Button tabButton;         
    public GameObject lockIcon;      
    
    [Header("=== RÀO CẢN & KỸ NĂNG ===")]
    public GameObject barrierT1T2;
    public GameObject barrierT2T3;
    public List<SkillNodeUI> nodes; 

    // Tính tổng điểm tiêu hao dựa theo Tầng
    public int GetSpSpent()
    {
        int spent = 0;
        foreach (var node in nodes) 
        { 
            if (node != null && node.data != null) 
            {
                int cost = 1;
                if (node.data.tier == SkillTier.Tier2) cost = 2;
                else if (node.data.tier == SkillTier.Tier3) cost = 3;
                spent += (node.currentLevel * cost);
            } 
        }
        return spent;
    }
}

public class SkillTreeManager : MonoBehaviour
{
    [Header("=== TÀI NGUYÊN & HIỂN THỊ ===")]
    public PlayerModel player;
    public TextMeshProUGUI spText; 
    public TextMeshProUGUI headerText; 
    public TextMeshProUGUI previewWarningText; 

    [Header("=== GIAO DIỆN TABS CHÍNH ===")]
    public Button btnMainSurvival;         
    public Button btnMainAwakening;        
    public Button btnMainGenetic; // <--- 1. THÊM NÚT TAB TIẾN HÓA GEN
    public TextMeshProUGUI txtMainAwakening; 
    public GameObject elementSubMenuPanel; 
    
    [Header("=== GIAO DIỆN TRANG GEN ===")]
    public GameObject geneticPanel; // <--- 2. KÉO GLITCH DNA PANEL VÀO ĐÂY

    [Header("=== DANH SÁCH CÁC NHÁNH KỸ NĂNG ===")]
    public List<SkillTreeBranch> branches; 

    public int spToUnlockTier2 = 5;
    public int spToUnlockTier3 = 12;

    private SkillTreeBranch currentActiveBranch;
    private int lastRecordedLevel = -1;
    private string lastRecordedElement = "";

    private void Start()
    {
        if (btnMainSurvival != null) btnMainSurvival.onClick.AddListener(ClickMainSurvival);
        if (btnMainAwakening != null) btnMainAwakening.onClick.AddListener(ClickMainAwakening);
        
        // 3. LẮNG NGHE SỰ KIỆN NÚT GEN
        if (btnMainGenetic != null) btnMainGenetic.onClick.AddListener(ClickMainGenetic); 

        foreach (var branch in branches)
        {
            if (branch == null) continue;
            if (branch.tabButton != null)
            {
                branch.tabButton.onClick.RemoveAllListeners();
                branch.tabButton.onClick.AddListener(() => SwitchTab(branch.branchID));
            }
            if (branch.nodes != null)
            {
                foreach (var node in branch.nodes)
                {
                    if (node != null)
                    {
                        node.SetupUI();
                        node.btn.onClick.RemoveAllListeners();
                        node.btn.onClick.AddListener(() => TryUpgradeSkill(node, branch));
                    }
                }
            }
        }
        ClickMainSurvival(); 
    }

    private void Update()
    {
        if (player != null)
        {
            if (player.currentLevel != lastRecordedLevel || player.currentElement != lastRecordedElement)
            {
                lastRecordedLevel = player.currentLevel;
                lastRecordedElement = player.currentElement;
                RefreshTree();
            }
        }
    }

    public void ClickMainSurvival()
    {
        if (elementSubMenuPanel != null) elementSubMenuPanel.SetActive(false); 
        if (geneticPanel != null) geneticPanel.SetActive(false); // Ẩn trang Gen
        SwitchTab("Survival");
    }

    public void ClickMainAwakening()
    {
        // 1. Ẩn trang Tiến Hóa Gen
        if (geneticPanel != null) geneticPanel.SetActive(false); 

        // 2. Kiểm tra an toàn: Chưa có hệ (rỗng hoặc "None")
        bool hasNoElement = string.IsNullOrEmpty(player.currentElement) || player.currentElement == "None";

        if (hasNoElement)
        {
            // Bật Sub Menu
            if (elementSubMenuPanel != null) elementSubMenuPanel.SetActive(true);
            
            // Tìm và bật 1 tab Thức tỉnh bất kỳ để làm nền (Preview)
            if (currentActiveBranch == null || !currentActiveBranch.isAwakeningBranch)
            {
                foreach (var branch in branches)
                {
                    if (branch.isAwakeningBranch) 
                    { 
                        SwitchTab(branch.branchID); 
                        break; 
                    }
                }
            }

            // [CHỐT CHẶN BẢO VỆ]: Ép Sub Menu bật lại SAU KHI SwitchTab
            // Phòng trường hợp Sub Menu bị lồng sai và bị SwitchTab tắt nhầm
            if (elementSubMenuPanel != null) 
            {
                elementSubMenuPanel.SetActive(true);
                elementSubMenuPanel.transform.SetAsLastSibling(); // Ép nổi lên lớp cao nhất, đè lên mọi UI khác!
            }
        }
        else
        {
            // Đã có hệ -> Tắt Sub Menu, nhảy thẳng vào tab của Hệ
            if (elementSubMenuPanel != null) elementSubMenuPanel.SetActive(false);
            SwitchTab(player.currentElement);
        }
    }

    // --- 4. HÀM XỬ LÝ KHI BẤM TAB TIẾN HÓA GEN ---
    // --- HÀM XỬ LÝ KHI BẤM TAB TIẾN HÓA GEN ---
    public void ClickMainGenetic()
    {
        if (elementSubMenuPanel != null) elementSubMenuPanel.SetActive(false);
        
        foreach (var branch in branches)
            if (branch.pagePanel != null) branch.pagePanel.SetActive(false);
        
        if (geneticPanel != null) 
        {
            geneticPanel.SetActive(true);
            GlitchDNAUIManager dnaUI = geneticPanel.GetComponent<GlitchDNAUIManager>();
            if (dnaUI != null) dnaUI.OpenPanel();
        }

        if (headerText != null) headerText.text = "TIẾN HÓA GEN MÃ KHUYẾT";
        
        // --- MỚI: Ẩn hẳn cụm Text SP đi cho gọn ---
        if (spText != null && spText.transform.parent != null) 
            spText.transform.parent.gameObject.SetActive(false); 
            
        if (previewWarningText != null) previewWarningText.gameObject.SetActive(false);
        currentActiveBranch = null; 
    }

    public void SwitchTab(string targetBranchID)
    {
        // --- MỚI: Bật lại cụm Text SP khi quay về các tab kỹ năng khác ---
        if (spText != null && spText.transform.parent != null) 
            spText.transform.parent.gameObject.SetActive(true);

        foreach (var branch in branches)
        {
            if (branch.pagePanel != null) branch.pagePanel.SetActive(false);
            if (branch.branchID == targetBranchID)
            {
                if (branch.pagePanel != null) branch.pagePanel.SetActive(true);
                currentActiveBranch = branch;
            }
        }
        RefreshTree();
    }

    public void TryUpgradeSkill(SkillNodeUI node, SkillTreeBranch branch)
    {
        if (player == null || node.currentLevel >= node.data.maxLevel) return; 
        if (IsPreviewOnly(branch)) return;

        int cost = 1;
        if (node.data.tier == SkillTier.Tier2) cost = 2;
        else if (node.data.tier == SkillTier.Tier3) cost = 3;

        if (branch.isAwakeningBranch && player.awakeningPoints < cost) return;
        if (!branch.isAwakeningBranch && player.survivalPoints < cost) return;

        int totalSpSpent = branch.GetSpSpent();
        if (IsNodeLockedByPoints(node, totalSpSpent)) return;
        if (IsNodeLockedByExclusivity(node, branch)) return;

        int maxAllowedLevel = Mathf.CeilToInt(player.currentLevel / 2f);
        if (node.currentLevel >= maxAllowedLevel && player.currentLevel < 20) return;

        if (branch.isAwakeningBranch) player.awakeningPoints -= cost;
        else player.survivalPoints -= cost;

        node.currentLevel++; 
        ApplySkillEffect(node.data);
        RefreshTree();
    }

    private bool IsNodeLockedByPoints(SkillNodeUI node, int totalSpSpent)
    {
        if (node.data.tier == SkillTier.Tier2 && totalSpSpent < spToUnlockTier2) return true;
        if (node.data.tier == SkillTier.Tier3 && totalSpSpent < spToUnlockTier3) return true;
        return false;
    }

    private bool IsNodeLockedByExclusivity(SkillNodeUI node, SkillTreeBranch branch)
    {
        if (node.data.tier == SkillTier.Tier1) return false;

        foreach (var otherNode in branch.nodes)
        {
            if (otherNode != null && otherNode != node)
            {
                if (otherNode.data.tier == node.data.tier && otherNode.currentLevel > 0) return true; 
            }
        }
        return false;
    }

    private bool IsPreviewOnly(SkillTreeBranch branch)
    {
        if (!branch.isAwakeningBranch) return false; 
        if (player.currentLevel < 20) return true;   
        if (player.currentElement != branch.branchID) return true; 
        return false;
    }

    public void RefreshTree()
    {
        if (player == null) return;

        if (txtMainAwakening != null)
        {
            if (player.currentElement == "None") txtMainAwakening.text = "THỨC TỈNH";
            else
            {
                string elName = "VÔ HỆ";
                if (player.currentElement == "Fire") elName = "HỆ LỬA";
                else if (player.currentElement == "Water") elName = "HỆ NƯỚC";
                else if (player.currentElement == "Earth") elName = "HỆ ĐẤT";
                txtMainAwakening.text = $"THỨC TỈNH {elName}";
            }
        }

        foreach (var branch in branches)
        {
            if (branch.isAwakeningBranch && branch.lockIcon != null)
            {
                bool isLocked = (player.currentLevel < 20 || player.currentElement != branch.branchID);
                branch.lockIcon.SetActive(isLocked);
            }
        }

        if (currentActiveBranch != null)
        {
            if (headerText != null) headerText.text = currentActiveBranch.branchDisplayName;
            if (spText != null)
            {
                if (currentActiveBranch.isAwakeningBranch) spText.text = $"ĐIỂM ĐỘT PHÁ: <color=orange>{player.awakeningPoints}</color>";
                else spText.text = $"ĐIỂM SINH TỒN: <color=green>{player.survivalPoints}</color>";
            }

            bool isPreview = IsPreviewOnly(currentActiveBranch);
            if (previewWarningText != null) 
            {
                previewWarningText.gameObject.SetActive(isPreview);
                if (isPreview)
                {
                    if (player.currentLevel < 20) previewWarningText.text = "CHẾ ĐỘ XEM TRƯỚC (Cần đạt Cấp 20 để Thức Tỉnh)";
                    else previewWarningText.text = "CHẾ ĐỘ XEM TRƯỚC (Bạn chưa thức tỉnh hệ này)";
                }
            }

            int totalSpSpent = currentActiveBranch.GetSpSpent();
            if (currentActiveBranch.barrierT1T2 != null) currentActiveBranch.barrierT1T2.SetActive(totalSpSpent < spToUnlockTier2);
            if (currentActiveBranch.barrierT2T3 != null) currentActiveBranch.barrierT2T3.SetActive(totalSpSpent < spToUnlockTier3);

            foreach (var node in currentActiveBranch.nodes)
            {
                if (node != null) 
                {
                    bool lockedByPoints = IsNodeLockedByPoints(node, totalSpSpent);
                    bool lockedByExclusivity = IsNodeLockedByExclusivity(node, currentActiveBranch);
                    node.UpdateUI(lockedByPoints, lockedByExclusivity, isPreview);
                }
            }
        }
    }

    private void ApplySkillEffect(SkillData data)
    {
        switch (data.effectType)
        {
            case SkillEffectType.IncreaseMaxHealth: player.baseMaxHealth += data.valuePerLevel; break;
            case SkillEffectType.IncreaseMaxStamina: player.maxStamina += data.valuePerLevel; break;
            case SkillEffectType.IncreaseBaseDamage: player.baseDamage += data.valuePerLevel; break;
            case SkillEffectType.IncreaseBaseArmor: player.baseArmor += data.valuePerLevel; break;
            case SkillEffectType.IncreaseMoveSpeed: player.baseMoveSpeed += data.valuePerLevel; break;
            case SkillEffectType.IncreaseStaminaRegen: player.staminaRegenRate += data.valuePerLevel; break;
            case SkillEffectType.IncreaseCritChance: player.baseCritChance += data.valuePerLevel; break;
            case SkillEffectType.IncreaseLifesteal: player.baseLifesteal += data.valuePerLevel; break;

            case SkillEffectType.IncreaseCritDamage: player.baseCritDamage += data.valuePerLevel; break;
            case SkillEffectType.IncreaseArmorPenetration: player.baseArmorPenetration += data.valuePerLevel; break;
            case SkillEffectType.IncreaseDodgeChance: player.baseDodgeChance += data.valuePerLevel; break;
            case SkillEffectType.IncreaseAccuracy: player.baseAccuracy += data.valuePerLevel; break;
            case SkillEffectType.IncreaseAntiCrit: player.antiCritChance += data.valuePerLevel; break;

            // --- BẬT CÔNG TẮC HỆ LỬA & TRUYỀN VFX TỪ DATA SANG PLAYER ---
            case SkillEffectType.UnlockCorpseExplosion: player.hasCorpseExplosion = true; player.activeCorpseExplosionVFX = data.skillVFX; break;
            case SkillEffectType.UnlockIgnite: player.hasIgnite = true; player.activeIgniteVFX = data.skillVFX; break;
            case SkillEffectType.UnlockMelt: player.hasMelt = true; player.activeMeltVFX = data.skillVFX; break;
            case SkillEffectType.UnlockHellfireTrail: player.hasHellfireTrail = true; player.activeFireTrailVFX = data.skillVFX; break;
            case SkillEffectType.UnlockPhoenix: player.hasPhoenix = true; player.activePhoenixVFX = data.skillVFX; break;
            case SkillEffectType.UnlockMeteor: player.hasMeteor = true; player.activeMeteorVFX = data.skillVFX; break;

            case SkillEffectType.UnlockFrostbite: player.hasFrostbite = true; break;
            case SkillEffectType.UnlockBloodShield: player.hasBloodShield = true; break;
            case SkillEffectType.UnlockMist: player.hasMist = true; break;
            case SkillEffectType.UnlockBlizzard: player.hasBlizzard = true; break;
            case SkillEffectType.UnlockBubbleShield: player.hasBubbleShield = true; break;
            case SkillEffectType.UnlockIllusion: player.hasIllusion = true; break;

            case SkillEffectType.UnlockThorns: player.hasThorns = true; break;
            case SkillEffectType.UnlockTremor: player.hasTremor = true; break;
            case SkillEffectType.UnlockStoneSkin: player.hasStoneSkin = true; break;
            case SkillEffectType.UnlockQuake: player.hasQuake = true; break;
            case SkillEffectType.UnlockTombstone: player.hasTombstone = true; break;
            case SkillEffectType.UnlockTitanGrasp: player.hasTitanGrasp = true; break;
        }
        player.RecalculateStats(); 
        player.currentHealth = player.maxHealth; 
    }
}