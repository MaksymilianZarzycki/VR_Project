using UnityEngine;

public class BowlingPin : MonoBehaviour
{
    [SerializeField] public Material fallMat;
    public Material defaultMat;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        defaultMat = GetComponent<MeshRenderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        if(transform.rotation.eulerAngles.x > 45 || transform.rotation.eulerAngles.z > 45)
        {
            GetComponent<MeshRenderer>().material = fallMat;
        }
        else
        {
            GetComponent<MeshRenderer>().material = defaultMat;
        }
    }
}
