using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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

    // Dictionary to map UIDs to specific names
    private Dictionary<string, string> uidToNameMap = new Dictionary<string, string>()
    {
        { "4:15:27:5:bc:2a:81", "Eric" },
        { "4:84:2c:5:bc:2a:81", "Kaye" },
        { "4:d5:f2:4:bc:2a:81", "Carl" },
        { "4:33:ea:4:bc:2a:81", "Aubrey" },
        { "4:81:fe:4:bc:2a:81", "Dan" },
        { "4:45:15:5:bc:2a:81", "JP" },
        { "4:e5:1a:5:bc:2a:81", "Paulo" },
        { "4:88:21:5:bc:2a:81", "Aira" },
        { "4:b1:f8:4:bc:2a:81", "Joenicel" },
        { "4:c6:ee:4:bc:2a:81", "Rey" }
    };

    private void Awake()
    {
        entryHolder = entryHolderObj.transform;
        entryTemplate = entryTemplateObj.transform;

        InitializeEntries();
        entryTemplate.gameObject.SetActive(false);

        UpdateUI();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            AddNewUser();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            AddScoreToUser(1, 100);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            AddScoreToUser(2, 100);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            AddScoreToUser(3, 100);
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

    private void AddScoreToUser(int userIndex, int scoreToAdd)
    {
        if (userMap.ContainsKey(userIndex))
        {
            SingleEntry userEntry = userMap[userIndex];
            userEntry.score += scoreToAdd;

            entries.Sort((a, b) => b.score.CompareTo(a.score));

            UpdateUI(userEntry); // Highlight updated entry
        }
        else
        {
            Debug.LogError("User index out of range.");
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
        // Use Unity's JsonUtility for deserialization
        LeaderboardData data = JsonUtility.FromJson<LeaderboardData>(jsonData);

        // Convert NFC_ID to the expected format (replacing ':' with '')
        string formattedNfcId = data.NFC_ID.Replace(":", "").ToLower();

        // Find the existing entry by the NFC ID
        var existingEntry = entries.Find(e => e.nfcId.ToLower() == formattedNfcId);

        if (existingEntry != null)
        {
            // Update the score if the new score is higher
            if (data.Score > existingEntry.score)
            {
                existingEntry.score = data.Score;
            }
        }
        else
        {
            // Assign a name based on the formatted NFC_ID
            string userName;
            if (uidToNameMap.TryGetValue(formattedNfcId, out userName))
            {
                // Create a new entry if it doesn't exist
                SingleEntry newEntry = new SingleEntry
                {
                    nfcId = formattedNfcId,
                    name = userName,
                    fixture = int.Parse(data.Station), // Convert Station to int
                    score = data.Score
                };
                entries.Add(newEntry);
            }
            else
            {
                // Handle unknown users
                SingleEntry newEntry = new SingleEntry
                {
                    nfcId = formattedNfcId,
                    name = "Unknown User", // Fallback for undefined users
                    fixture = int.Parse(data.Station),
                    score = data.Score
                };
                entries.Add(newEntry);
            }
        }

        // Sort and update the UI
        entries.Sort((a, b) => b.score.CompareTo(a.score));
        UpdateUI(existingEntry ?? entries[entries.Count - 1]);
    }

    // Update UI and highlight specific entry
    private void UpdateUI(SingleEntry updatedEntry = null)
    {
        foreach (Transform child in entryHolder)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < Mathf.Min(8, entries.Count); i++)
        {
            SingleEntry entry = entries[i];
            Transform entryTransform = Instantiate(entryTemplate, entryHolder);
            entryTransform.gameObject.SetActive(true);

            Image entryImage = entryTransform.GetComponent<Image>();
            switch (i + 1)
            {
                default: break;
                case 1:
                    entryImage.color = new Color(1f, 0.84f, 0f, 100f / 255f);
                    break;
                case 2:
                    entryImage.color = new Color(192f / 255f, 192f / 255f, 192f / 255f, 100f / 255f);
                    break;
                case 3:
                    entryImage.color = new Color(0.8f, 0.52f, 0.25f, 100f / 255f);
                    break;
            }

            entryTransform.GetChild(0).GetComponent<TMP_Text>().text = (i + 1).ToString();
            entryTransform.GetChild(1).GetComponent<TMP_Text>().text = entry.name;
            entryTransform.GetChild(2).GetComponent<TMP_Text>().text = entry.fixture.ToString();
            entryTransform.GetChild(3).GetComponent<TMP_Text>().text = entry.score.ToString();

            // Highlight the updated entry
            if (entry == updatedEntry)
            {
                StartCoroutine(HighlightEntryWithLerp(entryTransform));
            }
        }
    }

    // Coroutine for smooth scaling using Lerp
    private IEnumerator HighlightEntryWithLerp(Transform entryTransform)
    {
        Vector3 originalScale = entryTransform.localScale;
        Vector3 targetScale = originalScale * 1.1f;
        float duration = 0.3f; // Time to scale up

        // Scale up
        float time = 0f;
        while (time < duration)
        {
            entryTransform.localScale = Vector3.Lerp(originalScale, targetScale, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        entryTransform.localScale = targetScale;

        // Wait for 3 seconds before scaling back
        yield return new WaitForSeconds(1);

        // Scale back down
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
        public string NFC_ID; // Format: "4:c6:ee:4:bc:2a:81"
        public string Station; // This corresponds to the fixture
        public int Score; // The score to update
    }
}
