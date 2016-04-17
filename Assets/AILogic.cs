﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * [Is Missing any original Comments]
 *
 *  Apr 15:
 *  	-	AI script modifyed for VIP. Now contain heading and weight given by pathing script, if exists.
 * 		-	Path weight needs to be set realtime.
 * 	-Charles L.
 * 
 * 
 */
public class AILogic : MonoBehaviour
{

	//this players team ID and individual ID to identify himself and his friendlies
	public int teamID, myID;
	//the heading scale speed indicates how much to scale the heading by

	public float collisionAvoidanceWeight = 0.025f;
	//the weighting to place on avoiding collisions between other friendlies, and between terrain objects (Walls)
	public float collisionAvoidanceRadius = 3f;
	//the minimum radius to keep between yourself and the nearest ally or Wall

	public float groupDirectionWeight = 0.025f;
	//the weight to place on what direction the group is moving in
	public float groupDirectionRadius = 6f;
	//the radius within which to detect what direction your allies are moving

	public float groupCohesionWeight = 1f;
	//the weight to place on staying with the group. This will likely decrease over time while in searching mode
	public float groupCohesionRadius = 6f;
	//the radius in which to check the location of your allies, this may increase over time while in searching mode

	//maximum, minimum, and base speeds. Base speed is random, so some boids are just naturally faster than others as a base
	public static float minSpeed = 1, maxSpeed = 400, baseSpeed = Random.Range (2, 6);

	//maximum rotation values
	public static float maxRotationSpeed = 10;

	//Vectors calculated towards or away from all relevent attractors
	Vector3 toFriends, toEnemies, toFriendlyVIP, toEnemyVIP, awayFromWalls, needHelp, pathNodeHeading;
	public Vector3 velocity;

	public float friendWeight = 1f, enemyWeight = 1f, friendlyVIPWeight = 0.025f, enemyVIPWeight = 1f, wallWeight = 1f, helpWeight = 1f, pathWeight = 0.0f;

	//an enum listing all of the possible states that the player can be in
	public enum AIStates
	{
		searching,
		callForHelp,
		attacking,
		retreating,
		defending}

	;
	//the current state that this player is in
	public AIStates playerState;
	//creates a Dictionary to track walls which should be avoided
	public Dictionary<int, Transform> wallObjects = new Dictionary<int, Transform> ();
	//gives constant access to the controller and subsequently all gamewide variables
	private Create_Teams controller;
	private Pathing pathController;

	// Use this for initialization
	void Start ()
	{
		velocity = Vector3.zero;
		playerState = AIStates.searching;
		controller = GameObject.Find ("controller").GetComponent<Create_Teams> ();
		// path controller
		pathController = GetComponent<Pathing> ();
		// sanity check if pathing does not exist.
		if (pathController == null) {
			pathWeight = 0;
		}
	}

	// Update is called once per frame
	void Update ()
	{
		toFriends = findFriendlyVector ();                                       //find the heading towards allies
		toEnemies = findEnemyVector ();                                          //find the heading towards visible enemies ONLY
		awayFromWalls = -findWallAvoidanceVector ();                             //repel from walls within range
		// pathing node depends on pathing script. This case checks if it exists.
		if (pathController != null) {
			pathNodeHeading = pathController.GetPathDirectionNow ();
		}
		toFriendlyVIP = controller.VIPs [teamID].position - transform.position;  //find the heading towards my VIP
		needHelp = findCallingForHelpVector ();                                  //find any allies calling for help
		toEnemyVIP = findEnemyVIPVector ();

		Vector3[] combinedHeadings = {
			toFriends,
			toEnemies,
			awayFromWalls,
			toFriendlyVIP,
			toEnemyVIP,
			needHelp,
			pathNodeHeading
		};
		float[] combinedWeights = {
			friendWeight,
			enemyWeight,
			wallWeight,
			friendlyVIPWeight,
			enemyVIPWeight,
			helpWeight,
			pathWeight
		};

		Vector3 newVelocity = combinedHeading (combinedHeadings, combinedWeights);
		//        Debug.Log(newVelocity);
		newVelocity.y = 0;
		transform.rotation = Quaternion.LookRotation (newVelocity);
		transform.position += newVelocity * Time.deltaTime;
		//        transform.forward = (transform.position - toFriends).normalized * Time.deltaTime * minSpeed;
	}

	void OnDrawGizmos ()
	{
		if (transform == null) {
			return;
		}
		Gizmos.color = (Color.Lerp (Color.yellow, Color.blue, (1.0f * this.teamID / 3)));
		Gizmos.DrawRay (transform.position, transform.forward * 10);
		Gizmos.color = Color.red;
		Gizmos.DrawRay (transform.position, toEnemies);
		Gizmos.color = Color.green;
		Gizmos.DrawRay (transform.position, toFriends);
		Gizmos.color = Color.blue;
		Gizmos.DrawRay (transform.position, toFriendlyVIP);
		Gizmos.color = Color.black;
		Gizmos.DrawRay (transform.position, awayFromWalls);
	}

	private Dictionary<int, Transform> enemyVIPSSpotted ()
	{
		Dictionary<int, Transform> canSee = new Dictionary<int, Transform> ();
		for (int i = 0; i < controller.numTeams; i++) {
			foreach (KeyValuePair<Transform, bool> enemyVIP in controller.visibleVIPs[i]) {
				if (enemyVIP.Value == true) {
					canSee.Add (enemyVIP.Key.GetInstanceID (), enemyVIP.Key.transform);
				}
			}
		}
		return canSee;
	}

	private Vector3 findFriendlyVector ()
	{

		int separationCount = 0, alignmentCount = 0, cohesionCount = 0;
		Vector3 separationSum = Vector3.zero, alignmentSum = Vector3.zero, cohesionSum = Vector3.zero;

		foreach (KeyValuePair<int, Transform> friend in controller.teamRosters[teamID]) {
			float distance = Vector3.Distance (transform.position, friend.Value.position);
			if (distance > 0) {
				if (distance < collisionAvoidanceRadius) {
					Vector3 direction = transform.position - friend.Value.position;
					separationSum += direction;
					separationCount++;
				}

				if (distance < groupDirectionRadius) {
					alignmentSum += friend.Value.GetComponent<AILogic> ().velocity;
					alignmentCount++;
				}

				if (distance < groupCohesionRadius) {
					cohesionSum += friend.Value.position;
					cohesionCount++;
				}
			}
		}

		separationSum = separationCount > 0 ? separationSum / separationCount : separationSum;
		separationSum.Normalize ();

		alignmentSum = alignmentCount > 0 ? Limit (alignmentSum / alignmentCount, maxRotationSpeed) : alignmentSum;
		alignmentSum.Normalize ();

		cohesionSum = cohesionCount > 0 ? Steer (cohesionSum / cohesionCount, false) : cohesionSum;
		cohesionSum -= transform.position;
		cohesionSum.Normalize ();

		return velocity + separationSum * collisionAvoidanceWeight + alignmentSum * groupDirectionWeight + cohesionSum * groupCohesionWeight;
	}

	private Vector3 findEnemyVector ()
	{
		Vector3 enemyCohesionSum = Vector3.zero;

		foreach (KeyValuePair<int, Transform> enemy in controller.visibleEnemies[teamID]) {
			float distance = Vector3.Distance (transform.position, enemy.Value.position);

			//define behaviour here, depending on size of enemy group, proximity, size of local force, etc.
		}

		return Vector3.zero;
	}

	private Vector3 findWallAvoidanceVector ()
	{

		Vector3 wallSum = Vector3.zero;

		foreach (KeyValuePair<int, Transform> wall in wallObjects) {
			wallSum += transform.position - wall.Value.position;
			if ((transform.position - wall.Value.position).magnitude < 1)
				wallWeight += 0.01f;
		}
		wallSum = wallObjects.Count > 0 ? wallSum / wallObjects.Count : wallSum;
		return wallSum.normalized;
	}

	private Vector3 findCallingForHelpVector ()
	{

		Vector3 helpSum = Vector3.zero;

		foreach (KeyValuePair<int, Transform> help in controller.callingForHelp[teamID]) {
			helpSum += transform.position - help.Value.position;
		}
		helpSum = controller.callingForHelp [teamID].Count > 0 ? helpSum / controller.callingForHelp [teamID].Count : helpSum;
		return helpSum.normalized;
	}

	private Vector3 findFriendlyVIPVector ()
	{

		float distance = Vector3.Distance (transform.position, controller.VIPs [teamID].position);
		if (distance > 0 && distance < collisionAvoidanceRadius)
			return Vector3.zero;
		return transform.position - controller.VIPs [teamID].position;
	}

	private Vector3 findEnemyVIPVector ()
	{
		List<Transform> visibleVIPs = new List<Transform> ();
		foreach (KeyValuePair<Transform, bool> enemyVIP in controller.visibleVIPs[teamID]) {
			if (enemyVIP.Value == true)
				visibleVIPs.Add (enemyVIP.Key);
		}
		Vector3 enemyVIPAttractor = Vector3.zero;
		if (visibleVIPs.Count != 0) {
			foreach (Transform enemyVIP in visibleVIPs) {
				enemyVIPAttractor += transform.position - enemyVIP.position;
			}
		}
		enemyVIPAttractor = visibleVIPs.Count > 0 ? enemyVIPAttractor / visibleVIPs.Count : enemyVIPAttractor;
		return enemyVIPAttractor.normalized;
	}

	private Vector3 combinedHeading (Vector3[] headings, float[] weights)
	{
		Vector3 result = Vector3.zero;
		for (int i = 0; i < headings.Length; i++) {
			result += headings [i] * weights [i];
		}
		return result;
	}

	protected virtual Vector3 Steer (Vector3 target, bool slowDown)
	{
		Vector3 steer = Vector3.zero;
		Vector3 targetDirection = target - transform.position;
		float targetDistance = targetDirection.magnitude;

		transform.LookAt (target);

		if (targetDistance > 0) {
			targetDirection.Normalize ();

			if (slowDown && targetDistance < 100f * baseSpeed) {
				targetDirection *= (maxSpeed * targetDistance / (100f * baseSpeed));
				targetDirection *= baseSpeed;
			} else {
				targetDirection *= maxSpeed;
			}

			steer = targetDirection - GetComponent<Rigidbody> ().velocity;
			steer = Limit (steer, maxRotationSpeed * Time.deltaTime);
		}

		return steer;
	}

	protected Vector3 Limit (Vector3 v, float max)
	{
		return (v.magnitude > max) ? v.normalized * max : v;
	}

}