using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collider : MonoBehaviour {
    public Vector3 curRelativeSpeed;
    public Vector3 collisionCenter;
    public RaycastHit hit;
    public bool isCollision;
    public Vector3 dir;
    // Use this for initialization
    void Start()
    {
        isCollision = false;
        curRelativeSpeed = Vector3.zero;
    }
	// Update is called once per frame
	void fixedUpdate () {
        
    }
    //void OnCollisionEnter(Collision collision)
    //{
    //    if (collision.gameObject.tag == "Collide")
    //    {
    //        curRelativeSpeed = collision.relativeVelocity;
    //    }
    //}
    //void OnCollisionExit(Collision collision)
    //{
    //    if (collision.gameObject.tag == "Collide")
    //    {
    //        curRelativeSpeed = Vector3.zero;
    //    }
    //}
    public bool RayCaster(Vector3 fwd)
    {
        dir = fwd;
        Ray myRay = new Ray(transform.position, fwd);
        isCollision = Physics.Raycast(myRay,out hit,0.3f);
        if (isCollision)
        {
           if( hit.collider.tag == "Collide")
            {
                isCollision = true;
                curRelativeSpeed = hit.point - transform.position;
                collisionCenter = hit.point;
                return true;
            }
            else
            {
                isCollision = false;
                curRelativeSpeed = Vector3.zero;
            }
        }

        return isCollision;
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, dir);
    }


}
