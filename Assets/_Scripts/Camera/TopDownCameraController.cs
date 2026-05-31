using UnityEngine;
using Unity.Cinemachine; 

[RequireComponent(typeof(CinemachineOrbitalFollow))]
public class TopDownCameraController : MonoBehaviour
{
    private CinemachineOrbitalFollow orbitalFollow;
    
    [Header("=== XOAY & ZOOM ===")]
    public bool allowRotation = true;    
    public float rotateSpeedX = 4f;      
    public float zoomSpeed = 2f;         

    [Header("=== GIỚI HẠN ZOOM ===")]
    public float minZoom = 0.5f;     
    public float maxZoom = 2.5f;       
    
    [Header("=== ỐNG KÍNH DÒ ĐƯỜNG (MOUSE PANNING) ===")]
    public bool enableMousePan = true;   
    public float maxPanDistance = 3f;    
    public float panSmoothness = 4f;     

    void Start()
    {
        orbitalFollow = GetComponent<CinemachineOrbitalFollow>();
        // Đã xóa lệnh ép góc 55 độ ở đây. Trả lại quyền kiểm soát 100% cho giao diện Unity!
    }

    void Update()
    {
        if (orbitalFollow == null) return;

        // 1. XOAY TRÁI PHẢI
        if (allowRotation && Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X");
            var hAxis = orbitalFollow.HorizontalAxis;
            hAxis.Value += mouseX * rotateSpeedX; 
            orbitalFollow.HorizontalAxis = hAxis;
        }

        // 2. ZOOM
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.001f)
        {
            var rAxis = orbitalFollow.RadialAxis;
            rAxis.Value -= scroll * zoomSpeed;
            rAxis.Value = Mathf.Clamp(rAxis.Value, minZoom, maxZoom);
            orbitalFollow.RadialAxis = rAxis;
        }

        // 3. ỐNG KÍNH DÒ ĐƯỜNG
        if (enableMousePan)
        {
            float mouseX = (Input.mousePosition.x / Screen.width) * 2f - 1f;
            float mouseY = (Input.mousePosition.y / Screen.height) * 2f - 1f;

            mouseX = Mathf.Clamp(mouseX, -1f, 1f);
            mouseY = Mathf.Clamp(mouseY, -1f, 1f);

            Vector3 targetPan = new Vector3(mouseX * maxPanDistance, 0f, mouseY * maxPanDistance);
            orbitalFollow.TargetOffset = Vector3.Lerp(orbitalFollow.TargetOffset, targetPan, Time.deltaTime * panSmoothness);
        }
        else
        {
            orbitalFollow.TargetOffset = Vector3.Lerp(orbitalFollow.TargetOffset, Vector3.zero, Time.deltaTime * panSmoothness);
        }
    }
}