using UnityEngine;

public class AudioManagerInitializer : MonoBehaviour
{
    [Header("Audio Setup")]
    [SerializeField] private AudioDatabase audioDatabase;
    
    void Awake()
    {
        if (audioDatabase != null)
        {
            AudioManager.Initialize(audioDatabase);
            ConsoleLogger.Log("AudioManager initialized successfully");
        }
        else
        {
            ConsoleLogger.LogError("AudioDatabase is not assigned!");
        }
    }
}