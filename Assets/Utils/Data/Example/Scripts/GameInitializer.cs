using UnityEngine;
using TirexGame.Utils.Data;
using MyGame.Data;

public class GameInitializer : MonoBehaviour
{
    private void Start()
    {
        var playerDataRepository = new FileDataRepository<TestPlayerData>(
            useEncryption: true,
            useCompression: true
        );

        DataManager.RegisterRepository(playerDataRepository);

        Debug.Log("Game Initialized with TestPlayerData Repository");
    }
}