using UnityEngine;
using System.Collections.Generic;

// 1. CÁC LOẠI CHỈ SỐ CÓ THỂ ROLL RA
public enum GenStatType
{
    None, 
    Damage,         // Sát thương
    MaxHealth,      // Máu tối đa
    Armor,          // Giáp
    CritChance,     // Tỉ lệ chí mạng
    CooldownReduction // Giảm hồi chiêu
}

// 2. BẬC CỦA MÃ GEN
public enum GenTier
{
    Tier1,      // Bậc 1 (Chỉ số Max thấp)
    Tier2,      // Bậc 2 (Chỉ số Max trung bình)
    Tier3,      // Bậc 3 (Chỉ số Max cao)
    Mutant      // Dị Biến (Có 1 dòng Cốt Lõi x2 Max)
}

// ==========================================
// CLASS: MỘT DÒNG CHỈ SỐ TRÊN MÃ GEN
// ==========================================
[System.Serializable]
public class GenStatLine
{
    public GenStatType statType;
    public float currentValue;
    public float minValue;
    public float maxValue;
    
    public bool isLocked;   // Khóa bằng Tinh hạch để không bị mất khi Roll
    public bool isCoreStat; // TRUE nếu là Dòng Cốt Lõi của Gen Dị Biến

    public GenStatLine(GenStatType type, float min, float max, bool isCore = false)
    {
        statType = type;
        minValue = min;
        maxValue = max;
        isCoreStat = isCore;
        isLocked = false;
        RollValue(); 
    }

    public void RollValue()
    {
        currentValue = Mathf.Round(Random.Range(minValue, maxValue) * 10f) / 10f;
    }
}

// ==========================================
// CLASS: CỤC MÃ GEN KHUYẾT
// ==========================================
[System.Serializable]
public class GlitchDNAInstance
{
    public string genName;
    public GenTier tier;
    
    public ActiveSkillData activeSkill; 

    public GenStatLine line1;
    public GenStatLine line2;

    public GlitchDNAInstance(string name, GenTier genTier, ActiveSkillData skill, GenStatType mutantCoreStat = GenStatType.None)
    {
        genName = name;
        tier = genTier;
        activeSkill = skill;

        GenerateInitialStats(mutantCoreStat);
    }

    private void GenerateInitialStats(GenStatType mutantCoreStat)
    {
        float min = 1f; float max = 5f;
        if (tier == GenTier.Tier2) { min = 3f; max = 10f; }
        else if (tier == GenTier.Tier3) { min = 8f; max = 15f; }

        if (tier == GenTier.Mutant)
        {
            line1 = new GenStatLine(mutantCoreStat, 10f, 30f, true); 
            GenStatType randomType2 = GetRandomStatType(mutantCoreStat);
            line2 = new GenStatLine(randomType2, 8f, 15f, false);
        }
        else
        {
            GenStatType type1 = GetRandomStatType(GenStatType.None);
            GenStatType type2 = GetRandomStatType(type1); 

            line1 = new GenStatLine(type1, min, max, false);
            line2 = new GenStatLine(type2, min, max, false);
        }
    }

    // ==========================================
    // LẮC XÍ NGẦU: TẨY LOẠI CHỈ SỐ (ĐÃ FIX LỖI)
    // ==========================================
    public void RerollStatTypes()
    {
        if (line1 != null && !line1.isLocked && !line1.isCoreStat)
        {
            // Báo cho Dòng 1 biết: "Hãy random loại mới, nhưng NÉ cái loại mà Dòng 2 đang cầm ra!"
            GenStatType exclude = (line2 != null) ? line2.statType : GenStatType.None;
            line1.statType = GetRandomStatType(exclude); 
            
            line1.maxValue = GenerateMaxValueByTier(tier, line1.statType);
            float randomVal = Random.Range(line1.maxValue * 0.3f, line1.maxValue);
            line1.currentValue = (float)System.Math.Round(randomVal, 1);
        }

        if (line2 != null && !line2.isLocked && !line2.isCoreStat)
        {
            // Báo cho Dòng 2 biết: "Hãy random loại mới, nhưng NÉ cái loại mà Dòng 1 đang cầm ra!"
            GenStatType exclude = (line1 != null) ? line1.statType : GenStatType.None;
            line2.statType = GetRandomStatType(exclude); 
            
            line2.maxValue = GenerateMaxValueByTier(tier, line2.statType);
            float randomVal = Random.Range(line2.maxValue * 0.3f, line2.maxValue);
            line2.currentValue = (float)System.Math.Round(randomVal, 1);
        }
    }

    // ==========================================
    // LẮC XÍ NGẦU: TẨY ĐIỂM SỐ
    // ==========================================
    public void RerollStatValues()
    {
        if (line1 != null && !line1.isLocked)
        {
            line1.maxValue = GenerateMaxValueByTier(tier, line1.statType);
            float randomVal = Random.Range(line1.maxValue * 0.3f, line1.maxValue);
            line1.currentValue = (float)System.Math.Round(randomVal, 1);
        }

        if (line2 != null && !line2.isLocked)
        {
            line2.maxValue = GenerateMaxValueByTier(tier, line2.statType);
            float randomVal = Random.Range(line2.maxValue * 0.3f, line2.maxValue);
            line2.currentValue = (float)System.Math.Round(randomVal, 1);
        }
    }

    // ==========================================
    // TỪ ĐIỂN QUY ĐỊNH SỨC MẠNH THEO BẬC (TIER)
    // ==========================================
    private float GenerateMaxValueByTier(GenTier currentTier, GenStatType type)
    {
        float tierMultiplier = 1f;
        switch (currentTier)
        {
            case GenTier.Tier1: tierMultiplier = 1.0f; break;
            case GenTier.Tier2: tierMultiplier = 1.8f; break; 
            case GenTier.Tier3: tierMultiplier = 3.0f; break; 
            case GenTier.Mutant: tierMultiplier = 5.0f; break; 
        }

        float baseMax = 0f;
        switch (type)
        {
            case GenStatType.MaxHealth: baseMax = 50f; break;        
            case GenStatType.Armor: baseMax = 10f; break;            
            case GenStatType.Damage: baseMax = 15f; break;           
            case GenStatType.CritChance: baseMax = 5f; break;        
            case GenStatType.CooldownReduction: baseMax = 5f; break; 
        }

        return (float)System.Math.Round(baseMax * tierMultiplier, 1);
    }

    // --- HÀM HỖ TRỢ: Random loại chỉ số và cấm trùng lặp (ĐÃ ĐẶT GIÁ TRỊ MẶC ĐỊNH LÀ NONE) ---
    private GenStatType GetRandomStatType(GenStatType excludeType = GenStatType.None)
    {
        List<GenStatType> availableTypes = new List<GenStatType> 
        { 
            GenStatType.Damage, GenStatType.MaxHealth, GenStatType.Armor, 
            GenStatType.CritChance, GenStatType.CooldownReduction 
        };

        availableTypes.Remove(excludeType); 
        
        int randomIndex = Random.Range(0, availableTypes.Count);
        return availableTypes[randomIndex];
    }
}