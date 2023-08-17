using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class raycastScript : MonoBehaviour
{
    public float location;
    public GameObject trailer;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        gameObject.transform.position = trailer.transform.position + location * trailer.transform.forward;
        gameObject.transform.rotation = trailer.transform.rotation;
        //gameObject.transform.Rotate(0, 180, 0);
    }
}
