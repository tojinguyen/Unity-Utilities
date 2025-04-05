using Cysharp.Threading.Tasks;

public interface IUIManager
{
    public UniTask<T> ShowUI<T>(string address) where T : UIBase;
    public UniTask Pop();
    public UIBase Peek();
}