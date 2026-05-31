using UnityEngine;
using System.Collections.Generic;

public class PlayerModel : MonoBehaviour, IDamageable
{
    [Header("=== CẤP ĐỘ & KINH NGHIỆM ===")]
    public int currentLevel = 1;
    public float currentExp = 0f;
    public float expToNextLevel = 100f; 
    public int survivalPoints = 0;  
    public int awakeningPoints = 0; 
    public float expCurveMultiplier = 1.5f;
    public float baseExpRequirement = 100f;

    [Header("=== NÚT THẮT TIẾN HÓA ĐỘNG ===")]
    public List<int> evolvedLevels = new List<int>(); 
    public string currentElement = "None"; 
    [HideInInspector] public bool hasEvolvedTier1 = false;
    [HideInInspector] public bool hasEvolvedTier2 = false;

    [Header("=== CHỈ SỐ GỐC & CÂY KỸ NĂNG ===")]
    public float baseMaxHealth = 100f;
    public float baseMoveSpeed = 6f;
    public float baseArmor = 0f;
    public float baseDamage = 10f; 
    public float baseCritChance = 0f;  
    public float baseLifesteal = 0f;   
    public float baseCritDamage = 1.5f;     
    public float baseArmorPenetration = 0f; 
    public float baseDodgeChance = 0f;      
    public float baseAccuracy = 0f;         
    public float antiCritChance = 0f;       

    [Header("=== CHỈ SỐ THỰC TẾ ===")]
    public float maxHealth;
    public float currentHealth;
    public float moveSpeed;
    public float armor;
    public float bonusDamage; 
    public float lifestealPercent = 0f;
    public float critChance = 0f;
    public float critDamageMultiplier = 1.5f;
    public float armorPenetration = 0f;
    public float dodgeChance = 0f;
    public float accuracy = 0f;
    public float reloadSpeedMultiplier = 1f;
    public float cooldownReduction = 0f; // MỚI: Chỉ số giảm hồi chiêu từ Gen

    [Header("=== SINH TỒN KHÁC ===")]
    public float speedBuffMultiplier = 1f;
    public float actionSpeedPenalty = 0.5f;
    public float maxStamina = 100f;
    public float currentStamina;
    public float maxVirus = 100f;
    public float currentVirus = 0f;
    public float staminaRegenRate = 15f;  
    public float dashMultiplier = 3.5f;   
    public float dashDuration = 0.2f;     
    public float dashStaminaCost = 25f;   
    public float jumpHeight = 1.5f;   

    [Header("=== TRANG BỊ & VŨ KHÍ ===")]
    public EquipmentInstance[] equippedGear = new EquipmentInstance[4]; 
    public ItemData[] equippedWeapons = new ItemData[4]; 
    public int activeWeaponIndex = 0; 
    public bool isAttacking = false;
    public float attackDuration = 0.6f; 
    public bool isBusy = false; 
    public float unarmedAttackRange = 1.0f;    
    public float unarmedAttackDuration = 0.5f; 
    public int currentPunchIndex = 0; 
    [HideInInspector] public float unarmedDamage = 10f;
    public int[] currentAmmoInMag = new int[4]; 
    public bool isReloading = false;

    [Header("=== MÃ GEN KHUYẾT (KỸ NĂNG CHỦ ĐỘNG) ===")]
    public ActiveSkillData activeSkill1; 
    public ActiveSkillData activeSkill2; 
    
    public GlitchDNAInstance equippedGen1; 
    public GlitchDNAInstance equippedGen2; 

    [Header("=== CÔNG TẮC BỊ ĐỘNG HỆ LỬA ===")]
    public bool hasCorpseExplosion = false;
    public bool hasIgnite = false;
    public bool hasMelt = false;
    public bool hasHellfireTrail = false;
    public bool hasPhoenix = false;
    public bool hasMeteor = false;

    [HideInInspector] public GameObject activeCorpseExplosionVFX;
    [HideInInspector] public GameObject activeFireTrailVFX;
    [HideInInspector] public GameObject activeMeteorVFX;
    [HideInInspector] public GameObject activeIgniteVFX; 
    [HideInInspector] public GameObject activeMeltVFX;   
    [HideInInspector] public GameObject activePhoenixVFX;

    [Header("=== CÔNG TẮC BỊ ĐỘNG HỆ NƯỚC ===")]
    public bool hasFrostbite = false;
    public bool hasBloodShield = false;
    public bool hasMist = false;
    public bool hasBlizzard = false;
    public bool hasBubbleShield = false;
    public bool hasIllusion = false;

    [Header("=== CÔNG TẮC BỊ ĐỘNG HỆ ĐẤT ===")]
    public bool hasThorns = false;
    public bool hasTremor = false;
    public bool hasStoneSkin = false;
    public bool hasQuake = false;
    public bool hasTombstone = false;
    public bool hasTitanGrasp = false;

    public ItemData CurrentWeapon => equippedWeapons[activeWeaponIndex];
    private PlayerView view;
    private float phoenixCooldownTimer = 0f;

    void Start()
    {
        view = GetComponent<PlayerView>();
        InitializeExperience(); 
        currentStamina = maxStamina;
        currentVirus = 0f;
        speedBuffMultiplier = 1f; 

        for (int i = 0; i < 4; i++)
        {
            if (equippedWeapons[i] != null && equippedWeapons[i].category == ItemCategory.Weapon)
                currentAmmoInMag[i] = equippedWeapons[i].ammoAmount;
        }
        RecalculateStats();
        currentHealth = maxHealth;
        if (view != null)
        {
            view.UpdateLevelUI(currentLevel, currentExp, expToNextLevel);
            view.UpdateHealthUI(currentHealth, maxHealth);
            view.UpdateTitleUI(GetPlayerTitle());
        }
    }

    void Update()
    {
        if (phoenixCooldownTimer > 0) phoenixCooldownTimer -= Time.deltaTime;
    }

    public string GetPlayerTitle()
    {
        if (currentLevel < 10) return "<color=#FFFFFF>KẺ SỐNG SÓT</color>"; 
        if (currentLevel < 20) return "<color=#00FF00>THỂ CƯỜNG HÓA</color>"; 

        if (currentLevel < 40)
        {
            string colorHex = "#00BFFF"; 
            string elementStr = "";
            
            if (currentElement == "Fire") { elementStr = " LỬA"; colorHex = "#FF4500"; } 
            else if (currentElement == "Water") { elementStr = " NƯỚC"; colorHex = "#1E90FF"; } 
            else if (currentElement == "Earth") { elementStr = " ĐẤT"; colorHex = "#DAA520"; } 

            string rankStr = "";
            if (currentLevel < 25) rankStr = "DỊ BIẾN THÍCH NGHI";
            else if (currentLevel < 30) rankStr = "DỊ BIẾN DUNG HỢP";
            else if (currentLevel < 35) rankStr = "DỊ BIẾN BẠO PHÁT";
            else rankStr = "DỊ BIẾN HOÀN MỸ";

            return $"<color={colorHex}>{rankStr}{elementStr}</color>";
        }
        
        if (currentLevel < 50) return "<color=#8A2BE2>TIẾN HÓA GEN</color>"; 
        return "<color=#FFD700>THỂ TỐI THƯỢNG</color>"; 
    }

    public void InitializeExperience() { CalculateExpRequirement(); }

    public void AddExperience(float amount)
    {
        if (currentLevel % 10 == 0 && !evolvedLevels.Contains(currentLevel)) return; 
        currentExp += amount;
        while (currentExp >= expToNextLevel)
        {
            if (currentLevel % 10 == 0 && !evolvedLevels.Contains(currentLevel))
            {
                currentExp = expToNextLevel; break;
            }
            LevelUp();
        }
        if (view != null) view.UpdateLevelUI(currentLevel, currentExp, expToNextLevel);
    }

    private void LevelUp()
    {
        string oldTitle = GetPlayerTitle(); 

        currentExp -= expToNextLevel; 
        currentLevel++;
        if (currentLevel <= 20) survivalPoints++; else awakeningPoints++; 
        CalculateExpRequirement(); 
        baseMaxHealth += 5f; baseDamage += 1f;
        RecalculateStats();
        currentHealth = maxHealth; currentStamina = maxStamina;
        
        if (view != null) 
        {
            view.UpdateLevelUI(currentLevel, currentExp, expToNextLevel);
            view.UpdateHealthUI(currentHealth, maxHealth);
            
            string newTitle = GetPlayerTitle();
            view.UpdateTitleUI(newTitle); 
            
            if (oldTitle != newTitle) 
            {
                view.AnnounceEvolution(oldTitle, newTitle); 
            }
        }
    }

    private void CalculateExpRequirement()
    {
        float rawExp = baseExpRequirement * Mathf.Pow(currentLevel, expCurveMultiplier);
        expToNextLevel = Mathf.Round(rawExp / 50f) * 50f; 
        if (expToNextLevel < 100f) expToNextLevel = 100f; 
    }

    public void RegisterEvolution(int level, string chosenElement = "None")
    {
        if (!evolvedLevels.Contains(level))
        {
            string oldTitle = GetPlayerTitle();

            evolvedLevels.Add(level);
            if (level == 10) hasEvolvedTier1 = true;
            if (level == 20) { hasEvolvedTier2 = true; currentElement = chosenElement; }
            AddExperience(0); 
            
            if (view != null) 
            {
                string newTitle = GetPlayerTitle();
                view.UpdateTitleUI(newTitle);
                
                if (oldTitle != newTitle) 
                {
                    view.AnnounceEvolution(oldTitle, newTitle); 
                }
            }
        }
    }

    public void RecalculateStats()
    {
        float flatHealth = baseMaxHealth, flatMoveSpeed = baseMoveSpeed, flatArmor = baseArmor, flatDamage = 0f;
        float pctHealth = 0f, pctMoveSpeed = 0f, pctArmor = 0f, pctDamage = 0f;

        // Reset toàn bộ chỉ số trước khi tính lại
        lifestealPercent = baseLifesteal; critChance = baseCritChance; critDamageMultiplier = baseCritDamage;
        armorPenetration = baseArmorPenetration; dodgeChance = baseDodgeChance; accuracy = baseAccuracy; reloadSpeedMultiplier = 1f;
        cooldownReduction = 0f; 

        foreach (EquipmentInstance gear in equippedGear)
        {
            if (gear == null) continue;
            foreach (StatModifier stat in gear.basicStats)
            {
                switch (stat.statType)
                {
                    case StatType.FlatHealth: flatHealth += stat.value; break;
                    case StatType.FlatArmor: flatArmor += stat.value; break;
                    case StatType.FlatDamage: flatDamage += stat.value; break;
                    case StatType.FlatMoveSpeed: flatMoveSpeed += stat.value; break;
                }
            }
            foreach (StatModifier stat in gear.specialStats)
            {
                switch (stat.statType)
                {
                    case StatType.HealthPct: pctHealth += stat.value; break;
                    case StatType.ArmorPct: pctArmor += stat.value; break;
                    case StatType.DamagePct: pctDamage += stat.value; break;
                    case StatType.MoveSpeedPct: pctMoveSpeed += stat.value; break;
                    case StatType.Lifesteal: lifestealPercent += stat.value; break;
                    case StatType.CritChance: critChance += stat.value; break;
                    case StatType.ReloadSpeedPct: reloadSpeedMultiplier -= (stat.value / 100f); break; 
                }
            }
        }

        // --- CỘNG DỒN CHỈ SỐ TỪ CÁC ĐOẠN GEN ĐANG CẤY GHÉP ---
        if (equippedGen1 != null) AddGenStatsToModifiers(equippedGen1, ref flatHealth, ref flatArmor, ref flatDamage, ref critChance);
        if (equippedGen2 != null) AddGenStatsToModifiers(equippedGen2, ref flatHealth, ref flatArmor, ref flatDamage, ref critChance);

        reloadSpeedMultiplier = Mathf.Clamp(reloadSpeedMultiplier, 0.2f, 1f);
        float oldMaxHealth = maxHealth;
        maxHealth = flatHealth * (1f + pctHealth / 100f);
        armor = flatArmor * (1f + pctArmor / 100f);
        moveSpeed = flatMoveSpeed * (1f + pctMoveSpeed / 100f);
        bonusDamage = flatDamage * (1f + pctDamage / 100f);
        unarmedDamage = baseDamage + bonusDamage; 
        if (oldMaxHealth > 0) currentHealth = (currentHealth / oldMaxHealth) * maxHealth; else currentHealth = maxHealth;

        // ==========================================
        // MỚI: BÁO CHO GIAO DIỆN CẬP NHẬT THỜI GIAN THỰC
        // ==========================================
        if (StatDisplayUI.Instance != null && StatDisplayUI.Instance.gameObject.activeInHierarchy)
        {
            StatDisplayUI.Instance.UpdateStatPanel();
        }
        
        // Cập nhật luôn Thanh Slider máu ở phía trên
        if (view != null) view.UpdateHealthUI(currentHealth, maxHealth);
    }

    public bool HasPassiveSkill(PassiveSkillType skillToCheck)
    {
        foreach (EquipmentInstance gear in equippedGear)
        {
            if (gear != null && gear.passiveSkills.Contains(skillToCheck)) return true;
        }
        return false;
    }

    public float TakeDamage(float amount, float enemyArmorPen = 0f, float enemyAccuracy = 0f, bool isCrit = false)
    {
        if (currentHealth <= 0) return 0f;

        float actualDodge = Mathf.Max(0f, dodgeChance - enemyAccuracy);
        if (Random.Range(0f, 100f) < actualDodge)
        {
            if (view != null) view.SpawnFloatingText(view.damageTextPrefab, transform.position + Vector3.up * 1.5f, "Né!", Color.gray);
            return -1f; 
        }

        float effectiveArmor = Mathf.Max(0f, armor - enemyArmorPen);
        float damageMultiplier = 100f / (100f + effectiveArmor);
        float actualDamage = Mathf.Max(1f, amount * damageMultiplier); 

        currentHealth -= actualDamage;

        if (currentHealth <= 0 && hasPhoenix && phoenixCooldownTimer <= 0)
        {
            currentHealth = maxHealth * 0.3f; 
            phoenixCooldownTimer = 120f;      
            Collider[] hits = Physics.OverlapSphere(transform.position, 5f);
            foreach (Collider hit in hits)
            {
                if (hit.CompareTag("Enemy"))
                {
                    UnityEngine.AI.NavMeshAgent agent = hit.GetComponent<UnityEngine.AI.NavMeshAgent>();
                    if (agent != null && agent.isOnNavMesh) agent.Move((hit.transform.position - transform.position).normalized * 3f);
                }
            }
            if (activePhoenixVFX != null)
            {
                GameObject phoenix = Instantiate(activePhoenixVFX, transform.position, Quaternion.identity);
                Destroy(phoenix, 3f); 
            }
            Debug.Log("<color=orange>PHƯỢNG HOÀNG THỨC TỈNH!</color>");
        }

        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        if (view != null) view.UpdateHealthUI(currentHealth, maxHealth);
        if (currentHealth <= 0) Die();

        return actualDamage;
    }

    private void Die()
    {
        Debug.Log("<color=black>💀 PLAYER ĐÃ CHẾT! GAME OVER.</color>");
    }

    private void AddGenStatsToModifiers(GlitchDNAInstance gen, ref float flatHealth, ref float flatArmor, ref float flatDamage, ref float critChance)
    {
        if (gen == null) return;
        ApplyGenLine(gen.line1, ref flatHealth, ref flatArmor, ref flatDamage, ref critChance);
        ApplyGenLine(gen.line2, ref flatHealth, ref flatArmor, ref flatDamage, ref critChance);
    }

    private void ApplyGenLine(GenStatLine line, ref float flatHealth, ref float flatArmor, ref float flatDamage, ref float critChance)
    {
        if (line == null) return;
        switch (line.statType)
        {
            case GenStatType.MaxHealth: flatHealth += line.currentValue; break;
            case GenStatType.Armor: flatArmor += line.currentValue; break;
            case GenStatType.Damage: flatDamage += line.currentValue; break;
            case GenStatType.CritChance: critChance += line.currentValue; break;
            case GenStatType.CooldownReduction: cooldownReduction += line.currentValue; break; // MỚI: Cộng ngầm Giảm Hồi Chiêu
        }
    }
}