using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class CamController : MonoBehaviour
{
    [SerializeField] private RawImage photoCameraPreview;
    [SerializeField] private RawImage referencePhoto;
    [SerializeField] private Toggle referenceVisibilityToggle;
    [SerializeField] private Slider scaleSlider;
    [SerializeField] private Slider ratioSlider;
    [SerializeField] private Slider xOffsetSlider;
    [SerializeField] private Slider yOffsetSlider;

    private WebCamDevice[] devices;
    private WebCamTexture texture;
    private float photoBaseWidth = 30f;
    private float scale = 1f;
    private float ratio = 1f;
    private Vector2 photoBaseSize;
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
            if(value != _referenceVisible)
            {
                _referenceVisible = value;
                referenceVisibilityToggle.isOn = !value;
                referencePhoto.gameObject.SetActive(value);
            }
        }
    }
    public void StartCamera()
    {
        StartCoroutine(StartCam());
    }

    private IEnumerator StartCam()
    {
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
        if (Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            Debug.Log("Webcam access granted.");
            devices = WebCamTexture.devices;
            if (devices.Any())
            {
                Debug.Log($"Webcam device found: {devices[0].name}" + (devices[0].isFrontFacing ? "Frontfacing" : ""));
                StartDevice(devices[0]);
            }
            else
            {
                Debug.LogError("no webcams found.");
            }
            //for (int cameraIndex = 0; cameraIndex < devices.Length; ++cameraIndex)
            //{
            //    Debug.Log("devices[cameraIndex].name: ");
            //    Debug.Log(devices[cameraIndex].name);
            //    Debug.Log("devices[cameraIndex].isFrontFacing");
            //    Debug.Log(devices[cameraIndex].isFrontFacing);
            //    StartCamera();
            //}
        }
        else
        {
            Debug.Log("Access denied.");
        }
    }

    //public void SwapCamera(int deviceIndex = 0)
    //{
    //    if (WebCamTexture.devices.Length > 0)
    //    {
    //        _currentCameraIndex++;
    //        _currentCameraIndex %= WebCamTexture.devices.Length;

    //        if (_texture != null)
    //        {
    //            StopCamera();
    //            StartCamera();
    //        }
    //    }
    //}

    private void StartDevice(WebCamDevice device)
    {
            texture = new WebCamTexture(device.name);
        photoCameraPreview.texture = texture;

        texture.Play();
        photoCameraPreview.gameObject.SetActive(true);
            //_display.color = Color.white;
            Debug.Log($"Webcam is playing.");        
    }

    public void StopCamera()
    {
        if (texture != null)
        {
            photoCameraPreview.gameObject.SetActive(false);
            texture.Stop();
            photoCameraPreview.texture = null;
            texture = null;
        }
    }

    public void TakePhoto()
    {
        StartCoroutine(Photo());
    }

    private int preferedWidth = 30;
    private IEnumerator Photo()
    {
        if(texture == null)
        {
            yield break;
        }
        yield return new WaitForEndOfFrame();
        Debug.Log($"Texture size {texture.width}");

        Texture2D photo = new Texture2D(texture.width, texture.height);
        photo.SetPixels(texture.GetPixels());
        photo.Apply();

        referencePhoto.texture = photo;

        float proportions = (float)texture.height/texture.width;
        photoBaseSize = new Vector2(photoBaseWidth, photoBaseWidth * proportions);
        ratioSlider.value = 1; //reset this to not confuse anyone
        ratio = 1f;
        ScaleReferencePhoto();

        ReferenceVisible = true;

        referenceVisibilityToggle.interactable = true;
        scaleSlider.interactable = true;
        xOffsetSlider.interactable = true;
        yOffsetSlider.interactable = true;
        ratioSlider.interactable = true;


        StopCamera();
    }



    private void ScaleReferencePhoto()
    {
        referencePhoto.rectTransform.sizeDelta = photoBaseSize * scale* new Vector2(1, ratio);
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
        referencePhoto.rectTransform.localPosition = referenceOffset;
    }
}
