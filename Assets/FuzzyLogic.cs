using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FuzzyLogic : MonoBehaviour {

    AILogic logic;
    Interaction interaction;
    Create_Teams controller;
    private const float epsilon = 1e-6f;
    /* healthRunAway - the health at which the player will attempt to disengage
     * healthSafeToAttack - the health at which a player thinks it is 100% "healthy" and ready to attack
     * localRange - the range to consider what enemies and allies are "near" to you
    */
    public float healthRunAway = 4, healthSafeToAttack = 10, localRange = 30f;
    /* health confidence - describes, based on it's current health, how confident a unit is in continuing to attack
     * vsConfidence - describes the confidence the unit has on attacking based on the ratio of in range allies, vs in range enemies.
     * threat to VIP
    */
    private float healthConfidence;//, VIPSafety;


	// Use this for initialization
	void Start () {
        logic = transform.GetComponent<AILogic>();
        interaction = transform.GetComponent<Interaction>();
        controller = GameObject.Find("controller").GetComponent<Create_Teams>();
	}
	
	// Update is called once per frame
	void Update () {
        healthConfidence = -grade(interaction.hp, healthRunAway, healthSafeToAttack);                       //calculate fuzzy value for player health
        logic.enemyWeight = confidence() + healthConfidence * 3;                                                //calculate enemyWeight pull value (see AILogic)
        int VIPDefenderCount = 0, VIPAttackerCount = 0;                                                     //counters for the number of people defending/attacking your VIP
        /* Count the number of enemies threatening for VIP, and the number of allies defending him
         */
        foreach (KeyValuePair<int, Transform> ally in controller.teamRosters[logic.teamID]) {           
            if (Vector3.Distance(ally.Value.position, controller.VIPs[ally.Value.GetComponent<AILogic>().teamID].position) < localRange)
                VIPDefenderCount++;
        }
        foreach (KeyValuePair<int, Transform> enemy in controller.visibleEnemies[logic.teamID]) {
            if (Vector3.Distance(enemy.Value.position, controller.VIPs[logic.teamID].position) < localRange)
                VIPAttackerCount++;
        }
        logic.friendlyVIPWeight = reverseGrade((float)VIPDefenderCount, 0f, (float)VIPAttackerCount) / (controller.teamSize / 10);
        logic.enemyVIPWeight = confidence() + healthConfidence * 5;
        if (logic.friendWeight > 0f && controller.visibleEnemies[logic.teamID].Count == 0)
            logic.friendWeight -= 0.01f;
        else if (logic.friendWeight < 1)
            logic.friendWeight += 0.01f;
//        logic.helpWeight = very(reverseGrade());
    }

    private float confidence() {
        int allyCount = 0, enemyCount = 0;                                                              //count number of enemies/friends in local group
        foreach (KeyValuePair<int, Transform> ally in controller.teamRosters[logic.teamID]) {           //count number of friendlies
            if (Vector3.Distance(transform.position, ally.Value.position) < localRange)
                allyCount++;
        }
        foreach (KeyValuePair<int, Transform> enemy in controller.visibleEnemies[logic.teamID]) {       //count number of visibile enemies in range
            if (Vector3.Distance(transform.position, enemy.Value.position) < localRange)
                enemyCount++;
        }
        float vsConfidence = reverseGrade(allyCount, 0f, enemyCount);
        if (vsConfidence < 0.5) {                                                                       //if you are outnumbered more than 2 to 1, call for help
            controller.callingForHelp[logic.teamID].Add(logic.myID, transform);
        }
        if (vsConfidence <= 0.5 && controller.callingForHelp[logic.teamID].ContainsKey(logic.myID)) {
            controller.callingForHelp[logic.teamID].Remove(logic.myID);
        }
        return vsConfidence;                                                                            //calculate strength of numbers confidence
    }

    public static float grade(float value, float x0, float x1) {
        if (value <= x0)
            return 0f;
        if (value >= x0)
            return (value - x0) / (x0 - x1);
        return 1f;
    }

    public static float triangular(float value, float x0, float x1, float x2) {
        if (value <= x0 || value >= x2)
            return 0f;
        if (Mathf.Abs(value - x1) < epsilon)
            return 1f;
        if (value < x1)
            return (value - x0) / (x1 - x0);
        return (x2 - value) / (x2 - x1);
    }

    public static float reverseGrade (float value, float x0, float x1) {
        if (value <= x0)
            return 1f;
        if (value < x1)
            return 1f - ((value - x0) / (x1 - x0));
        return 0f;
    }

    public static float trapezoid(float value, float x0, float x1, float x2, float x3) {
        if (value < -x0 || value >= x3)
            return 0f;
        if (value < x1)
            return (value - x0) / (x1 - x0);
        if (value < x2)
            return 1f;
        return (x3 - value) / (x3 - x2);
    }

    public static float not(float value) {
        return 1f - value;
    }

    public static float and (float v1, float v2) {
        return Mathf.Min(v1, v2);
    }
    public static float or(float v1, float v2) {
        return Mathf.Max(v1, v2);
    }
    public static float very (float value) {
        return value * value;
    }
    public static float notVery (float value) {
        return Mathf.Sqrt(value);
    }
}