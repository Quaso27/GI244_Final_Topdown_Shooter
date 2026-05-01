using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class AugmentManager : MonoBehaviour
{
    public static AugmentManager instance;

    // ตัวแปรสำคัญสำหรับเช็คสถานะการเปิดเมนู
    public bool isMenuOpen = false;

    [Header("Augment Library")]
    public List<AugmentCard> allAugments;

    [Header("UI References")]
    public GameObject augmentCanvas;
    public Transform cardContainer;
    public GameObject augmentButtonPrefab;

    [Header("Settings")]
    public int wavesPerAugment = 1;
    public int selectionCount = 2;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        if (augmentCanvas != null) augmentCanvas.SetActive(false);
    }

    public void OnWaveCleared(int wave)
    {
        if (wave % wavesPerAugment == 0)
        {
            StartAugmentSelection();
        }
    }

    public void StartAugmentSelection()
    {
        isMenuOpen = true; // ตั้งสถานะว่าเมนูเปิดอยู่
        Time.timeScale = 0f;

        if (augmentCanvas != null) augmentCanvas.SetActive(true);

        foreach (Transform child in cardContainer) Destroy(child.gameObject);

        List<AugmentCard> pool = new List<AugmentCard>(allAugments);
        int cardsToSpawn = Mathf.Min(selectionCount, pool.Count);

        for (int i = 0; i < cardsToSpawn; i++)
        {
            int randomIndex = Random.Range(0, pool.Count);
            CreateAugmentUI(pool[randomIndex]);
            pool.RemoveAt(randomIndex);
        }
    }

    void CreateAugmentUI(AugmentCard data)
    {
        GameObject btnObj = Instantiate(augmentButtonPrefab, cardContainer);
        try
        {
            btnObj.transform.Find("Icon").GetComponent<Image>().sprite = data.icon;
            btnObj.transform.Find("NameText").GetComponent<TextMeshProUGUI>().text = data.augmentName;
            btnObj.transform.Find("DescText").GetComponent<TextMeshProUGUI>().text = data.description;
        }
        catch { Debug.LogError("UI Components missing in Prefab"); }

        btnObj.GetComponent<Button>().onClick.AddListener(() => SelectAugment(data));
    }

    void SelectAugment(AugmentCard data)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) data.ApplyEffect(player);

        if (augmentCanvas != null) augmentCanvas.SetActive(false);

        Time.timeScale = 1f;
        isMenuOpen = false; // ปิดเมนูเพื่อให้ Spawner ทำงานต่อ
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            if (!augmentCanvas.activeSelf) StartAugmentSelection();
        }
    }
}