using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class AnimeContainer : MonoBehaviour
{
    public Anime Anime;
    [SerializeField] RawImage banner;

    public void Init()
    {
        StartCoroutine(DownloadImage(Anime.BannerLink));
    }

    public void WhenClicked()
    {
        Controller.i.OpenAnimeInfos(Anime);
    }

    IEnumerator DownloadImage(string MediaUrl)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(MediaUrl);
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError)
            Debug.Log(request.error);
        else
            banner.texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
    }
}
