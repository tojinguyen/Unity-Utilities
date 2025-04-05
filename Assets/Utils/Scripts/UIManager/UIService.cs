using Cysharp.Threading.Tasks;

public static class UIService
{
    private static IUIManager _instance;
    
    public static void Register(IUIManager instance)
    {
        _instance = instance;
    }
    
    public static UniTask<T> ShowUI<T>(string address) where T : UIBase => _instance.ShowUI<T>(address);
    public static UniTask Pop() => _instance.Pop();
    public static UIBase Peek() => _instance.Peek();
}