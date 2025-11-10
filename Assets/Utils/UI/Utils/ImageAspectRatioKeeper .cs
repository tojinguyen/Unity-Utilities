using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("TirexGame/UI/Image Aspect Ratio Keeper")]
[RequireComponent(typeof(Image))]
public class ImageAspectRatioKeeper : MonoBehaviour
{
    [SerializeField] private bool fitToScreen = true;
    [SerializeField] private bool updateOnResize = true;
    
    [SerializeField] private Image image;
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private Canvas canvas;
    [SerializeField] private RectTransform canvasRect;
    
    private float originalAspectRatio;
    private Vector2 lastScreenSize;

    private void OnValidate()
    {
        if (image == null)
            image = GetComponent<Image>();
        
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();
        
        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();
        
        if (canvas != null && canvasRect == null)
            canvasRect = canvas.GetComponent<RectTransform>();
    }

    private void Awake()
    {
        // Ensure components are assigned
        if (image == null)
            image = GetComponent<Image>();
        
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();
        
        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();
        
        if (canvas != null && canvasRect == null)
            canvasRect = canvas.GetComponent<RectTransform>();
        
        // Store original aspect ratio
        if (image.sprite != null)
        {
            var spriteRect = image.sprite.rect;
            originalAspectRatio = spriteRect.width / spriteRect.height;
        }
        else if (image.mainTexture != null)
        {
            originalAspectRatio = (float)image.mainTexture.width / image.mainTexture.height;
        }
        else
        {
            ConsoleLogger.LogWarning("ImageAspectRatioKeeper: No sprite or texture found on Image component!");
            originalAspectRatio = 1f;
        }
        
        lastScreenSize = new Vector2(Screen.width, Screen.height);
    }

    private void Start()
    {
        AdjustAspectRatio();
    }

    private void Update()
    {
        if (updateOnResize)
        {
            var currentScreenSize = new Vector2(Screen.width, Screen.height);
            if (currentScreenSize != lastScreenSize)
            {
                lastScreenSize = currentScreenSize;
                AdjustAspectRatio();
            }
        }
    }

    public void AdjustAspectRatio()
    {
        if (rectTransform == null || !fitToScreen)
            return;

        if (canvasRect == null)
        {
            ConsoleLogger.LogWarning("ImageAspectRatioKeeper: No Canvas RectTransform found!");
            return;
        }

        var screenSize = canvasRect.sizeDelta;

        // Set anchors to stretch
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        // Calculate scale to fit screen while maintaining aspect ratio
        var scaleWidth = screenSize.x;
        var scaleHeight = screenSize.x / originalAspectRatio;

        // If height doesn't fill screen, scale by height instead
        if (scaleHeight < screenSize.y)
        {
            scaleHeight = screenSize.y;
            scaleWidth = screenSize.y * originalAspectRatio;
        }

        // Apply the scale
        rectTransform.sizeDelta = new Vector2(scaleWidth - screenSize.x, scaleHeight - screenSize.y);
    }

    public void SetOriginalAspectRatio(float aspectRatio)
    {
        originalAspectRatio = aspectRatio;
        AdjustAspectRatio();
    }

    public float GetOriginalAspectRatio()
    {
        return originalAspectRatio;
    }

    public void RefreshAspectRatio()
    {
        if (image.sprite != null)
        {
            var spriteRect = image.sprite.rect;
            originalAspectRatio = spriteRect.width / spriteRect.height;
        }
        else if (image.mainTexture != null)
        {
            originalAspectRatio = (float)image.mainTexture.width / image.mainTexture.height;
        }
        
        AdjustAspectRatio();
    }
}