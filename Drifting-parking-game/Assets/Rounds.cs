using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

public class Rounds : MonoBehaviour
{
    public GameObject platform;
    public GameObject menu;
    public GameObject explosionEffect;
    public GameObject player;
    public List<GameObject> cars;
    public GameObject[] allSpots;

    int currentRound = 0;
    bool parking = false;
    bool roundEnding = false;
    float timer;
    Vector3[] startPos;
    Quaternion[] startRot;

    void Start()
    {
        platform.SetActive(false);
        menu.SetActive(false);
        startPos = new Vector3[cars.Count];
        startRot = new Quaternion[cars.Count];
        for (int i = 0; i < cars.Count; i++)
        {
            startPos[i] = cars[i].transform.position;
            startRot[i] = cars[i].transform.rotation;
        }
    }

    void Update()
    {
        if (!parking)
        {
            timer += Time.deltaTime;
            if (timer > 8f && Random.value < 0.01f)
                StartParking();
        }
        else if (!roundEnding && AllSpotsTaken())
            EndRound();
        
    }

    void StartParking()
    {
        parking = true;
        timer = 0f;
        platform.SetActive(true);
        for (int i = 0; i < allSpots.Length; i++)
            allSpots[i].SetActive(i >= currentRound);
    }

    bool AllSpotsTaken()
    {
        foreach (GameObject s in allSpots)
        {
            if (!s.activeInHierarchy) continue;
            if (!(bool)Variables.Object(s).Get("Taken")) return false;
        }
        return true;
    }

    void EndRound()
    {
        roundEnding = true;
        bool playerDied = false;
        foreach (GameObject c in cars)
        {
            if (c == null || !c.activeSelf) continue;
            Rigidbody crb = c.GetComponent<Rigidbody>();
            if (crb != null && !crb.isKinematic)
            {
                if (c == player) playerDied = true;
                Explode(c);
            }
        }
        if (playerDied) Invoke("ShowMenu", 2.5f);
        else Invoke("NextRound", 2.5f);
    }

    void ShowMenu() { menu.SetActive(true); }

    void Explode(GameObject c)
    {
        BotAI bot = c.GetComponent<BotAI>();
        if (bot != null) bot.enabled = false;
        PlayerRecovery pr = c.GetComponent<PlayerRecovery>();
        if (pr != null) pr.enabled = false;
        PrometeoCarController ctrl = c.GetComponent<PrometeoCarController>();
        if (ctrl != null) ctrl.enabled = false;

        Rigidbody crb = c.GetComponent<Rigidbody>();
        if (explosionEffect != null)
           Instantiate(explosionEffect, c.transform.position, Quaternion.identity);
        crb.isKinematic = false;
        crb.AddForce(Vector3.up * 8f + Random.insideUnitSphere * 4f, ForceMode.VelocityChange);
        crb.AddTorque(Random.insideUnitSphere * 20f, ForceMode.VelocityChange);
        StartCoroutine(HideAfter(c, 2f));
    }

    IEnumerator HideAfter(GameObject c, float t)
    {
        yield return new WaitForSeconds(t);
        if (c != null) c.SetActive(false);
    }

    void NextRound()
    {
        int alive = 0;
        foreach (GameObject c in cars) if (c != null && c.activeSelf) alive++;
        if (alive <= 1) { ShowMenu(); return; }

        currentRound++;
        platform.SetActive(false);
        foreach (GameObject s in allSpots)
            Variables.Object(s).Set("Taken", false);

        for (int i = 0; i < cars.Count; i++)
        {
            GameObject c = cars[i];
            if (c == null || !c.activeSelf) continue;
            c.transform.position = startPos[i];
            c.transform.rotation = startRot[i];
            Rigidbody crb = c.GetComponent<Rigidbody>();
            crb.isKinematic = false;
            crb.linearVelocity = Vector3.zero;
            crb.angularVelocity = Vector3.zero;
            BotAI bot = c.GetComponent<BotAI>();
            if (bot != null) bot.NewRound();
        }

        roundEnding = false;
        parking = false;
    }

    public void RestartGame()
    {
        menu.SetActive(false);
        currentRound = 0;
        platform.SetActive(false);
        parking = false;
        roundEnding = false;

        foreach (GameObject s in allSpots)
            Variables.Object(s).Set("Taken", false);

        for (int i = 0; i < cars.Count; i++)
        {
            GameObject c = cars[i];
            if (c == null) continue;
            c.SetActive(true);
            c.transform.position = startPos[i];
            c.transform.rotation = startRot[i];
            Rigidbody crb = c.GetComponent<Rigidbody>();
            crb.isKinematic = false;
            crb.linearVelocity = Vector3.zero;
            crb.angularVelocity = Vector3.zero;
            BotAI bot = c.GetComponent<BotAI>();
            if (bot != null) { bot.enabled = true; bot.NewRound(); }
            PlayerRecovery pr = c.GetComponent<PlayerRecovery>();
            if (pr != null) pr.enabled = true;
            PrometeoCarController ctrl = c.GetComponent<PrometeoCarController>();
            if (ctrl != null) ctrl.enabled = true;
        }
    }
}