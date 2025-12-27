using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class Black : MonoBehaviour
{
    public Rigidbody2D rb;
    public AudioSource audioSource;
    public AudioClip killSound;

    public void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnCollisionEnter2D(Collision2D other) 
    {
        if (other.gameObject.tag == "Player")
        {
            ChaseManager.instance.player.dead = true;
            ChaseManager.instance.player.gameObject.SetActive(false);
            ScoreBoard.scoreValue = 0;
            Debug.Log("You died!");
            if (audioSource != null && killSound != null)
            {
                audioSource.PlayOneShot(killSound);
            }
        }
    }
    
   
}