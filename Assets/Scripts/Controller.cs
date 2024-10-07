using System.Collections;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Controller : MonoBehaviour
{
    [SerializeField] AnimeListContainer containers;
    [SerializeField] GameObject AddMenu;
    [SerializeField] GameObject AnimeSelectionMenu;
    [SerializeField] GameObject AnimeInfoMenu;

    [Header("Add Menu Variables")]
    [SerializeField] TMP_InputField nameField;
    [SerializeField] TMP_InputField synopsisField;
    [SerializeField] TMP_InputField bannerField;
    [SerializeField] TMP_Dropdown stateField;
    [SerializeField] Button submitButton;
    [SerializeField] Button backButton_add;
    [SerializeField] TMP_Text errorText;

    [Header("Infos Menu Variables")]
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI synopsisText;
    [SerializeField] TextMeshProUGUI stateText;
    [SerializeField] RawImage banner;
    [SerializeField] Button backButton_info;
    [SerializeField] Button modifyButton;
    [SerializeField] Button deleteButton;

    private bool isLinkGood;
    private ControllerState state = ControllerState.None;
    private Anime currentAnime;
    private DisplayState displayState = 0;

    public static Controller i;

    private void Awake()
    {
        i = this;
        AnimeList.i.OnUpdated += listUpdated;
    }

    private void Start()
    {
        containers.DoReset();
        SavingSystem.i.Load("AnimeList");
    }

    void listUpdated()
    {
        SavingSystem.i.Save("AnimeList");
        StartCoroutine(WaitForObjectToBeEnableForUpdate());
    }

    IEnumerator WaitForObjectToBeEnableForUpdate()
    {
        yield return new WaitUntil(() => AnimeSelectionMenu.activeInHierarchy);
        containers.DoReset();
        foreach (var anime in AnimeList.i.GetAnimeList(displayState))
        {
            containers.Print(anime);
        }
    }

    public void Button_AddNewAnime()
    {
        ResetAddMenu();
        AddMenu.SetActive(true);
        AnimeSelectionMenu.SetActive(false);
        state = ControllerState.Adding;
    }

    public void OpenAnimeInfos(Anime anime)
    {
        currentAnime = anime;
        AnimeInfoMenu.SetActive(true);
        AnimeSelectionMenu.SetActive(false);
        ResetAnimeInfoMenu();
        nameText.text = anime.Name;
        synopsisText.text = anime.Synopsis != "" ? anime.Synopsis : "No synopsis have been entered.";
        stateText.text = anime.State == AnimeState.NotWatched ? "Not Watched" : anime.State == AnimeState.CurrentlyWatching ? "Currently Watching" : "Finished";
        StartCoroutine(DownloadImage(anime.BannerLink));
    }

    public void DeleteButtonPressed()
    {
        AnimeList.i.RemoveAnime(currentAnime);
        currentAnime = null;
        BackToAnimeSelection();
    }

    public void DisplayButtons(int n)
    {
        displayState = n == 0 ? DisplayState.None : n == 1 && (int)displayState != 1 ? DisplayState.NotWatched : n == 2 && (int)displayState != 2 ? DisplayState.CurrentlyWatching : n == 3 && (int)displayState != 3 ? DisplayState.Finished : DisplayState.None;
        StartCoroutine(WaitForObjectToBeEnableForUpdate());
    }

    public void ModifyButtonPressed()
    {
        state = ControllerState.Modifying;
        AddMenu.SetActive(true);
        AnimeInfoMenu.SetActive(false);
        nameField.text = currentAnime.Name;
        synopsisField.text = currentAnime.Synopsis;
        bannerField.text = currentAnime.BannerLink;
        stateField.value = (int)currentAnime.State;
        errorText.text = "";
    }

    public void BackToAnimeSelection()
    {
        AnimeSelectionMenu.SetActive(true);
        AddMenu.SetActive(false);
        AnimeInfoMenu.SetActive(false);
        state = ControllerState.None;
    }

    public void Button_AddAnimeSubmit()
    {
        if (state == ControllerState.Adding)
            StartCoroutine(TryAddAnime());
        else if (state == ControllerState.Modifying)
            StartCoroutine(TryModifyAnime(currentAnime));
    }

    IEnumerator TryModifyAnime(Anime anime)
    {
        if (nameField.text == "")
        {
            errorText.text = "Can't add a new anime without having the name.";
            yield break;
        }
        else if (AnimeList.i.AlreadyHaveThisAnime(nameField.text) && nameField.text != currentAnime.Name)
        {
            errorText.text = "There's already an anime with the same name inside the database.";
            yield break;
        }

        if (bannerField.text == "")
        {
            errorText.text = "You must add a link for a banner to display it correctly.";
            yield break;
        }
        else
        {
            yield return IsLinkGood(bannerField.text);
            if (!isLinkGood)
            {
                errorText.text = "The link for the banner isn't working.";
                yield break;
            }
        }

        AnimeList.i.ModifyAnime(anime, new Anime(nameField.text, synopsisField.text, bannerField.text, stateField.value));
        BackToAnimeSelection();
    }

    IEnumerator TryAddAnime()
    {
        if (nameField.text == "")
        {
            errorText.text = "Can't add a new anime without having the name.";
            yield break;
        }
        else if (AnimeList.i.AlreadyHaveThisAnime(nameField.text))
        {
            errorText.text = "There's already an anime with the same name inside the database.";
            yield break;
        }

        if (bannerField.text == "")
        {
            errorText.text = "You must add a link for a banner to display it correctly.";
            yield break;
        }
        else
        {
            yield return IsLinkGood(bannerField.text);
            if (!isLinkGood)
            {
                errorText.text = "The link for the banner isn't working.";
                yield break;
            }
        }

        AnimeList.i.AddAnime(new Anime(nameField.text, synopsisField.text, bannerField.text, stateField.value));
        BackToAnimeSelection();
    }

    public void ResetErrorText()
    {
        errorText.text = "";
    }

    public void ResetAddMenu()
    {
        ResetErrorText();
        nameField.text = "";
        synopsisField.text = "";
        bannerField.text = "";
        stateField.value = 0;
        isLinkGood = false;
    }

    public void ResetAnimeInfoMenu()
    {
        nameText.text = "";
        synopsisText.text = "";
        stateText.text = "";
        banner.texture = null;
    }

    IEnumerator IsLinkGood(string MediaUrl)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(MediaUrl);
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError)
            isLinkGood = false;
        else
            isLinkGood = true;
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

public enum ControllerState
{
    None, Adding, Modifying
}

public enum DisplayState
{
    None, NotWatched, CurrentlyWatching, Finished
}