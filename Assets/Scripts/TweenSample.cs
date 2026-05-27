using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;

public class TweenSample : MonoBehaviour
{
    public RectTransform UITarget;
    public Image UIImage;
    public GameObject ObjectTarget;

    public TMP_Text countText;
    public int currentValue;
    public int addValue;

    public int targetValue;

    public Color flashColor = Color.red;

    private Color oringinalColor;

    public CanvasGroup fadeTarget;
    void Start()
    {
        oringinalColor = UIImage.color;

        fadeTarget.alpha = 0;
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            PlayPunchUIScale();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            PlayPunchObjectScale();
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            PlayUIShake();
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            PlayCountUp();
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            PlayColorFlash();
        }
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            PlayFade();
        }
    }
    public void PlayPunchUIScale()
    {
        if (UITarget == null) return;
        UITarget.DOKill();
        UITarget.localScale = Vector3.one;
        UITarget.DOPunchScale(Vector3.one * 0.3f, 0.25f, 8, 1.0f);
    }
    public void PlayPunchObjectScale()
    {
        if (ObjectTarget == null) return;
        ObjectTarget.transform.DOKill();
        ObjectTarget.transform.localScale = Vector3.one;
        ObjectTarget.transform.DOPunchScale(Vector3.one * 0.3f, 0.25f, 8, 1.0f);
    }
    public void PlayUIShake()
    {
        if (ObjectTarget == null) return;
        ObjectTarget.transform.DOKill();
        ObjectTarget.transform.DOShakePosition(0.3f, 20f, 20, 90f);
    }
    public void PlayCountUp()
    {
        if (countText == null) return;

        targetValue += addValue;
        DOTween.Kill("CountTween", true);

        DOTween.To(
            () => currentValue,
            value =>
            {
                currentValue = value;
                countText.text = currentValue.ToString();
            },
            targetValue,
            0.5f
        )
        .SetEase(Ease.OutQuad)
        .SetId("CountTween");
    }
    public void PlayColorFlash()
    {
        if (UIImage == null) return;

        UIImage.DOKill();
        UIImage.color = oringinalColor;
        UIImage.DOColor(flashColor, 0.01f);
        .OnComplete(() =>
         {
             UIImage.DOColor(oringinalColor, 0.2f);
         });
    }
    public void PlayFade()
    {
        if (fadeTarget == null) return;
        fadeTarget.DOKill();
        fadeTarget.alpha = 0;

        Sequence seq = DOTween.Sequence();

        seq.Append(fadeTarget.DOFade(1, 0.2f));
        seq.AppendInterval(0.5f);
        seq.Addend(fadeTarget.DOFade(0f, 0.3f)); 
    }
}
