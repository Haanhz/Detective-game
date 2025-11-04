using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreBoard : MonoBehaviour
{   
    
    public static int scoreValue;
    public TMP_Text score;

    void Start()
    {
        scoreValue = 0;
    }

    void Update()
    {
        score.text = ""+ scoreValue.ToString();
    }
    
}
