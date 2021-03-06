﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance { get { return instance; } }

    // --- UI Elements ---
    public CoinTextController CoinageText;
    public HeartController hearts;

    // --- END UI Elements ---

    public DungeonController dungeonController;

    public List<int> items = new List<int>();
    public bool debug = false;


    // --- Player Stats ---
    public int maxHealth = 3;
    public int MaxHealth { get { return maxHealth; } set
        {
            maxHealth = value;
            if (hearts != null)
            {
                hearts.CreateHearts();
            }
        } }

    public HelmetController equippedHelmet;

    public TorsoController equippedTorso;
    
    public int equippedWeapon;
    public bool inDungeon = false;
    
    // --- END Plyer Stats ---




    // dungeonFloors = DungeonGenerator.generateFloors(random parameters, 3)

    public void transitionToDungeonScene()
    {
        dungeonController.generateDungeon(maxHealth + items.Count);
        SceneManager.LoadScene(2);
        hearts = FindObjectOfType<HeartController>();
        inDungeon = true;
    }
    public void transitionToCampScene()
    {
        SceneManager.LoadScene(0);
        inDungeon = false;
    }

    public void TransitionToWinScene()
    {
        SceneManager.LoadScene(1);
    }


    public int value;
    public int Value { get { return value; } set {
            this.value = value;
            if (CoinageText != null) CoinageText.textElement.text = this.value.ToString();
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            dungeonController = new DungeonController();
            GameObject.DontDestroyOnLoad(gameObject);
            if (debug) value = 10000;
            SceneManager.sceneLoaded += SceneLoaded;
        }
    }

    public void SceneLoaded(Scene scene, LoadSceneMode mode)
    {
        hearts = FindObjectOfType<HeartController>(true);
        CoinageText = FindObjectOfType<CoinTextController>(true);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= SceneLoaded;
    }


    public void AddValue(int value)
    {
        Value += value;
        if (CoinageText == null) return;
        CoinageText.textElement.text = Value.ToString();
    }

    public void AddItem(AbsItem item)
    {
        if (items.Contains(item.GetItemId())) return;
        items.Add(item.GetItemId());
        Value -= item.cost;
        if (HasAllWinItems()) TransitionToWinScene();
        if (CoinageText == null) return;
        CoinageText.textElement.text = Value.ToString();
    }

    public bool HasBoughtItem(AbsItem item)
    {
        return Instance.items.Contains(item.GetItemId());
    }

    public bool HasAllWinItems()
    {
        return items.Contains(200) && items.Contains(201) && items.Contains(202);
    }
}
