using UnityEngine;
using System.Collections;

public class EnemyModel : MonoBehaviour, IDamageable
{
    [Header("=== DỮ LIỆU CỐT LÕI ===")]
    public EnemyData data;
    
    [Header("=== TRẠNG THÁI HIỆN TẠI ===")]
    public float currentHealth;
    public bool isDead = false;

    [Header("=== CHỈ SỐ PHÒNG NGỰ ===")]
    public float armor = 0f;            
    public float dodgeChance = 0f;      
    public float armorPenetration = 0f; 
    public float accuracy = 0f;         

    private EnemyController controller;
    private EnemyView view;
    private PlayerModel playerRef;

    private int meltHitCount = 0;
    private bool isMelted = false;
    private bool isIgnited = false;
    private float meltTimer = 0f;
    
    // Biến lưu trữ Icon Phá giáp để quản lý vị trí
    private GameObject currentMeltVfx; 

    void Start()
    {
        controller = GetComponent<EnemyController>();
        view = GetComponent<EnemyView>();
        if (data != null) currentHealth = data.maxHealth;
        
        GameObject pObj = GameObject.FindGameObjectWithTag("Player");
        if (pObj != null) playerRef = pObj.GetComponent<PlayerModel>();
    }

    void Update()
    {
        // QUẢN LÝ THỜI GIAN VÀ VỊ TRÍ ICON PHÁ GIÁP
        if (isMelted)
        {
            meltTimer -= Time.deltaTime;
            
            // Kỹ thuật Billboarding: Icon đi theo quái nhưng xoay theo Camera
            if (currentMeltVfx != null && Camera.main != null)
            {
                // Lấy hướng "Bên Phải" và "Bên Trên" tương đối so với Camera
                Vector3 rightOffset = Camera.main.transform.right * 0.8f; // Dịch sang phải 0.8m
                Vector3 upOffset = Camera.main.transform.up * 1.5f;       // Dịch lên trên 1.5m
                
                // Cập nhật vị trí
                currentMeltVfx.transform.position = transform.position + upOffset + rightOffset;
                
                // Luôn ép Icon xoay mặt song song với Camera (Không xoay theo quái)
                currentMeltVfx.transform.rotation = Camera.main.transform.rotation;
            }

            if (meltTimer <= 0) 
            { 
                isMelted = false; 
                meltHitCount = 0; 
                if (currentMeltVfx != null) Destroy(currentMeltVfx); // Xóa Icon khi hết giờ
            }
        }
    }

    public float TakeDamage(float amount, float playerArmorPen = 0f, float playerAccuracy = 0f, bool isCrit = false)
    {
        if (isDead) return 0f;

        float actualDodge = Mathf.Max(0f, dodgeChance - playerAccuracy);
        if (Random.Range(0f, 100f) < actualDodge) return -1f; 

        // --- HỆ LỬA: PHÁ BÍNH (TRỪ GIÁP) ---
        float currentArmor = armor;
        if (playerRef != null && playerRef.hasMelt)
        {
            meltHitCount++;
            if (meltHitCount >= 3 && !isMelted) 
            { 
                isMelted = true; 
                meltTimer = 5f; 
                
                // HIỆN ICON PHÁ GIÁP (Không làm con của quái để tránh bị xoay bậy)
                if (playerRef.activeMeltVFX != null)
                {
                    currentMeltVfx = Instantiate(playerRef.activeMeltVFX, transform.position, Quaternion.identity);
                    // Không dùng Destroy ở đây nữa vì đã có Update() và Die() lo việc dọn rác
                }
            }
            if (isMelted) currentArmor *= 0.7f; 
        }

        float effectiveArmor = Mathf.Max(0f, currentArmor - playerArmorPen);
        float damageMultiplier = 100f / (100f + effectiveArmor);
        float actualDamage = Mathf.Max(1f, amount * damageMultiplier);

        currentHealth -= actualDamage;
        if (view != null) view.PlayHitEffect();

        // --- HỆ LỬA: THIÊU RỤI ---
        if (isCrit && playerRef != null && playerRef.hasIgnite && !isIgnited)
        {
            StartCoroutine(IgniteRoutine(actualDamage * 0.2f)); 
        }

        if (currentHealth <= 0) Die();
        else
        {
            bool isFleeing = false;
            if (data.category == EnemyCategory.Mutant && currentHealth / data.maxHealth <= data.fleeHealthThreshold)
            {
                if (controller != null) controller.ChangeState(EnemyState.Flee);
                isFleeing = true; 
            }
            if (!isFleeing && controller != null)
            {
                controller.ChangeState(EnemyState.Hit);
                if (view != null) view.PlayHitAnimation(); 
            }
        }
        return actualDamage; 
    }

    private IEnumerator IgniteRoutine(float burnDamage)
    {
        isIgnited = true;
        GameObject igniteVfx = null;

        if (playerRef != null && playerRef.activeIgniteVFX != null)
        {
            igniteVfx = Instantiate(playerRef.activeIgniteVFX, transform.position, Quaternion.identity, transform);
        }

        for (int i = 0; i < 3; i++) 
        {
            yield return new WaitForSeconds(1f);
            if (isDead) break;
            currentHealth -= burnDamage;
            
            if (playerRef != null)
            {
                PlayerView pView = playerRef.GetComponent<PlayerView>();
                if (pView != null) pView.SpawnFloatingText(pView.damageTextPrefab, transform.position + Vector3.up * 1.5f, $"{burnDamage:F0}", new Color(1f, 0.5f, 0f));
            }

            if (currentHealth <= 0) { Die(); break; }
        }
        
        if (igniteVfx != null) Destroy(igniteVfx);
        isIgnited = false;
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;
        
        // --- DỌN RÁC ICON PHÁ GIÁP TRƯỚC KHI CHẾT ---
        if (currentMeltVfx != null) Destroy(currentMeltVfx);
        
        if (view != null) view.PlayDeathAnimation();
        if (controller != null) controller.ChangeState(EnemyState.Dead);
        Collider col = GetComponent<Collider>(); if (col != null) col.enabled = false;

        // --- HỆ LỬA: NỔ XÁC ---
        if (playerRef != null && playerRef.hasCorpseExplosion)
        {
            if (Random.Range(0f, 100f) <= 30f) StartCoroutine(CorpseExplosionRoutine());
        }

        if (data != null && playerRef != null)
        {
            playerRef.AddExperience(data.expReward);
            PlayerView pView = playerRef.GetComponent<PlayerView>();
            if (pView != null) pView.SpawnFloatingText(pView.expTextPrefab, transform.position + Vector3.up * 2f, $"+{data.expReward} EXP", Color.cyan);
        }

        if (data.lootTable != null && data.lootTable.Length > 0)
        {
            foreach (LootDrop loot in data.lootTable)
            {
                float roll = Random.Range(0f, 100f);
                if (roll <= loot.dropChance && loot.dropPrefab != null && loot.itemData != null)
                {
                    int dropAmount = Random.Range(loot.minAmount, loot.maxAmount + 1);
                    GameObject droppedItem = Instantiate(loot.dropPrefab, transform.position + Vector3.up * 1f, Quaternion.identity);
                    ItemPickup pickupScript = droppedItem.GetComponent<ItemPickup>();
                    if (pickupScript != null) { pickupScript.itemData = Instantiate(loot.itemData); pickupScript.itemData.name = loot.itemData.name; pickupScript.itemData.ammoAmount = dropAmount; }
                    Rigidbody dropRb = droppedItem.GetComponent<Rigidbody>();
                    if (dropRb != null) { dropRb.AddForce(new Vector3(Random.Range(-2f, 2f), 5f, Random.Range(-2f, 2f)), ForceMode.Impulse); dropRb.AddTorque(new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f), Random.Range(-5f, 5f)), ForceMode.Impulse); }
                }
            }
        }
    }

    private IEnumerator CorpseExplosionRoutine()
    {
        yield return new WaitForSeconds(1f); 
        
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 4f); 
        foreach (Collider hit in hitColliders)
        {
            if (hit.CompareTag("Enemy") && hit.gameObject != this.gameObject)
            {
                IDamageable damageable = hit.GetComponent<IDamageable>();
                if (damageable == null) damageable = hit.GetComponentInParent<IDamageable>();
                if (damageable != null) damageable.TakeDamage(playerRef.baseDamage * 2f); 
            }
        }
        
        if (playerRef.activeCorpseExplosionVFX != null) 
        {
            GameObject explosion = Instantiate(playerRef.activeCorpseExplosionVFX, transform.position, Quaternion.identity);
            Destroy(explosion, 3f); 
        }
        
        Destroy(gameObject, 0.5f); 
    }
    
    // Hàm bảo hiểm: Nếu con quái bị xóa khỏi game vì lý do nào đó (như qua màn), phải chắc chắn Icon cũng bị xóa theo
    private void OnDestroy()
    {
        if (currentMeltVfx != null) Destroy(currentMeltVfx);
    }
}