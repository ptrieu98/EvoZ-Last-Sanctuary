using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class ItemPickup : MonoBehaviour
{
    public ItemData itemData; 
    
    private bool isPlayerInRange = false; // Biến kiểm tra xem Player có đang đứng gần không

    private void Update()
    {
        // Nếu Player đang đứng trong vùng và nhấn phím F
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.F))
        {
            // Kiểm tra xem có dữ liệu không để chống lỗi trắng hình
            if (itemData == null)
            {
                Debug.LogWarning("Cục đồ này chưa được gắn file ItemData!");
                return; 
            }

            // Xin phép quản lý nhét vào túi
            bool pickedUp = InventoryManager.Instance.AddItem(itemData);
            
            if (pickedUp)
            {
                Destroy(gameObject);
            }
        }
    }

    // Khi Player bước vào vùng nhặt đồ
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            // (Tuỳ chọn: Bạn có thể bật một chữ "Nhấn F để nhặt" hiện lên màn hình ở đây)
        }
    }

    // Khi Player bước ra khỏi vùng nhặt đồ
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            // (Tuỳ chọn: Tắt chữ "Nhấn F để nhặt" đi)
        }
    }
}