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
        { "Limit2", "Hang the Ghost: Hồn ma của Hang: the intruder... the one....10 years ago.....why me?" },
        { "Crack", "Why there is crack outside this room?"},
        {  "StrangeTable", "Four chairs around the table, three of them fall, one of them stand."},
        { "OpenWindow", "The attic is locked for long time, who open the window? It is opened all the time?"},
        { "Rope", "No rope at the scene, but rope in the attic..."},
        { "Limit3", "Mai's Diary: It’s definitely her—there’s no way I could be mistaken… I can’t believe she’s still alive. But why doesn’t she recognize me? Could it be that she’s hiding something… or doesn’t want to talk about the past? Poor thing. Back then, she had to grow up in a family like that… Maybe I should keep quiet too. I promise I won’t ever leave you alone again."},
        {"Limit4", "Photo of a family of four. There is a little girl...so familiar...wait, is that May???"},
        {"Limit5", "That past—I don’t want to remember it, no, I didn’t do that, I didn’t mean to, but Dad, Mom and my brother, Mom was wrong, Dad was wrong too, AAAAAAAAAAAAA, Hang… she has to d..."},
        {"Limit6", "My child… she is not evil……She was gentle… once…But her mind……was broken.…Not by strangers…Not by ghosts…By the man… she called father...Love became fear…Fear became obedience…She guards the house…Because she believes…If the truth escapes……she deserves to disappear"}

    };

    public string GetEvidenceDescription(string name)
    {
        if (evidenceDescriptions.ContainsKey(name))
            return evidenceDescriptions[name];
        return "No description available.";
    }

    public bool HasEvidence(string tagName)
    {
        return collectedEvidence.Contains(tagName);
    }


}
