using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class ItemPickup : MonoBehaviour
{
    [Header("=== DỮ LIỆU VẬT PHẨM ===")]
    public ItemData itemData;
    
    [Tooltip("Dành cho trang bị rớt ra từ quái/Boss đã được Roll chỉ số")]
    public EquipmentInstance droppedEquipment; 

    [Tooltip("Dành cho Mã Gen Khuyết (Kỹ năng chủ động)")]
    public GlitchDNAInstance droppedGlitchDNA;

    [Header("=== CÀI ĐẶT NHẶT ===")]
    public float pickupRadius = 2.5f; 
    public AudioClip pickupSound;     
    
    private Transform player;
    private bool isInRange = false;
    private bool isHovering = false; 

   void Start()
    {
        // Tự động tìm nhân vật chính
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        // Cài đặt vùng kích hoạt nhặt đồ
        SphereCollider col = GetComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = pickupRadius;

        // ==========================================
        // MỚI: TỰ ĐỘNG ROLL CHỈ SỐ KHI VỪA RỚT XUỐNG ĐẤT
        // ==========================================
        if (droppedGlitchDNA != null && droppedGlitchDNA.activeSkill != null)
        {
            // Nếu điểm số hiện tại đang là 0 (Tức là Gen mới tinh chưa được Roll)
            if (droppedGlitchDNA.line1 != null && droppedGlitchDNA.line1.currentValue <= 0)
            {
                droppedGlitchDNA.RerollStatTypes();  // Roll Máu, Giáp, Sát thương...
                droppedGlitchDNA.RerollStatValues(); // Roll điểm ngẫu nhiên dựa theo maxValue
                
                Debug.Log($"<color=magenta>Hệ thống: Đã Roll random chỉ số cho [{droppedGlitchDNA.genName}] ngay khi rớt!</color>");
            }
        }
    }

    void Update()
    {
        if (isInRange && Input.GetKeyDown(KeyCode.F))
        {
            PickUp();
        }
    }

    private void PickUp()
    {
        if (TooltipManager.Instance != null) TooltipManager.Instance.HideTooltip();

        // 1. NHẶT TRANG BỊ GACHA
        if (droppedEquipment != null && droppedEquipment.baseTemplate != null)
        {
            bool success = InventoryManager.Instance.AddEquipment(droppedEquipment);
            if (success) PlaySoundAndDestroy();
            return;
        }

        // 2. NHẶT MÃ GEN KHUYẾT
        if (droppedGlitchDNA != null && droppedGlitchDNA.activeSkill != null)
        {
            bool success = InventoryManager.Instance.AddGlitchDNA(droppedGlitchDNA);
            if (success) PlaySoundAndDestroy();
            return;
        }

        // 3. NHẶT VẬT PHẨM THƯỜNG / ĐẠN DƯỢC / TINH HẠCH
        if (itemData == null) 
        {
            Debug.LogWarning("Hệ thống: Không có dữ liệu vật phẩm để nhặt!");
            return;
        }

        // --- CƠ CHẾ GOM ĐỒ (STACKING) ĐÃ FIX LOGIC THEO BẬC VÀ HỆ ---
        if (itemData.category == ItemCategory.Ammo || itemData.category == ItemCategory.Core || itemData.category == ItemCategory.Material)
        {
            DraggableItem[] itemsInInventory = Resources.FindObjectsOfTypeAll<DraggableItem>();
            
            int remainingAmmoToPack = itemData.ammoAmount <= 0 ? 1 : itemData.ammoAmount;

            foreach (DraggableItem item in itemsInInventory)
            {
                if (item.gameObject.scene.rootCount == 0) continue;
                if (item.transform.parent != null && item.transform.parent.GetComponent<EquipSlotSync>() != null) continue;

                if (item.itemData != null && item.itemData.category == itemData.category)
                {
                    bool isSameItem = false;

                    // KIỂM TRA ĐIỀU KIỆN GỘP:
                    if (itemData.category == ItemCategory.Ammo) 
                    {
                        isSameItem = (item.itemData.ammoType == itemData.ammoType);
                    }
                    else if (itemData.category == ItemCategory.Core) 
                    {
                        // LUẬT MỚI: PHÂN TÁCH BẬC 1 VÀ BẬC 2/3/DỊ BIẾN
                        if (itemData.coreTier == CoreTier.Tier1)
                        {
                            // Bậc 1: Không có hệ, chỉ cần cùng là Bậc 1 và cùng tên là gộp
                            isSameItem = (item.itemData.coreTier == CoreTier.Tier1 && 
                                          item.itemData.itemName == itemData.itemName);
                        }
                        else
                        {
                            // Bậc 2, 3, Dị biến: Bắt buộc soi kỹ Hệ (Element)
                            isSameItem = (item.itemData.coreTier == itemData.coreTier && 
                                          item.itemData.coreElement == itemData.coreElement && 
                                          item.itemData.itemName == itemData.itemName);
                        }
                    }
                    else 
                    {
                        // Các vật liệu chế tạo khác (Material)
                        isSameItem = (item.itemData.itemName == itemData.itemName);
                    }

                    // Đã tìm thấy đúng loại -> Bắt đầu gộp
                    if (isSameItem)
                    {
                        if (!item.itemData.name.Contains("(Clone)"))
                        {
                            item.itemData = ScriptableObject.Instantiate(item.itemData);
                            item.itemData.name += "(Clone)";
                        }

                        int spaceLeft = 120 - item.itemData.ammoAmount; // Giới hạn Stack Max 1 ô là 120
                        
                        if (spaceLeft > 0)
                        {
                            int toAdd = Mathf.Min(spaceLeft, remainingAmmoToPack);
                            item.itemData.ammoAmount += toAdd;
                            remainingAmmoToPack -= toAdd;
                            
                            item.InitializeItem(); 
                            if (remainingAmmoToPack <= 0) break;
                        }
                    }
                }
            }

            // Nếu gom xong mà vẫn dư (do đầy 1 ô 120) hoặc túi chưa có viên nào loại này -> Tạo ô mới
            if (remainingAmmoToPack > 0)
            {
                ItemData ammoClone = ScriptableObject.Instantiate(itemData);
                ammoClone.ammoAmount = remainingAmmoToPack;
                ammoClone.name += "(Clone)";

                bool success = InventoryManager.Instance.AddItem(ammoClone);
                
                if (!success)
                {
                    Debug.LogWarning("Hệ thống: Balo đã đầy!");
                    if (player != null)
                    {
                        PlayerController pc = player.GetComponent<PlayerController>();
                        if (pc != null) pc.UpdateAmmoDisplay();
                    }
                    
                    itemData.ammoAmount = remainingAmmoToPack;
                    UpdateTooltipDisplay(); 
                    return; 
                }
            }

            if (player != null)
            {
                PlayerController pc = player.GetComponent<PlayerController>();
                if (pc != null) pc.UpdateAmmoDisplay();
            }

            PlaySoundAndDestroy();
        }
        else
        {
            // Các đồ không thể gộp (Súng, Trang bị...)
            bool success = InventoryManager.Instance.AddItem(itemData);
            if (success) PlaySoundAndDestroy();
        }
    }

    private void PlaySoundAndDestroy()
    {
        if (pickupSound != null && player != null)
        {
            AudioSource audioSrc = player.GetComponentInChildren<AudioSource>();
            if (audioSrc != null) audioSrc.PlayOneShot(pickupSound);
        }
        Destroy(gameObject);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) { isInRange = true; UpdateTooltipDisplay(); }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) { isInRange = false; UpdateTooltipDisplay(); }
    }

    private void OnMouseOver()
    {
        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            if (isHovering) { isHovering = false; UpdateTooltipDisplay(); }
            return;
        }

        if (!isHovering) { isHovering = true; UpdateTooltipDisplay(); }
    }
    
    private void OnMouseExit()
    {
        isHovering = false; UpdateTooltipDisplay();
    }

    private string DichChiSo(string tenChiSoGoc)
    {
        switch (tenChiSoGoc)
        {
            case "Health": case "MaxHealth": case "FlatHealth": return "Máu Tối Đa";
            case "Stamina": case "FlatStamina": return "Thể Lực";
            case "Armor": case "FlatArmor": return "Giáp Bảo Vệ";
            case "Damage": case "FlatDamage": return "Sát Thương";
            case "CritChance": case "CritRate": return "Tỉ Lệ Chí Mạng (%)";
            case "CooldownReduction": return "Giảm Hồi Chiêu (%)";
            default: return tenChiSoGoc; 
        }
    }

    private void UpdateTooltipDisplay()
    {
        if (TooltipManager.Instance == null || InventoryManager.isUIOpen) 
        {
            if (TooltipManager.Instance != null) TooltipManager.Instance.HideTooltip();
            return;
        }

        string itemName = "Vật phẩm";
        if (itemData != null) itemName = itemData.itemName;
        else if (droppedEquipment != null && droppedEquipment.baseTemplate != null) itemName = droppedEquipment.baseTemplate.itemName;
        else if (droppedGlitchDNA != null && droppedGlitchDNA.activeSkill != null) itemName = droppedGlitchDNA.genName; 

        if (isHovering)
        {
            string title = "";
            string content = "";

            if (isInRange) content += "<color=yellow>Nhấn [F] để nhặt</color>\n\n";

            if (droppedEquipment != null && droppedEquipment.baseTemplate != null)
            {
                title = $"<b><color=#FFD700>{droppedEquipment.baseTemplate.itemName}</color></b> [{droppedEquipment.starLevel} Sao]";
                foreach (StatModifier stat in droppedEquipment.basicStats) content += $"+{stat.value:F0} {DichChiSo(stat.statType.ToString())}\n";
                foreach (StatModifier stat in droppedEquipment.specialStats) content += $"+{stat.value:F1}% {DichChiSo(stat.statType.ToString())}\n";
            }
            else if (droppedGlitchDNA != null && droppedGlitchDNA.activeSkill != null)
            {
                string tierStr = droppedGlitchDNA.tier == GenTier.Mutant ? "<color=red>Dị Biến</color>" : $"Bậc {droppedGlitchDNA.tier.ToString().Replace("Tier", "")}";
                title = $"<b><color=#8A2BE2>{droppedGlitchDNA.genName}</color></b> [{tierStr}]";
                
                content += "Loại: Kỹ năng Chủ động\n";
                
                string coreTag1 = droppedGlitchDNA.line1.isCoreStat ? "<color=red>(Cốt Lõi)</color>" : "";
                content += $"- {DichChiSo(droppedGlitchDNA.line1.statType.ToString())}: {droppedGlitchDNA.line1.currentValue} {coreTag1}\n";
                
                string coreTag2 = droppedGlitchDNA.line2.isCoreStat ? "<color=red>(Cốt Lõi)</color>" : "";
                content += $"- {DichChiSo(droppedGlitchDNA.line2.statType.ToString())}: {droppedGlitchDNA.line2.currentValue} {coreTag2}";
            }
            else if (itemData != null)
            {
                title = $"<b>{itemData.itemName}</b>";

                if (itemData.category == ItemCategory.Weapon) 
                    content += $"Sát thương: {itemData.damage}\nTầm đánh: {itemData.attackRange}";
                else if (itemData.category == ItemCategory.Material) 
                    content += "Nguyên liệu chế tạo";
                else if (itemData.category == ItemCategory.Ammo || itemData.category == ItemCategory.Consumable)
                    content += $"Số lượng: {itemData.ammoAmount}";
                else if (itemData.category == ItemCategory.Core)
                {
                    string tierName = itemData.coreTier.ToString().Replace("Tier", "Bậc ");
                    string elementColor = "#FFFFFF"; string elementName = "Vô Hệ";
                    switch (itemData.coreElement)
                    {
                        case CoreElement.Fire: elementColor = "#FF4500"; elementName = "Hệ Lửa"; break;
                        case CoreElement.Water: elementColor = "#00BFFF"; elementName = "Hệ Nước"; break;
                        case CoreElement.Earth: elementColor = "#8B4513"; elementName = "Hệ Đất"; break;
                    }
                    content += $"Đẳng cấp: <color=yellow>{tierName}</color>\nThuộc tính: <color={elementColor}>{elementName}</color>";
                    if (itemData.coreTier == CoreTier.Mutant) content += $"\n<color=#FF00FF><b>Biến dị:</b></color> {itemData.mutantEffectDescription}";
                }
            }

            TooltipManager.Instance.ShowTooltip(title, content);
        }
        else if (isInRange)
        {
            TooltipManager.Instance.ShowTooltip($"<b>{itemName}</b>", "<color=yellow>Nhấn [F] để nhặt</color>");
        }
        else
        {
            TooltipManager.Instance.HideTooltip();
        }
    }
}