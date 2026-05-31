using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EnemyData))]
public class EnemyDataEditor : Editor
{
    SerializedProperty enemyName, category, prefab;
    SerializedProperty basicTier, element;
    SerializedProperty maxHealth, moveSpeed, damage, attackRange, attackCooldown;
    SerializedProperty fleeHealthThreshold, fleeSpeedMultiplier;
    SerializedProperty aggroSound, attackSound, deathSound, hitVFX;
    
    // ĐÃ KHAI BÁO BIẾN LOOT TABLE TẠI ĐÂY
    SerializedProperty expReward, lootTable; 

    private void OnEnable()
    {
        enemyName = serializedObject.FindProperty("enemyName");
        category = serializedObject.FindProperty("category");
        prefab = serializedObject.FindProperty("prefab");

        basicTier = serializedObject.FindProperty("basicTier");
        element = serializedObject.FindProperty("element");

        maxHealth = serializedObject.FindProperty("maxHealth");
        moveSpeed = serializedObject.FindProperty("moveSpeed");
        damage = serializedObject.FindProperty("damage");
        attackRange = serializedObject.FindProperty("attackRange");
        attackCooldown = serializedObject.FindProperty("attackCooldown");

        fleeHealthThreshold = serializedObject.FindProperty("fleeHealthThreshold");
        fleeSpeedMultiplier = serializedObject.FindProperty("fleeSpeedMultiplier");

        aggroSound = serializedObject.FindProperty("aggroSound");
        attackSound = serializedObject.FindProperty("attackSound");
        deathSound = serializedObject.FindProperty("deathSound");
        hitVFX = serializedObject.FindProperty("hitVFX");

        expReward = serializedObject.FindProperty("expReward");
        
        // ĐÃ LIÊN KẾT BIẾN TẠI ĐÂY
        lootTable = serializedObject.FindProperty("lootTable"); 
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("=== THÔNG TIN TỔNG QUAN ===", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(enemyName);
        EditorGUILayout.PropertyField(prefab, new GUIContent("Mô hình 3D (Prefab)"));
        EditorGUILayout.Space(5);
        EditorGUILayout.PropertyField(category, new GUIContent("Phân loại kẻ thù"));

        EnemyCategory currentCat = (EnemyCategory)category.enumValueIndex;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("=== CHI TIẾT THẾ LỰC ===", EditorStyles.boldLabel);
        
        if (currentCat == EnemyCategory.Basic)
        {
            EditorGUILayout.PropertyField(basicTier, new GUIContent("Cấp bậc Zombie"));
        }
        else if (currentCat == EnemyCategory.Mutant || currentCat == EnemyCategory.RegionalBoss)
        {
            EditorGUILayout.PropertyField(element, new GUIContent("Thuộc tính Nguyên Tố"));
        }
        else if (currentCat == EnemyCategory.WaveBoss)
        {
            EditorGUILayout.HelpBox("Wave Boss (Tấn công căn cứ mỗi 30 ngày) không có thuộc tính nguyên tố.", MessageType.Info);
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("=== CHỈ SỐ CHIẾN ĐẤU ===", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(maxHealth, new GUIContent("Máu tối đa"));
        EditorGUILayout.PropertyField(moveSpeed, new GUIContent("Tốc độ di chuyển"));
        EditorGUILayout.PropertyField(damage, new GUIContent("Sát thương/đòn"));
        EditorGUILayout.PropertyField(attackRange, new GUIContent("Tầm đánh"));
        EditorGUILayout.PropertyField(attackCooldown, new GUIContent("Thời gian giữa 2 đòn (s)"));

        if (currentCat == EnemyCategory.Mutant)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("=== CƠ CHẾ AI BỎ CHẠY (DÀNH CHO MUTANT) ===", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Mutant sẽ bỏ chạy khi máu tụt xuống dưới ngưỡng này để người chơi có cơ hội tóm sống.", MessageType.Warning);
            EditorGUILayout.PropertyField(fleeHealthThreshold, new GUIContent("Ngưỡng máu bỏ chạy (%)"));
            EditorGUILayout.PropertyField(fleeSpeedMultiplier, new GUIContent("Hệ số tăng tốc khi trốn"));
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("=== HIỆU ỨNG (VFX & SFX) ===", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(aggroSound, new GUIContent("Âm thanh phát hiện mục tiêu"));
        EditorGUILayout.PropertyField(attackSound, new GUIContent("Âm thanh vung đòn"));
        EditorGUILayout.PropertyField(deathSound, new GUIContent("Âm thanh gục ngã"));
        EditorGUILayout.PropertyField(hitVFX, new GUIContent("Hiệu ứng máu/tia lửa khi bị bắn"));

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("=== PHẦN THƯỞNG KHI CHẾT ===", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(expReward, new GUIContent("Kinh nghiệm (EXP)"));
        
        EditorGUILayout.Space(5);
        // HIỂN THỊ DANH SÁCH LOOT VỚI TÙY CHỌN INCLUDE CHILDREN (true)
        EditorGUILayout.PropertyField(lootTable, new GUIContent("Bảng vật phẩm rớt (Loot Table)"), true);

        serializedObject.ApplyModifiedProperties();
    }
}