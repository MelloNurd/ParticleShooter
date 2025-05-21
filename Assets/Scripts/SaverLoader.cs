using System;
using System.IO;
using UnityEngine;

[Serializable]
public class SimSettings
{
    public Vector2 ScreenSpace;
    public int NumberOfParticles;
    public int NumberOfTypes;

    public Vector2 forces;
    public Vector2 minDistances;
    public Vector2 radii;

    public float Friction;
    public float Dampening;
    public float RepulsionEffector;

    public float TimeScale;

    public void PrintSettings()
    {
        Debug.Log("ScreenSpace: " + ScreenSpace);
        Debug.Log("NumberOfParticles: " + NumberOfParticles);
        Debug.Log("NumberOfTypes: " + NumberOfTypes);
        Debug.Log("forces: " + forces);
        Debug.Log("minDistances: " + minDistances);
        Debug.Log("radii: " + radii);
        Debug.Log("Friction: " + Friction);
        Debug.Log("Dampening: " + Dampening);
        Debug.Log("RepulsionEffector: " + RepulsionEffector);
        Debug.Log("TimeScale: " + TimeScale);
    }
}

public static class SaverLoader
{
    public static void SaveData(SimSettings settings)
    {
        string jsonData = JsonUtility.ToJson(settings, prettyPrint: true);
        File.WriteAllText(Application.persistentDataPath + "/settings.json", jsonData);
        Debug.Log("Settings saved to " + Application.persistentDataPath + "/settings.json");
    }

    public static SimSettings LoadData(string filePath)
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            Debug.Log("Settings saved to " + Application.persistentDataPath + "/settings.json");
            return JsonUtility.FromJson<SimSettings>(json);
        }
        Debug.Log("Unable to find " + Application.persistentDataPath + "/settings.json");
        return null;

    }
}
