using System.Collections.Generic;
using UnityEngine;

public class EvidenceManager : MonoBehaviour
{
    public static EvidenceManager Instance;

    public List<string> collectedEvidence = new List<string>();
    public Dictionary<string, float> evidenceWeights = new Dictionary<string, float>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddEvidence(string tagName, float weight)
    {
        collectedEvidence.Add(tagName);
        evidenceWeights[tagName] = weight;

        Debug.Log($"You found: {tagName} with weight {weight}");

        // In list
        string listStr = "Collected Evidence: ";
        foreach (var ev in collectedEvidence)
            listStr += ev + ", ";

        Debug.Log(listStr);

        // Tính tổng weight
        float totalWeight = CalculateTotalWeight();
        Debug.Log("Total Weight = " + totalWeight);
    }

    public float CalculateTotalWeight()
    {
        float total = 0f;
        foreach (var kv in evidenceWeights)
            total += kv.Value;

        return total;
    }

    public Dictionary<string, string> evidenceDescriptions = new Dictionary<string, string>()
    {
        { "HangPhone", "There’s a new message from Tấn that hasn’t been read: Why did you let May look at the document but not me? Do you hate me that much?" },
        { "LivingCorner", "The living corner in the shed includes: an old blanket, a pillow, a torn food bag, a water bottle, and a piece of clothing." },
        { "Ultimatum", "YOU ARE BEING WATCHED" },
        { "HangNoteBook", "Page 1: It seems the house has been broken into, Page 2: Mai knows the house too well, is she hiding something?" },
        { "Limit1", "The torn paper in Hang's notebook: I found a photo, and in the photo, there’s a girl who looks familiar." },
        { "Limit2", "May's Diary: That past... I don't want to remember... NO, I DIDN'T DO THAT... I DIDN'T MEAN IT... Hang investigates too fast, what should I do now?" }
    };

    public string GetEvidenceDescription(string name)
    {
        if (evidenceDescriptions.ContainsKey(name))
            return evidenceDescriptions[name];
        return "No description available.";
    }

}
