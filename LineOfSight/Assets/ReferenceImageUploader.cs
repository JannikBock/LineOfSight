using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ReferenceImageUploader : MonoBehaviour
{
    private ReferenceImageController referenceController;

    private void Awake()
    {
        referenceController = FindObjectOfType<ReferenceImageController>();
    }

    public void UploadReference()
    {
        FileUploaderHelper.RequestFile((path) =>
        {
            if (string.IsNullOrWhiteSpace(path))
                return;

            StartCoroutine(UploadImage(path));
        });
    }

    IEnumerator UploadImage(string path)
    {
        Texture2D texture;

        using (UnityWebRequest imageWeb = new UnityWebRequest(path, UnityWebRequest.kHttpVerbGET))
        {

            imageWeb.downloadHandler = new DownloadHandlerTexture();

            yield return imageWeb.SendWebRequest();

            texture = ((DownloadHandlerTexture)imageWeb.downloadHandler).texture;
        }

        referenceController.SetReference(texture);
    }

}
