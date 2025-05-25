using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Animation Settings")]
    public float hoverScale = 1.1f;
    public float animationSpeed = 10f;
    
    [Header("Colors")]
    public Color normalColor = new Color(0.18f, 0.36f, 0.66f, 1f);
    public Color hoverColor = new Color(0.23f, 0.44f, 0.85f, 1f);
    
    private Button button;
    private RectTransform rectTransform;
    private Vector3 originalScale;
    private Image buttonImage;
    
    void Start()
    {
        button = GetComponent<Button>();
        rectTransform = GetComponent<RectTransform>();
        buttonImage = GetComponent<Image>();
        originalScale = rectTransform.localScale;
        
        if (buttonImage)
            buttonImage.color = normalColor;
        
        if (button)
        {
            button.onClick.AddListener(PlayClickSound);
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(AnimateScale(originalScale * hoverScale));
        StartCoroutine(AnimateColor(hoverColor));
        
        PlayHoverSound();
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(AnimateScale(originalScale));
        StartCoroutine(AnimateColor(normalColor));
    }
    
    private void PlayHoverSound()
    {
        if (MenuAudioManager.Instance != null)
        {
            MenuAudioManager.Instance.PlayHoverSound();
        }
    }
    
    private void PlayClickSound()
    {
        if (MenuAudioManager.Instance)
        {
            MenuAudioManager.Instance.PlayClickSound();
        }
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
        if (!buttonImage) yield break;
        
        while (Mathf.Abs(buttonImage.color.r - targetColor.r) > 0.01f)
        {
            buttonImage.color = Color.Lerp(buttonImage.color, targetColor, Time.deltaTime * animationSpeed);
            yield return null;
        }
        buttonImage.color = targetColor;
    }
}