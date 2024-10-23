using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LeaderboardManager : MonoBehaviour
{
    //----- Variables
    public Transform entryHolder; // Holder for the entries
    public Transform entryTemplate; // Template for each entry

    //----- Functions

    void Awake()
    {
        entryTemplate.gameObject.SetActive(false);

        // Create a list to hold entries
        List<SingleEntry> entries = new List<SingleEntry>();

        // Populate entries with random scores
        for (int i = 0; i < 8; i++)
        {
            int score = Random.Range(0, 9999);
            // Populate the entry with name, fixture, and score
            entries.Add(new SingleEntry
            {
                name = "Rey", // You can customize this as needed
                fixture = 5,  // You can customize this as needed
                score = score
            });
        }

        // Sort the entries by score in descending order
        entries.Sort((a, b) => b.score.CompareTo(a.score));

        // Instantiate UI elements based on the sorted entries
        for (int i = 0; i < entries.Count; i++)
        {
            SingleEntry entry = entries[i];
            Transform entryTransform = Instantiate(entryTemplate, entryHolder);
            RectTransform entryRectTransform = entryTransform.GetComponent<RectTransform>();
            entryTransform.gameObject.SetActive(true);

            // Set ranking color
            Image entryImage = entryTransform.GetComponent<Image>();
            switch (i + 1)
            { // Update color based on the rank
                default: break;
                case 1:
                    entryImage.color = new Color(1f, 0.84f, 0f, 100f / 255f); // Gold
                    break;
                case 2:
                    entryImage.color = new Color(192f / 255f, 192f / 255f, 192f / 255f, 100f / 255f); // Silver
                    break;
                case 3:
                    entryImage.color = new Color(0.8f, 0.52f, 0.25f, 100f / 255f); // Bronze
                    break;
            }

            // Text Displays
            entryTransform.GetChild(0).GetComponent<TMP_Text>().text = (i + 1).ToString(); // Rank
            entryTransform.GetChild(1).GetComponent<TMP_Text>().text = entry.name; // Name
            entryTransform.GetChild(2).GetComponent<TMP_Text>().text = entry.fixture.ToString(); // Fixture
            entryTransform.GetChild(3).GetComponent<TMP_Text>().text = entry.score.ToString(); // Score
        }
    } //-- Awake end

    private class SingleEntry
    {
        public string name;   // Name of the entry
        public int fixture;   // Fixture number
        public int score;     // Score
    } //-- SingleEntry end
}

/*
Project Name : LUT Leaderboard
Created by   : Sir Reyyy
*/
