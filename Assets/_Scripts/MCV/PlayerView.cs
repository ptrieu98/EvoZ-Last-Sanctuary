using System.Collections;
using UnityEngine;

public class PlayerView : MonoBehaviour
{
    [Header("=== ANIMATION ===")]
    public Animator animator;

    [Header("=== NGOẠI HÌNH & PHỤ KIỆN NAM ===")]
    public GameObject[] maleOutfits;
    public GameObject[] maleHairs;
    public GameObject[] maleBeards;
    public GameObject[] maleGlasses;

    [Header("=== NGOẠI HÌNH & PHỤ KIỆN NỮ ===")]
    public GameObject[] femaleOutfits;
    public GameObject[] femaleHairs;
    public GameObject[] femaleGlasses;

    [Header("=== HIỆU ỨNG TÀN ẢNH (GHOST TRAIL) ===")]
    [Tooltip("Kéo file Ghost_Mat bạn vừa tạo vào đây")]
    public Material ghostMaterial;
    [Tooltip("Khoảng cách thời gian sinh ra 1 bóng (Giây)")]
    public float ghostSpawnRate = 0.05f; 
    [Tooltip("Thời gian tồn tại của bóng trước khi tan biến")]
    public float ghostFadeTime = 0.4f;   

    [Header("=== ÂM THANH ===")]
    public AudioSource audioSource;
    public AudioClip dashSound;

    [Header("=== VŨ KHÍ 3D ===")]
    [Tooltip("Kéo WeaponHoldPoint trên tay nhân vật vào đây")]
    public Transform weaponHoldPoint;
    private GameObject currentWeaponObj; // Lưu vũ khí đang cầm trên tay

    // --- HÀM LOAD NGOẠI HÌNH ---
    public void UpdateAppearance(int gender, int outfitID, int hairID, int beardID, int glassesID)
    {
        TurnOffArray(maleOutfits);
        TurnOffArray(femaleOutfits);
        TurnOffArray(maleHairs);
        TurnOffArray(femaleHairs);
        TurnOffArray(maleBeards);
        TurnOffArray(maleGlasses);
        TurnOffArray(femaleGlasses);

        if (gender == 0) // NAM
        {
            if (outfitID >= 0 && outfitID < maleOutfits.Length) maleOutfits[outfitID].SetActive(true);
            if (hairID >= 0 && hairID < maleHairs.Length) maleHairs[hairID].SetActive(true);
            if (beardID >= 0 && beardID < maleBeards.Length) maleBeards[beardID].SetActive(true);
            if (glassesID >= 0 && glassesID < maleGlasses.Length) maleGlasses[glassesID].SetActive(true);
        }
        else // NỮ
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

    // --- HÀM CẦM VŨ KHÍ 3D ---
    public void EquipWeapon3D(GameObject weaponPrefab)
    {
        if (currentWeaponObj != null)
        {
            Destroy(currentWeaponObj);
        }

        if (weaponPrefab == null) return;

        if (weaponHoldPoint != null)
        {
            // Giữ nguyên thông số vị trí gốc của Prefab
            currentWeaponObj = Instantiate(weaponPrefab, weaponHoldPoint, false);
        }
    }

    // --- HÀM THIẾT LẬP TƯ THẾ CHIẾN ĐẤU ---
    public void SetAimingStance(bool isAiming)
    {
        if (animator != null)
        {
            animator.SetBool("IsAiming", isAiming);
        }
    }

    // --- HÀM GỌI ANIMATION TẤN CÔNG ---
    public void PlayAttackAnimation(WeaponType type)
    {
        if (animator == null) return;

        if (type == WeaponType.Melee)
        {
            animator.SetTrigger("Attack"); 
        }
        else if (type == WeaponType.Ranged)
        {
            animator.SetTrigger("Shoot");  
        }
    }

    // --- HÀM KÍCH HOẠT HIỆU ỨNG LƯỚT ---
    public void PlayDashEffects(float duration)
    {
        if (audioSource != null && dashSound != null)
        {
            audioSource.PlayOneShot(dashSound);
        }

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

    // --- THUẬT TOÁN ĐÚC BÓNG MA (BAKE MESH) ---
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
}

// ========================================================
// CLASS PHỤ TRỢ: Tự động làm mờ màu sắc của bóng ma
// ========================================================
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
        
        for (int i = 0; i < renderers.Length; i++)
        {
            materials[i] = renderers[i].material;
        }
    }

    void Update()
    {
        timer += Time.deltaTime;
        float alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration); 

        foreach (Material mat in materials)
        {
            if (mat.HasProperty("_BaseColor")) 
            {
                Color c = mat.GetColor("_BaseColor");
                c.a = alpha;
                mat.SetColor("_BaseColor", c);
            }
            else if (mat.HasProperty("_Color")) 
            {
                Color c = mat.color;
                c.a = alpha;
                mat.color = c;
            }
        }
    }
}