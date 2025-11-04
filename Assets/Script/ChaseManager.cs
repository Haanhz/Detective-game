using System.Collections;
using UnityEngine;

public class ChaseManager : MonoBehaviour
{
    public Player player;
    public Black black;
    public float probAppear;
    public float timeAtNight = 0f;
    public float nightLength = 300f;
    public bool isNight = true;


    //Singleton pattern
    public static ChaseManager instance;
    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(instance.gameObject);
        }
    }

    public void Start()
    {
        StartCoroutine(CalculateProbAppear());
    }

    IEnumerator CalculateProbAppear()
    {
        while (true)
        {
            if (isNight)
            {
                timeAtNight += Time.deltaTime;
                //probAppear = Mathf.Pow(1.22f, timeAtNight / 60f) / 100f;
                probAppear = 0.5f;
                if (timeAtNight >= nightLength)
                {
                    isNight = false;
                    timeAtNight = 0f;
                    probAppear = 0f;
                }
            }
            else
            {
                yield return new WaitUntil(() => isNight == true);
            }

            yield return null;
        }
    }
    
    
}
