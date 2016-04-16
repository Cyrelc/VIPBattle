using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*	cmpt 430
 * 	Charles L.
 * 
 * impliments Looped path. Set it a list of GOs to follow and will update
 * the heading to follow by command.
 * 
 * Add this script to GO that paths. Add path nodes to list order. Call GetPathDirectionNow()
 * for the heading to the next node.  This script will auto switch to the next node at proximity.
 * 
 * Set the set of gameobjects to follow (with no collision), it will return direction
 * to current node given 
 * by acceration force.
 */


public class Pathing : MonoBehaviour
{
	/// <summary>
	/// The path points, set set of non-collider objects in the editor.
	/// </summary>
	public List<GameObject> pathPoints = new List<GameObject> ();



	// object speed
	public float maxSpeed = 10;
	// driver force using self mass.
	public float accelerationForce = 100;
	/// <summary>
	///  distance to node to change to the next.
	/// </summary>
	public float nodeDistanceChange = 1f;


	public Vector3 pathDirection;



	int currentNode = 0;
	int nodeDistance = 0;
	//Rigidbody selfRb;
	Transform selfTrans;

	// Use this for initialization
	void Start ()
	{
		/*
		selfRb = GetComponent<Rigidbody> ();
		if (selfRb == null) {
			print ("Rigidbody required for this script");
		}*/

		selfTrans = GetComponent<Transform> ();
		nodeDistance = float.MaxValue;

	}
	
	// Update is called once per frame
	void FixedUpdate ()
	{
		Vector3 selfPos = selfTrans.position;
		// do nothing if path is not defined.
		if (pathPoints.Count == 0) {
			print ("not path set given");
			pathDirection = selfPos;
			return;
		}
			


		GameObject currentTarget = pathPoints [currentNode];
		Vector3 posTarget = currentTarget.transform.position;
		// flatten target to self.
		Vector3 flatTarget = posTarget;
		flatTarget.y = selfPos.y;

		// node change
		float distanceCheck = Vector3.Distance (flatTarget, selfPos);
		nodeDistance = distanceCheck;
		if (distanceCheck <= nodeDistanceChange) {
			if (currentNode + 1 <= pathPoints.Count - 1) {
				currentNode++;
			} else {
				currentNode = 0;
			}
		}


		/* Final motor control.
		 * Place override code here. Or set the script off.
		 */

		Vector3 headTowards = posTarget - selfPos;
		pathDirection = headTowards.normalized;

		/*
		// demo code. remove
		Rigidbody rb = GetComponent<Rigidbody>();
		rb.AddForce (GetPathDirectionNow () * accelerationForce * Time.fixedDeltaTime, ForceMode.Acceleration);
		// speed cap
		if (rb.velocity.magnitude > maxSpeed) {
			float y = rb.velocity.y;
			Vector3 tempV3 = rb.velocity.normalized * maxSpeed;
			tempV3.y = y;
			rb.velocity = tempV3;
		}*/

	}

	/// <summary>
	/// Insert the path node. Used for scripts
	/// </summary>
	/// <param name="node">Node.</param>
	public void insertPathNode (GameObject node)
	{
		pathPoints.Add (node);
	}

	public int Count ()
	{
		return pathPoints.Count; 
	}

	//
	public int GetCurrentNodeIndex ()
	{
		return currentNode;
	}

	//
	public void SetCurrentIndex (int nodeIndex)
	{
		currentNode = nodeIndex;
	}

	/// <summary>
	/// Given current path node direction.
	/// </summary>
	/// <returns>The path direction at time.</returns>
	public Vector3 GetPathDirectionNow ()
	{
		return pathDirection;
	}

	public float GetNodeDistance(){
		return nodeDistance;
	}
}
