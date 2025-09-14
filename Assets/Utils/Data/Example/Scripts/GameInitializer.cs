using UnityEngine;
using TirexGame.Utils.Data;

public class GameInitializer : MonoBehaviour
{
    private void Start()
    {
        var dataManager = DataManager.Instance;

        var playerDataRepository = new FileDataRepository<PlayerData>(
            useEncryption: true, 
            useCompression: true
        );
        
        dataManager.RegisterRepository(playerDataRepository);
        
        Debug.Log("Game Initialized with PlayerData Repository");
    }
}