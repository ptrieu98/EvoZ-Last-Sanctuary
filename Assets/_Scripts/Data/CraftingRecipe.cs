using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ResourceCost
{
    public ItemData material;
    public int amount;
}

[CreateAssetMenu(fileName = "New Recipe", menuName = "EvoZ/Crafting Recipe")]
public class CraftingRecipe : ScriptableObject
{
    [Header("=== KẾT QUẢ CHẾ TẠO ===")]
    public ItemData targetEquipment;

    [Header("=== NGUYÊN LIỆU YÊU CẦU ===")]
    public List<ResourceCost> requiredMaterials;

    [Header("=== TỈ LỆ RA SAO (%) ===")]
    [Tooltip("Tổng các số này nên bằng 100. Ví dụ: Nếu muốn đồ chỉ ra từ 3-5 sao, hãy đặt 1 và 2 sao = 0")]
    [Range(0f, 100f)] public float rate1Star = 50f;
    [Range(0f, 100f)] public float rate2Star = 30f;
    [Range(0f, 100f)] public float rate3Star = 15f;
    [Range(0f, 100f)] public float rate4Star = 4.9f;
    [Range(0f, 100f)] public float rate5Star = 0.1f;

    // Hàm tiện ích để gói các tỉ lệ lại gửi cho "Lò đúc" (EquipmentGenerator)
    public float[] GetStarRates()
    {
        return new float[] { rate1Star, rate2Star, rate3Star, rate4Star, rate5Star };
    }
}