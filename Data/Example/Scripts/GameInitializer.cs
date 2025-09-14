using UnityEngine;
using TirexGame.Utils.Data;

public class GameInitializer : MonoBehaviour
{
    private void Start()
    {
        var playerDataRepository = new FileDataRepository<PlayerData>(
            useEncryption: true, 
            useCompression: true
        );
        
        DataManager.RegisterRepository(playerDataRepository);
        
        Debug.Log("Game Initialized with PlayerData Repository");
    }
}