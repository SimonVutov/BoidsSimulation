using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public bool moveAway = true;
    public bool moveWith = true;
    public bool moveToward = true;
    
    public GameObject boidsPrefab;
    public int amount = 200;
    float speed = 0.01f;

    public Movement[] boidsList;

    void Start()
    {
        Invoke("Spawn", speed);
        boidsList = new Movement[amount];
    }

    void Update(){
        /*foreach(Movement boid in boidsList){
            if(boid != null){
                boid.moveAway = moveAway;
                boid.moveWith = moveWith;
                boid.moveToward = moveToward;
            }
        }*/
    }

    void Spawn()
    {
        if (amount > 0)
        {
            Vector3 euler = transform.eulerAngles;
            euler.z = Random.Range(0f, 360f);

            boidsList[amount - 1] = Instantiate(boidsPrefab, new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)), Quaternion.Euler(euler)).GetComponent<Movement>();

            amount--;
            Invoke("Spawn", speed);
        }
    }
}
