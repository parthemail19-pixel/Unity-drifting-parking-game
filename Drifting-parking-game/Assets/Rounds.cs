using UnityEngine;
using System.Collections.Generic;

public class Rounds : MonoBehaviour
{
    public GameObject platform;
    public List<GameObject> cars;
    public GameObject[] allSpots;

    int currentRound = 0;
    bool parking = false;

    void Start()
    {
        platform.SetActive(false);
    }

    void Update()
    {
        if (!parking && Random.value < 0.005f)
            StartParking();
    }

    void StartParking()
    {
        parking = true;
        platform.SetActive(true);
        for (int i = 0; i < allSpots.Length; i++)
            allSpots[i].SetActive(i >= currentRound);
    }
}