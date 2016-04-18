using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Interaction : MonoBehaviour {


	// Handles soldiers health, attack, targeting. if the soldier dies, then delete it from the list
	public int teamID, myID, hp, damage; 
	public float range, distance;
	private Create_Teams controller;
	public GameObject target;
	// Use this for initialization
	void Start () {
		controller = GameObject.Find ("controller").GetComponent<Create_Teams>();
		teamID = transform.GetComponent<AILogic> ().teamID; // this soldiers team id
		myID = transform.GetComponent<AILogic> ().myID; // this soldiers id
		AssignRange (); // Randomly assign a range for this soldier
		hp = 15; // set the HP for this soldier
		damage = 2; // the damage that this soldier does per frame
		target = null; // the target that this soldier will damage
	}
	
	// Update is called once per frame
	void Update () {
		if (hp <= 0){
			// If this soldier's health is 0 or below, then it is dead, remove from list of soldiers and destroy this object
			Debug.Log ("Hit");
			controller.teamRosters [teamID].Remove (myID);
			Destroy (transform.gameObject);
			//transform.gameObject.SetActive(false);
		}
		foreach (KeyValuePair <int, Transform> enemy in controller.visibleEnemies[teamID]) {
			// iterate through the list of enemies which are visible to the group, determine the closest one
			distance = Vector3.Distance (transform.position, enemy.Value.position);
				if (distance <= range) {
				// if the target is within the assigned range, then we can attack it, so assign it as the target
					target = enemy.Value.gameObject;
					//Debug.Log (target);
				}
		}
        foreach (KeyValuePair<Transform, bool> vip in controller.visibleVIPs[transform.GetComponent<AILogic>().teamID]) {
            if (vip.Value) {
                distance = Vector3.Distance(transform.position, vip.Key.position);
                if (distance <= range) {
                    // if the target is within the assigned range, then we can attack it, so assign it as the target
                    target = vip.Key.gameObject;
                    //Debug.Log (target);
                }
            }
        }
		if (target != null) {
			// if we have the target, then we can damage it by accessing its hp value and subtracting our damage from it
			if (target.tag.Contains ("VIP"))
				//if target is a vip, access that script
				target.GetComponent<VIPVitals> ().hp = target.GetComponent<VIPVitals> ().hp - damage;
			else
				// if target is not vip, then 
				target.GetComponent<Interaction> ().hp = target.GetComponent<Interaction> ().hp - damage;
		}
	}

	private void AssignRange(){
		int random = Random.Range (0, 3);
		if (random < 1)
			range = 5f;
		else
			range = 1f;
	}
}
