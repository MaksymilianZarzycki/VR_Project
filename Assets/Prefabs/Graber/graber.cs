using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.InputSystem;

public class graber : MonoBehaviour
{

    [SerializeField] public InputActionProperty input;

    [SerializeField] public float pullForce;
    public GameObject heldObject;
    public Rigidbody heldObjectRB;

    public GameObject target;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        input.action.performed += OnGrab;
    }

    // Update is called once per frame
    void Update()
    {
        if (heldObject && Vector3.Distance(transform.position, heldObject.transform.position) > 0.1f)
        {
            Vector3 direction = transform.position - heldObject.transform.position;
            direction = direction.normalized;
            heldObjectRB.AddForce(direction * pullForce * Time.deltaTime);
        }
    }

    void OnTriggerStay(Collider other)
    {
        GameObject obj = other.gameObject;
        if (obj.GetComponent<Rigidbody>())
        {
            if(target == null)
            {
                target = obj;
            }
            else if(Vector3.Distance(target.transform.position, transform.position)>Vector3.Distance(obj.transform.position, transform.position))
            {
                target = obj;
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(other.gameObject == target)
        {
            target = null;
        }
    }

    void OnGrab(InputAction.CallbackContext callbackContext)
    {
        if(heldObject)
        {
            heldObjectRB.linearDamping = 0;
            heldObjectRB.useGravity = true;
            heldObject = null;
            heldObjectRB = null;
            return;
        }
        if (target)
        {
            heldObject = target;
            heldObjectRB = heldObject.GetComponent<Rigidbody>();
            heldObjectRB.useGravity = false;
            heldObjectRB.linearDamping = 10;
        }
    } 
}
