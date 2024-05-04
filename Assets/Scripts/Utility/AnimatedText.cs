using UnityEngine;

[RequireComponent(typeof(TMPro.TextMeshProUGUI))]
public class AnimatedText : MonoBehaviour
{
    protected Vector3 initialScale;
    [SerializeField]
    protected Vector2 endScale = Vector2.zero;
    [SerializeField]
    protected float scaleDuration = 0f;
    protected float scaleTime = -1f;

    protected float initialAlpha;
    [SerializeField]
    protected float endAlpha = -1f;
    [SerializeField]
    protected float fadeDuration = 0f;
    protected float fadeTime = -1f;

    protected TMPro.TextMeshProUGUI text;

    protected void Awake()
    {
        text = GetComponent<TMPro.TextMeshProUGUI>();
        initialAlpha = text.alpha;

        initialScale = transform.localScale;
    }

    protected void Update()
    {
        scaleTime += Time.deltaTime;
        if (scaleTime >= 0f && scaleTime < scaleDuration)
        {
            transform.localScale = Vector3.Lerp(initialScale, endScale, scaleTime);
        }

        fadeTime += Time.deltaTime;
        if(fadeTime >= 0f && fadeTime < fadeDuration) {
            text.alpha = Mathf.Lerp(initialAlpha, endAlpha, fadeTime);
        }
    }

    public float Alpha { get => text.alpha; set => text.alpha = value; }

    public string   Text {
        get => text.text;
        set {
            if(text.text == value) {
                return;
            }

            text.text = value;
            
            scaleTime = 0f;
            fadeTime = 0f;
            if (text != null) { text.alpha = initialAlpha; }
            transform.localScale = initialScale;
        }
    }
}