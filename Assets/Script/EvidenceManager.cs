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
        { "HangPhone", "Điện thoại của Hang, tìm thấy gần giếng." }
    };

    public string GetEvidenceDescription(string name)
    {
        if (evidenceDescriptions.ContainsKey(name))
            return evidenceDescriptions[name];
        return "No description available.";
    }

}
