using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using System;
using UnityEditor.Experimental.GraphView;


public class Dialogue : MonoBehaviour
{
    public GameObject player;
    public GameObject npc;
    public GameObject dialogueBox;
    public TextMeshProUGUI textComponent;
    public string[] lines;
    public float textSpeed;
    private int index;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        textComponent.text = string.Empty;
        dialogueBox.SetActive(false);
        // StartDialogue();   
    }

    // Update is called once per frame
    void Update()
    {
        if ((dialogueBox.activeInHierarchy == false) && (Vector3.Distance(player.transform.position, npc.transform.position) < 3f))
        {
            dialogueBox.SetActive(true);
            StartDialogue();   
        }

        else if((dialogueBox.activeInHierarchy == true) && (Vector3.Distance(player.transform.position, npc.transform.position) < 3f))
        {
            if (Input.GetMouseButtonDown(0) && index != lines.Length - 1)
            {
                
                if (textComponent.text == lines[index])
                {
                    NextLine();
                }
                else
                {
                    StopAllCoroutines();
                    textComponent.text = lines[index];    
                } 
                
            if (Input.GetMouseButtonDown(0) && index == lines.Length)
                {
                    dialogueBox.SetActive(false);
                    textComponent.text = string.Empty;
                }
            }  
        }    
        else if ((dialogueBox.activeInHierarchy == true) && (Vector3.Distance(player.transform.position, npc.transform.position) >= 3f))
        {
            
            dialogueBox.SetActive(false);
            StopAllCoroutines();
            textComponent.text = string.Empty;
        }
            
    }
    
    void StartDialogue()
    {
        index = 0;
        StartCoroutine(TypeLine());
    }

    IEnumerator TypeLine()
    {
        foreach (char c in lines[index].ToCharArray())
        {
            textComponent.text += c;
            yield return new WaitForSeconds(textSpeed);
        }
    }

    void NextLine()
    {
        if (index < lines.Length - 1)
        {
            index++;
            textComponent.text = string.Empty;
            StartCoroutine(TypeLine());
        }
        else
        {
            dialogueBox.SetActive(false);
            textComponent.text = string.Empty;
           
        }   
    }
}
