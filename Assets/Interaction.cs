using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Interaction : MonoBehaviour {

	public int teamID, myID, hp, damage; 
	public float range, distance;
	private Create_Teams controller;
	public GameObject target;
	// Use this for initialization
	void Start () {
		controller = GameObject.Find ("controller").GetComponent<Create_Teams>();
		teamID = transform.GetComponent<AILogic> ().teamID;
		myID = transform.GetComponent<AILogic> ().myID;
		AssignRange ();
		hp = 10;
		damage = 2;
		target = null;
	}
	
	// Update is called once per frame
	void Update () {
		if (hp <= 0){
			Debug.Log ("Hit");
			controller.teamRosters [teamID].Remove (myID);
			Destroy (transform.gameObject);
			//transform.gameObject.SetActive(false);
		}
		foreach (KeyValuePair <int, Transform> enemy in controller.visibleEnemies[teamID]) {
			distance = Vector3.Distance (transform.position, enemy.Value.position);
				if (distance <= range) {
					target = enemy.Value.gameObject;
					Debug.Log (target);
				}
		}
		if (target != null)
			target.GetComponent<Interaction> ().hp = target.GetComponent<Interaction> ().hp - damage;
	}

	private void AssignRange(){
		int random = Random.Range (0, 3);
		if (random < 1)
			range = 5f;
		else
			range = 1f;
	}
}
