using UnityEngine;
using System.Collections;

public class VIPVitals : MonoBehaviour {

    //Handles VIP health and existence in game
    private AILogic logic;

	public int hp; 
	private Create_Teams CT;
	// Use this for initialization
	void Start () {
		hp = 30; // Hp of VIPs 
        logic = transform.GetComponent<AILogic>();
		CT = GameObject.Find ("controller").GetComponent<Create_Teams> (); // the accessor for the list we need to remove VIPs from when they are destroyed
	}
	
	// Update is called once per frame
	void Update () {
		if (hp <= 0) {
            // if this VIP is dead
			CT.VIPs[logic.teamID] = null; // remove from list of VIPs for win condition
			Destroy (transform.gameObject); // destroy this object
        }
    }
}
