using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelUIMapping : MonoBehaviour
{
    [Header("Prefab & Parent")]
    [SerializeField] private Transform levelParent;       // Tempat spawn prefab
    [SerializeField] private GameObject levelPrefabs;     // Prefab Level
    [SerializeField] private float xOffset = 300f;        // Jarak antar level

    [Header("Manual References")]
    [SerializeField] private UIManagers uiManagers;
    [SerializeField] private Animator levelSelectAnimator;
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private Texture defaultLevelClip;

    [Header("Navigation Buttons")]
    [SerializeField] private GameObject nextButton;
    [SerializeField] private GameObject previousButton;
    [Header("UI Selected Level")]
    [SerializeField] private TMP_Text selectedLevelText;

    [System.Serializable]
    public class LevelUIData
    {
        public string levelName;
        public Texture levelClip;
        public string sceneName;
    }

    [SerializeField] private List<LevelUIData> levelUIDataList;

    private List<GameObject> spawnedLevels = new List<GameObject>();
    private int selectedLevelIndex = 0;

    void Start()
    {
        MappingDataToUIObjects();
    }

    public void MappingDataToUIObjects()
    {
        // Bersihkan semua level sebelumnya
        foreach (Transform child in levelParent)
        {
            Destroy(child.gameObject);
        }

        spawnedLevels.Clear();

        for (int i = 0; i < levelUIDataList.Count; i++)
        {
            LevelUIData data = levelUIDataList[i];

            GameObject levelInstance = Instantiate(levelPrefabs, levelParent, false);
            levelInstance.name = $"Level_{data.levelName}_{i}";
            spawnedLevels.Add(levelInstance);

            // Posisi awal akan diatur oleh UpdateLevelPositions()

            // Ambil komponen UI
            TMP_Text levelNameText = levelInstance.transform.Find("Level Name").GetComponent<TMP_Text>();
            RawImage clipImage = levelInstance.GetComponent<RawImage>();
            Button selectButton = levelInstance.transform.Find("Select Level").GetComponent<Button>();

            // Isi data
            levelNameText.text = data.levelName;
            clipImage.texture = data.levelClip != null ? data.levelClip : defaultLevelClip;

            if (data.levelClip == null)
            {
                Debug.LogWarning($"Level clip for {data.levelName} is null, using default texture.");
            }

            int currentIndex = i;
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(() => uiManagers.ChangeScene(data.sceneName));
            selectButton.onClick.AddListener(() => levelSelectAnimator.SetTrigger("Close"));
            selectButton.onClick.AddListener(() => mainMenu.SetActive(false));
            selectButton.onClick.AddListener(() => UpdateSelectedLevelUI(currentIndex));
        }

        UpdateLevelPositions();
        UpdateSelectedLevelUI(selectedLevelIndex);
    }

    private void UpdateLevelPositions()
    {
        for (int i = 0; i < spawnedLevels.Count; i++)
        {
            RectTransform rt = spawnedLevels[i].GetComponent<RectTransform>();
            if (rt != null)
            {
                float offsetX = (i - selectedLevelIndex) * xOffset;
                rt.anchoredPosition = new Vector2(offsetX, rt.anchoredPosition.y);
            }
        }
    }

    private void UpdateSelectedLevelUI(int index)
    {
        selectedLevelIndex = index;

        if (index >= 0 && index < levelUIDataList.Count)
        {
            selectedLevelText.text = $"Selected: {levelUIDataList[index].levelName}";
        }
        else
        {
            selectedLevelText.text = "Selected: None";
        }
        if (previousButton != null)
            previousButton.SetActive(selectedLevelIndex > 0);

        if (nextButton != null)
            nextButton.SetActive(selectedLevelIndex < levelUIDataList.Count - 1);
        UpdateLevelPositions(); // Update posisi setelah berubah
    }

    public void NextLevel()
    {
        if (selectedLevelIndex < levelUIDataList.Count - 1)
        {
            selectedLevelIndex++;
            UpdateSelectedLevelUI(selectedLevelIndex);
        }
    }

    public void PreviousLevel()
    {
        if (selectedLevelIndex > 0)
        {
            selectedLevelIndex--;
            UpdateSelectedLevelUI(selectedLevelIndex);
        }
    }

    public int GetLevelCount()
    {
        return levelUIDataList.Count;
    }

    public int GetSelectedLevelIndex()
    {
        return selectedLevelIndex;
    }

    public LevelUIData GetSelectedLevelData()
    {
        return (selectedLevelIndex >= 0 && selectedLevelIndex < levelUIDataList.Count)
            ? levelUIDataList[selectedLevelIndex]
            : null;
    }
}
