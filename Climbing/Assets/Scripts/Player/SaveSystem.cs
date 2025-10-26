using System;
using System.IO;
using UnityEngine;

public class SaveSystem
{
    public static string SavePath
    {
        get
        {
            return Path.Combine(Application.persistentDataPath, "player_save.json");
        }
    }

    private static string TempPath
    {
        get
        {
            return SavePath + ".tmp";
        }
    }

    private static string BackupPath
    {
        get
        {
            return SavePath + ".bak";
        }
    }

    public static bool Save(PlayerSaveData data)
    {
        try
        {
            string json = JsonUtility.ToJson(data);

            // 1. write to temp
            File.WriteAllText(TempPath, json);

            // 2. move current to backup (if it exists)
            if (File.Exists(SavePath))
            {
                File.Copy(SavePath, BackupPath, true);
            }

            // 3. replace save with temp (atomic on most OS)
            if (File.Exists(SavePath))
            {
                File.Delete(SavePath);
            }
            File.Move(TempPath, SavePath);

            return true;
        }
        
        catch (Exception e)
        {
            Debug.LogError("Save failed: " + e.Message);
            return false;
        }
    }

    public static PlayerSaveData Load()
    {
        try
        {
            if (File.Exists(SavePath))
            {
                string json = File.ReadAllText(SavePath);
                PlayerSaveData data = JsonUtility.FromJson<PlayerSaveData>(json);
                return data;
            }

            // try backup if main missing/corrupt
            if (File.Exists(BackupPath))
            {
                string json = File.ReadAllText(BackupPath);
                PlayerSaveData data = JsonUtility.FromJson<PlayerSaveData>(json);
                return data;
            }

            return null;
        }

        catch (Exception e)
        {
            Debug.LogWarning("Load failed, trying backup: " + e.Message);
            try
            {
                if (File.Exists(BackupPath))
                {
                    string json = File.ReadAllText(BackupPath);
                    PlayerSaveData data = JsonUtility.FromJson<PlayerSaveData>(json);
                    return data;
                }
            }
            catch (Exception e2)
            {
                Debug.LogError("Backup load failed: " + e2.Message);
            }
            return null;
        }
    }

    public static void DeleteAllSaves()
    {
        try
        {
            if (File.Exists(SavePath)) File.Delete(SavePath);
            if (File.Exists(BackupPath)) File.Delete(BackupPath);
            if (File.Exists(TempPath)) File.Delete(TempPath);
        }
        catch (Exception e)
        {
            Debug.LogError("Delete saves failed: " + e.Message);
        }
    }
}
