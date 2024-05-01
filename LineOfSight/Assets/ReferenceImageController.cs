using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class ReferenceImageController : MonoBehaviour
{
    [SerializeField] private RawImage referenceImage;
    [SerializeField] private Toggle referenceVisibilityToggle;
    [SerializeField] private Slider scaleSlider;
    [SerializeField] private Slider ratioSlider;
    [SerializeField] private Slider xOffsetSlider;
    [SerializeField] private Slider yOffsetSlider;

    private float baseWidth = 30f;
    private float scale = 1f;
    private float ratio = 1f;
    private Vector2 baseSize;
    private Vector2 referenceOffset;

    private bool _referenceVisible = false;
    private bool ReferenceVisible
    {
        get
        {
            return _referenceVisible;
        }
        set
        {
            if (value != _referenceVisible)
            {
                _referenceVisible = value;
                referenceVisibilityToggle.isOn = !value;
                referenceImage.gameObject.SetActive(value);
            }
        }
    }

    public void SetReference(Texture2D texture)
    {
        Debug.Log($"Texture size {texture.width}");

        referenceImage.texture = texture;

        float proportions = (float)texture.height / texture.width;
        baseSize = new Vector2(baseWidth, baseWidth * proportions);
        ratioSlider.value = 1; //reset this to not confuse anyone
        ratio = 1f;
        ScaleReferencePhoto();

        ReferenceVisible = true;

        referenceVisibilityToggle.interactable = true;
        scaleSlider.interactable = true;
        xOffsetSlider.interactable = true;
        yOffsetSlider.interactable = true;
        ratioSlider.interactable = true;


    }

    private void ScaleReferencePhoto()
    {
        referenceImage.rectTransform.sizeDelta = baseSize * scale * new Vector2(1, ratio);
    }

    public void OnVisibilityChanged()
    {
        ReferenceVisible = !referenceVisibilityToggle.isOn;

    }
    public void OnScaleChanged()
    {
        scale = scaleSlider.value;
        ScaleReferencePhoto();
    }

    public void OnRatioChanged()
    {
        ratio = ratioSlider.value;
        ScaleReferencePhoto();
    }

    public void OnOffsetChanged()
    {
        referenceOffset = new Vector2(xOffsetSlider.value, -yOffsetSlider.value);
        referenceImage.rectTransform.localPosition = referenceOffset;
    }
}
