using UnityEngine;
using UnityEngine.AI;

public enum EnemyState { Idle, Aggro, Chase, Attack, Flee, Dead, Hit }

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour
{
    [Header("=== LIÊN KẾT MVC ===")]
    public EnemyModel model;
    public EnemyView view;
    
    [Header("=== TRẠNG THÁI AI ===")]
    public EnemyState currentState = EnemyState.Idle;
    public float detectionRadius = 15f; 

    [Header("=== CÀI ĐẶT THỜI GIAN ===")]
    public float aggroDuration = 1.5f; 
    public float hitDuration = 0.3f; 

    private NavMeshAgent agent;
    private Transform targetPlayer;
    private float lastAttackTime = 0f;
    private float aggroTimer = 0f;
    private float hitTimer = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (model == null) model = GetComponent<EnemyModel>();
        if (view == null) view = GetComponent<EnemyView>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) targetPlayer = playerObj.transform;

        if (model.data != null)
        {
            agent.speed = model.data.moveSpeed;
            agent.stoppingDistance = model.data.attackRange - 0.2f; 
        }

        GameObject healthBarPrefab = Resources.Load<GameObject>("UI_EnemyHealthBar");
        if (healthBarPrefab != null)
        {
            GameObject hb = Instantiate(healthBarPrefab, transform);
            hb.transform.localPosition = new Vector3(0, 2f, 0); 
            EnemyHealthBar hbScript = hb.GetComponent<EnemyHealthBar>();
            if (hbScript != null) hbScript.Setup(model); 
        }
    }

    void Update()
    {
        if (model.isDead) return;

        switch (currentState)
        {
            case EnemyState.Idle: HandleIdle(); break;
            case EnemyState.Aggro: HandleAggro(); break;
            case EnemyState.Chase: HandleChase(); break;
            case EnemyState.Attack: HandleAttack(); break;
            case EnemyState.Hit: HandleHit(); break;
            case EnemyState.Flee: HandleFlee(); break;
        }

        if (view != null && agent.isOnNavMesh) 
            view.SetMoveSpeedAnimation(agent.velocity.magnitude);
    }

    public void ChangeState(EnemyState newState)
    {
        if (currentState == EnemyState.Dead) return; 
        currentState = newState;

        if (currentState == EnemyState.Aggro || currentState == EnemyState.Hit)
        {
            if (currentState == EnemyState.Aggro) aggroTimer = aggroDuration;
            if (currentState == EnemyState.Hit) hitTimer = hitDuration;

            if (agent != null && agent.isOnNavMesh)
            {
                agent.isStopped = true;        
                agent.ResetPath();             
                agent.velocity = Vector3.zero; 
            }
        }
        else if (currentState == EnemyState.Dead)
        {
            if (agent != null && agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.ResetPath();
                agent.velocity = Vector3.zero;
            }
            if (agent != null) agent.enabled = false; 
            Destroy(gameObject, 5f);
        }
    }

    private void HandleIdle()
    {
        if (targetPlayer == null) return;
        if (Vector3.Distance(transform.position, targetPlayer.position) <= detectionRadius)
        {
            ChangeState(EnemyState.Aggro);
            if (view != null) view.PlayAggroAnimation(); 
        }
    }

    private void HandleAggro()
    {
        if (targetPlayer == null) return;
        LookAtTarget();
        aggroTimer -= Time.deltaTime;
        if (aggroTimer <= 0) ChangeState(EnemyState.Chase);
    }

    private void HandleChase()
    {
        if (targetPlayer == null) return;
        if (agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.speed = model.data.moveSpeed; 
            agent.SetDestination(targetPlayer.position); 
        }
        if (Vector3.Distance(transform.position, targetPlayer.position) <= model.data.attackRange)
        {
            ChangeState(EnemyState.Attack);
        }
    }

    private void HandleAttack()
    {
        if (targetPlayer == null) return;
        if (agent.isOnNavMesh) agent.isStopped = true; 
        
        LookAtTarget();

        if (Vector3.Distance(transform.position, targetPlayer.position) > model.data.attackRange)
        {
            ChangeState(EnemyState.Chase);
            return;
        }

        if (Time.time >= lastAttackTime + model.data.attackCooldown)
        {
            if (view != null) view.PlayAttackAnimation(); 
            lastAttackTime = Time.time;
        }
    }

    private void HandleHit()
    {
        hitTimer -= Time.deltaTime;
        if (hitTimer <= 0)
        {
            if (targetPlayer != null && Vector3.Distance(transform.position, targetPlayer.position) <= model.data.attackRange)
                ChangeState(EnemyState.Attack); 
            else
                ChangeState(EnemyState.Chase);  
        }
    }

    private void HandleFlee()
    {
        if (targetPlayer == null) return;
        if (agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.speed = model.data.moveSpeed * model.data.fleeSpeedMultiplier; 
            Vector3 dirAwayFromPlayer = (transform.position - targetPlayer.position).normalized;
            Vector3 fleePos = transform.position + dirAwayFromPlayer * 10f; 
            agent.SetDestination(fleePos);
        }
    }

    private void LookAtTarget()
    {
        Vector3 direction = (targetPlayer.position - transform.position).normalized;
        direction.y = 0; 
        if (direction != Vector3.zero) 
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 5f);
    }

    // ==========================================
    // QUÁI GÂY SÁT THƯƠNG (TRUYỀN CHỈ SỐ MỚI)
    // ==========================================
    public void ExecuteDamageFrame()
    {
        if (model.isDead || targetPlayer == null) return;

        float currentDist = Vector3.Distance(transform.position, targetPlayer.position);
        if (currentDist <= model.data.attackRange + 0.3f) 
        {
            IDamageable damageable = targetPlayer.GetComponent<IDamageable>();
            if (damageable == null) damageable = targetPlayer.GetComponentInParent<IDamageable>();

            if (damageable != null)
            {
                // CẬP NHẬT: Gửi kèm Xuyên giáp và Chuẩn xác của quái sang Player
                damageable.TakeDamage(model.data.damage, model.armorPenetration, model.accuracy); 
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}