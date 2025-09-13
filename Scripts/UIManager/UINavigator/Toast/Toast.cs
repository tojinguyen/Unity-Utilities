using TMPro;
using UnityEngine;

namespace Utils.Scripts.UIManager.UINavigator.Toast
{
    public class Toast : EView
    {
        [SerializeField] protected TextMeshProUGUI contentTxt;
        
        public virtual void Init(string content)
        {
            contentTxt.text = content;
        }
    }
}