using Cysharp.Threading.Tasks;
using UnityEngine;
using Utils.Scripts.UIManager.UINavigator;

namespace Utils.Scripts.UI.Example.Scripts
{
    public class NavigatorStarter : MonoBehaviour
    {
        private void Start()
        {
            ScreenContainer.Push("MainScreen").Forget();
        }
    }
}