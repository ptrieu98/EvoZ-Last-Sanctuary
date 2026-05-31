using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public enum MatriarchState { Chase, Attacking, Backstepping, Spawning, Transitioning, Dead }

[RequireComponent(typeof(NavMeshAgent))]
public class MatriarchController : MonoBehaviour
{
    [Header("=== LIÊN KẾT MVC ===")]
    public EnemyModel model;
    public EnemyView view;

    [Header("=== TRẠNG THÁI & CHIẾN THUẬT ===")]
    public MatriarchState currentState = MatriarchState.Chase;
    public float attackRange = 2f;
    public float attackCooldown = 2f;
    
    [Header("=== KỸ NĂNG ĐẺ CON (SPAWN) ===")]
    public GameObject childPrefab; 
    public Transform spawnPoint;   
    public float spawnCooldown = 8f; 
    public int childCountPhase1 = 2;
    public int childCountPhase2 = 4;
    public GameObject spawnVFX; 

    [Header("=== ĐỒNG BỘ ANIMATION ĐẺ CON ===")]
    public float timeToCrouch = 0.8f;
    public float timeBetweenSpawns = 0.3f;
    public float timeToStandUp = 1.0f;

    [Header("=== PHASE 2: ĐỘT BIẾN & LƯỚT TẤN CÔNG ===")]
    public float phase2HpThreshold = 0.5f;
    public GameObject phase2AuraVFX; 
    public float frenzySpeedMultiplier = 1.5f;
    
    [Tooltip("Tầm xa để kích hoạt chiêu lướt vồ tới tấn công ở Phase 2")]
    public float dashAttackRange = 7f;
    [Tooltip("Thời gian hồi chiêu lướt vồ tới")]
    public float dashAttackCooldown = 5f;
    [Tooltip("Tốc độ lướt thẳng tới phía trước")]
    public float dashSpeed = 16f;

    [Header("=== CHI TIẾT CHIÊU LƯỚT (DASH ATTACK) ===")]
    [Tooltip("Thời gian chờ (tích lực) kể từ lúc phát Anim đến lúc thực sự lướt đi")]
    public float dashWindupTime = 0.5f; 
    [Tooltip("Thời gian duy trì việc lướt đi")]
    public float dashDuration = 0.3f;
    
    // ĐÃ THÊM: HIỆU ỨNG VẾT CÀO KHI KẾT THÚC CÚ LƯỚT
    [Tooltip("Prefab hiệu ứng vết cào/chém (slash)")]
    public GameObject clawSlashVFX;
    [Tooltip("Căn chỉnh vị trí vết cào xuất hiện so với con quái (Y nhích lên, Z ra trước)")]
    public Vector3 slashOffset = new Vector3(0, 1.2f, 1.2f);

    [Header("=== HIỆU ỨNG TÀN ẢNH (GHOST TRAIL) ===")]
    [Tooltip("Material dùng cho tàn ảnh (Nên chọn loại trong suốt/phát sáng)")]
    public Material ghostMaterial;
    [Tooltip("Tốc độ đẻ tàn ảnh (Càng nhỏ tàn ảnh càng dày)")]
    public float ghostInterval = 0.05f; 
    [Tooltip("Thời gian tồn tại của mỗi tàn ảnh trước khi tan biến")]
    public float ghostLifeTime = 0.5f;

    // --- BIẾN NỘI BỘ ---
    private NavMeshAgent agent;
    private Transform playerTarget;
    private bool isPhase2 = false;
    private float lastAttackTime;
    private float lastSpawnTime;
    private float lastDashAttackTime;
    private GameObject activeFrenzyVFX;
    private Coroutine activeRoutine;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (model == null) model = GetComponent<EnemyModel>();
        if (view == null) view = GetComponent<EnemyView>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTarget = player.transform;

        GameObject healthBarPrefab = Resources.Load<GameObject>("UI_EnemyHealthBar");
        if (healthBarPrefab != null)
        {
            GameObject hb = Instantiate(healthBarPrefab, transform);
            hb.transform.localPosition = new Vector3(0, 2.2f, 0); 
            EnemyHealthBar hbScript = hb.GetComponent<EnemyHealthBar>();
            if (hbScript != null) hbScript.Setup(model);
        }

        if (model.data != null) agent.speed = model.data.moveSpeed;
        lastSpawnTime = Time.time; 
    }

    void Update()
    {
        if (model.isDead)
        {
            if (currentState != MatriarchState.Dead) ChangeState(MatriarchState.Dead);
            return;
        }

        if (!isPhase2 && model.currentHealth / model.data.maxHealth <= phase2HpThreshold)
        {
            if (currentState != MatriarchState.Transitioning && currentState != MatriarchState.Dead)
            {
                if (activeRoutine != null) StopCoroutine(activeRoutine);
                ChangeState(MatriarchState.Transitioning);
                activeRoutine = StartCoroutine(FrenzyTransitionRoutine()); 
                return;
            }
        }

        switch (currentState)
        {
            case MatriarchState.Chase: HandleChase(); break;
        }

        if (view != null && view.animator != null)
        {
            view.animator.SetFloat("MoveSpeed", agent.velocity.magnitude);
        }
    }

    private void ChangeState(MatriarchState newState)
    {
        currentState = newState;

        if (currentState == MatriarchState.Dead)
        {
            if (activeRoutine != null) StopCoroutine(activeRoutine);
            if (agent != null) agent.enabled = false;
            if (activeFrenzyVFX != null) Destroy(activeFrenzyVFX);
            Destroy(gameObject, 5f); 
        }
    }

    private void HandleChase()
    {
        if (playerTarget == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

        if (Time.time >= lastSpawnTime + spawnCooldown)
        {
            ChangeState(MatriarchState.Backstepping);
            activeRoutine = StartCoroutine(BackstepAndSpawnRoutine());
            return;
        }

        if (isPhase2 && distanceToPlayer <= dashAttackRange && distanceToPlayer > attackRange)
        {
            if (Time.time >= lastDashAttackTime + dashAttackCooldown)
            {
                ChangeState(MatriarchState.Attacking);
                activeRoutine = StartCoroutine(DashAttackRoutine());
                return;
            }
        }

        if (distanceToPlayer <= attackRange)
        {
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                ChangeState(MatriarchState.Attacking);
                activeRoutine = StartCoroutine(AttackRoutine());
            }
            else
            {
                agent.SetDestination(transform.position); 
            }
        }
        else
        {
            agent.isStopped = false;
            agent.SetDestination(playerTarget.position);
        }
    }

    // ==========================================
    // CÁC CHIÊU THỨC (COROUTINES)
    // ==========================================

    private IEnumerator AttackRoutine()
    {
        agent.isStopped = true;
        transform.LookAt(new Vector3(playerTarget.position.x, transform.position.y, playerTarget.position.z));
        
        if (view != null) view.animator.SetTrigger("Attack");

        yield return new WaitForSeconds(0.5f);

        float dist = Vector3.Distance(transform.position, playerTarget.position);
        if (dist <= attackRange + 0.5f) 
        {
            IDamageable playerHp = playerTarget.GetComponent<IDamageable>();
            if (playerHp != null) playerHp.TakeDamage(model.data.damage, model.armorPenetration, model.accuracy);
        }

        lastAttackTime = Time.time;
        yield return new WaitForSeconds(1f); 
        ChangeState(MatriarchState.Chase);
    }

    // ĐÃ NÂNG CẤP: LƯỚT, PHANH THÔNG MINH, TÀN ẢNH VÀ VFX VẾT CÀO (MASTER)
    private IEnumerator DashAttackRoutine()
    {
        agent.isStopped = true;
        
        // Khóa mục tiêu
        transform.LookAt(new Vector3(playerTarget.position.x, transform.position.y, playerTarget.position.z));

        // 1. KÍCH HOẠT ANIMATION
        if (view != null) view.animator.SetTrigger("DashAttack");

        // 2. CHỜ TÍCH LỰC (dashWindupTime)
        yield return new WaitForSeconds(dashWindupTime);

        // 3. BẮT ĐẦU LƯỚT
        // Tracking lại hướng lần cuối đề phòng Player né
        Vector3 dashDir = (playerTarget.position - transform.position).normalized;
        dashDir.y = 0;
        
        float elapsed = 0f;
        Coroutine ghostRoutine = null;
        
        // Bật tàn ảnh
        if (ghostMaterial != null) 
        {
            ghostRoutine = StartCoroutine(SpawnGhostTrailRoutine());
        }

        // Tầm phanh gấp để đứng trước mặt Player
        float stopDistance = attackRange - 0.2f; 

        while (elapsed < dashDuration)
        {
            elapsed += Time.deltaTime;
            
            // CƠ CHẾ PHANH THÔNG MINH (Chống lướt xuyên người)
            float currentDist = Vector3.Distance(transform.position, playerTarget.position);
            if (currentDist <= stopDistance)
            {
                break; // Ngắt lướt ngay lập tức
            }

            if (agent.enabled) agent.Move(dashDir * dashSpeed * Time.deltaTime);
            yield return null;
        }

        // Dừng đẻ tàn ảnh khi lướt xong
        if (ghostRoutine != null) StopCoroutine(ghostRoutine);

        // =========================================================
        // ĐÃ THÊM: SINH VFX VẾT CÀO KHI KẾT THÚC CÚ LƯỚT (NGAY KHI ĐẤM)
        // =========================================================
        if (clawSlashVFX != null)
        {
            // Tính toán vị trí spawn dựa trên Forward của con quái
            Vector3 spawnPos = transform.position + transform.forward * slashOffset.z + transform.up * slashOffset.y;
            
            // Quay VFX theo hướng nhìn của con quái để vết cào chém đúng hướng
            GameObject slashObj = Instantiate(clawSlashVFX, spawnPos, transform.rotation);
            
            // Tự hủy sau 1.5 giây cho sạch game
            Destroy(slashObj, 1.5f);
        }

        // 4. KIỂM TRA SÁT THƯƠNG
        float finalDist = Vector3.Distance(transform.position, playerTarget.position);
        if (finalDist <= attackRange + 1.2f)
        {
            IDamageable playerHp = playerTarget.GetComponent<IDamageable>();
            if (playerHp != null) playerHp.TakeDamage(model.data.damage * 1.4f, model.armorPenetration, model.accuracy);
        }

        lastDashAttackTime = Time.time;
        
        yield return new WaitForSeconds(0.4f);
        ChangeState(MatriarchState.Chase);
    }

    // HỆ THỐNG TẠO TÀN ẢNH (GHOST TRAIL) - ĐÃ FIX SCALE VÀ OPTIMIZE MEMORY
    private IEnumerator SpawnGhostTrailRoutine()
    {
        SkinnedMeshRenderer[] meshes = GetComponentsInChildren<SkinnedMeshRenderer>();
        
        while (true)
        {
            foreach (SkinnedMeshRenderer smr in meshes)
            {
                if (!smr.enabled || smr.gameObject.activeInHierarchy == false) continue; 

                GameObject ghostObj = new GameObject("GhostTrail");
                
                ghostObj.transform.position = smr.transform.position;
                ghostObj.transform.rotation = smr.transform.rotation;
                // FIX LỖI TO QUÁ: Ép Scale chuẩn (1,1,1) vì BakeMesh đã tự tính toán scale rồi
                ghostObj.transform.localScale = Vector3.one; 
                
                MeshFilter meshFilter = ghostObj.AddComponent<MeshFilter>();
                MeshRenderer meshRenderer = ghostObj.AddComponent<MeshRenderer>();
                
                Mesh bakedMesh = new Mesh();
                smr.BakeMesh(bakedMesh); // Đúc lưới tĩnh
                meshFilter.mesh = bakedMesh;
                
                meshRenderer.material = ghostMaterial;
                
                Destroy(ghostObj, ghostLifeTime);
                
                // TỐI ƯU: Xóa cục bakedMesh khỏi RAM sau khi object tàn ảnh bị hủy
                StartCoroutine(CleanupMesh(ghostObj, bakedMesh, ghostLifeTime));
            }
            
            yield return new WaitForSeconds(ghostInterval);
        }
    }

    // Hàm phụ dọn rác Mesh (Tối ưu game)
    private IEnumerator CleanupMesh(GameObject obj, Mesh mesh, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (mesh != null) Destroy(mesh); 
    }

    private IEnumerator BackstepAndSpawnRoutine()
    {
        agent.isStopped = true;

        if (view != null) view.animator.SetTrigger("Backstep");

        Vector3 backstepDir = (transform.position - playerTarget.position).normalized;
        backstepDir.y = 0;
        
        float dashTime = 0.3f;
        float elapsedTime = 0f;
        float backstepSpeed = 10f; 

        while (elapsedTime < dashTime)
        {
            elapsedTime += Time.deltaTime;
            if (agent.enabled) agent.Move(backstepDir * backstepSpeed * Time.deltaTime);
            yield return null;
        }

        if (view != null) view.animator.SetTrigger("Spawn");
        yield return new WaitForSeconds(timeToCrouch); 

        int spawnAmount = isPhase2 ? childCountPhase2 : childCountPhase1;
        Vector3 spawnLoc = (spawnPoint != null) ? spawnPoint.position : transform.position;

        for (int i = 0; i < spawnAmount; i++)
        {
            Vector3 randomOffset = new Vector3(Random.Range(-1.5f, 1.5f), 0, Random.Range(-1.5f, 1.5f));
            Vector3 finalSpawnPos = spawnLoc + randomOffset;

            if (spawnVFX != null)
            {
                GameObject vfx = Instantiate(spawnVFX, finalSpawnPos + Vector3.up * 0.5f, Quaternion.identity);
                Destroy(vfx, 2f); 
            }

            if (childPrefab != null)
            {
                Instantiate(childPrefab, finalSpawnPos, Quaternion.identity);
            }
            
            yield return new WaitForSeconds(timeBetweenSpawns); 
        }

        lastSpawnTime = Time.time;
        yield return new WaitForSeconds(timeToStandUp);
        ChangeState(MatriarchState.Chase);
    }

    private IEnumerator FrenzyTransitionRoutine()
    {
        isPhase2 = true;
        agent.isStopped = true;

        if (view != null) view.animator.SetTrigger("Frenzy");

        if (phase2AuraVFX != null)
        {
            activeFrenzyVFX = Instantiate(phase2AuraVFX, transform.position, Quaternion.identity, transform);
        }

        yield return new WaitForSeconds(2f); 

        if (model.data != null)
        {
            agent.speed = model.data.moveSpeed * frenzySpeedMultiplier;
            model.data.damage *= 1.5f; 
        }

        spawnCooldown *= 0.6f; 
        ChangeState(MatriarchState.Chase);
    }
}