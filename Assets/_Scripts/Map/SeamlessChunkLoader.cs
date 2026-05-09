using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SeamlessChunkLoader : MonoBehaviour
{
    [Tooltip("Gõ chính xác tên của Scene bản đồ bạn muốn load (VD: Map_KhuRungTay)")]
    public string sceneToLoad;

    // Biến theo dõi xem bản đồ này đã được tải lên chưa
    private bool isLoaded = false;

    private void OnTriggerEnter(Collider other)
    {
        // Khi nhân vật bước vào khu vực, nếu cảnh chưa load thì bắt đầu load
        if (other.CompareTag("Player") && !isLoaded)
        {
            StartCoroutine(LoadSceneCoroutine());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Khi nhân vật đi xa khỏi khu vực, xóa cảnh đó đi để giải phóng RAM
        if (other.CompareTag("Player") && isLoaded)
        {
            StartCoroutine(UnloadSceneCoroutine());
        }
    }

    private IEnumerator LoadSceneCoroutine()
    {
        // LoadSceneMode.Additive: Đắp bản đồ mới lên trên bản đồ hiện tại (Không xóa cái cũ)
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);
        
        // Vòng lặp chờ: Game vẫn chạy bình thường trong lúc file đang được tải ngầm
        while (!asyncLoad.isDone)
        {
            yield return null; 
        }
        isLoaded = true;
    }

    private IEnumerator UnloadSceneCoroutine()
    {
        // Gỡ bỏ bản đồ ra khỏi RAM
        AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(sceneToLoad);
        while (!asyncUnload.isDone)
        {
            yield return null;
        }
        isLoaded = false;
    }
}