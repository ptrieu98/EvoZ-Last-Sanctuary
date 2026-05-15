using UnityEngine;

public class EnemyController : MonoBehaviour, IDamageable
{
    public float health = 50f;

    // Bắt buộc phải có hàm này vì đã "ký hợp đồng" với IDamageable
    public void TakeDamage(float amount)
    {
        health -= amount;
        Debug.Log(gameObject.name + " còn lại: " + health + " máu.");

        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log(gameObject.name + " đã bị tiêu diệt!");
        Destroy(gameObject);
    }
}