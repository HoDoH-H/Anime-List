using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Anime
{
    [SerializeField] string name;
    [SerializeField] string synopsis;
    [SerializeField] string bannerLink;
    [SerializeField] AnimeState state;

    public string Name => name;
    public string Synopsis => synopsis;
    public string BannerLink => bannerLink;
    public AnimeState State => state;

    public Anime(AnimeSaveData data)
    {
        this.name = data.Name;
        synopsis = data.Synopsis;
        bannerLink = data.BannerLink;
        state = data.State;
    }

    public Anime(string name, string synopsis = "", string bannerLink = "", int state = 0)
    {
        this.name = name;
        this.synopsis = synopsis;
        this.bannerLink = bannerLink;
        this.state = state == 0 ? AnimeState.NotWatched: state == 1 ? AnimeState.CurrentlyWatching: AnimeState.Finished;
    }

    public void SetNewData(Anime anime)
    {
        this.name = anime.Name;
        this.synopsis = anime.Synopsis;
        this.state = anime.State;
        this.bannerLink = anime.BannerLink;
    }

    public AnimeSaveData GetSaveData()
    {
        var saveData = new AnimeSaveData()
        {
            Name = name,
            Synopsis = synopsis,
            BannerLink = bannerLink,
            State = state,
        };
        return saveData;
    }
}

public enum AnimeState
{
    NotWatched,
    CurrentlyWatching,
    Finished
}

[System.Serializable]
public class AnimeSaveData
{
    public string Name;
    public string Synopsis;
    public string BannerLink;
    public AnimeState State;
}
