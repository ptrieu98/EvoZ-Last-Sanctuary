using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    [Header("=== LIÊN KẾT MVC ===")]
    public PlayerModel model;
    public PlayerView view;

    [Header("=== TRẠNG THÁI ===")]
    public bool isJumping = false;
    public bool isDashing = false;
    public float jumpDistance = 3.5f; 

    private Rigidbody rb;
    private CapsuleCollider capsuleCol;
    private Camera mainCamera;
    private LedgeJumpPoint currentLedge;
    private Vector3 movementInput;
    private Coroutine currentActionCoroutine; 
    private int meteorHitCount = 0; 

    // --- BIẾN ĐẾM THỜI GIAN HỒI CHIÊU (COOLDOWN) MÃ GEN ---
    private float skill1CooldownTimer = 0f;
    private float skill2CooldownTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        capsuleCol = GetComponent<CapsuleCollider>();
        mainCamera = Camera.main; 

        if (model == null) model = GetComponent<PlayerModel>();
        if (view == null) view = GetComponent<PlayerView>();

        PlayerData savedData = SaveSystem.LoadPlayerAppearance();
        if (savedData != null && view != null) view.UpdateAppearance(savedData.gender, savedData.outfitID, savedData.hairID, savedData.beardID, savedData.glassesID);
        UpdateAmmoDisplay();
    }

    void Update()
    {
        if (isJumping) return;

        // Trừ thời gian hồi chiêu Mã Gen
        if (skill1CooldownTimer > 0) skill1CooldownTimer -= Time.deltaTime;
        if (skill2CooldownTimer > 0) skill2CooldownTimer -= Time.deltaTime;

        float h = Input.GetAxisRaw("Horizontal"); float v = Input.GetAxisRaw("Vertical");   

        if (mainCamera != null)
        {
            Vector3 camForward = mainCamera.transform.forward; Vector3 camRight = mainCamera.transform.right;
            camForward.y = 0f; camRight.y = 0f; camForward.Normalize(); camRight.Normalize();
            movementInput = (camForward * v + camRight * h).normalized;
        }
        else movementInput = new Vector3(h, 0f, v).normalized;

        if (!isDashing) AimAtMouse();

        if (!isDashing && !isJumping)
        {
            Vector3 localMove = transform.InverseTransformDirection(movementInput);
            if (view != null) view.UpdateMovementAnimation(localMove.x, localMove.z);
        }

        if (model.isBusy) { if (Input.GetKeyDown(KeyCode.X)) CancelCurrentAction(); return; }

        if (model.currentStamina < model.maxStamina && !isDashing)
        {
            model.currentStamina += model.staminaRegenRate * Time.deltaTime;
            model.currentStamina = Mathf.Clamp(model.currentStamina, 0, model.maxStamina);
        }

        if (Input.GetKeyDown(KeyCode.Space) && !isDashing && model.currentStamina >= model.dashStaminaCost)
        {
            model.currentStamina -= model.dashStaminaCost; 
            StartCoroutine(DashRoutine());
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && currentLedge != null && !isDashing) StartCoroutine(JumpDownRoutine());

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (view != null) view.CloseSkillTreePanel();
            if (InventoryManager.Instance != null) InventoryManager.Instance.ToggleMainUI();
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            if (InventoryManager.Instance != null) InventoryManager.Instance.CloseMainUI();
            if (view != null) view.ToggleSkillTreePanel();
        }

        HandleWeaponSwitch(); 
        HandleReloadInput(); 
        HandleLaserSight(); 
        HandleAttack();  

        // --- LẮNG NGHE PHÍM TUNG CHIÊU MÃ GEN ---
        HandleActiveSkills();     
    }

    // ==========================================
    // LOGIC KÍCH HOẠT MÃ GEN KHUYẾT (ACTIVE SKILLS)
    // ==========================================
    private void HandleActiveSkills()
    {
        if (model.isBusy || isDashing || isJumping) return;

        // Bấm Q
        if (Input.GetKeyDown(KeyCode.Q) && model.activeSkill1 != null)
        {
            TryUseActiveSkill(model.activeSkill1, ref skill1CooldownTimer);
        }
        
        // Bấm E
        if (Input.GetKeyDown(KeyCode.E) && model.activeSkill2 != null)
        {
            TryUseActiveSkill(model.activeSkill2, ref skill2CooldownTimer);
        }
    }

    private void TryUseActiveSkill(ActiveSkillData skillData, ref float cooldownTimer)
    {
        if (cooldownTimer > 0)
        {
            if (view != null) view.SpawnFloatingText(view.damageTextPrefab, transform.position + Vector3.up * 2f, "Đang hồi chiêu!", Color.yellow);
            return;
        }

        if (model.currentStamina < skillData.staminaCost)
        {
            if (view != null) view.SpawnFloatingText(view.damageTextPrefab, transform.position + Vector3.up * 2f, "Hết thể lực!", Color.red);
            return;
        }

        // Trừ thể lực và vào thời gian hồi chiêu
        model.currentStamina -= skillData.staminaCost;
        cooldownTimer = skillData.cooldownTime;

        // Kích hoạt múa chiêu
        currentActionCoroutine = StartCoroutine(CastActiveSkillRoutine(skillData));
    }

    private IEnumerator CastActiveSkillRoutine(ActiveSkillData skillData)
    {
        model.isBusy = true; // Khóa nhân vật không cho di chuyển/bắn súng

        if (view != null && !string.IsNullOrEmpty(skillData.animationTriggerName))
        {
            // Tạm thời dùng trigger "Punch" nếu bạn chưa có animation riêng
            view.animator.SetTrigger(skillData.animationTriggerName);
        }

        // Đợi nhân vật múa xong (Ví dụ 0.5s)
        yield return new WaitForSeconds(skillData.actionDuration);

        // --- XÁC ĐỊNH MỨC ĐỘ TƯƠNG THÍCH ĐỂ CHỌN ĐÚNG VFX ---
        GameObject vfxToSpawn = skillData.defaultVFX;

        if (model.currentElement == "Fire" && skillData.fireVFX != null) 
            vfxToSpawn = skillData.fireVFX;
        else if (model.currentElement == "Water" && skillData.waterVFX != null) 
            vfxToSpawn = skillData.waterVFX;
        else if (model.currentElement == "Earth" && skillData.earthVFX != null) 
            vfxToSpawn = skillData.earthVFX;

        // Sinh ra cục VFX (Kẻ thực thi)
        if (vfxToSpawn != null)
        {
            Instantiate(vfxToSpawn, transform.position, transform.rotation);
            Debug.Log($"<color=magenta>Đã kích hoạt Mã Gen: {skillData.skillName} (Hệ: {model.currentElement})</color>");
        }

        model.isBusy = false; // Nhả khóa nhân vật
    }
    // ==========================================


    void FixedUpdate()
    {
        if (isJumping || isDashing) return;

        float finalSpeed = model.moveSpeed * model.speedBuffMultiplier;
        if (model.CurrentWeapon != null && model.CurrentWeapon.category == ItemCategory.Weapon)
        {
            finalSpeed *= model.CurrentWeapon.moveSpeedMultiplier;
            if (model.isAttacking && model.CurrentWeapon.isAutomatic) finalSpeed *= 0.5f; 
        }
        if (model.isBusy) finalSpeed *= model.actionSpeedPenalty; 

        Vector3 targetVelocity = movementInput * finalSpeed;

        if (movementInput.sqrMagnitude > 0)
        {
            Vector3 chestPosition = transform.position + Vector3.up * 1f; 
            if (Physics.SphereCast(chestPosition, 0.4f, movementInput, out RaycastHit hit, 0.5f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                if (hit.normal.y < 0.3f) targetVelocity = Vector3.ProjectOnPlane(targetVelocity, hit.normal);
            }
        }

        float currentYVel = rb.linearVelocity.y; 
        if (currentYVel > 0) currentYVel = Mathf.Clamp(currentYVel, 0f, 2f); 
        rb.linearVelocity = new Vector3(targetVelocity.x, currentYVel, targetVelocity.z);
        rb.AddForce(Vector3.down * 30f, ForceMode.Acceleration);
    }

    private void AimAtMouse()
    {
        if (mainCamera == null) return;
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, new Vector3(0, transform.position.y, 0));
        if (groundPlane.Raycast(ray, out float rayDistance))
        {
            Vector3 pointToLook = ray.GetPoint(rayDistance);
            Vector3 lookDirection = pointToLook - transform.position;
            lookDirection.y = 0f; 
            if (lookDirection.sqrMagnitude > 0.1f) transform.rotation = Quaternion.LookRotation(lookDirection);
        }
    }

    private void HandleLaserSight()
    {
        if (model.currentHealth <= 0 || isJumping || isDashing || model.isBusy) 
        {
            if (view != null) view.UpdateLaser(false, Vector3.zero, Vector3.zero); return;
        }

        ItemData weapon = model.CurrentWeapon;
        if (weapon != null && weapon.category == ItemCategory.Weapon && weapon.weaponType == WeaponType.Ranged && weapon.ammoType == AmmoType.Sniper)
        {
            Vector3 laserStart = transform.position + Vector3.up * 1.2f; 
            if (view != null)
            {
                Transform muzzle = view.GetCurrentMuzzlePoint();
                if (muzzle != null) laserStart = muzzle.position;
            }
            Vector3 laserDir = transform.forward;
            float maxRange = weapon.attackRange;
            Vector3 laserEnd = laserStart + laserDir * maxRange;

            if (Physics.Raycast(laserStart, laserDir, out RaycastHit hit, maxRange))
            {
                if (hit.collider.gameObject != this.gameObject && !hit.collider.transform.IsChildOf(transform) && !hit.collider.isTrigger) laserEnd = hit.point;
            }
            if (view != null) view.UpdateLaser(true, laserStart, laserEnd);
        }
        else { if (view != null) view.UpdateLaser(false, Vector3.zero, Vector3.zero); }
    }

    private IEnumerator DashRoutine()
    {
        isDashing = true;
        if (view != null) view.PlayDashEffects(model.dashDuration);
        Vector3 dashDirection = movementInput.magnitude > 0.1f ? movementInput : transform.forward;
        float actualDashSpeed = model.moveSpeed * model.dashMultiplier;
        float timePassed = 0f;
        float fireTrailTimer = 0f; 

        while (timePassed < model.dashDuration)
        {
            rb.linearVelocity = dashDirection * actualDashSpeed;
            
            // --- GỌI VFX VỆT LỬA DÀY ĐẶC HƠN VÀ DỌN RÁC ---
            if (model.hasHellfireTrail)
            {
                fireTrailTimer += Time.fixedDeltaTime;
                if (fireTrailTimer >= 0.03f) // Nhả liên tục
                {
                    fireTrailTimer = 0f;
                    if (model.activeFireTrailVFX != null) 
                    {
                        GameObject trail = Instantiate(model.activeFireTrailVFX, transform.position, Quaternion.identity);
                        Destroy(trail, 3f); // Xóa rác
                    }
                }
            }

            timePassed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate(); 
        }
        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        isDashing = false;
    }

    private IEnumerator JumpDownRoutine()
    {
        isJumping = true; currentLedge = null; rb.isKinematic = true; capsuleCol.enabled = false;      

        Vector3 startPosition = transform.position;
        Vector3 jumpDirection = transform.forward; 
        Vector3 predictedLandingXZ = startPosition + (jumpDirection * jumpDistance);
        Vector3 rayStartPos = new Vector3(predictedLandingXZ.x, startPosition.y + 5f, predictedLandingXZ.z);
        float groundY = startPosition.y - 10f; 

        if (Physics.Raycast(rayStartPos, Vector3.down, out RaycastHit hit, 20f)) groundY = hit.point.y; 
        float pivotToFeetOffset = transform.position.y - capsuleCol.bounds.min.y;
        Vector3 targetPosition = new Vector3(predictedLandingXZ.x, groundY + pivotToFeetOffset, predictedLandingXZ.z);

        float timePassed = 0f; float duration = 0.5f; 
        while (timePassed < duration)
        {
            timePassed += Time.deltaTime; float linearT = timePassed / duration;
            Vector3 currentPos = Vector3.Lerp(startPosition, targetPosition, linearT);
            currentPos.y += Mathf.Sin(linearT * Mathf.PI) * model.jumpHeight;
            transform.position = currentPos;
            yield return null;
        }

        transform.position = targetPosition; rb.linearVelocity = Vector3.zero;
        capsuleCol.enabled = true; rb.isKinematic = false; isJumping = false;
    }

    private void OnTriggerEnter(Collider other) { LedgeJumpPoint ledge = other.GetComponent<LedgeJumpPoint>(); if (ledge != null) currentLedge = ledge; }
    private void OnTriggerExit(Collider other) { LedgeJumpPoint ledge = other.GetComponent<LedgeJumpPoint>(); if (ledge != null && currentLedge == ledge) currentLedge = null; }

    private void HandleWeaponSwitch()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) TriggerHotbarSlot(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) TriggerHotbarSlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) TriggerHotbarSlot(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) TriggerHotbarSlot(3);
    }

    private void TriggerHotbarSlot(int slotIndex)
    {
        ItemData item = model.equippedWeapons[slotIndex];
        if (item == null || item.category == ItemCategory.Weapon) SwitchWeapon(slotIndex);
        else if (item.category == ItemCategory.Consumable) currentActionCoroutine = StartCoroutine(UseConsumableRoutine(slotIndex));
    }

    public void SwitchWeapon(int index)
    {
        model.activeWeaponIndex = index;
        ItemData newWeapon = model.equippedWeapons[index];
        if (newWeapon != null && newWeapon.category == ItemCategory.Weapon)
        {
            view.EquipWeapon3D(newWeapon.weaponPrefab); view.SetWeaponStance(newWeapon.animationStance);
        }
        else { view.EquipWeapon3D(null); view.SetWeaponStance(0); }
        UpdateAmmoDisplay(); 
    }

    public int GetTotalAmmoInBackpack(AmmoType type)
    {
        if (type == AmmoType.None) return 0;
        int total = 0;
        DraggableItem[] allItems = Resources.FindObjectsOfTypeAll<DraggableItem>();
        foreach (DraggableItem item in allItems)
        {
            if (item.gameObject.scene.rootCount == 0 || (item.transform.parent != null && item.transform.parent.GetComponent<EquipSlotSync>() != null)) continue; 
            if (item.itemData != null && item.itemData.category == ItemCategory.Ammo && item.itemData.ammoType == type) total += item.itemData.ammoAmount;
        }
        return total;
    }

    public void ConsumeAmmoFromBackpack(AmmoType type, int amountToConsume)
    {
        if (type == AmmoType.None || amountToConsume <= 0) return;
        DraggableItem[] allItems = Resources.FindObjectsOfTypeAll<DraggableItem>();
        int remaining = amountToConsume;

        foreach (DraggableItem item in allItems)
        {
            if (item.gameObject.scene.rootCount == 0 || (item.transform.parent != null && item.transform.parent.GetComponent<EquipSlotSync>() != null)) continue;
            if (item.itemData != null && item.itemData.category == ItemCategory.Ammo && item.itemData.ammoType == type)
            {
                if (!item.itemData.name.Contains("(Clone)")) { item.itemData = ScriptableObject.Instantiate(item.itemData); item.itemData.name += "(Clone)"; }
                if (item.itemData.ammoAmount >= remaining)
                {
                    item.itemData.ammoAmount -= remaining; remaining = 0; item.InitializeItem();
                    if (item.itemData.ammoAmount <= 0) Destroy(item.gameObject); break;
                }
                else { remaining -= item.itemData.ammoAmount; item.itemData.ammoAmount = 0; Destroy(item.gameObject); }
            }
        }
    }

    public void UpdateAmmoDisplay()
    {
        EquipSlotSync[] allSlots = Resources.FindObjectsOfTypeAll<EquipSlotSync>();
        foreach (EquipSlotSync slot in allSlots) if (slot.gameObject.scene.rootCount != 0) slot.RefreshAmmoDisplayOnly();
    }

    public void CancelCurrentAction()
    {
        if (!model.isBusy || currentActionCoroutine == null) return;
        StopCoroutine(currentActionCoroutine); 
        model.isBusy = false; model.isReloading = false;
        if (view != null) { view.ToggleActionProgress(false); view.animator.Play("Idle_Upper"); }
    }

    private void ApplyDamageToTarget(IDamageable target, float weaponDamage, bool isUnarmed = false)
    {
        if (target == null) return;

        float rawDamage = weaponDamage + model.baseDamage + model.bonusDamage;
        bool isCrit = Random.Range(0f, 100f) < model.critChance;
        if (isCrit) rawDamage *= model.critDamageMultiplier; 

        float actualDamageDealt = target.TakeDamage(rawDamage, model.armorPenetration, model.accuracy, isCrit);

        if (actualDamageDealt < 0)
        {
            MonoBehaviour targetObj = target as MonoBehaviour;
            if (targetObj != null && view != null) view.SpawnFloatingText(view.damageTextPrefab, targetObj.transform.position + Vector3.up * 1.5f, "Né!", Color.gray);
            return;
        }

        // --- GỌI VFX SAO BĂNG VÀ DỌN RÁC ---
        if (model.hasMeteor)
        {
            meteorHitCount++;
            if (meteorHitCount >= 10)
            {
                meteorHitCount = 0;
                MonoBehaviour tObj = target as MonoBehaviour;
                if (tObj != null)
                {
                    target.TakeDamage(model.baseDamage * 3f, model.armorPenetration, model.accuracy, true);
                    if (model.activeMeteorVFX != null) 
                    {
                        GameObject meteor = Instantiate(model.activeMeteorVFX, tObj.transform.position, Quaternion.identity);
                        Destroy(meteor, 3f); // Xóa rác
                    }
                }
            }
        }

        MonoBehaviour textObj = target as MonoBehaviour;
        if (textObj != null && view != null)
        {
            Color dmgColor = isCrit ? Color.yellow : Color.white;
            string dmgText = isCrit ? $"{actualDamageDealt:F0}!" : actualDamageDealt.ToString("F0");
            view.SpawnFloatingText(view.damageTextPrefab, textObj.transform.position + Vector3.up * 1.5f, dmgText, dmgColor);
        }

        if (model.lifestealPercent > 0 && actualDamageDealt > 0)
        {
            model.currentHealth += actualDamageDealt * (model.lifestealPercent / 100f);
            model.currentHealth = Mathf.Clamp(model.currentHealth, 0, model.maxHealth);
            if (view != null) view.UpdateHealthUI(model.currentHealth, model.maxHealth);
        }
    }

    private void HandleReloadInput()
    {
        if (Input.GetKeyDown(KeyCode.R) && !model.isBusy && !model.isAttacking)
        {
            ItemData weapon = model.CurrentWeapon;
            if (weapon != null && weapon.weaponType == WeaponType.Ranged && weapon.category == ItemCategory.Weapon)
            {
                int index = model.activeWeaponIndex;
                int reserve = GetTotalAmmoInBackpack(weapon.ammoType);
                if (model.currentAmmoInMag[index] < weapon.ammoAmount && reserve > 0) currentActionCoroutine = StartCoroutine(ReloadRoutine(weapon));
            }
        }
    }

    private IEnumerator ReloadRoutine(ItemData weapon)
    {
        model.isBusy = true; model.isReloading = true; int index = model.activeWeaponIndex;

        if (view != null) { view.PlayReloadAnimation(); view.PlayReloadSound(weapon); view.ToggleActionProgress(true); }

        float timer = 0f; float totalTime = weapon.reloadTime;
        while (timer < totalTime) { timer += Time.deltaTime; if (view != null) view.UpdateActionProgress(timer / totalTime, totalTime - timer); yield return null; }

        int bulletsNeeded = weapon.ammoAmount - model.currentAmmoInMag[index];
        int bulletsToReload = Mathf.Min(bulletsNeeded, GetTotalAmmoInBackpack(weapon.ammoType));

        model.currentAmmoInMag[index] += bulletsToReload;
        ConsumeAmmoFromBackpack(weapon.ammoType, bulletsToReload);
        UpdateAmmoDisplay(); 
        
        model.isReloading = false; model.isBusy = false; 
        if (view != null) view.ToggleActionProgress(false); 
    }

    private void HandleAttack()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        if (!model.isAttacking && !model.isReloading && !model.isBusy)
        {
            ItemData weapon = model.CurrentWeapon;
            bool shouldShoot = false;
            if (weapon != null && weapon.category == ItemCategory.Weapon && weapon.weaponType == WeaponType.Ranged && weapon.isAutomatic) shouldShoot = Input.GetMouseButton(0); 
            else shouldShoot = Input.GetMouseButtonDown(0); 

            if (shouldShoot)
            {
                if (weapon == null) StartCoroutine(AttackUnarmedRoutine());
                else if (weapon.category == ItemCategory.Weapon)
                {
                    if (weapon.weaponType == WeaponType.Melee) StartCoroutine(AttackMeleeRoutine());
                    else if (weapon.weaponType == WeaponType.Ranged) StartCoroutine(ShootRangedRoutine());
                }
            }
        }
    }

    private IEnumerator AttackUnarmedRoutine()
    {
        model.isAttacking = true; model.currentPunchIndex = (model.currentPunchIndex == 0) ? 1 : 0;
        if (view != null) view.PlayAttackAnimation(0, model.currentPunchIndex);
        yield return new WaitForSeconds(model.unarmedAttackDuration / 2f);

        float range = model.unarmedAttackRange; 
        Vector3 hitCenter = transform.position + transform.forward * (range / 2f); 
        Collider[] hitTargets = Physics.OverlapSphere(hitCenter, range / 2f);

        foreach (Collider target in hitTargets)
        {
            if (target.gameObject == gameObject || target.transform.IsChildOf(transform)) continue;
            IDamageable damageable = target.GetComponent<IDamageable>();
            if (damageable == null) damageable = target.GetComponentInParent<IDamageable>();
            if (damageable != null) ApplyDamageToTarget(damageable, 0f, true);
        }
        yield return new WaitForSeconds(model.unarmedAttackDuration / 2f);
        model.isAttacking = false; 
    }

    private IEnumerator AttackMeleeRoutine()
    {
        model.isAttacking = true;
        if (view != null) { view.PlayAttackAnimation(1); view.PlayWeaponVFX(model.CurrentWeapon); }
        yield return new WaitForSeconds(model.attackDuration / 2f);

        if (model.CurrentWeapon != null)
        {
            float range = model.CurrentWeapon.attackRange; 
            Vector3 hitCenter = transform.position + transform.forward * (range / 2f); 
            Collider[] hitTargets = Physics.OverlapSphere(hitCenter, range / 2f);

            foreach (Collider target in hitTargets)
            {
                if (target.gameObject == gameObject || target.transform.IsChildOf(transform)) continue;
                IDamageable damageable = target.GetComponent<IDamageable>();
                if (damageable == null) damageable = target.GetComponentInParent<IDamageable>();

                if (damageable != null) 
                {
                    ApplyDamageToTarget(damageable, model.CurrentWeapon.damage, false);
                    if (target.CompareTag("Enemy") && view != null) view.PlayHitImpact(model.CurrentWeapon.hitEnemyVFX, target.ClosestPoint(hitCenter), transform.forward * -1);
                }
            }
        }
        yield return new WaitForSeconds(model.attackDuration / 2f);
        model.isAttacking = false; 
    }

    private IEnumerator ShootRangedRoutine()
    {
        if (model.isReloading) yield break;
        int index = model.activeWeaponIndex;

        if (model.currentAmmoInMag[index] <= 0)
        {
            if (view != null) view.PlayEmptyClickSound(model.CurrentWeapon); yield return new WaitForSeconds(0.2f); yield break; 
        }

        model.isAttacking = true; model.currentAmmoInMag[index]--; UpdateAmmoDisplay(); 
        if (view != null) view.PlayAttackAnimation(2);

        ItemData weapon = model.CurrentWeapon; 
        if (weapon != null)
        {
            if (view != null) view.PlayWeaponVFX(weapon);
            Vector3 shootOrigin = transform.position + Vector3.up * 1.2f; 
            if (view != null) { Transform muzzle = view.GetCurrentMuzzlePoint(); if (muzzle != null) shootOrigin = muzzle.position; }

            RaycastHit[] hits = Physics.SphereCastAll(shootOrigin, 0.4f, transform.forward, weapon.attackRange);
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform) || hit.collider.isTrigger) continue;
                IDamageable damageable = hit.collider.GetComponent<IDamageable>();
                if (damageable == null) damageable = hit.collider.GetComponentInParent<IDamageable>();

                GameObject vfxToPlay = (hit.collider.CompareTag("Enemy") || damageable != null) ? weapon.hitEnemyVFX : weapon.hitEnvironmentVFX;
                Vector3 finalHitPoint = hit.point; Vector3 finalHitNormal = hit.normal;

                if (hit.distance <= 0f || finalHitPoint == Vector3.zero)
                {
                    finalHitPoint = hit.collider.ClosestPoint(shootOrigin);
                    if (finalHitPoint == Vector3.zero) finalHitPoint = shootOrigin + transform.forward * 0.5f;
                    finalHitNormal = (shootOrigin - finalHitPoint).normalized;
                    if (finalHitNormal == Vector3.zero) finalHitNormal = -transform.forward;
                }

                if (view != null) { view.PlayHitImpact(vfxToPlay, finalHitPoint, finalHitNormal); view.PlayBulletTracer(shootOrigin, finalHitPoint); }
                if (damageable != null) ApplyDamageToTarget(damageable, weapon.damage, false);
                break; 
            }
        }
        yield return new WaitForSeconds((weapon != null && weapon.isAutomatic) ? weapon.fireRate : model.attackDuration);
        model.isAttacking = false;
    }

    private IEnumerator UseConsumableRoutine(int slotIndex)
    {
        ItemData consumable = model.equippedWeapons[slotIndex];
        if (consumable == null || consumable.category != ItemCategory.Consumable) yield break;
        if ((consumable.consumableType == ConsumableType.Bandage || consumable.consumableType == ConsumableType.Medkit) && model.currentHealth >= model.maxHealth) yield break;

        model.isBusy = true; 
        if (view != null)
        {
            if (consumable.consumableType == ConsumableType.EnergyDrink) view.PlayConsumeAnimation("Drink"); else view.PlayConsumeAnimation("Heal");  
            if (consumable.useSound != null && view.audioSource != null) view.audioSource.PlayOneShot(consumable.useSound);
            view.ToggleActionProgress(true); 
        }

        float timer = 0f; float totalTime = consumable.useTime;
        while (timer < totalTime) { timer += Time.deltaTime; if (view != null) view.UpdateActionProgress(timer / totalTime, totalTime - timer); yield return null; }

        if (consumable.consumableType == ConsumableType.Bandage || consumable.consumableType == ConsumableType.Medkit)
        {
            model.currentHealth += consumable.healAmount; model.currentHealth = Mathf.Clamp(model.currentHealth, 0, model.maxHealth);
            if (view != null) view.UpdateHealthUI(model.currentHealth, model.maxHealth);
        }
        else if (consumable.consumableType == ConsumableType.EnergyDrink) StartCoroutine(SpeedBoostBuffRoutine(consumable.speedBoostMultiplier, consumable.buffDuration));

        consumable.ammoAmount--; if (consumable.ammoAmount <= 0) model.equippedWeapons[slotIndex] = null; 
        model.isBusy = false; if (view != null) view.ToggleActionProgress(false); UpdateAmmoDisplay(); 
    }

    private IEnumerator SpeedBoostBuffRoutine(float multiplier, float duration)
    {
        model.speedBuffMultiplier = multiplier; if (view != null) view.ToggleSpeedBuffUI(true); 
        float timer = 0f;
        while (timer < duration) { timer += Time.deltaTime; if (view != null) view.UpdateSpeedBuffUI(duration - timer); yield return null; }
        model.speedBuffMultiplier = 1f; if (view != null) view.ToggleSpeedBuffUI(false); 
    }
}