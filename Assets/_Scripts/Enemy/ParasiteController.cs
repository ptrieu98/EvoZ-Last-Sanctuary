using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public enum ParasiteState { Search, Leaping, Parasitizing, Fleeing, Dead }

[RequireComponent(typeof(NavMeshAgent))]
public class ParasiteController : MonoBehaviour
{
    [Header("=== LIÊN KẾT MVC CỦA ẾCH ===")]
    public EnemyModel model;
    public EnemyView view;

    [Header("=== CÀI ĐẶT KÝ SINH ===")]
    public ParasiteState currentState = ParasiteState.Search;
    public float searchRadius = 15f;
    public float jumpDuration = 0.5f; 
    public float jumpHeight = 2.5f;   
    public float hostHpThreshold = 0.15f; 

    [Header("=== VỊ TRÍ, GÓC XOAY VÀ HIỆU ỨNG ===")]
    public Vector3 headOffset = new Vector3(0, 0.2f, 0); 
    public Vector3 headRotationOffset = Vector3.zero; 
    public float hostScaleMultiplier = 1.15f;
    
    public float mutationDuration = 0.5f; 
    public GameObject buffAuraVFX; 
    public Vector3 vfxOffset = new Vector3(0, 1f, 0); 
    public GameObject frogMindControlVFX; 

    [Header("=== VỤ NỔ (DETONATE) ===")]
    public GameObject explosionVFX; 
    public float explosionDamage = 50f;
    public float explosionRadius = 4f;

    // --- BIẾN NỘI BỘ ---
    private NavMeshAgent agent;
    private EnemyModel currentHostModel; 
    private Transform playerTarget;
    private Collider[] parasiteColliders; 
    private GameObject activeBuffVFX;  
    private GameObject activeFrogVFX; 
    private Vector3 originalHostScale; 
    private Vector3 originalFrogScale; 
    private GameObject frogHealthBarObj; 

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        parasiteColliders = GetComponentsInChildren<Collider>(); 
        
        if (model == null) model = GetComponent<EnemyModel>();
        if (view == null) view = GetComponent<EnemyView>();

        originalFrogScale = transform.localScale;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTarget = player.transform;

        GameObject healthBarPrefab = Resources.Load<GameObject>("UI_EnemyHealthBar");
        if (healthBarPrefab != null)
        {
            frogHealthBarObj = Instantiate(healthBarPrefab, transform);
            frogHealthBarObj.transform.localPosition = new Vector3(0, 0.6f, 0); 
            EnemyHealthBar hbScript = frogHealthBarObj.GetComponent<EnemyHealthBar>();
            if (hbScript != null) hbScript.Setup(model);
        }
    }

    void Update()
    {
        if (model.isDead) 
        {
            if (currentState != ParasiteState.Dead) ChangeState(ParasiteState.Dead);
            return;
        }

        switch (currentState)
        {
            case ParasiteState.Search: HandleSearch(); break;
            case ParasiteState.Parasitizing: HandleParasitizing(); break;
            case ParasiteState.Fleeing: HandleFleeing(); break;
        }
    }

    private void ChangeState(ParasiteState newState)
    {
        currentState = newState;

        if (currentState == ParasiteState.Leaping || currentState == ParasiteState.Parasitizing)
        {
            foreach(Collider col in parasiteColliders) 
            {
                if (col != null) col.enabled = false; 
            }
            if (frogHealthBarObj != null) frogHealthBarObj.SetActive(false); 
        }
        else if (currentState == ParasiteState.Fleeing || currentState == ParasiteState.Search)
        {
            if (currentState == ParasiteState.Fleeing) ResetTimeScale();
            CleanupHostVFX();

            transform.SetParent(null); 
            transform.localScale = originalFrogScale; 
            transform.localRotation = Quaternion.identity; 

            if (agent != null)
            {
                agent.enabled = true;
                NavMeshHit navHit;
                if (NavMesh.SamplePosition(transform.position, out navHit, 5.0f, NavMesh.AllAreas))
                {
                    transform.position = navHit.position; 
                }
                
                agent.isStopped = false;
                if (model.data != null) agent.speed = model.data.moveSpeed; 
            }

            foreach(Collider col in parasiteColliders) 
            {
                if (col != null) col.enabled = true; 
            }
            
            if (frogHealthBarObj != null) frogHealthBarObj.SetActive(true); 

            if (currentState == ParasiteState.Fleeing && view != null) 
            {
                view.animator.SetTrigger("Walk");
            }
        }
        else if (currentState == ParasiteState.Dead)
        {
            ResetTimeScale();
            CleanupHostVFX();

            transform.SetParent(null); 
            transform.localScale = originalFrogScale; 
            
            if (agent != null) agent.enabled = false;
            
            foreach(Collider col in parasiteColliders) 
            {
                if (col != null) col.enabled = false; 
            }
            
            Destroy(gameObject, 2f); 
        }
    }

    // =========================================================
    // RADAR QUÉT THÔNG MINH (TÌM MỌI LOẠI ZOMBIE)
    // =========================================================
    private Transform FindBestHost()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, searchRadius);
        Transform bestHost = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider col in hitColliders)
        {
            if (col.gameObject == this.gameObject) continue;

            EnemyModel potentialHostModel = col.GetComponent<EnemyModel>();
            if (potentialHostModel == null) potentialHostModel = col.GetComponentInParent<EnemyModel>();

            if (potentialHostModel != null && !potentialHostModel.isDead)
            {
                // Bỏ qua nếu mục tiêu là một con ếch khác
                if (potentialHostModel.GetComponent<ParasiteController>() != null || potentialHostModel.GetComponentInParent<ParasiteController>() != null) continue;

                // Bỏ qua nếu mục tiêu đã bị ký sinh rồi
                if (potentialHostModel.GetComponentInChildren<ParasiteController>() == null)
                {
                    float dist = Vector3.Distance(transform.position, potentialHostModel.transform.position);
                    if (dist < closestDistance)
                    {
                        closestDistance = dist;
                        bestHost = potentialHostModel.transform;
                        currentHostModel = potentialHostModel; 
                    }
                }
            }
        }
        return bestHost;
    }

    private void HandleSearch()
    {
        Transform bestHost = FindBestHost();

        if (bestHost != null)
        {
            if (agent != null) agent.enabled = false;
            ChangeState(ParasiteState.Leaping);
            StartCoroutine(LeapToHost(bestHost));
        }
        else
        {
            ChangeState(ParasiteState.Fleeing);
        }
    }

    // =========================================================
    // HÀM CHẠY TRỐN ĐÃ ĐƯỢC ĐẶT LẠI VÀO ĐÂY (SẼ KHÔNG CÒN BÁO LỖI)
    // =========================================================
    private void HandleFleeing()
    {
        // Đang chạy trốn vẫn bật Radar quét liên tục
        Transform bestHost = FindBestHost();
        
        if (bestHost != null)
        {
            if (agent != null) agent.enabled = false;
            ChangeState(ParasiteState.Leaping);
            StartCoroutine(LeapToHost(bestHost));
            return;
        }

        if (playerTarget == null) return;

        if (agent != null && agent.isOnNavMesh)
        {
            Vector3 dirAwayFromPlayer = (transform.position - playerTarget.position).normalized;
            Vector3 fleePos = transform.position + dirAwayFromPlayer * 10f; 
            agent.SetDestination(fleePos);
        }
    }

    private IEnumerator LeapToHost(Transform targetZombie)
    {
        transform.SetParent(null);
        transform.localScale = originalFrogScale; 

        Time.timeScale = 0.3f;
        Time.fixedDeltaTime = 0.02f * Time.timeScale; 

        Vector3 lookDir = (targetZombie.position - transform.position).normalized;
        lookDir.y = 0;
        if (lookDir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(lookDir);
            float rotateTime = 0f;
            while (rotateTime < 0.15f)
            {
                rotateTime += Time.unscaledDeltaTime; 
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotateTime / 0.15f);
                yield return null;
            }
        }

        if (view != null) view.animator.SetTrigger("Jump");

        Vector3 startPos = transform.position;
        float timeElapsed = 0f;

        Transform targetHead = targetZombie;
        Animator targetAnim = targetZombie.GetComponentInChildren<Animator>();
        if (targetAnim != null && targetAnim.isHuman)
        {
            Transform headBone = targetAnim.GetBoneTransform(HumanBodyBones.Head);
            if (headBone != null) targetHead = headBone;
        }

        while (timeElapsed < jumpDuration)
        {
            if (currentHostModel == null || currentHostModel.isDead)
            {
                ResetTimeScale();
                transform.parent = null;
                ChangeState(ParasiteState.Search);
                yield break;
            }

            timeElapsed += Time.unscaledDeltaTime; 
            float percent = timeElapsed / jumpDuration;

            Vector3 currentTargetPos = targetHead.position + headOffset; 
            Vector3 lerpPos = Vector3.Lerp(startPos, currentTargetPos, percent);
            lerpPos.y += Mathf.Sin(percent * Mathf.PI) * jumpHeight; 

            transform.position = lerpPos;

            yield return null;
        }

        ResetTimeScale();

        transform.SetParent(targetHead);
        transform.localPosition = headOffset; 
        transform.localRotation = Quaternion.Euler(headRotationOffset); 
        
        if (view != null) view.animator.SetTrigger("Idle");

        if (buffAuraVFX != null) 
        {
            Vector3 vfxSpawnPos = currentHostModel.transform.position + vfxOffset;
            activeBuffVFX = Instantiate(buffAuraVFX, vfxSpawnPos, Quaternion.identity, currentHostModel.transform);
        }

        if (frogMindControlVFX != null) 
        {
            activeFrogVFX = Instantiate(frogMindControlVFX, transform.position, Quaternion.identity, transform);
            activeFrogVFX.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        }

        originalHostScale = currentHostModel.transform.localScale;
        Vector3 targetHostScale = originalHostScale * hostScaleMultiplier;
        float mutationTime = 0f;

        while (mutationTime < mutationDuration)
        {
            if (currentHostModel == null || currentHostModel.isDead)
            {
                CleanupHostVFX();
                transform.parent = null;
                transform.localScale = originalFrogScale;
                ChangeState(ParasiteState.Search);
                yield break;
            }

            mutationTime += Time.deltaTime;
            float percent = mutationTime / mutationDuration;
            float smoothPercent = Mathf.SmoothStep(0f, 1f, percent); 

            currentHostModel.transform.localScale = Vector3.Lerp(originalHostScale, targetHostScale, smoothPercent);

            float currentMultiplier = Mathf.Lerp(1f, hostScaleMultiplier, smoothPercent);
            transform.localScale = originalFrogScale / currentMultiplier;

            yield return null;
        }

        currentHostModel.transform.localScale = targetHostScale;
        transform.localScale = originalFrogScale / hostScaleMultiplier; 

        if (currentHostModel.data != null)
        {
            currentHostModel.data = Instantiate(currentHostModel.data);
            currentHostModel.data.maxHealth *= 1.5f;
            currentHostModel.currentHealth = currentHostModel.data.maxHealth; 
            currentHostModel.data.moveSpeed *= 1.5f;
            currentHostModel.data.damage *= 2f;

            NavMeshAgent hostAgent = currentHostModel.GetComponent<NavMeshAgent>();
            if (hostAgent != null) hostAgent.speed = currentHostModel.data.moveSpeed;
        }

        EnemyHealthBar hostHealthBar = currentHostModel.GetComponentInChildren<EnemyHealthBar>();
        if (hostHealthBar != null)
        {
            hostHealthBar.UpgradeTier();
        }

        ChangeState(ParasiteState.Parasitizing);
    }

    private void HandleParasitizing()
    {
        if (currentHostModel == null || currentHostModel.isDead)
        {
            CleanupHostVFX();
            transform.SetParent(null);
            transform.localScale = originalFrogScale; 
            ChangeState(ParasiteState.Search);
            return;
        }

        float hpPercent = currentHostModel.currentHealth / currentHostModel.data.maxHealth;
        if (hpPercent <= hostHpThreshold)
        {
            DetonateHost();
        }
    }

    private void DetonateHost()
    {
        CleanupHostVFX(); 

        Vector3 detonatePos = currentHostModel.transform.position;

        if (explosionVFX != null)
        {
            GameObject vfx = Instantiate(explosionVFX, detonatePos + Vector3.up * 1f, Quaternion.identity);
            Destroy(vfx, 2f);
        }

        Collider[] hitColliders = Physics.OverlapSphere(detonatePos, explosionRadius);
        foreach (Collider hit in hitColliders)
        {
            if (hit.gameObject == gameObject) continue; 

            if (hit.CompareTag("Player"))
            {
                IDamageable damageable = hit.GetComponent<IDamageable>();
                if (damageable == null) damageable = hit.GetComponentInParent<IDamageable>();
                if (damageable != null) damageable.TakeDamage(explosionDamage, model.armorPenetration, model.accuracy);
            }
        }

        Destroy(currentHostModel.gameObject); 
        currentHostModel = null;

        transform.SetParent(null);
        transform.localScale = originalFrogScale;

        ChangeState(ParasiteState.Search);
    }

    private void CleanupHostVFX()
    {
        if (activeBuffVFX != null) Destroy(activeBuffVFX);
        if (activeFrogVFX != null) Destroy(activeFrogVFX);
    }

    private void ResetTimeScale()
    {
        Time.timeScale = 1.0f;
        Time.fixedDeltaTime = 0.02f;
    }
}