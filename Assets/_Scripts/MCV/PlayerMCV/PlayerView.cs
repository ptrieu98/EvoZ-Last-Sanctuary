using System.Collections;
using UnityEngine;
using TMPro; 
using UnityEngine.UI; 

public class PlayerView : MonoBehaviour
{
    [Header("=== ANIMATION ===")]
    public Animator animator;

    [Header("=== GIAO DIỆN CHỈ SỐ (MỚI THÊM) ===")]
    public TextMeshProUGUI levelText;     
    public Slider expSlider;              
    public TextMeshProUGUI expText;
    public TextMeshProUGUI titleText;       

    [Header("=== GIAO DIỆN BẢNG KỸ NĂNG ===")]
    public GameObject skillTreePanel;
    
    // MỚI: Thanh Slider Máu
    public Slider hpSlider;               
    public TextMeshProUGUI healthText;    


    [Header("=== GIAO DIỆN ĐỘT PHÁ (PANEL TO) ===")]
    public GameObject evolutionPanel;
    public TextMeshProUGUI oldTitleText;
    public TextMeshProUGUI newTitleText;
    public UnityEngine.UI.Button closeEvolutionButton;
    

    [Header("=== CHỮ BAY (FLOATING TEXT) ===")]
    public GameObject damageTextPrefab;   
    public GameObject expTextPrefab;      

    [Header("=== NGOẠI HÌNH & PHỤ KIỆN ===")]
    public GameObject[] maleOutfits;
    public GameObject[] maleHairs;
    public GameObject[] maleBeards;
    public GameObject[] maleGlasses;

    public GameObject[] femaleOutfits;
    public GameObject[] femaleHairs;
    public GameObject[] femaleGlasses;

    [Header("=== GIAO DIỆN TIẾN TRÌNH COOLDOWN ===")]
    public GameObject actionProgressPanel; 
    public Image actionProgressFill;       
    public TextMeshProUGUI actionProgressText; 

    [Header("=== GIAO DIỆN HIỆU ỨNG BUFF ===")]
    public GameObject speedBuffPanel;      
    public TextMeshProUGUI speedBuffTimeText; 

    [Header("=== HIỆU ỨNG TÀN ẢNH (GHOST TRAIL) ===")]
    public Material ghostMaterial;
    public float ghostSpawnRate = 0.05f; 
    public float ghostFadeTime = 0.4f;   

    [Header("=== ÂM THANH & VŨ KHÍ 3D ===")]
    public AudioSource audioSource;
    public AudioClip dashSound;
    public Transform weaponHoldPoint;
    public LineRenderer laserLine; 
    private GameObject currentWeaponObj; 

    // ==========================================
    // KHỞI TẠO (ĐÃ ĐƯỢC THÊM VÀO)
    // ==========================================
    void Start()
    {
        // Gắn sự kiện cho nút Đóng Panel Đột Phá
        if (closeEvolutionButton != null)
        {
            closeEvolutionButton.onClick.RemoveAllListeners();
            closeEvolutionButton.onClick.AddListener(CloseEvolutionPanel);
        }
    }

    // ==========================================
    // HÀM CẬP NHẬT GIAO DIỆN (UI) 
    // ==========================================
    public void UpdateLevelUI(int level, float currentExp, float maxExp)
    {
        if (levelText != null) levelText.text = level.ToString(); 
        if (expText != null) expText.text = $"{currentExp} / {maxExp}";
        if (expSlider != null && maxExp > 0) expSlider.value = currentExp / maxExp;
    }

    public void UpdateHealthUI(float currentHp, float maxHp)
    {
        if (healthText != null) healthText.text = $"{Mathf.Round(currentHp)} / {Mathf.Round(maxHp)}";
        
        if (hpSlider != null && maxHp > 0) hpSlider.value = currentHp / maxHp; 
    }

    public void SpawnFloatingText(GameObject prefab, Vector3 position, string text, Color color)
    {
        if (prefab == null) return;
        GameObject floatingObj = Instantiate(prefab, position, Quaternion.identity);
        FloatingText ft = floatingObj.GetComponent<FloatingText>();
        if (ft != null) ft.Setup(text, color);
    }

    // ==========================================
    // GIAO DIỆN NHÂN VẬT & HIỆU ỨNG
    // ==========================================
    public void UpdateAppearance(int gender, int outfitID, int hairID, int beardID, int glassesID)
    {
        TurnOffArray(maleOutfits); TurnOffArray(femaleOutfits);
        TurnOffArray(maleHairs); TurnOffArray(femaleHairs);
        TurnOffArray(maleBeards); TurnOffArray(maleGlasses); TurnOffArray(femaleGlasses);

        if (gender == 0) 
        {
            if (outfitID >= 0 && outfitID < maleOutfits.Length) maleOutfits[outfitID].SetActive(true);
            if (hairID >= 0 && hairID < maleHairs.Length) maleHairs[hairID].SetActive(true);
            if (beardID >= 0 && beardID < maleBeards.Length) maleBeards[beardID].SetActive(true);
            if (glassesID >= 0 && glassesID < maleGlasses.Length) maleGlasses[glassesID].SetActive(true);
        }
        else 
        {
            if (outfitID >= 0 && outfitID < femaleOutfits.Length) femaleOutfits[outfitID].SetActive(true);
            if (hairID >= 0 && hairID < femaleHairs.Length) femaleHairs[hairID].SetActive(true);
            if (glassesID >= 0 && glassesID < femaleGlasses.Length) femaleGlasses[glassesID].SetActive(true);
        }
    }

    public void UpdateMovementAnimation(float dirX, float dirZ)
    {
        if (animator == null) return;
        animator.SetFloat("DirX", dirX, 0.1f, Time.deltaTime);
        animator.SetFloat("DirZ", dirZ, 0.1f, Time.deltaTime);
    }

    public void SetWeaponStance(int stance)
    {
        if (animator != null) animator.SetInteger("WeaponStance", stance);
    }

    public void PlayAttackAnimation(int attackType, int attackIndex = 0)
    {
        if (animator == null) return;
        animator.SetInteger("AttackIndex", attackIndex);

        if (attackType == 0) animator.SetTrigger("Punch");       
        else if (attackType == 1) animator.SetTrigger("Attack"); 
        else if (attackType == 2) animator.SetTrigger("Shoot");  
    }

    public void PlayReloadAnimation()
    {
        if (animator != null) animator.SetTrigger("Reload");
    }

    public void PlayConsumeAnimation(string triggerName)
    {
        if (animator != null) animator.SetTrigger(triggerName);
    }

    public void ToggleActionProgress(bool isShowing)
    {
        if (actionProgressPanel != null) actionProgressPanel.SetActive(isShowing);
    }

    public void UpdateActionProgress(float fillAmount, float timeRemaining)
    {
        if (actionProgressFill != null) actionProgressFill.fillAmount = fillAmount;
        if (actionProgressText != null) actionProgressText.text = timeRemaining.ToString("F1") + "s"; 
    }

    public void ToggleSpeedBuffUI(bool isShowing)
    {
        if (speedBuffPanel != null) speedBuffPanel.SetActive(isShowing);
    }

    public void UpdateSpeedBuffUI(float timeRemaining)
    {
        if (speedBuffTimeText != null) speedBuffTimeText.text = timeRemaining.ToString("F0") + "s";
    }

    public void EquipWeapon3D(GameObject weaponPrefab)
    {
        if (currentWeaponObj != null) Destroy(currentWeaponObj);
        if (weaponPrefab == null) return;
        if (weaponHoldPoint != null) currentWeaponObj = Instantiate(weaponPrefab, weaponHoldPoint, false);
    }

    public Transform GetCurrentMuzzlePoint()
    {
        if (currentWeaponObj != null)
        {
            return FindChildRecursive(currentWeaponObj.transform, "Muzzle");
        }
        return null;
    }

    public void PlayWeaponVFX(ItemData weaponData)
    {
        if (weaponData == null) return;

        if (audioSource != null && weaponData.attackSound != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(weaponData.attackSound);
        }

        if (currentWeaponObj != null && weaponData.muzzleFlashVFX != null)
        {
            Transform muzzlePoint = GetCurrentMuzzlePoint();
            if (muzzlePoint != null)
            {
                GameObject flash = Instantiate(weaponData.muzzleFlashVFX, muzzlePoint.position, muzzlePoint.rotation);
                flash.transform.SetParent(muzzlePoint); 
                Destroy(flash, 1f); 
            }
        }
    }

    public void PlayHitImpact(GameObject vfxPrefab, Vector3 hitPosition, Vector3 hitNormal)
    {
        if (vfxPrefab != null)
        {
            GameObject impact = Instantiate(vfxPrefab, hitPosition, Quaternion.LookRotation(hitNormal));
            Destroy(impact, 1f);
        }
    }

    public void PlayEmptyClickSound(ItemData weaponData)
    {
        if (audioSource != null && weaponData != null && weaponData.emptyClickSound != null)
            audioSource.PlayOneShot(weaponData.emptyClickSound);
    }

    public void PlayReloadSound(ItemData weaponData)
    {
        if (audioSource != null && weaponData != null && weaponData.reloadSound != null)
            audioSource.PlayOneShot(weaponData.reloadSound);
    }

    private Transform FindChildRecursive(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName) return child;
            Transform result = FindChildRecursive(child, childName);
            if (result != null) return result;
        }
        return null;
    }

    public void PlayDashEffects(float duration)
    {
        if (audioSource != null && dashSound != null) audioSource.PlayOneShot(dashSound);
        StartCoroutine(SpawnGhostsCoroutine(duration));
    }

    private IEnumerator SpawnGhostsCoroutine(float duration)
    {
        float timePassed = 0f;
        while (timePassed < duration)
        {
            CreateGhost();
            yield return new WaitForSeconds(ghostSpawnRate);
            timePassed += ghostSpawnRate;
        }
    }

    private void CreateGhost()
    {
        if (ghostMaterial == null) return;
        GameObject ghostObj = new GameObject("DashGhost");
        ghostObj.transform.position = transform.position;
        ghostObj.transform.rotation = transform.rotation;

        SkinnedMeshRenderer[] renderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer smr in renderers)
        {
            if (!smr.gameObject.activeInHierarchy) continue;
            Mesh bakedMesh = new Mesh();
            smr.BakeMesh(bakedMesh);

            GameObject ghostPart = new GameObject(smr.gameObject.name + "_Ghost");
            ghostPart.transform.SetParent(ghostObj.transform);
            ghostPart.transform.localPosition = smr.transform.localPosition;
            ghostPart.transform.localRotation = smr.transform.localRotation;
            ghostPart.transform.localScale = smr.transform.localScale;

            MeshFilter mf = ghostPart.AddComponent<MeshFilter>();
            mf.mesh = bakedMesh;

            MeshRenderer mr = ghostPart.AddComponent<MeshRenderer>();
            mr.material = ghostMaterial;
        }

        GhostFader fader = ghostObj.AddComponent<GhostFader>();
        fader.fadeDuration = ghostFadeTime;
        Destroy(ghostObj, ghostFadeTime); 
    }

    private void TurnOffArray(GameObject[] array)
    {
        if (array == null) return;
        foreach (GameObject item in array)
        {
            if (item != null) item.SetActive(false);
        }
    }

    public void UpdateLaser(bool isVisible, Vector3 startPoint, Vector3 endPoint)
    {
        if (laserLine == null) return;
        laserLine.enabled = isVisible;
        if (isVisible)
        {
            laserLine.SetPosition(0, startPoint);
            laserLine.SetPosition(1, endPoint);
        }
    }

    public void PlayBulletTracer(Vector3 startPoint, Vector3 endPoint)
    {
        StartCoroutine(TracerRoutine(startPoint, endPoint));
    }

    private IEnumerator TracerRoutine(Vector3 start, Vector3 end)
    {
        GameObject tracerObj = new GameObject("BulletTracer");
        LineRenderer lr = tracerObj.AddComponent<LineRenderer>();
        
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = new Color(1f, 0.8f, 0f, 1f); 
        lr.endColor = new Color(1f, 0.5f, 0f, 0f);   
        lr.startWidth = 0.05f;
        lr.endWidth = 0.01f;
        
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);

        yield return new WaitForSeconds(0.05f);
        Destroy(tracerObj);
    }

    public void ToggleSkillTreePanel()
    {
        if (skillTreePanel != null)
        {
            bool isActive = !skillTreePanel.activeSelf;
            skillTreePanel.SetActive(isActive);
            
            if (isActive)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
        else
        {
            Debug.LogWarning("Chưa gắn Panel Kỹ Năng vào PlayerView!");
        }
    }
    public void CloseSkillTreePanel()
    {
        if (skillTreePanel != null && skillTreePanel.activeSelf)
        {
            ToggleSkillTreePanel(); 
        }
    }

    // --- CẬP NHẬT GIAO DIỆN DANH HIỆU ---
    public void UpdateTitleUI(string title)
    {
        if (titleText != null) 
        {
            titleText.text = title;
        }
    }

    // --- HIỆU ỨNG THÔNG BÁO ĐỘT PHÁ (PANEL) ---
    public void AnnounceEvolution(string oldTitle, string newTitle)
    {
        if (evolutionPanel != null)
        {
            evolutionPanel.SetActive(true);
            
            if (oldTitleText != null) oldTitleText.text = oldTitle;
            if (newTitleText != null) newTitleText.text = newTitle;
            
            // Tạm dừng thời gian trong game
            Time.timeScale = 0f;
            
            // Bật con trỏ chuột lên để người chơi có thể bấm nút
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Debug.LogWarning("Chưa gắn Evolution Panel vào PlayerView!");
        }
    }

    public void CloseEvolutionPanel()
    {
        if (evolutionPanel != null)
        {
            evolutionPanel.SetActive(false);
            
            // Tiếp tục thời gian trong game
            Time.timeScale = 1f;
        }
    }
}

// LỚP PHỤ TRỢ LÀM MỜ BÓNG MA (GHOST FADER)
public class GhostFader : MonoBehaviour
{
    public float fadeDuration = 0.5f;
    private MeshRenderer[] renderers;
    private Material[] materials;
    private float timer = 0f;

    void Start()
    {
        renderers = GetComponentsInChildren<MeshRenderer>();
        materials = new Material[renderers.Length];
        for (int i = 0; i < renderers.Length; i++) materials[i] = renderers[i].material;
    }

    void Update()
    {
        timer += Time.deltaTime;
        float alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration); 
        foreach (Material mat in materials)
        {
            if (mat.HasProperty("_BaseColor")) 
            {
                Color c = mat.GetColor("_BaseColor"); c.a = alpha; mat.SetColor("_BaseColor", c);
            }
            else if (mat.HasProperty("_Color")) 
            {
                Color c = mat.color; c.a = alpha; mat.color = c;
            }
        }
    }
}