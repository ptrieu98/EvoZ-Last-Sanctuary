using UnityEngine;

public class PlayerView : MonoBehaviour
{
    [Header("=== NGOẠI HÌNH & PHỤ KIỆN NAM ===")]
    public GameObject[] maleOutfits;
    public GameObject[] maleHairs;
    public GameObject[] maleBeards;
    public GameObject[] maleGlasses;

    [Header("=== NGOẠI HÌNH & PHỤ KIỆN NỮ ===")]
    public GameObject[] femaleOutfits;
    public GameObject[] femaleHairs;
    public GameObject[] femaleGlasses;

    // Hàm nhận 5 chỉ số từ Menu Tạo Nhân Vật
    public void UpdateAppearance(int gender, int outfitID, int hairID, int beardID, int glassesID)
    {
        // BƯỚC 1: RESET - Tắt sập toàn bộ mọi thứ để làm sạch
        TurnOffArray(maleOutfits);
        TurnOffArray(femaleOutfits);
        TurnOffArray(maleHairs);
        TurnOffArray(femaleHairs);
        TurnOffArray(maleBeards);
        TurnOffArray(maleGlasses);
        TurnOffArray(femaleGlasses);

        // BƯỚC 2: BẬT MÔ HÌNH THEO GIỚI TÍNH
        if (gender == 0) // NẾU LÀ NAM
        {
            if (outfitID >= 0 && outfitID < maleOutfits.Length) maleOutfits[outfitID].SetActive(true);
            if (hairID >= 0 && hairID < maleHairs.Length) maleHairs[hairID].SetActive(true);
            if (beardID >= 0 && beardID < maleBeards.Length) maleBeards[beardID].SetActive(true);
            if (glassesID >= 0 && glassesID < maleGlasses.Length) maleGlasses[glassesID].SetActive(true);
        }
        else // NẾU LÀ NỮ
        {
            if (outfitID >= 0 && outfitID < femaleOutfits.Length) femaleOutfits[outfitID].SetActive(true);
            if (hairID >= 0 && hairID < femaleHairs.Length) femaleHairs[hairID].SetActive(true);
            if (glassesID >= 0 && glassesID < femaleGlasses.Length) femaleGlasses[glassesID].SetActive(true);
            
            // Nữ hoàn toàn bỏ qua râu
        }
    }

    // Hàm hỗ trợ tắt nhanh một mảng GameObject
    private void TurnOffArray(GameObject[] array)
    {
        if (array == null) return;
        foreach (GameObject item in array)
        {
            if (item != null) item.SetActive(false);
        }
    }

    public void PlayDashEffects()
    {
        // Chạy hiệu ứng hạt bụi (Particle System) khi lướt
    }
}