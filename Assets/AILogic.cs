using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AILogic : MonoBehaviour {

    //this players team ID and individual ID to identify himself and his friendlies
    public int teamID, myID;
    //the heading scale speed indicates how much to scale the heading by
    public static float headingScaleSpeed = 100;
    //maximum and minimum speeds
    public static float minSpeed = 1, maxSpeed = 400;
    //maximum rotation values
    public static float rotationSpeed = 10;
    //Vectors calculated towards or away from all relevent attractors
    Vector3 toFriends, toEnemies, toFriendlyVIP, toEnemyVIP, awayFromWalls, needHelp;


    public static float critterMotorRate = 33f;
    public static float critterFullPercent = 0.21f;

    //an enum listing all of the possible states that the player can be in
    public enum AIStates {searching, callForHelp, attacking, retreating, defending};
    //the current state that this player is in
    public AIStates playerState;
    //creates a Dictionary to track walls which should be avoided
    public Dictionary<int, Transform> wallObjects = new Dictionary<int, Transform>();
    //gives constant access to the controller and subsequently all gamewide variables
    private Create_Teams controller;

	// Use this for initialization
	void Start () {
        playerState = AIStates.searching;
        controller = GameObject.Find("controller").GetComponent<Create_Teams>();
    }
	
	// Update is called once per frame
	void Update () {
        toFriends = findHeadingByTargetsLinear(controller.teamRosters[teamID]);         //find the heading towards allies
        toEnemies = findHeadingByTargetsLinear(controller.visibleEnemies[teamID]);      //find the heading towards visible enemies ONLY
        awayFromWalls = -findHeadingByTargetsLinear(wallObjects);                       //repel from walls within range
        toFriendlyVIP = controller.VIPs[teamID].position - transform.position;          //find the heading towards my VIP
        needHelp = findHeadingByTargetsLinear(controller.callingForHelp[teamID]);       //find any allies calling for help
        Dictionary<int, Transform> VIPSpotted = enemyVIPSSpotted();                     //See whether any enemy VIPs are visible
        if (VIPSpotted.Count != 0)                                                      //If there is one visible, then calculate the heading to it or them
            toEnemyVIP = findHeadingByTargetsLinear(VIPSpotted);
//        transform.forward = (transform.position - toFriends).normalized * Time.deltaTime * minSpeed;
    }

    void OnDrawGizmos() {
        if (transform == null) {
            return;
        }
        Gizmos.color = (Color.Lerp(Color.yellow, Color.blue, (1.0f * this.teamID / 3)));
        Gizmos.DrawRay(transform.position, transform.forward);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, toEnemies);
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, toFriends);
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, toFriendlyVIP);
        Gizmos.color = Color.black;
        Gizmos.DrawRay(transform.position, awayFromWalls);
    }


    private Dictionary<int, Transform> enemyVIPSSpotted() {
        Dictionary<int, Transform> canSee = new Dictionary<int, Transform>();
        for (int i =0; i < controller.numTeams; i++) {
            foreach(KeyValuePair<Transform, bool> enemyVIP in controller.visibleVIPs[i]) {
                if (enemyVIP.Value == true) {
                    canSee.Add(enemyVIP.Key.GetInstanceID(), enemyVIP.Key.transform);
                }
            }
        }
        return canSee;
    }

    /// <summary>
    /// Finds the heading by targets cluster of postions of gameobjects. 
    /// Higher weight to closer targets, by linear scale.
    /// </summary>
    /// <returns>The unit heading to move towards from critter transform.</returns>
    /// <param name="targets">Targets.</param>
    public Vector3 findHeadingByTargetsLinear(Dictionary<int, Transform> targets) {

        Vector3 selfPos = transform.position;
        //	case where no targets, then heading forward.
        if (targets.Count == 0) {
            return transform.forward;
        }

        Dictionary<int, float> distances = new Dictionary<int, float>(targets.Count);
        float maxDis = 0;
        //float maxDis = float.MaxValue;
        //	
        foreach (int key in targets.Keys) {
            float dis = Vector3.Distance(selfPos, targets[key].position);
            distances.Add(key, dis);
            maxDis = Mathf.Max(maxDis, dis);
        }
        //	find weighted heading. Closer targets have higher pull.
        //	Inital heading is self forward as extra direction.
        float sum = maxDis - 1;
        Vector3 headingWeighted = transform.forward;
        //for (int i = 0; i < targets.Count; i++) {
        foreach (int key in targets.Keys) {
            // 
            float invertDistance = (maxDis - distances[key]);

            headingWeighted += ((targets[key].position - selfPos) * invertDistance);
            sum += invertDistance;
            //print ("d: " + invertDistance);
        }
        //print ("sum: " + sum);
        headingWeighted /= ((sum == 0) ? 1 : sum);
        return headingWeighted;
    }
}