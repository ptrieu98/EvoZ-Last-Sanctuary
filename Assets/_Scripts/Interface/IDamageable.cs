public interface IDamageable
{
    // ĐÃ THÊM: bool isCrit = false để nhận diện đòn chí mạng
    float TakeDamage(float amount, float armorPenetration = 0f, float accuracy = 0f, bool isCrit = false);
}