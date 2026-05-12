using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterCreationManager : MonoBehaviour
{
    [Header("=== THAM CHIẾU MODEL 3D ===")]
    public PlayerView previewModel; 

    [Header("=== NÚT DANH MỤC (TAB BUTTONS) ===")]
    public GameObject btnTabBeard; 

    [Header("=== DANH SÁCH CUỘN (PANELS/SCROLL VIEWS) ===")]
    public GameObject panelOutfit_Male;
    public GameObject panelOutfit_Female;
    public GameObject panelHair_Male;
    public GameObject panelHair_Female;
    public GameObject panelGlasses_Male;
    public GameObject panelGlasses_Female;
    public GameObject panelBeard; 

    [Header("=== ĐIỀU KHIỂN XOAY NHÂN VẬT ===")]
    [Tooltip("Tốc độ xoay nhân vật khi kéo chuột")]
    public float rotationSpeed = 10f;

    [Header("=== DỮ LIỆU ĐANG CHỌN ===")]
    private int currentGender = 0; 
    private int currentOutfit = 0;
    private int currentHair = 0;
    private int currentGlasses = 0;
    private int currentBeard = 0;
    private int currentTab = 0; 

    private void Start()
    {
        // 1. KHÓA DI CHUYỂN: Tìm và tắt Component PlayerController trên khối mô hình
        // Việc này chỉ có tác dụng trong Scene này, không ảnh hưởng đến Prefab gốc
        PlayerController pc = previewModel.GetComponent<PlayerController>();
        if (pc != null) pc.enabled = false;

        // 2. Load giao diện mặc định
        SelectGender(0);
        OpenTab(0);
    }

    // --- THÊM HÀM UPDATE NÀY ĐỂ XỬ LÝ KÉO CHUỘT XOAY NGƯỜI ---
    private void Update()
    {
        // Input.GetMouseButton(0) là nhấn giữ chuột trái. 
        // (Bạn có thể đổi thành số 1 nếu muốn dùng chuột phải để xoay)
        if (Input.GetMouseButton(0))
        {
            // Lấy khoảng cách chuột di chuyển theo chiều ngang
            float mouseX = Input.GetAxis("Mouse X");

            // Xoay nhân vật quanh trục Y (Vector3.up). 
            // Dấu trừ (-) giúp nhân vật xoay theo chiều rê chuột (cảm giác "nắm kéo" tự nhiên)
            previewModel.transform.Rotate(Vector3.up, -mouseX * rotationSpeed, Space.World);
        }
    }

    // --- Các hàm bên dưới giữ nguyên y hệt bản trước ---

    public void SelectGender(int genderID)
    {
        currentGender = genderID;
        currentOutfit = 0; 
        currentHair = 0;
        currentGlasses = 0;

        if (currentGender == 1) // NỮ
        {
            btnTabBeard.SetActive(false); 
            currentBeard = 0; 
            if (currentTab == 3) currentTab = 0; 
        }
        else // NAM
        {
            btnTabBeard.SetActive(true); 
        }

        OpenTab(currentTab); 
        UpdatePreview();
    }

    public void OpenTab(int tabIndex)
    {
        currentTab = tabIndex; 

        panelOutfit_Male.SetActive(false);
        panelOutfit_Female.SetActive(false);
        panelHair_Male.SetActive(false);
        panelHair_Female.SetActive(false);
        panelGlasses_Male.SetActive(false);
        panelGlasses_Female.SetActive(false);
        panelBeard.SetActive(false);

        if (tabIndex == 0) 
        {
            if (currentGender == 0) panelOutfit_Male.SetActive(true);
            else panelOutfit_Female.SetActive(true);
        }
        else if (tabIndex == 1) 
        {
            if (currentGender == 0) panelHair_Male.SetActive(true);
            else panelHair_Female.SetActive(true);
        }
        else if (tabIndex == 2) 
        {
            if (currentGender == 0) panelGlasses_Male.SetActive(true);
            else panelGlasses_Female.SetActive(true);
        }
        else if (tabIndex == 3 && currentGender == 0) 
        {
            panelBeard.SetActive(true); 
        }
    }

    public void SelectOutfit(int id) { currentOutfit = id; UpdatePreview(); }
    public void SelectHair(int id)   { currentHair = id; UpdatePreview(); }
    public void SelectGlasses(int id){ currentGlasses = id; UpdatePreview(); }
    public void SelectBeard(int id)  { currentBeard = id; UpdatePreview(); }

    private void UpdatePreview()
    {
        previewModel.UpdateAppearance(currentGender, currentOutfit, currentHair, currentBeard, currentGlasses);
    }

    public void StartGame()
    {
        PlayerPrefs.SetInt("Saved_Gender", currentGender);
        PlayerPrefs.SetInt("Saved_Outfit", currentOutfit);
        PlayerPrefs.SetInt("Saved_Hair", currentHair);
        PlayerPrefs.SetInt("Saved_Beard", currentBeard);
        PlayerPrefs.SetInt("Saved_Glasses", currentGlasses);
        PlayerPrefs.Save();

        SceneManager.LoadScene("Map1"); 
    }
}