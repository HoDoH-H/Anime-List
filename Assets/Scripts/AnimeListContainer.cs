using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class AnimeListContainer : MonoBehaviour
{
    [SerializeField] GameObject containerPrefab;

    private List<GameObject> containers = new List<GameObject> ();

    public void Print(Anime anime)
    {
        var container = Instantiate(containerPrefab, this.transform);
        container.GetComponent<AnimeContainer>().Anime = anime;
        container.GetComponent<AnimeContainer>().Init();
        containers.Add(container);
    }

    public void DoReset()
    {
        foreach (var container in containers)
        {
            Destroy(container);
        }
        containers = new List<GameObject>();
    }
}
