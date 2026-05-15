using UnityEngine;

public class EquipSlotSync : MonoBehaviour
{
    public PlayerModel model;
    public PlayerView view;
    public PlayerController controller;
    public GameObject itemUIPrefab;

    [Header("Cài đặt ô")]
    public int slotID; 

    private void Start()
    {
        if (InventoryManager.Instance != null) InventoryManager.Instance.RegisterEquipSlot(this);
        UpdateSlotUI();
    }

    public void UpdateSlotUI()
    {
        int index = slotID - 1;
        ItemData dataInModel = model.equippedWeapons[index];

        // Lấy UI hiện tại
        DraggableItem existingUI = null;
        foreach (Transform child in transform)
        {
            existingUI = child.GetComponent<DraggableItem>();
            if (existingUI != null) break;
        }

        if (dataInModel != null)
        {
            if (existingUI != null)
            {
                if (existingUI.itemData == dataInModel)
                {
                    existingUI.InitializeItem(); 
                    return; 
                }
                else
                {
                    // 🔒 ÉP TÁCH LÌA NGAY LẬP TỨC để không bị tính là child đẻ ra bản sao
                    existingUI.transform.SetParent(null); 
                    Destroy(existingUI.gameObject); 
                }
            }
            
            GameObject newItem = Instantiate(itemUIPrefab, transform);
            DraggableItem dragScript = newItem.GetComponent<DraggableItem>();
            dragScript.itemData = dataInModel;
            dragScript.InitializeItem();
            newItem.transform.localPosition = Vector3.zero;
        }
        else
        {
            // Dọn sạch ô trống
            foreach (Transform child in transform) 
            {
                child.SetParent(null);
                Destroy(child.gameObject);
            }
        }
    }

    public void OnItemDroppedInSlot(ItemData newItemData)
    {
        int index = slotID - 1;
        model.equippedWeapons[index] = newItemData;

        if (model.activeWeaponIndex == index) controller.SwitchWeapon(index);

        InventoryManager.Instance.RefreshAllEquipSlots();
    }
}