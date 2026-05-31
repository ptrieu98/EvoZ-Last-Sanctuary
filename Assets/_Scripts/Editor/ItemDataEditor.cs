using UnityEngine;
using UnityEditor; 

[CustomEditor(typeof(ItemData))] 
public class ItemDataEditor : Editor
{
    SerializedProperty itemName, icon, category;
    
    SerializedProperty coreTier, coreElement, mutantEffectDescription; // Nhóm Tinh hạch
    
    SerializedProperty equipmentType; 
    
    SerializedProperty weaponType, damage, attackRange, weaponPrefab, animationStance, moveSpeedMultiplier;
    SerializedProperty attackSound, muzzleFlashVFX, hitEnemyVFX, hitEnvironmentVFX;
    
    SerializedProperty isAutomatic, fireRate;
    
    SerializedProperty ammoType, ammoAmount, reloadTime, reloadSound, emptyClickSound;
    
    SerializedProperty consumableType, healAmount, speedBoostMultiplier, buffDuration, useTime, useSound;

    private void OnEnable()
    {
        itemName = serializedObject.FindProperty("itemName");
        icon = serializedObject.FindProperty("icon"); 
        category = serializedObject.FindProperty("category");

        coreTier = serializedObject.FindProperty("coreTier"); 
        coreElement = serializedObject.FindProperty("coreElement"); 
        mutantEffectDescription = serializedObject.FindProperty("mutantEffectDescription"); 

        equipmentType = serializedObject.FindProperty("equipmentType"); 

        weaponType = serializedObject.FindProperty("weaponType");
        damage = serializedObject.FindProperty("damage");
        attackRange = serializedObject.FindProperty("attackRange");
        weaponPrefab = serializedObject.FindProperty("weaponPrefab");
        animationStance = serializedObject.FindProperty("animationStance");
        moveSpeedMultiplier = serializedObject.FindProperty("moveSpeedMultiplier");

        attackSound = serializedObject.FindProperty("attackSound");
        muzzleFlashVFX = serializedObject.FindProperty("muzzleFlashVFX");
        hitEnemyVFX = serializedObject.FindProperty("hitEnemyVFX");
        hitEnvironmentVFX = serializedObject.FindProperty("hitEnvironmentVFX");

        isAutomatic = serializedObject.FindProperty("isAutomatic");
        fireRate = serializedObject.FindProperty("fireRate");

        ammoType = serializedObject.FindProperty("ammoType");
        ammoAmount = serializedObject.FindProperty("ammoAmount");
        reloadTime = serializedObject.FindProperty("reloadTime");
        reloadSound = serializedObject.FindProperty("reloadSound");
        emptyClickSound = serializedObject.FindProperty("emptyClickSound");

        consumableType = serializedObject.FindProperty("consumableType");
        healAmount = serializedObject.FindProperty("healAmount");
        speedBoostMultiplier = serializedObject.FindProperty("speedBoostMultiplier");
        buffDuration = serializedObject.FindProperty("buffDuration");
        useTime = serializedObject.FindProperty("useTime");
        useSound = serializedObject.FindProperty("useSound");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("=== THÔNG TIN CHUNG ===", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(itemName);
        EditorGUILayout.PropertyField(icon);
        EditorGUILayout.Space(5);
        EditorGUILayout.PropertyField(category);
        
        EditorGUILayout.Space(15);

        ItemCategory currentCategory = (ItemCategory)category.enumValueIndex;

        // BẢNG ĐIỀU KHIỂN HIỂN THỊ ĐỘNG DỰA TRÊN CATEGORY
        if (currentCategory == ItemCategory.Core)
        {
            EditorGUILayout.LabelField("=== THÔNG SỐ TINH HẠCH (CORES) ===", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(ammoAmount, new GUIContent("Số lượng chứa tối đa 1 ô (Stack)"));
            
            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(coreTier, new GUIContent("Bậc (Tier)"));
            
            CoreTier currentTier = (CoreTier)coreTier.enumValueIndex;
            
            // Chỉ hiện chọn Hệ cho Tier 2 và 3
            if (currentTier == CoreTier.Tier2 || currentTier == CoreTier.Tier3)
            {
                EditorGUILayout.PropertyField(coreElement, new GUIContent("Hệ Nguyên Tố"));
            }
            // Chỉ hiện khung mô tả nếu là Tinh hạch đột biến
            else if (currentTier == CoreTier.Mutant)
            {
                EditorGUILayout.PropertyField(mutantEffectDescription, new GUIContent("Mô tả Biến Dị"));
            }
        }
        else if (currentCategory == ItemCategory.Equipment)
        {
            EditorGUILayout.LabelField("=== KHUÔN MẪU TRANG BỊ ===", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(equipmentType, new GUIContent("Vị trí mặc (Slot)"));
            EditorGUILayout.HelpBox("Chỉ số chi tiết (Máu, Giáp, Chí mạng) sẽ được hệ thống Gacha tự động quay (Roll) lúc chế tạo thành công, không nhập ở đây.", MessageType.Info);
        }
        else if (currentCategory == ItemCategory.Material)
        {
            EditorGUILayout.LabelField("=== THÔNG SỐ NGUYÊN LIỆU ===", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(ammoAmount, new GUIContent("Số lượng chứa tối đa 1 ô (Stack)"));
        }
        else if (currentCategory == ItemCategory.Weapon)
        {
            EditorGUILayout.LabelField("=== THÔNG SỐ VŨ KHÍ ===", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(weaponType);
            EditorGUILayout.PropertyField(damage);
            EditorGUILayout.PropertyField(attackRange);
            EditorGUILayout.PropertyField(weaponPrefab);
            EditorGUILayout.PropertyField(animationStance, new GUIContent("Mã Dáng Cầm (Stance ID)"));
            EditorGUILayout.PropertyField(moveSpeedMultiplier, new GUIContent("Hệ số tốc độ (Độ nặng)"));

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("=== HIỆU ỨNG CHIẾN ĐẤU ===", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(attackSound);
            EditorGUILayout.PropertyField(hitEnemyVFX);
            EditorGUILayout.PropertyField(hitEnvironmentVFX);

            WeaponType currentWeaponType = (WeaponType)weaponType.enumValueIndex;
            if (currentWeaponType == WeaponType.Ranged)
            {
                EditorGUILayout.PropertyField(muzzleFlashVFX);

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("=== CHỈ SỐ BẮN LIÊN THANH ===", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(isAutomatic, new GUIContent("Bắn liên thanh (Đè chuột)"));
                
                if (isAutomatic.boolValue == true)
                {
                    EditorGUILayout.PropertyField(fireRate, new GUIContent("Tốc độ bắn (Giây/viên)"));
                }

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("=== THÔNG SỐ BĂNG ĐẠN ===", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(ammoType);
                EditorGUILayout.PropertyField(ammoAmount, new GUIContent("Sức chứa 1 băng đạn"));
                EditorGUILayout.PropertyField(reloadTime);
                EditorGUILayout.PropertyField(reloadSound);
                EditorGUILayout.PropertyField(emptyClickSound);
            }
        }
        else if (currentCategory == ItemCategory.Ammo)
        {
            EditorGUILayout.LabelField("=== THÔNG SỐ HỘP ĐẠN ===", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(ammoType);
            EditorGUILayout.PropertyField(ammoAmount, new GUIContent("Số lượng đạn trong hộp"));
        }
        else if (currentCategory == ItemCategory.Consumable)
        {
            EditorGUILayout.LabelField("=== THÔNG SỐ VẬT PHẨM TIÊU HAO ===", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(consumableType);
            EditorGUILayout.PropertyField(ammoAmount, new GUIContent("Số lượng trong 1 ô (Stack)"));
            EditorGUILayout.PropertyField(useTime);
            EditorGUILayout.PropertyField(useSound);

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("=== HIỆU ỨNG MANG LẠI ===", EditorStyles.boldLabel);
            
            ConsumableType currentConsumable = (ConsumableType)consumableType.enumValueIndex;
            
            if (currentConsumable == ConsumableType.Bandage || currentConsumable == ConsumableType.Medkit)
            {
                EditorGUILayout.PropertyField(healAmount);
            }
            else if (currentConsumable == ConsumableType.EnergyDrink)
            {
                EditorGUILayout.PropertyField(speedBoostMultiplier);
                EditorGUILayout.PropertyField(buffDuration);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}