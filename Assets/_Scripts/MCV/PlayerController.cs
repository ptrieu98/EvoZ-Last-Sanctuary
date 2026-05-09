using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerModel))]
[RequireComponent(typeof(CapsuleCollider))] // Khai báo thêm Capsule Collider
public class PlayerController : MonoBehaviour
{
    private PlayerModel model;
    private Rigidbody rb;
    private CapsuleCollider capsuleCol; // Biến kiểm soát va chạm
    private Camera mainCamera;

    private Vector3 movementInput;
    
    private bool isDashing = false;
    private float currentSpeedMultiplier = 1f;

    private bool isJumping = false;
    private LedgeJumpPoint currentLedge = null;

    void Start()
    {
        model = GetComponent<PlayerModel>();
        rb = GetComponent<Rigidbody>();
        capsuleCol = GetComponent<CapsuleCollider>(); // Khởi tạo
        mainCamera = Camera.main;

        rb.freezeRotation = true; 
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    void Update()
    {
        if (isJumping) return;

        // 1. NHẬN LỆNH
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");   
        movementInput = new Vector3(moveX, 0f, moveZ).normalized; 

        // 2. DASH (Lướt)
        if (Input.GetKeyDown(KeyCode.Space) && !isDashing && model.currentStamina >= model.dashStaminaCost && movementInput.magnitude > 0)
        {
            StartCoroutine(DashRoutine());
        }

        // 3. LEDGE JUMP (Nhảy vách đá)
        if (Input.GetKeyDown(KeyCode.LeftShift) && currentLedge != null && !isDashing)
        {
            StartCoroutine(JumpDownRoutine(currentLedge.landingSpot.position));
        }

        // 4. HỒI THỂ LỰC
        if (!isDashing && model.currentStamina < model.maxStamina)
        {
            model.currentStamina += model.staminaRegenRate * Time.deltaTime;
            if (model.currentStamina > model.maxStamina) 
                model.currentStamina = model.maxStamina;
        }
    }

    void FixedUpdate()
    {
        if (isJumping) return;
        Move();
        AimAtMouse();
    }

    private void Move()
    {
        float currentSpeed = model.moveSpeed * currentSpeedMultiplier;
        Vector3 newVelocity = new Vector3(movementInput.x * currentSpeed, rb.linearVelocity.y, movementInput.z * currentSpeed);

        // --- FIX LỖI 1: CHỐNG BAY LÊN KHI ĐẠP BẬC THỀM ---
        // Nếu vận tốc trục Y bị đẩy lên > 0 (văng lên) do dẫm phải đá vụn hay bậc thềm, ta ép nó về 0 ngay lập tức.
        if (newVelocity.y > 0) 
        {
            newVelocity.y = 0; 
        }

        rb.linearVelocity = newVelocity;
    }

    private void AimAtMouse()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.up * rb.position.y);
        float rayDistance;

        if (groundPlane.Raycast(ray, out rayDistance))
        {
            Vector3 pointToLook = ray.GetPoint(rayDistance);
            Vector3 lookDirection = new Vector3(pointToLook.x, rb.position.y, pointToLook.z);
            transform.LookAt(lookDirection);
        }
    }

    private IEnumerator DashRoutine()
    {
        isDashing = true;                               
        model.currentStamina -= model.dashStaminaCost;  
        currentSpeedMultiplier = model.dashMultiplier;  
        yield return new WaitForSeconds(model.dashDuration); 
        currentSpeedMultiplier = 1f;                    
        isDashing = false;                              
    }

    private void OnTriggerEnter(Collider other)
    {
        LedgeJumpPoint ledge = other.GetComponent<LedgeJumpPoint>();
        if (ledge != null) currentLedge = ledge;
    }

    private void OnTriggerExit(Collider other)
    {
        LedgeJumpPoint ledge = other.GetComponent<LedgeJumpPoint>();
        if (ledge != null && ledge == currentLedge) currentLedge = null;
    }

    // --- BẢN HOÀN THIỆN TỐI THƯỢNG: DÙNG TIA LASER DÒ ĐỊA HÌNH ---
    private IEnumerator JumpDownRoutine(Vector3 landingSpotPos)
    {
        isJumping = true;
        
        // 1. TẮT VA CHẠM (Chống vướng mép đá 100%)
        rb.isKinematic = true;           
        capsuleCol.enabled = false;      

        Vector3 startPosition = transform.position;

        // 2. DÒ TÌM MẶT ĐẤT THẬT SỰ BẰNG RAYCAST
        // Đưa điểm bắn tia lên cao hơn nhân vật 5 đơn vị để đảm bảo bao quát được địa hình
        Vector3 rayStartPos = new Vector3(landingSpotPos.x, startPosition.y + 5f, landingSpotPos.z);
        
        float groundY = landingSpotPos.y; // Giá trị dự phòng

        // Bắn tia laser xuống dưới (tối đa 20 đơn vị). Nếu chạm vào bề mặt map (mặt đất):
        if (Physics.Raycast(rayStartPos, Vector3.down, out RaycastHit hit, 20f))
        {
            groundY = hit.point.y; // Ghi nhận độ cao thực tế của địa hình tại điểm đó
        }

        // 3. TỰ ĐỘNG ĐO GÓT CHÂN (Chống lún 100% dù Pivot ở đâu)
        // Lấy tọa độ Y của tâm nhân vật TRỪ ĐI điểm thấp nhất của Collider = Khoảng cách từ tâm đến chân
        float pivotToFeetOffset = transform.position.y - capsuleCol.bounds.min.y;

        // 4. CHỐT ĐIỂM ĐÁP: X,Z của khối Landing + Y của mặt đất thật + Độ dài chân
        Vector3 targetPosition = new Vector3(landingSpotPos.x, groundY + pivotToFeetOffset, landingSpotPos.z);

        float timePassed = 0f;
        float duration = 0.5f; 

        // 5. DI CHUYỂN VÒNG CUNG
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

        // 6. TIẾP ĐẤT CHÍNH XÁC VÀ TRẢ LẠI VẬT LÝ
        transform.position = targetPosition; 
        
        capsuleCol.enabled = true; 
        rb.isKinematic = false;    
        isJumping = false;
    }
}