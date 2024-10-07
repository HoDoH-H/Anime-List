using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AnimeList : MonoBehaviour, ISavable
{
    [SerializeField] List<Anime> animes = new List<Anime> { };

    public static AnimeList i;

    private void Awake()
    {
        i = this;
    }

    public List<Anime> Animes => animes;

    public event Action OnUpdated;

    public object CaptureState()
    {
        var saveData = new SaveData()
        {
            animes = animes.Select(a => a.GetSaveData()).ToList(),
        };

        return saveData;
    }

    public List<Anime> GetAnimeList(DisplayState state)
    {
        if(state == DisplayState.None)
            return animes;

        List<Anime> newList = new List<Anime>();

        foreach (var anime in animes)
        {
            if ((int)anime.State == (int)state - 1)
                newList.Add(anime);
        }
        return newList;
    }

    public void RemoveAnime(Anime anime)
    {
        animes.Remove(anime);
        OnUpdated?.Invoke();
    }

    public void AddAnime(Anime anime)
    {
        animes.Add(anime);
        OnUpdated?.Invoke();
    }

    public void ModifyAnime(Anime baseAnime, Anime newDatas)
    {
        foreach(var anime in animes)
        {
            if(anime.Name == baseAnime.Name)
            {
                anime.SetNewData(newDatas);
            }
        }
    }

    public bool AlreadyHaveThisAnime(string name)
    {
        foreach (var anime in animes)
        {
            if (anime.Name == name) return true;
        }
        return false;
    }

    public void RestoreState(object state)
    {
        var saveData = (SaveData)state;
        animes.Clear();
        animes = saveData.animes.Select(a => new Anime(a)).ToList();
        OnUpdated?.Invoke();
    }
}

[System.Serializable]
public class SaveData
{
    public List<AnimeSaveData> animes;
}
