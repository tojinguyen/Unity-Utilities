using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Utils.Scripts.UIManager.UINavigator.Tooltip
{
    public class Tooltip : EView
    {
        [SerializeField] private TextMeshProUGUI contentTxt;

        public async UniTaskVoid Init(string content, Vector2 position, bool isLocalPosition = false)
        {
            if (contentTxt)
                contentTxt.text = content;
            await OnShow(position, isLocalPosition);
        }
    }
}