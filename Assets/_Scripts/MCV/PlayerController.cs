using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems; // Bắt buộc thêm dòng này để nhận diện chuột trên UI

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    [Header("=== LIÊN KẾT MVC ===")]
    public PlayerModel model;
    public PlayerView view;

    [Header("=== TRẠNG THÁI ===")]
    public bool isJumping = false;
    public bool isDashing = false;
    public float jumpDistance = 3.5f; 

    private Rigidbody rb;
    private CapsuleCollider capsuleCol;
    private Camera mainCamera;
    private LedgeJumpPoint currentLedge;
    private Vector3 movementInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        capsuleCol = GetComponent<CapsuleCollider>();
        mainCamera = Camera.main; 

        if (model == null) model = GetComponent<PlayerModel>();
        if (view == null) view = GetComponent<PlayerView>();

        PlayerData savedData = SaveSystem.LoadPlayerAppearance();
        if (savedData != null && view != null)
        {
            view.UpdateAppearance(savedData.gender, savedData.outfitID, savedData.hairID, savedData.beardID, savedData.glassesID);
        }
    }

    void Update()
    {
        if (isJumping) return;

        if (model.currentStamina < model.maxStamina && !isDashing)
        {
            model.currentStamina += model.staminaRegenRate * Time.deltaTime;
            model.currentStamina = Mathf.Clamp(model.currentStamina, 0, model.maxStamina);
        }

        movementInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")).normalized;

        if (Input.GetKeyDown(KeyCode.Space) && !isDashing && model.currentStamina >= model.dashStaminaCost)
        {
            model.currentStamina -= model.dashStaminaCost; 
            StartCoroutine(DashRoutine());
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && currentLedge != null && !isDashing)
        {
            StartCoroutine(JumpDownRoutine());
        }

        if (!isDashing) AimAtMouse();

        if (!isDashing && !isJumping)
        {
            Vector3 localMove = transform.InverseTransformDirection(movementInput);
            if (view != null) view.UpdateMovementAnimation(localMove.x, localMove.z);
        }
        
        HandleWeaponSwitch(); 
        HandleAttack();       
    }

    void FixedUpdate()
    {
        if (isJumping || isDashing) return;
        Vector3 targetVelocity = movementInput * model.moveSpeed;

        if (movementInput.sqrMagnitude > 0)
        {
            Vector3 chestPosition = transform.position + Vector3.up * 1f; 
            if (Physics.SphereCast(chestPosition, 0.4f, movementInput, out RaycastHit hit, 0.5f))
            {
                if (hit.normal.y < 0.3f) targetVelocity = Vector3.ProjectOnPlane(targetVelocity, hit.normal);
            }
        }

        float currentYVel = rb.linearVelocity.y; 
        if (currentYVel > 0) currentYVel = Mathf.Clamp(currentYVel, 0f, 2f); 

        rb.linearVelocity = new Vector3(targetVelocity.x, currentYVel, targetVelocity.z);
        rb.AddForce(Vector3.down * 30f, ForceMode.Acceleration);
    }

    private void AimAtMouse()
    {
        if (mainCamera == null) return;
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, new Vector3(0, transform.position.y, 0));
        
        if (groundPlane.Raycast(ray, out float rayDistance))
        {
            Vector3 pointToLook = ray.GetPoint(rayDistance);
            Vector3 lookDirection = pointToLook - transform.position;
            lookDirection.y = 0f; 
            if (lookDirection.sqrMagnitude > 0.1f) transform.rotation = Quaternion.LookRotation(lookDirection);
        }
    }

    private void HandleWeaponSwitch()
    {
        if (model.isAttacking) return; 

        if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchWeapon(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchWeapon(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchWeapon(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SwitchWeapon(3);
    }

    public void SwitchWeapon(int index)
    {
        model.activeWeaponIndex = index;
        ItemData newWeapon = model.equippedWeapons[index];
        
        if (newWeapon != null)
        {
            view.EquipWeapon3D(newWeapon.weaponPrefab);
            view.SetAimingStance(newWeapon.weaponType == WeaponType.Ranged);
        }
        else
        {
            view.EquipWeapon3D(null);
            view.SetAimingStance(false); 
        }
    }

    private IEnumerator DashRoutine()
    {
        isDashing = true;
        if (view != null) view.PlayDashEffects(model.dashDuration);
        Vector3 dashDirection = movementInput.magnitude > 0.1f ? movementInput : transform.forward;
        float actualDashSpeed = model.moveSpeed * model.dashMultiplier;
        float timePassed = 0f;

        while (timePassed < model.dashDuration)
        {
            rb.linearVelocity = dashDirection * actualDashSpeed;
            timePassed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate(); 
        }
        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        isDashing = false;
    }

    private IEnumerator JumpDownRoutine()
    {
        isJumping = true;
        currentLedge = null; 
        rb.isKinematic = true;           
        capsuleCol.enabled = false;      

        Vector3 startPosition = transform.position;
        Vector3 jumpDirection = transform.forward; 
        Vector3 predictedLandingXZ = startPosition + (jumpDirection * jumpDistance);
        Vector3 rayStartPos = new Vector3(predictedLandingXZ.x, startPosition.y + 5f, predictedLandingXZ.z);
        float groundY = startPosition.y - 10f; 

        if (Physics.Raycast(rayStartPos, Vector3.down, out RaycastHit hit, 20f)) groundY = hit.point.y; 
        float pivotToFeetOffset = transform.position.y - capsuleCol.bounds.min.y;
        Vector3 targetPosition = new Vector3(predictedLandingXZ.x, groundY + pivotToFeetOffset, predictedLandingXZ.z);

        float timePassed = 0f;
        float duration = 0.5f; 

        while (timePassed < duration)
        {
            timePassed += Time.deltaTime;
            float linearT = timePassed / duration;
            Vector3 currentPos = Vector3.Lerp(startPosition, targetPosition, linearT);
            currentPos.y += Mathf.Sin(linearT * Mathf.PI) * model.jumpHeight;
            transform.position = currentPos;
            yield return null;
        }

        transform.position = targetPosition; 
        rb.linearVelocity = Vector3.zero;
        capsuleCol.enabled = true; 
        rb.isKinematic = false;    
        isJumping = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        LedgeJumpPoint ledge = other.GetComponent<LedgeJumpPoint>();
        if (ledge != null) currentLedge = ledge;
    }

    private void OnTriggerExit(Collider other)
    {
        LedgeJumpPoint ledge = other.GetComponent<LedgeJumpPoint>();
        if (ledge != null && currentLedge == ledge) currentLedge = null;
    }

    // --- LOGIC CHIẾN ĐẤU ---
    private void HandleAttack()
    {
        // 🔒 CHỐNG LỖI 1: Bấm chuột vào UI (Kéo đồ) thì không được tính là đang đánh
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        if (Input.GetMouseButtonDown(0) && !model.isAttacking && model.CurrentWeapon != null)
        {
            if (model.CurrentWeapon.weaponType == WeaponType.Melee) StartCoroutine(AttackMeleeRoutine());
            else StartCoroutine(ShootRangedRoutine());
        }
    }

    private IEnumerator AttackMeleeRoutine()
    {
        model.isAttacking = true;
        if (view != null) view.PlayAttackAnimation(WeaponType.Melee);

        yield return new WaitForSeconds(model.attackDuration / 2f);

        // 🔒 CHỐNG LỖI 2: Vũ khí bị kéo vứt đi giữa chừng thì hủy sát thương, không báo lỗi
        if (model.CurrentWeapon != null)
        {
            float range = model.CurrentWeapon.attackRange; 
            Vector3 hitCenter = transform.position + transform.forward * (range / 2f); 
            Collider[] hitTargets = Physics.OverlapSphere(hitCenter, range / 2f);

            foreach (Collider target in hitTargets)
            {
                IDamageable damageable = target.GetComponent<IDamageable>();
                if (damageable != null) damageable.TakeDamage(model.CurrentWeapon.damage);
            }
        }

        yield return new WaitForSeconds(model.attackDuration / 2f);
        model.isAttacking = false; // Phải đảm bảo dòng này LUÔN ĐƯỢC CHẠY
    }

    private IEnumerator ShootRangedRoutine()
    {
        model.isAttacking = true;
        if (view != null) view.PlayAttackAnimation(WeaponType.Ranged);

        if (model.CurrentWeapon != null)
        {
            Vector3 shootOrigin = transform.position + Vector3.up * 1.2f; 
            float range = model.CurrentWeapon.attackRange; 
            if (Physics.Raycast(shootOrigin, transform.forward, out RaycastHit hit, range))
            {
                IDamageable damageable = hit.collider.GetComponent<IDamageable>();
                if (damageable != null) damageable.TakeDamage(model.CurrentWeapon.damage);
            }
        }

        yield return new WaitForSeconds(model.attackDuration);
        model.isAttacking = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (model == null || model.CurrentWeapon == null) return;
        if (model.CurrentWeapon.weaponType == WeaponType.Melee)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position + transform.forward * (model.CurrentWeapon.attackRange / 2f), model.CurrentWeapon.attackRange / 2f);
        }
        else if (model.CurrentWeapon.weaponType == WeaponType.Ranged)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position + Vector3.up * 1.2f, transform.forward * model.CurrentWeapon.attackRange);
        }
    }
}