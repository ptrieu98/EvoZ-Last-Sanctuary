using UnityEngine;
using UnityEngine.UI; // Thư viện để làm việc với nút bấm

public class TestLevelUp : MonoBehaviour
{
    [Header("=== KẾT NỐI ===")]
    public PlayerModel playerModel; // Kéo nhân vật vào đây
    public Button hackButton;       // Kéo cái nút bấm vào đây

    void Start()
    {
        // Tự động gắn sự kiện bấm nút khi bắt đầu game
        if (hackButton != null)
        {
            hackButton.onClick.AddListener(ForceLevelUp);
        }
    }

    // Hàm thực thi khi bấm nút
    public void ForceLevelUp()
    {
        if (playerModel != null)
        {
            // Tính toán chính xác lượng EXP còn thiếu để lên cấp tiếp theo
            float expNeeded = playerModel.expToNextLevel - playerModel.currentExp;
            
            // Gọi hàm AddExperience để nó tự động chạy các bước kiểm tra (lên máu, kẹt cấp 10/20)
            playerModel.AddExperience(expNeeded);
            
            Debug.Log($"<color=magenta>[CHEAT] Đã hack thêm {expNeeded} EXP để lên cấp!</color>");
        }
    }
}