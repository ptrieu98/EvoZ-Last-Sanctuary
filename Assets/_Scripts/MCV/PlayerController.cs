using System.Collections;
using UnityEngine;

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

    // --- Các Component hệ thống ---
    private Rigidbody rb;
    private CapsuleCollider capsuleCol;
    private Camera mainCamera;

    // --- Các biến tính toán ---
    private LedgeJumpPoint currentLedge;
    private Vector3 movementInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        capsuleCol = GetComponent<CapsuleCollider>();
        mainCamera = Camera.main; 

        if (model == null) model = GetComponent<PlayerModel>();
        if (view == null) view = GetComponent<PlayerView>();
    }

    void Update()
    {
        if (isJumping) return;

        // --- HỒI PHỤC THỂ LỰC ---
        if (model.currentStamina < model.maxStamina && !isDashing)
        {
            model.currentStamina += model.staminaRegenRate * Time.deltaTime;
            model.currentStamina = Mathf.Clamp(model.currentStamina, 0, model.maxStamina);
        }

        // 1. Nhận phím di chuyển
        movementInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")).normalized;

        // 2. LƯỚT (Dash)
        if (Input.GetKeyDown(KeyCode.Space) && !isDashing && model.currentStamina >= model.dashStaminaCost)
        {
            model.currentStamina -= model.dashStaminaCost; 
            StartCoroutine(DashRoutine());
        }

        // 3. NHẢY VÁCH (Ledge Jump)
        if (Input.GetKeyDown(KeyCode.LeftShift) && currentLedge != null && !isDashing)
        {
            StartCoroutine(JumpDownRoutine());
        }

        // 4. XOAY MẶT
        if (!isDashing)
        {
            AimAtMouse();
        }
    }

    void FixedUpdate()
    {
        // Khóa di chuyển bình thường nếu nhân vật đang nhảy vách hoặc đang lướt
        if (isJumping || isDashing) return;

        Vector3 targetVelocity = movementInput * model.moveSpeed;

        // --- LỚP BẢO VỆ 1: RADAR TRƯỢT TƯỜNG ---
        if (movementInput.sqrMagnitude > 0)
        {
            // Bắn tia radar hình cầu từ vị trí ngực nhân vật ra phía trước
            Vector3 chestPosition = transform.position + Vector3.up * 1f; 
            
            // Dò tìm vật cản trong bán kính 0.4 mét (vừa khít với bề ngang cơ thể)
            if (Physics.SphereCast(chestPosition, 0.4f, movementInput, out RaycastHit hit, 0.5f))
            {
                // Nếu bề mặt va chạm dốc đứng (normal.y < 0.3 nghĩa là dốc hơn 70 độ) -> Đây là Tường
                if (hit.normal.y < 0.3f) 
                {
                    // Nắn lại vận tốc: Chuyển từ "đâm thẳng" sang "trượt dọc" theo mặt tường
                    targetVelocity = Vector3.ProjectOnPlane(targetVelocity, hit.normal);
                }
            }
        }

        // --- LỚP BẢO VỆ 2: GHÌ TRỌNG TÂM (CHỐNG BAY) ---
        float currentYVel = rb.linearVelocity.y;
        
        // Nếu vật lý Unity cố tình nảy nhân vật lên (Y > 0) do leo dốc/bậc thang
        if (currentYVel > 0)
        {
            // Ép vận tốc nảy lại tối đa chỉ ở mức 2f (Đủ để bước lên bậc thang, tuyệt đối cấm bay lên không trung)
            currentYVel = Mathf.Clamp(currentYVel, 0f, 2f); 
        }

        // Áp dụng vận tốc cuối cùng
        rb.linearVelocity = new Vector3(targetVelocity.x, currentYVel, targetVelocity.z);
        
        // Bồi thêm trọng lực nhân tạo để gót chân luôn bám dính xuống sàn
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

            if (lookDirection.sqrMagnitude > 0.1f) 
            {
                transform.rotation = Quaternion.LookRotation(lookDirection);
            }
        }
    }

    // --- THUẬT TOÁN LƯỚT (Đã Tối Ưu Chống Xuyên Tường) ---
    private IEnumerator DashRoutine()
    {
        isDashing = true;
        
        if (view != null) view.PlayDashEffects();

        Vector3 dashDirection = movementInput.magnitude > 0.1f ? movementInput : transform.forward;
        float actualDashSpeed = model.moveSpeed * model.dashMultiplier;
        float timePassed = 0f;

        while (timePassed < model.dashDuration)
        {
            // ÉP VẬN TỐC THUẦN TÚY: Để Unity tự tính toán va chạm theo thời gian thực (Continuous)
            rb.linearVelocity = dashDirection * actualDashSpeed;
            
            timePassed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate(); 
        }

        // PHANH KHẨN CẤP: Lướt xong phải xóa vận tốc ngay lập tức để không bị trượt đi tiếp
        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        
        isDashing = false;
    }

    // --- THUẬT TOÁN NHẢY VÁCH TỰ ĐỘNG DÒ ĐƯỜNG ---
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

        if (Physics.Raycast(rayStartPos, Vector3.down, out RaycastHit hit, 20f))
        {
            groundY = hit.point.y; 
        }

        float pivotToFeetOffset = transform.position.y - capsuleCol.bounds.min.y;
        Vector3 targetPosition = new Vector3(predictedLandingXZ.x, groundY + pivotToFeetOffset, predictedLandingXZ.z);

        float timePassed = 0f;
        float duration = 0.5f; 

        while (timePassed < duration)
        {
            timePassed += Time.deltaTime;
            float linearT = timePassed / duration;

            Vector3 currentPos = Vector3.Lerp(startPosition, targetPosition, linearT);
            float heightOffset = Mathf.Sin(linearT * Mathf.PI) * model.jumpHeight;
            currentPos.y += heightOffset;

            transform.position = currentPos;
            yield return null;
        }

        transform.position = targetPosition; 
        
        // Reset lại vận tốc vật lý trước khi bật va chạm lại
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
}