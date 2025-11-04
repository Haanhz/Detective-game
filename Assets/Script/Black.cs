using System.Collections;
using System.Collections.Generic;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.EventSystems;

public class Black : MonoBehaviour
{
    public Rigidbody2D rb;
    public void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnCollisionEnter2D(Collision2D other) 
    {
        if (other.gameObject.tag == "Player")
        {
            ChaseManager.instance.player.gameObject.SetActive(false);
            ChaseManager.instance.player.dead = true;
            Debug.Log("You died!");
        }
    }
}