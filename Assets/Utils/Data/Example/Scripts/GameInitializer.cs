using UnityEngine;
using TirexGame.Utils.Data;
using TirexGame.Utils.Data.Examples;

public class GameInitializer : MonoBehaviour
{
    private void Start()
    {
        var playerDataRepository = new FileDataRepository<TirexExamplePlayerData>(
            useEncryption: true, 
            useCompression: true
        );
        
        DataManager.RegisterRepository(playerDataRepository);
        
        Debug.Log("Game Initialized with TirexExamplePlayerData Repository");
    }
}