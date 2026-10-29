using NUnit.Framework;
using UnityEngine;

public class BallReturn : MonoBehaviour
{

    [SerializeField] public GameObject ball;
    [SerializeField] public Transform returnPos;
    [SerializeField] public float time;

    private Rigidbody ballRB;
    private bool timerActive = false;
    private float counter;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ballRB = ball.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (timerActive)
        {
            counter -= Time.deltaTime;
            if (counter <= 0)
            {
                ballRB.linearVelocity = Vector3.zero;
                ballRB.angularVelocity = Vector3.zero;
                ball.transform.position = returnPos.position;
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(other.gameObject == ball)
        {
            startTimer();
        }      
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject == ball)
        {
            timerActive = false;
        }  
    }

    void startTimer()
    {
        counter = time;
        timerActive = true;
    }
}
