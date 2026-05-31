using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(SkillData))]
public class SkillDataEditor : Editor
{
    private SerializedProperty skillName, description, icon, category, tier, maxLevel, effectType, valuePerLevel, skillVFX;

    private void OnEnable()
    {
        skillName = serializedObject.FindProperty("skillName");
        description = serializedObject.FindProperty("description");
        icon = serializedObject.FindProperty("icon");
        category = serializedObject.FindProperty("category");
        tier = serializedObject.FindProperty("tier");
        maxLevel = serializedObject.FindProperty("maxLevel");
        effectType = serializedObject.FindProperty("effectType");
        valuePerLevel = serializedObject.FindProperty("valuePerLevel");
        skillVFX = serializedObject.FindProperty("skillVFX");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // 1. THÔNG TIN CƠ BẢN
        EditorGUILayout.LabelField("=== THÔNG TIN CƠ BẢN ===", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(skillName);
        EditorGUILayout.LabelField("Mô tả kỹ năng:");
        description.stringValue = EditorGUILayout.TextArea(description.stringValue, GUILayout.Height(60));
        EditorGUILayout.PropertyField(icon);
        EditorGUILayout.Space(10);

        // 2. PHÂN LOẠI
        EditorGUILayout.LabelField("=== PHÂN LOẠI HỆ & TẦNG ===", EditorStyles.boldLabel);
        SkillCategory currentCategory = (SkillCategory)category.enumValueIndex;
        
        Color guiColor = Color.white;
        switch (currentCategory)
        {
            case SkillCategory.Survival: guiColor = new Color(0.6f, 1f, 0.6f); break;
            case SkillCategory.Fire: guiColor = new Color(1f, 0.6f, 0.6f); break;    
            case SkillCategory.Water: guiColor = new Color(0.6f, 0.8f, 1f); break;   
            case SkillCategory.Earth: guiColor = new Color(0.9f, 0.7f, 0.5f); break; 
        }
        GUI.color = guiColor;
        EditorGUILayout.BeginVertical("box");
        GUI.color = Color.white; 

        EditorGUILayout.PropertyField(category);
        EditorGUILayout.PropertyField(tier);
        EditorGUILayout.PropertyField(maxLevel);
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(10);

        // 3. BỘ LỌC HIỆU ỨNG
        EditorGUILayout.LabelField("=== HIỆU ỨNG KÍCH HOẠT ===", EditorStyles.boldLabel);
        SkillTier currentTier = (SkillTier)tier.enumValueIndex;
        List<SkillEffectType> validEffects = GetValidEffects(currentCategory, currentTier);

        string[] options = new string[validEffects.Count];
        for (int i = 0; i < validEffects.Count; i++) options[i] = validEffects[i].ToString();

        SkillEffectType currentEffect = (SkillEffectType)effectType.intValue;
        int selectedIndex = validEffects.IndexOf(currentEffect);
        if (selectedIndex == -1) selectedIndex = 0; 

        selectedIndex = EditorGUILayout.Popup("Effect Type", selectedIndex, options);
        effectType.intValue = (int)validEffects[selectedIndex]; 

        string effectName = validEffects[selectedIndex].ToString();
        if (effectName.StartsWith("Unlock"))
        {
            GUI.enabled = false; 
            valuePerLevel.floatValue = 0f; 
            EditorGUILayout.PropertyField(valuePerLevel, new GUIContent("Value Per Level (Locked)"));
            GUI.enabled = true;  
            EditorGUILayout.HelpBox("Đây là chiêu Mở Khóa, không cần nhập chỉ số.", MessageType.Info);
        }
        else
        {
            EditorGUILayout.PropertyField(valuePerLevel);
        }

        // 4. VFX
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("=== HIỆU ỨNG HÌNH ẢNH (VFX) ===", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(skillVFX);

        serializedObject.ApplyModifiedProperties();
    }

    private List<SkillEffectType> GetValidEffects(SkillCategory cat, SkillTier t)
    {
        List<SkillEffectType> list = new List<SkillEffectType>();
        list.Add(SkillEffectType.None);

        if (cat == SkillCategory.Survival)
        {
            list.AddRange(new[] { SkillEffectType.IncreaseMaxHealth, SkillEffectType.IncreaseMaxStamina, SkillEffectType.IncreaseBaseDamage, SkillEffectType.IncreaseBaseArmor, SkillEffectType.IncreaseMoveSpeed, SkillEffectType.IncreaseStaminaRegen, SkillEffectType.IncreaseCritChance, SkillEffectType.IncreaseLifesteal });
        }
        else if (cat == SkillCategory.Fire)
        {
            if (t == SkillTier.Tier1) list.AddRange(new[] { SkillEffectType.IncreaseBaseDamage, SkillEffectType.IncreaseCritChance, SkillEffectType.IncreaseArmorPenetration });
            else if (t == SkillTier.Tier2) list.AddRange(new[] { SkillEffectType.UnlockCorpseExplosion, SkillEffectType.UnlockIgnite, SkillEffectType.UnlockMelt, SkillEffectType.IncreaseCritDamage, SkillEffectType.IncreaseArmorPenetration });
            else if (t == SkillTier.Tier3) list.AddRange(new[] { SkillEffectType.UnlockHellfireTrail, SkillEffectType.UnlockPhoenix, SkillEffectType.UnlockMeteor, SkillEffectType.IncreaseBaseDamage, SkillEffectType.IncreaseCritChance });
        }
        else if (cat == SkillCategory.Water)
        {
            if (t == SkillTier.Tier1) list.AddRange(new[] { SkillEffectType.IncreaseDodgeChance, SkillEffectType.IncreaseLifesteal, SkillEffectType.IncreaseAccuracy });
            else if (t == SkillTier.Tier2) list.AddRange(new[] { SkillEffectType.UnlockFrostbite, SkillEffectType.UnlockBloodShield, SkillEffectType.UnlockMist, SkillEffectType.IncreaseLifesteal, SkillEffectType.IncreaseDodgeChance });
            else if (t == SkillTier.Tier3) list.AddRange(new[] { SkillEffectType.UnlockBlizzard, SkillEffectType.UnlockBubbleShield, SkillEffectType.UnlockIllusion, SkillEffectType.IncreaseMaxStamina, SkillEffectType.IncreaseMoveSpeed, SkillEffectType.IncreaseMaxHealth });
        }
        else if (cat == SkillCategory.Earth)
        {
            if (t == SkillTier.Tier1) list.AddRange(new[] { SkillEffectType.IncreaseMaxHealth, SkillEffectType.IncreaseBaseArmor, SkillEffectType.IncreaseAntiCrit });
            else if (t == SkillTier.Tier2) list.AddRange(new[] { SkillEffectType.UnlockThorns, SkillEffectType.UnlockTremor, SkillEffectType.UnlockStoneSkin, SkillEffectType.IncreaseBaseArmor, SkillEffectType.IncreaseMaxHealth });
            else if (t == SkillTier.Tier3) list.AddRange(new[] { SkillEffectType.UnlockQuake, SkillEffectType.UnlockTombstone, SkillEffectType.UnlockTitanGrasp, SkillEffectType.IncreaseBaseArmor, SkillEffectType.IncreaseBaseDamage });
        }
        return list;
    }
}