using UnityEditor;
using UnityEngine;
using Utils.Scripts.UIManager.UINavigator;
using ToastView = Utils.Scripts.UIManager.UINavigator.Toast.Toast;

namespace Utils.Scripts.UIManager.UINavigator.Editor
{
    [InitializeOnLoad]
    public static class UINavigatorComponentAutoAssign
    {
        static UINavigatorComponentAutoAssign()
        {
            ObjectFactory.componentWasAdded += OnComponentAdded;
        }

        private static void OnComponentAdded(Component component)
        {
            switch (component)
            {
                case ScreenView sv:
                    UINavigatorMenuItems.AutoAssignScreenViewAnimations(sv);
                    break;
                case ToastView toast:
                    UINavigatorMenuItems.AutoAssignEViewAnimations(toast, toastMode: true);
                    break;
                case EView ev:
                    UINavigatorMenuItems.AutoAssignEViewAnimations(ev);
                    break;
            }
        }
    }
}
