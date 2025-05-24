using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Button button;
    private RectTransform rectTransform;
    
    [Header("Animation Settings")]
    public float hoverScale = 1.1f;
    public float animationSpeed = 10f;
    
    [Header("Colors")]
    public Color normalColor = new Color(0.18f, 0.36f, 0.66f, 1f);
    public Color hoverColor = new Color(0.23f, 0.44f, 0.85f, 1f);
    
    private Vector3 originalScale;
    private Image buttonImage;
    
    void Start()
    {
        button = GetComponent<Button>();
        rectTransform = GetComponent<RectTransform>();
        buttonImage = GetComponent<Image>();
        originalScale = rectTransform.localScale;
        
        // Set initial color
        if (buttonImage != null)
            buttonImage.color = normalColor;
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Start hover animation
        StopAllCoroutines();
        StartCoroutine(AnimateScale(originalScale * hoverScale));
        StartCoroutine(AnimateColor(hoverColor));
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        // Return to normal
        StopAllCoroutines();
        StartCoroutine(AnimateScale(originalScale));
        StartCoroutine(AnimateColor(normalColor));
    }
    
    System.Collections.IEnumerator AnimateScale(Vector3 targetScale)
    {
        while (Vector3.Distance(rectTransform.localScale, targetScale) > 0.01f)
        {
            rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, targetScale, Time.deltaTime * animationSpeed);
            yield return null;
        }
        rectTransform.localScale = targetScale;
    }
    
    System.Collections.IEnumerator AnimateColor(Color targetColor)
    {
        while (Mathf.Abs(buttonImage.color.r - targetColor.r) > 0.01f)
        {
            buttonImage.color = Color.Lerp(buttonImage.color, targetColor, Time.deltaTime * animationSpeed);
            yield return null;
        }
        buttonImage.color = targetColor;
    }
}