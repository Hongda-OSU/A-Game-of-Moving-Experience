using UnityEngine;

public class SeeSawHandler : MonoBehaviour
{
    [SerializeField] private GameObject Weighter;
    private bool isHit;
    private Vector3 originalPos;
    
    private static SeeSawHandler instance;
    public static SeeSawHandler Instance { get { return instance; } }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        instance = this;
        originalPos = transform.position;
    }

    void Update()
    {
        CheckFalling();
    }

    private void CheckFalling()
    {
        if (transform.position.y <= -10f)
            transform.position = originalPos;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Weighter"))
        {
            Debug.Log("Weighter Hit");
            isHit = true;
            this.gameObject.GetComponent<Rigidbody>().isKinematic = true;
            this.gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition;
            this.gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
            Weighter.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition;
            Weighter.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
            Weighter.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
        }
    }

    public bool IsHit => isHit;
}
