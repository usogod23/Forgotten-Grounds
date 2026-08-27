using System;
using System.IO;
using UnityEngine;

public sealed class PlayerPositionSaveSystem : MonoBehaviour
{
    [Serializable]
    private sealed class PlayerPositionData
    {
        public int version = 1;
        public Vector3 position;
    }

    [SerializeField] private string saveFileName = "player-position.json";

    public string SaveFilePath => Path.Combine(Application.persistentDataPath, saveFileName);
    public bool HasSave => File.Exists(SaveFilePath);

    public bool SavePosition(out string message)
    {
        try
        {
            Directory.CreateDirectory(Application.persistentDataPath);

            var data = new PlayerPositionData
            {
                position = transform.position
            };

            File.WriteAllText(SaveFilePath, JsonUtility.ToJson(data, true));
            message = "Position saved.";
            Debug.Log($"{message} File: {SaveFilePath}");
            return true;
        }
        catch (Exception exception)
        {
            message = "The position could not be saved.";
            Debug.LogError($"{message}\n{exception}");
            return false;
        }
    }

    public bool LoadPosition(out string message)
    {
        if (!HasSave)
        {
            message = "No save file was found.";
            Debug.LogWarning(message);
            return false;
        }

        try
        {
            string json = File.ReadAllText(SaveFilePath);
            PlayerPositionData data = JsonUtility.FromJson<PlayerPositionData>(json);

            if (data == null || data.version != 1 || !IsValid(data.position))
            {
                message = "The save file is invalid.";
                Debug.LogError(message);
                return false;
            }

            CharacterController controller = GetComponent<CharacterController>();
            bool controllerWasEnabled = controller != null && controller.enabled;

            try
            {
                if (controllerWasEnabled)
                {
                    controller.enabled = false;
                }

                transform.position = data.position;
            }
            finally
            {
                if (controllerWasEnabled)
                {
                    controller.enabled = true;
                }
            }

            Physics.SyncTransforms();
            message = "Position loaded.";
            Debug.Log(message);
            return true;
        }
        catch (Exception exception)
        {
            message = "The position could not be loaded.";
            Debug.LogError($"{message}\n{exception}");
            return false;
        }
    }

    private static bool IsValid(Vector3 position)
    {
        return IsFinite(position.x) && IsFinite(position.y) && IsFinite(position.z);
    }

    private static bool IsFinite(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
