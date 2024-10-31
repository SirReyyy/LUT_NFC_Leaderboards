using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LeaderboardManager : MonoBehaviour
{
    public GameObject entryHolderObj; // Holder for the entries
    public GameObject entryTemplateObj; // Template for each entry
    private Transform entryHolder;
    private Transform entryTemplate;
    private List<SingleEntry> entries = new List<SingleEntry>();



    private Dictionary<int, SingleEntry> userMap = new Dictionary<int, SingleEntry>(); // Map for user references
    private int newUserCount = 0; // Counter for dynamically created users

    // Dictionary to map UIDs to specific names, populated from UID_NFC.json
    private Dictionary<string, string> uidToNameMap = new Dictionary<string, string>();


    private void Awake()
    {
        entryHolder = entryHolderObj.transform;
        entryTemplate = entryTemplateObj.transform;

        LoadUIDMappings(); // Load UID.json and populate uidToNameMap
        InitializeEntries();
        entryTemplate.gameObject.SetActive(false);

        UpdateUI();
    }

    private void LoadUIDMappings()
    {
        // Debug.Log("Loading UID mappings...");

        string path = Path.Combine(Application.streamingAssetsPath, "UID_NFC.json");

        if (File.Exists(path))
        {
            string jsonContent = File.ReadAllText(path);
            // Debug.Log("JSON Content: " + jsonContent); // Log the raw JSON content
            UIDMappings uidMappings = JsonUtility.FromJson<UIDMappings>(jsonContent);

            foreach (var mapping in uidMappings.uidToNameMap)
            {
                string formattedUID = mapping.UID.Trim().Replace(":", "").ToLower();
                uidToNameMap[formattedUID] = mapping.Name;
                // Debug.Log($"Loaded UID: {formattedUID} with Name: {mapping.Name}");
            }
        }
        else
        {
            Debug.LogError("UID_NFC.json file not found at path: " + path);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            AddNewUser();
        }

        for (int i = 1; i <= 8; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i)) // Using KeyCode Alpha1 - Alpha8
            {
                AddScoreToPosition(i, 100); // Call the method based on the current leaderboard position
            }
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    private void AddScoreToPosition(int leaderboardPosition, int scoreToAdd)
    {
        if (leaderboardPosition <= entries.Count && leaderboardPosition > 0)
        {
            SingleEntry entryAtPosition = entries[leaderboardPosition - 1]; // Get entry at specified position (1-based to 0-based index)
            entryAtPosition.score += scoreToAdd;

            // Re-sort and update UI to reflect any rank changes
            entries.Sort((a, b) => b.score.CompareTo(a.score));
            UpdateUI(entryAtPosition); // Highlight updated entry
        }
        else
        {
            Debug.LogError("Position out of leaderboard range.");
        }
    }


    private void InitializeEntries()
    {
        for (int i = 0; i < 8; i++)
        {
            SingleEntry entry = new SingleEntry
            {
                nfcId = "",
                name = "User " + (i + 1),
                fixture = 0,
                score = 0
            };
            entries.Add(entry);
            userMap.Add(i + 1, entry);
        }
    }

    private void AddNewUser()
    {
        newUserCount++;
        SingleEntry newEntry = new SingleEntry
        {
            nfcId = "new_" + newUserCount,
            name = "New User " + newUserCount,
            fixture = 0,
            score = 100
        };

        entries.Add(newEntry);

        entries.Sort((a, b) => b.score.CompareTo(a.score));

        UpdateUI(newEntry); // Highlight the new entry
    }

    public void UpdateLeaderboard(string jsonData)
    {
        LeaderboardData data = JsonUtility.FromJson<LeaderboardData>(jsonData);
        string formattedNfcId = data.NFC_ID.Replace(":", "").ToLower();

        Debug.Log("Received UID: " + formattedNfcId);

        // Check for an existing entry
        var existingEntry = entries.Find(e => e.nfcId.ToLower() == formattedNfcId);

        if (existingEntry != null)
        {
            // Update score if new score is greater or simply add score incrementally
            existingEntry.score += data.Score; // Increment the score
            entries.Sort((a, b) => b.score.CompareTo(a.score));

            UpdateUI(existingEntry); // Highlight updated entry
        }
        else
        {
            // If UID is not found, add a new entry
            string userName = GetNameByUID(formattedNfcId);

            SingleEntry newEntry = new SingleEntry
            {
                nfcId = formattedNfcId,
                name = userName,
                fixture = int.Parse(data.Station),
                score = data.Score
            };
            entries.Add(newEntry);

            entries.Sort((a, b) => b.score.CompareTo(a.score));
            UpdateUI(newEntry); // Highlight the new entry
        }
    }


    private string GetNameByUID(string uid)
    {
        if (uidToNameMap.TryGetValue(uid, out string name))
        {
            return name;
        }
        return "Unknown User"; // Default name if UID is not found
    }

    private void UpdateUI(SingleEntry updatedEntry = null)
    {
        foreach (Transform child in entryHolder)
        {
            Destroy(child.gameObject);
        }

        int currentRank = 1; // Start ranking from 1
        int displayedRank = 1; // Rank displayed in the UI, accounting for ties

        for (int i = 0; i < Mathf.Min(8, entries.Count); i++)
        {
            SingleEntry entry = entries[i];
            Transform entryTransform = Instantiate(entryTemplate, entryHolder);
            entryTransform.gameObject.SetActive(true);

            Image entryImage = entryTransform.GetComponent<Image>();
            Transform entryRanking_0 = entryTransform.GetChild(0).GetChild(0);
            Transform entryRanking_1 = entryTransform.GetChild(0).GetChild(1);
            Transform entryRanking_2 = entryTransform.GetChild(0).GetChild(2);

            // Determine if the entry has a valid score for ranking
            if (entry.score > 0)
            {
                // Assign color and symbol based on displayedRank
                switch (displayedRank)
                {
                    case 1:
                        entryImage.color = new Color(1f, 0.84f, 0f, 100f / 255f);
                        entryRanking_0.gameObject.SetActive(true);
                        break;
                    case 2:
                        entryImage.color = new Color(192f / 255f, 192f / 255f, 192f / 255f);
                        entryRanking_1.gameObject.SetActive(true);
                        break;
                    case 3:
                        entryImage.color = new Color(0.8f, 0.52f, 0.25f, 100f / 255f);
                        entryRanking_2.gameObject.SetActive(true);
                        break;
                    default:
                        break;
                }

                // Display rank number
                entryTransform.GetChild(0).GetComponent<TMP_Text>().text = displayedRank.ToString();

                // Increment displayed rank for next valid entry
                if (i < entries.Count - 1 && entry.score != entries[i + 1].score)
                {
                    currentRank++;
                    displayedRank = currentRank;
                }
            }
            else
            {
                // If score is 0 or less, set the rank display to blank
                entryTransform.GetChild(0).GetComponent<TMP_Text>().text = ""; // Blank rank display
            }

            // Display user details
            entryTransform.GetChild(1).GetComponent<TMP_Text>().text = entry.name;
            entryTransform.GetChild(2).GetComponent<TMP_Text>().text = entry.fixture.ToString();
            entryTransform.GetChild(3).GetComponent<TMP_Text>().text = entry.score.ToString();

            // Highlight if this is the updated entry
            if (entry == updatedEntry)
            {
                StartCoroutine(HighlightEntryWithLerp(entryTransform));
            }
        }
    }


    private IEnumerator HighlightEntryWithLerp(Transform entryTransform)
    {
        Vector3 originalScale = entryTransform.localScale;
        Vector3 targetScale = originalScale * 1.1f;
        float duration = 0.3f;

        float time = 0f;
        while (time < duration)
        {
            entryTransform.localScale = Vector3.Lerp(originalScale, targetScale, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        entryTransform.localScale = targetScale;

        yield return new WaitForSeconds(1);

        time = 0f;
        while (time < duration)
        {
            entryTransform.localScale = Vector3.Lerp(targetScale, originalScale, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        entryTransform.localScale = originalScale;
    }

    [System.Serializable]
    private class SingleEntry
    {
        public string nfcId;
        public string name;
        public int fixture;
        public int score;
    }

    [System.Serializable]
    private class LeaderboardData
    {
        public string NFC_ID;
        public string Station;
        public int Score;
    }

    [System.Serializable]
    private class UIDMappings
    {
        public List<UIDMapping> uidToNameMap;
    }

    [System.Serializable]
    private class UIDMapping
    {
        public string UID; // Should match the JSON key "UID"
        public string Name; // Should match the JSON key "Name"
    }
}