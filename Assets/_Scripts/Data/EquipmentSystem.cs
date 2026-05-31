using System.Collections.Generic;
using UnityEngine;

// ==========================================
// 1. TỪ ĐIỂN CHỈ SỐ & KỸ NĂNG (ENUMS)
// ==========================================
public enum EquipmentSlot { Helmet, Chest, Pants, Shoes } 

public enum StatType 
{ 
    FlatHealth, FlatDamage, FlatArmor, FlatMoveSpeed, 
    Lifesteal, CritChance, ReloadSpeedPct, StaminaPct, AttackSpeedPct, 
    HealthPct, DamagePct, ArmorPct, MoveSpeedPct 
}

public enum PassiveSkillType 
{ 
    None, 
    AoECleave,          
    TripleSpreadCrit,   
    TrueDamageThirdHit, 
    Regen5s,            
    DashDamageBuff      
}

// ==========================================
// 2. KHỐI LƯU TRỮ DỮ LIỆU (DATA CONTAINERS)
// ==========================================
[System.Serializable]
public class StatModifier
{
    public StatType statType;
    public float value;

    public StatModifier(StatType type, float val)
    {
        statType = type;
        value = val;
    }
}

[System.Serializable]
public class EquipmentInstance
{
    public string instanceID;       
    public ItemData baseTemplate;   
    public int starLevel;           

    public List<StatModifier> basicStats = new List<StatModifier>();
    public List<StatModifier> specialStats = new List<StatModifier>();
    public List<PassiveSkillType> passiveSkills = new List<PassiveSkillType>();

    public EquipmentInstance(ItemData template)
    {
        instanceID = System.Guid.NewGuid().ToString(); 
        baseTemplate = template;
    }
}

// ==========================================
// 3. LÒ ĐÚC TRANG BỊ (GACHA ENGINE)
// ==========================================
public static class EquipmentGenerator
{
    // Đã thay đổi tham số truyền vào: Nhận mảng Tỉ lệ Sao thay vì coreQuality
    public static EquipmentInstance GenerateRandomEquipment(ItemData template, float[] starRates = null)
    {
        EquipmentInstance newEquip = new EquipmentInstance(template);
        
        // ==========================================
        // VÒNG QUAY NHÂN PHẨM (XÁC SUẤT SAO)
        // ==========================================
        newEquip.starLevel = 1; // Mặc định là 1 sao để chống lỗi
        
        if (starRates != null && starRates.Length == 5)
        {
            float totalWeight = 0f;
            foreach (float rate in starRates) totalWeight += rate;

            // Nếu người thiết kế quên nhập tỉ lệ (tổng = 0), tự random từ 1-5
            if (totalWeight <= 0f)
            {
                newEquip.starLevel = UnityEngine.Random.Range(1, 6);
            }
            else
            {
                // Quay một con số ngẫu nhiên từ 0 đến Tổng tỉ lệ
                float randomRoll = UnityEngine.Random.Range(0f, totalWeight);
                float currentSum = 0f;

                for (int i = 0; i < 5; i++)
                {
                    currentSum += starRates[i];
                    if (randomRoll <= currentSum)
                    {
                        newEquip.starLevel = i + 1; // Chỉ số mảng 0 tương ứng 1 sao
                        break;
                    }
                }
            }
        }
        else
        {
            // Dự phòng nếu gọi hàm mà không truyền tỉ lệ
            newEquip.starLevel = UnityEngine.Random.Range(1, 6); 
        }

        // ==========================================
        // BỐC THĂM CHỈ SỐ VÀ KỸ NĂNG THEO SỐ SAO
        // ==========================================
        int basicStatCount = 0;
        int specialStatCount = 0;
        int passiveCount = 0;

        switch (newEquip.starLevel)
        {
            case 1: basicStatCount = 1; break;
            case 2: basicStatCount = 2; break;
            case 3: basicStatCount = 2; specialStatCount = 1; break;
            case 4: basicStatCount = 2; specialStatCount = 1; passiveCount = 1; break;
            case 5: 
                basicStatCount = 2;  
                specialStatCount = 1; 
                passiveCount = 1;
                if (UnityEngine.Random.value <= 0.2f) passiveCount = 2; 
                break;
        }

        float statMultiplier = (newEquip.starLevel == 5) ? 1.5f : 1.0f; 

        for (int i = 0; i < basicStatCount; i++)
        {
            StatType randomBasic = GetRandomBasicStatType();
            float val = RollStatValue(randomBasic) * statMultiplier;
            newEquip.basicStats.Add(new StatModifier(randomBasic, val));
        }

        for (int i = 0; i < specialStatCount; i++)
        {
            StatType randomSpecial = GetRandomSpecialStatType();
            float val = RollStatValue(randomSpecial) * statMultiplier;
            newEquip.specialStats.Add(new StatModifier(randomSpecial, val));
        }

        for (int i = 0; i < passiveCount; i++)
        {
            PassiveSkillType randomPassive = GetRandomPassive();
            if (!newEquip.passiveSkills.Contains(randomPassive))
            {
                newEquip.passiveSkills.Add(randomPassive);
            }
        }

        return newEquip;
    }


    private static StatType GetRandomBasicStatType()
    {
        StatType[] basics = { StatType.FlatHealth, StatType.FlatDamage, StatType.FlatArmor, StatType.FlatMoveSpeed };
        return basics[UnityEngine.Random.Range(0, basics.Length)];
    }

    private static StatType GetRandomSpecialStatType()
    {
        StatType[] specials = { StatType.Lifesteal, StatType.CritChance, StatType.ReloadSpeedPct, StatType.StaminaPct, StatType.AttackSpeedPct, StatType.HealthPct, StatType.DamagePct, StatType.ArmorPct, StatType.MoveSpeedPct };
        return specials[UnityEngine.Random.Range(0, specials.Length)];
    }

    private static PassiveSkillType GetRandomPassive()
    {
        PassiveSkillType[] passives = { PassiveSkillType.AoECleave, PassiveSkillType.TripleSpreadCrit, PassiveSkillType.TrueDamageThirdHit, PassiveSkillType.Regen5s, PassiveSkillType.DashDamageBuff };
        return passives[UnityEngine.Random.Range(1, passives.Length)]; // Bỏ qua None
    }

    private static float RollStatValue(StatType type)
    {
        switch (type)
        {
            case StatType.FlatHealth: return UnityEngine.Random.Range(50f, 150f);
            case StatType.FlatDamage: return UnityEngine.Random.Range(5f, 20f);
            case StatType.FlatArmor: return UnityEngine.Random.Range(10f, 30f);
            case StatType.FlatMoveSpeed: return UnityEngine.Random.Range(0.2f, 0.8f);
            
            case StatType.Lifesteal: return UnityEngine.Random.Range(2f, 8f); 
            case StatType.CritChance: return UnityEngine.Random.Range(5f, 25f); 
            
            default: return UnityEngine.Random.Range(10f, 30f); 
        }
    }
}