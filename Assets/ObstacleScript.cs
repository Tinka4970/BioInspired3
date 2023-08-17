using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleScript : MonoBehaviour
{
    // Start is called before the first frame update
    public float startZ;
    public float endZ;
    public float obstacleVelocity;
    public float x;
    public float y;
    private Vector3 newPosition;


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        newPosition = new Vector3(x, y, gameObject.transform.position.z + obstacleVelocity * Time.deltaTime);
        gameObject.transform.position = newPosition;

        if(gameObject.transform.position.z > endZ)
        {
            gameObject.transform.position = new Vector3(x, y, startZ);
        }
    }
}
