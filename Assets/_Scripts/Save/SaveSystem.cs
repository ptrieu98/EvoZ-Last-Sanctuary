using UnityEngine;
using System.IO;

public static class SaveSystem
{
    // Đường dẫn lưu file trên máy tính/điện thoại
    private static string savePath = Application.persistentDataPath + "/player_save.json";

    public static void SavePlayerAppearance(int gen, int outfit, int hair, int beard, int glasses)
    {
        PlayerData data = new PlayerData();
        data.gender = gen;
        data.outfitID = outfit;
        data.hairID = hair;
        data.beardID = beard;
        data.glassesID = glasses;

        // Chuyển đối tượng data thành chuỗi JSON
        string json = JsonUtility.ToJson(data);
        // Ghi vào file
        File.WriteAllText(savePath, json);
    }

    public static PlayerData LoadPlayerAppearance()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            return JsonUtility.FromJson<PlayerData>(json);
        }
        return null; // Trả về null nếu chưa có file lưu
    }
}