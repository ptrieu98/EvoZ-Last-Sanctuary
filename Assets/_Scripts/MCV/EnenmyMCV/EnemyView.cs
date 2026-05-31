using UnityEngine;

public class EnemyView : MonoBehaviour
{
    [Header("=== THÀNH PHẦN ===")]
    public Animator animator;
    public AudioSource audioSource;
    
    private EnemyModel model;

    void Start()
    {
        model = GetComponent<EnemyModel>();
    }

    public void SetMoveSpeedAnimation(float speed)
    {
        if (animator != null) animator.SetFloat("MoveSpeed", speed);
    }

    public void PlayAttackAnimation()
    {
        if (animator != null) animator.SetTrigger("Attack");
        
        if (audioSource != null && model.data != null && model.data.attackSound != null)
            audioSource.PlayOneShot(model.data.attackSound);
    }

    // --- HÀM KHỰNG KHI BỊ ĐÁNH ---
    public void PlayHitAnimation()
    {
        if (animator != null) animator.SetTrigger("Hit");
    }

    // --- HÀM GẦM GỪ MỚI THÊM VÀO ---
    public void PlayAggroAnimation()
    {
        if (animator != null) animator.SetTrigger("Aggro");
        PlayAggroSound(); // Phát luôn âm thanh gầm rống ở đây
    }

    public void PlayAggroSound()
    {
        if (audioSource != null && model.data != null && model.data.aggroSound != null)
            audioSource.PlayOneShot(model.data.aggroSound);
    }

    public void PlayHitEffect()
    {
        if (model.data != null && model.data.hitVFX != null)
        {
            Vector3 hitPos = transform.position + Vector3.up * 1f;
            GameObject vfx = Instantiate(model.data.hitVFX, hitPos, Quaternion.identity);
            Destroy(vfx, 1f); 
        }
    }

    public void PlayDeathAnimation()
    {
        if (animator != null) animator.SetTrigger("Die");
        
        if (audioSource != null && model.data != null && model.data.deathSound != null)
            audioSource.PlayOneShot(model.data.deathSound);
    }

    public void OnZombieAttackHit()
    {
        EnemyController controller = GetComponent<EnemyController>();
        if (controller != null)
        {
            controller.ExecuteDamageFrame();
        }
    }
}