using UnityEngine;
using TMPro;
using System.Collections;

public class FloatingText : MonoBehaviour
{
    public float moveSpeed = 1.5f;     // Tốc độ bay lên
    public float destroyTime = 1.2f;   // Thời gian tồn tại
    public Vector3 randomOffset = new Vector3(0.5f, 0.2f, 0f); // Độ nảy tản mát ra xung quanh

    private TextMeshPro textMesh;

    public void Setup(string text, Color color)
    {
        textMesh = GetComponent<TextMeshPro>();
        if (textMesh != null)
        {
            textMesh.text = text;
            textMesh.color = color;
        }
        
        // 1. Văng chữ ngẫu nhiên một chút để không bị đè lên nhau
        transform.localPosition += new Vector3(
            Random.Range(-randomOffset.x, randomOffset.x),
            Random.Range(-randomOffset.y, randomOffset.y),
            Random.Range(-randomOffset.z, randomOffset.z)
        );

        // 2. Kích hoạt hiệu ứng Nảy và Mờ dần
        StartCoroutine(AnimateText());
    }

    private IEnumerator AnimateText()
    {
        float timer = 0f;
        Vector3 startScale = Vector3.zero;
        Vector3 targetScale = Vector3.one * 1.2f; // Phóng to hơn bình thường 20%
        Vector3 normalScale = Vector3.one;

        // HIỆU ỨNG 1: Nảy to ra (Pop-in) trong 0.15s đầu
        while (timer < 0.15f)
        {
            timer += Time.deltaTime;
            transform.localScale = Vector3.Lerp(startScale, targetScale, timer / 0.15f);
            yield return null;
        }
        transform.localScale = normalScale; // Trả về kích thước chuẩn

        // HIỆU ỨNG 2: Bay lên & Mờ dần (Fade-out)
        timer = 0f;
        Color startColor = textMesh.color;
        Color fadeColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

        while (timer < destroyTime)
        {
            timer += Time.deltaTime;
            
            // Bay từ từ lên trên
            transform.position += Vector3.up * moveSpeed * Time.deltaTime;
            
            // Luôn xoay mặt về phía Camera
            if (Camera.main != null)
            {
                transform.LookAt(transform.position + Camera.main.transform.forward);
            }

            // Bắt đầu mờ đi khi thời gian trôi qua một nửa
            if (timer > destroyTime / 2f)
            {
                float fadeProgress = (timer - (destroyTime / 2f)) / (destroyTime / 2f);
                textMesh.color = Color.Lerp(startColor, fadeColor, fadeProgress);
            }

            yield return null;
        }

        // 3. Xóa chữ sau khi xong hiệu ứng
        Destroy(gameObject);
    }
}