using UnityEngine;

public class Singleton : MonoBehaviour
{
    public static Singleton Instance { get; private set; }
    public string jsonData; // Store received JSON data

    private void Awake() {
        if (Instance == null) {
            Instance = this; // Singleton instance
            DontDestroyOnLoad(gameObject); // Persist through scene loads
        }
        else {
            Destroy(gameObject); // Destroy duplicate instances
        }
    } //-- Awake end
}
