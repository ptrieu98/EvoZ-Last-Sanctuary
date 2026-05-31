using UnityEngine;
using UnityEngine.UI;

public class CraftingTabManager : MonoBehaviour
{
    [Header("=== CÁC TRANG CHẾ TẠO (PAGES) ===")]
    public GameObject rangedWeaponPage; // 0. Vũ khí tầm xa
    public GameObject meleeWeaponPage;  // 1. Vũ khí cận chiến
    public GameObject helmetPage;       // 2. Mũ
    public GameObject chestPage;        // 3. Áo
    public GameObject pantsPage;        // 4. Quần
    public GameObject shoesPage;        // 5. Giày

    [Header("=== NÚT BẤM (TABS) ===")]
    public Button[] tabButtons;
    public Color activeColor = new Color(1f, 0.8f, 0f, 1f); // Màu vàng khi đang chọn
    public Color inactiveColor = new Color(0.8f, 0.8f, 0.8f, 1f); // Màu xám khi không chọn

    private void Start()
    {
        // Mặc định mở trang 0 khi khởi động
        OpenTab(0);
    }

    public void OpenTab(int tabIndex)
    {
        // 1. TẮT TẤT CẢ CÁC TRANG
        if (rangedWeaponPage != null) rangedWeaponPage.SetActive(false);
        if (meleeWeaponPage != null) meleeWeaponPage.SetActive(false);
        if (helmetPage != null) helmetPage.SetActive(false);
        if (chestPage != null) chestPage.SetActive(false);
        if (pantsPage != null) pantsPage.SetActive(false);
        if (shoesPage != null) shoesPage.SetActive(false);

        // 2. BẬT TRANG ĐƯỢC CHỌN
        switch (tabIndex)
        {
            case 0: if (rangedWeaponPage != null) rangedWeaponPage.SetActive(true); break;
            case 1: if (meleeWeaponPage != null) meleeWeaponPage.SetActive(true); break;
            case 2: if (helmetPage != null) helmetPage.SetActive(true); break;
            case 3: if (chestPage != null) chestPage.SetActive(true); break;
            case 4: if (pantsPage != null) pantsPage.SetActive(true); break;
            case 5: if (shoesPage != null) shoesPage.SetActive(true); break;
        }

        // 3. FIX LỖI MÀU: Đổi màu trực tiếp trên component Image của nút
        for (int i = 0; i < tabButtons.Length; i++)
        {
            if (tabButtons[i] != null)
            {
                Image btnImage = tabButtons[i].GetComponent<Image>();
                if (btnImage != null)
                {
                    // Nút nào đang được chọn thì bôi màu Active, còn lại bôi màu Inactive
                    btnImage.color = (i == tabIndex) ? activeColor : inactiveColor;
                }
            }
        }
    }
}