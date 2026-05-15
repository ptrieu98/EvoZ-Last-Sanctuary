using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Dữ liệu Item")]
    public ItemData itemData;
    
    [HideInInspector] public Transform parentAfterDrag;
    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    public void InitializeItem()
    {
        if (image == null) image = GetComponent<Image>();

        if (itemData != null && itemData.icon != null)
        {
            image.sprite = itemData.icon;
        }
        else
        {
            Debug.LogError("Lỗi hiển thị trắng: Vật phẩm " + gameObject.name + " bị thiếu ItemData hoặc chưa gắn Icon!");
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        EquipSlotSync oldSlotSync = transform.parent.GetComponent<EquipSlotSync>();

        // 1. DỜI NHÀ TRƯỚC (Cứu vũ khí khỏi việc bị xóa nhầm bởi lệnh Refresh)
        parentAfterDrag = transform.parent; 
        transform.SetParent(transform.root); 
        transform.SetAsLastSibling();
        image.raycastTarget = false; 

        // 2. SAU ĐÓ MỚI XÓA DỮ LIỆU VÀ ĐỒNG BỘ
        if (oldSlotSync != null)
        {
            int index = oldSlotSync.slotID - 1;
            oldSlotSync.model.equippedWeapons[index] = null;
            
            // Cất vũ khí trên tay nhân vật
            if (oldSlotSync.model.activeWeaponIndex == index)
                oldSlotSync.controller.SwitchWeapon(index);
                
            // Lúc này gọi hàm Refresh cực kỳ an toàn vì vũ khí đã "chuyển hộ khẩu"
            InventoryManager.Instance.RefreshAllEquipSlots();
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Nhảy về nhà mới (hoặc nhà cũ nếu thả hụt)
        transform.SetParent(parentAfterDrag);
        image.raycastTarget = true; 
        transform.localPosition = Vector3.zero; 

        // 3. CHỐNG MẤT ĐỒ KHI THẢ HỤT (RỚT RA NGOÀI GIAO DIỆN)
        EquipSlotSync finalSlot = parentAfterDrag.GetComponent<EquipSlotSync>();
        if (finalSlot != null)
        {
            // Nếu người chơi thả hụt và vũ khí phải nảy về lại một ô trang bị
            // Chúng ta phải nạp lại dữ liệu cho Model để chống lỗi
            finalSlot.OnItemDroppedInSlot(itemData);
        }
    }
}