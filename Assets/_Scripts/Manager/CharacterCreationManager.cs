using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterCreationManager : MonoBehaviour
{
    [Header("=== THAM CHIẾU MODEL 3D ===")]
    public PlayerView previewModel; 

    [Header("=== GIAO DIỆN UI ===")]
    [Tooltip("Kéo nguyên cả cụm UI Râu (gồm Chữ và 2 nút mũi tên) vào đây để ẩn khi chọn Nữ")]
    public GameObject beardSection; 

    [Header("=== ĐIỀU KHIỂN XOAY NHÂN VẬT ===")]
    public float rotationSpeed = 10f;

    [Header("=== DỮ LIỆU ĐANG CHỌN ===")]
    private int currentGender = 0; // 0: Nam, 1: Nữ
    private int currentOutfit = 0;
    private int currentHair = 0;
    private int currentGlasses = 0;
    private int currentBeard = 0;

    private void Start()
    {
        // Khóa di chuyển khi đang ở giao diện tạo nhân vật
        PlayerController pc = previewModel.GetComponent<PlayerController>();
        if (pc != null) pc.enabled = false;

        SelectGender(0);
    }

    private void Update()
    {
        // Giữ chuột trái để xoay nhân vật
        if (Input.GetMouseButton(0))
        {
            float mouseX = Input.GetAxis("Mouse X");
            previewModel.transform.Rotate(Vector3.up, -mouseX * rotationSpeed, Space.World);
        }
    }

    // --- 1. XỬ LÝ CHỌN GIỚI TÍNH ---
    public void SelectGender(int genderID)
    {
        currentGender = genderID;
        
        // Reset toàn bộ thông số về 0
        currentOutfit = 0; 
        currentHair = 0;
        currentGlasses = 0;
        currentBeard = 0;

        if (currentGender == 1) // NỮ
        {
            if (beardSection != null) beardSection.SetActive(false); 
        }
        else // NAM
        {
            if (beardSection != null) beardSection.SetActive(true); 
        }

        UpdatePreview();
    }

    // ========================================================
    // --- 2. CÁC HÀM XỬ LÝ NÚT MŨI TÊN BẤM TRÁI / PHẢI ---
    // ========================================================

    // --- TRANG PHỤC (OUTFIT) ---
    public void NextOutfit()
    {
        // Lấy tổng số lượng trang phục tùy theo giới tính
        int max = (currentGender == 0) ? previewModel.maleOutfits.Length : previewModel.femaleOutfits.Length;
        if (max <= 0) return; // Chống lỗi nếu bạn chưa kéo 3D vào
        
        currentOutfit = (currentOutfit + 1) % max; // Tăng 1, nếu vượt quá thì quay về 0
        UpdatePreview();
    }
    public void PrevOutfit()
    {
        int max = (currentGender == 0) ? previewModel.maleOutfits.Length : previewModel.femaleOutfits.Length;
        if (max <= 0) return;

        currentOutfit = (currentOutfit - 1 + max) % max; // Lùi 1, nếu dưới 0 thì nhảy lên số to nhất
        UpdatePreview();
    }

    // --- TÓC (HAIR) ---
    public void NextHair()
    {
        int max = (currentGender == 0) ? previewModel.maleHairs.Length : previewModel.femaleHairs.Length;
        if (max <= 0) return;
        currentHair = (currentHair + 1) % max;
        UpdatePreview();
    }
    public void PrevHair()
    {
        int max = (currentGender == 0) ? previewModel.maleHairs.Length : previewModel.femaleHairs.Length;
        if (max <= 0) return;
        currentHair = (currentHair - 1 + max) % max;
        UpdatePreview();
    }

    // --- KÍNH (GLASSES) ---
    public void NextGlasses()
    {
        int max = (currentGender == 0) ? previewModel.maleGlasses.Length : previewModel.femaleGlasses.Length;
        if (max <= 0) return;
        currentGlasses = (currentGlasses + 1) % max;
        UpdatePreview();
    }
    public void PrevGlasses()
    {
        int max = (currentGender == 0) ? previewModel.maleGlasses.Length : previewModel.femaleGlasses.Length;
        if (max <= 0) return;
        currentGlasses = (currentGlasses - 1 + max) % max;
        UpdatePreview();
    }

    // --- RÂU (BEARD - CHỈ DÀNH CHO NAM) ---
    public void NextBeard()
    {
        if (currentGender == 1) return; // Nữ thì không chạy hàm này
        int max = previewModel.maleBeards.Length;
        if (max <= 0) return;
        currentBeard = (currentBeard + 1) % max;
        UpdatePreview();
    }
    public void PrevBeard()
    {
        if (currentGender == 1) return;
        int max = previewModel.maleBeards.Length;
        if (max <= 0) return;
        currentBeard = (currentBeard - 1 + max) % max;
        UpdatePreview();
    }

    // --- CẬP NHẬT MÔ HÌNH THỰC TẾ ---
    private void UpdatePreview()
    {
        previewModel.UpdateAppearance(currentGender, currentOutfit, currentHair, currentBeard, currentGlasses);
    }

    // --- LƯU VÀ VÀO GAME ---
    public void StartGame()
    {
        // Lưu dữ liệu vào file JSON thay vì PlayerPrefs
        SaveSystem.SavePlayerAppearance(currentGender, currentOutfit, currentHair, currentBeard, currentGlasses);
        
        SceneManager.LoadScene("Core"); 
    }
}