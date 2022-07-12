using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public bool drawCircle = false;

    float theta_scale = 0.01f;        //Set lower to add more points
    int size; //Total number of points in circle
    float radius = 3f;
    LineRenderer lineRenderer;

    public float angleToGo;

    float fishCollisionSpeed = 0.7f;

    float seeDistance = 1f;

    float awayStength = 1;
    float withStrength = 1;
    float towardsStrength = 1;
    float avoidWallStrength = 10f;

    [HideInInspector]
    public bool moveAway = true;
    [HideInInspector]
    public bool moveWith = true;
    [HideInInspector]
    public bool moveToward = true;
    
    public bool inMyControl;

    [HideInInspector]
    public Vector2 lookDir;

    bool canTeleport = true;

    Vector2 mousePos;

    float distance;

    bool hitBox = true;

    GameObject nearest;
    GameObject[] agentsAll;

    Vector2 awayLookDir = Vector2.zero;
    Vector2 radiusLookDir = Vector2.zero;
    Vector2 towardLookDir = Vector2.zero;
    float awayLookDirMulti;
    float radiusLookDirMulti;
    float towardLookDirMulti;

    float colorChange = 0.03f;
    float maxDistanceOfOther = 1.5f;
    float xPerimeter = 14f;
    float yPerimeter = 8f;
    float rotationSpeed = 0.12f;
    float speed = 0.04f;

    float zRot;
    Vector2 moddedPos;
    float rotationAdjustment;

    private void Start()
    {
        zRot = Random.Range(0, 360);
    }

    void Awake(){
        float sizeValue = (2.0f * Mathf.PI) / theta_scale; 
        size = (int)sizeValue;
        size++;
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        //lineRenderer.material = new Material(Shader.Find("Particles/Additive"));
        lineRenderer.SetWidth(0.02f, 0.02f); //thickness of line
        lineRenderer.SetVertexCount(size);
    }

    void Update()
    {
        radius = distance;

        if (drawCircle)
        {
            Vector3 pos;
            float theta = 0f;
            for (int i = 0; i < size; i++)
            {
                theta += (2.0f * Mathf.PI * theta_scale);
                float x = radius * Mathf.Cos(theta);
                float y = radius * Mathf.Sin(theta);
                x += gameObject.transform.position.x;
                y += gameObject.transform.position.y;
                pos = new Vector3(x, y, 0);
                lineRenderer.SetPosition(i, pos);
            }
        }
        
        /*if (canTeleport) {

            if (transform.position.y > yPerimeter)
            {
                moddedPos = new Vector2(transform.position.x, -yPerimeter);
                transform.position = moddedPos;
                canTeleport = false;
                StartCoroutine(TeleportTimer());
            }

            if (transform.position.y < -yPerimeter)
            {
                moddedPos = new Vector2(transform.position.x, yPerimeter);
                transform.position = moddedPos;
                canTeleport = false;
                StartCoroutine(TeleportTimer());
            }

            if (transform.position.x > xPerimeter)
            {
                moddedPos = new Vector2(-xPerimeter, transform.position.y);
                transform.position = moddedPos;
                canTeleport = false;
                StartCoroutine(TeleportTimer());
            }

            if (transform.position.x < -xPerimeter)
            {
                moddedPos = new Vector2(xPerimeter, transform.position.y);
                transform.position = moddedPos;
                canTeleport = false;
                StartCoroutine(TeleportTimer());
            }

        } */

        agentsAll = GameObject.FindGameObjectsWithTag("Agent");

        nearest = GetClosestEnemy(agentsAll);
        //find awaylookdir, radiuslookdir, and towardlookdir vectors
        foreach (GameObject agent in agentsAll)
        {
            if ((agent.transform.position - transform.position).magnitude <= maxDistanceOfOther)
            {
                awayLookDir = awayLookDir + ( (new Vector2(agent.transform.position.x, agent.transform.position.y) - new Vector2(transform.position.x, transform.position.y)) * (maxDistanceOfOther - (agent.transform.position - transform.position).magnitude ) );
                radiusLookDir = ( radiusLookDir + (radiusLookDir + (agent.GetComponent<Movement>().lookDir)) ).normalized;
                towardLookDir = ( towardLookDir + (new Vector2(transform.position.x, transform.position.y) - new Vector2(agent.transform.position.x, agent.transform.position.y)).normalized ).normalized;
            }

            if (nearest != null) distance = Vector2.Distance(transform.position, nearest.transform.position);
            else distance = 0;
        }
        //get weights to smoothly transition between them
        if (moveAway) awayLookDirMulti = Mathf.Clamp(0.8f - Mathf.Pow(distance*3, 2), 0f, 0.5f);
        else awayLookDirMulti = 0;
        if (moveWith)radiusLookDirMulti = Mathf.Clamp(2*Mathf.Clamp(0.5f-distance, 0f, 0.25f) + 2*Mathf.Clamp(distance-0.5f, 0f, 0.25f), 0f, 0.5f);
        else radiusLookDirMulti = 0;
        if (moveToward)towardLookDirMulti = Mathf.Clamp(distance - 0.8f, 0f, 0.5f);
        else towardLookDirMulti = 0;

        //changing color depending on what its doing
        if (inMyControl) GetComponentInChildren<SpriteRenderer>().material.color = new Color(Mathf.Lerp(GetComponentInChildren<SpriteRenderer>().material.color.r, 1, colorChange), Mathf.Lerp(GetComponentInChildren<SpriteRenderer>().material.color.g, 1, colorChange), Mathf.Lerp(GetComponentInChildren<SpriteRenderer>().material.color.b, 1, colorChange));
        else if (distance < 0.2f) GetComponentInChildren<SpriteRenderer>().material.color = new Color(Mathf.Lerp(GetComponentInChildren<SpriteRenderer>().material.color.r, 1, colorChange), Mathf.Lerp(GetComponentInChildren<SpriteRenderer>().material.color.g, 0, colorChange), Mathf.Lerp(GetComponentInChildren<SpriteRenderer>().material.color.b, 0, colorChange));
        else if (distance < 0.6) GetComponentInChildren<SpriteRenderer>().material.color = new Color(Mathf.Lerp(GetComponentInChildren<SpriteRenderer>().material.color.r, 0, colorChange), Mathf.Lerp(GetComponentInChildren<SpriteRenderer>().material.color.g, 1, colorChange), Mathf.Lerp(GetComponentInChildren<SpriteRenderer>().material.color.b, 0, colorChange));
        else GetComponentInChildren<SpriteRenderer>().material.color = new Color(Mathf.Lerp(GetComponentInChildren<SpriteRenderer>().material.color.r, 0, colorChange), Mathf.Lerp(GetComponentInChildren<SpriteRenderer>().material.color.g, 0, colorChange), Mathf.Lerp(GetComponentInChildren<SpriteRenderer>().material.color.b, 1, colorChange));
        //get mouse position
        if (inMyControl)
        {
            mousePos = (-(Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position) / 5).normalized * 5;
        }
        else if (Input.GetMouseButton(0))
        {
            mousePos = (-(Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position) / 5).normalized;
        }
        else
        {
            mousePos = Vector2.zero;
        }
    }

    private void FixedUpdate()
    { //set the rotation and move forwards
        lookDir = Vector2.Lerp(lookDir, (FindUnobstructedDirection()*avoidWallStrength + awayLookDir.normalized*awayLookDirMulti*awayStength + radiusLookDir*radiusLookDirMulti*withStrength + towardLookDir*towardLookDirMulti*towardsStrength).normalized + mousePos, 0.1f);

        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;

        if (transform.InverseTransformDirection(lookDir).x > 0)
        {
            rotationAdjustment = Quaternion.Angle(transform.rotation, Quaternion.Euler(0f, 0f, angle));
        }
        else if (transform.InverseTransformDirection(lookDir).x < 0)
        {
            rotationAdjustment = -Quaternion.Angle(transform.rotation, Quaternion.Euler(0f, 0f, angle));
        }

        zRot = (rotationAdjustment * rotationSpeed);
        transform.rotation = Quaternion.Euler(transform.eulerAngles + new Vector3(0, 0, zRot));
        //stops them from getting super close
        if (distance < 0.2f && hitBox) {
            if (nearest != null) transform.position = transform.position + (transform.position-nearest.transform.position)/20 + (transform.up * speed * fishCollisionSpeed);
        } else {
            transform.position = transform.position + (transform.up * speed);
        }
    }

    GameObject GetClosestEnemy(GameObject[] enemies)
    {
        GameObject tMin = null;
        float minDist = Mathf.Infinity;
        Vector3 currentPos = transform.position;
        foreach (GameObject t in enemies)
        {
            float dist = Vector3.Distance(t.transform.position, currentPos);
            if ((dist < minDist) && (t.transform.position != transform.position))
            {
                tMin = t;
                minDist = dist;
            }

        }
        return tMin;
    }

    IEnumerator TeleportTimer()
    {
        yield return new WaitForSeconds(2);
        canTeleport = true;
    }

    Vector2 FindUnobstructedDirection () {
        Vector2 direction = Vector2.zero;
        Vector2 right = new Vector2(transform.right.x, transform.right.y);

        int angle = 9; //10 is forwards, 0 is right, -10 is backwards

        while (ScanThisAngle(angle) == Vector2.zero){
            angle--;
            if (angle == -10) { angle = -10; break; }
        }

        angleToGo = angle;

        direction = ScanThisAngle(angle);

        return direction.normalized;
    }

    Vector2 ScanThisAngle (int angle) {
        Vector2 pos = new Vector2(transform.position.x, transform.position.y);

        RaycastHit2D hitRightSide = Physics2D.Raycast(transform.position, 
        (transform.up * angle + transform.right * (10 - Mathf.Abs(angle))).normalized, 
        seeDistance);

        RaycastHit2D hitLeftSide = Physics2D.Raycast(transform.position, 
        (transform.up * angle - transform.right * (10 - Mathf.Abs(angle))).normalized, 
        seeDistance);

        if (angle < -9 && hitRightSide.collider != null && hitLeftSide.collider != null) return new Vector2(transform.right.x, transform.right.y); 

        if (hitRightSide.collider != null && hitLeftSide.collider != null) {
            return Vector2.zero;
        }
        else if (hitLeftSide.collider == null && hitRightSide.collider != null) {
            //return (pos - hitLeftSide.point).normalized;
            return new Vector2(transform.right.x, transform.right.y);
        }
        else if (hitRightSide.collider == null && hitLeftSide.collider != null) {
            //return (pos - hitRightSide.point).normalized;
            return new Vector2(-transform.right.x, -transform.right.y);
        }
        else return Vector2.zero;
    }
}
