﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Create_Teams : MonoBehaviour {

    /* Variables:
     * numTeams     - defines the number of teams playing
     * teamSize     - number of players on each team
     * startRadius  - distance from start position within which it is OK to spawn (for random spawn position determination)
     * team1StartPos- position which defines the team 1 start location
     * team2StartPos- position which defines the team 2 start location
    */

    public int numTeams = 2, teamSize = 20, startRadius = 20;
    public Vector3[] teamStartPositions = new Vector3[2];
    public Color[] teamColours = new[] { Color.red, Color.blue, Color.green, Color.yellow };
    public Transform playerPrefab, VIPPrefab;
    public Transform[] VIPs = new Transform[4];
    public List<Dictionary<int, Transform>> teamRosters;
    public Dictionary<int, Transform>[] callingForHelp = new Dictionary<int, Transform>[4];
    public Dictionary<Transform, bool>[] visibleVIPs = new Dictionary<Transform, bool>[4];
    public List<Dictionary<int, Transform>> visibleEnemies = new List<Dictionary<int,Transform>>();

	// Use this for initialization
	void Start () {
        if (numTeams > 4) numTeams = 4;                         //cannot support more than four teams. Base game is designed for only two, this is just to prevent accidents
        teamRosters = new List<Dictionary<int, Transform>>();   //initialize teamRoster list
        for (int j = 0; j < numTeams; j++) {                     //initialize all state trackers for VIP visibility
            visibleVIPs[j] = new Dictionary<Transform, bool>();
            callingForHelp[j] = new Dictionary<int, Transform>();
        }
        for (int i = 0; i < numTeams; i++) {
            teamRosters.Add(new Dictionary<int, Transform>());  //initialize the list of players for this team
            visibleEnemies.Add(new Dictionary<int,Transform>());//initialize the list of visible enemy players for this team
            createVIP(i);                                       //create a VIP
            for (int j = 0; j < teamSize; j++) {                //create the appropriate number of players
                createPlayer(i, j);
            }
        }
        playerPrefab.gameObject.SetActive(false); // disable the prefabs
        VIPPrefab.gameObject.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
        enemiesInSight();   //calculates, for each team, which enemies are in line of sight (therefore can be taken into account for swarm movement)
    }

    private void createVIP(int i) {
        Transform v = (Transform)Instantiate(VIPPrefab, teamStartPositions[i], Quaternion.identity);    //instantiate a VIP for this team
        v.GetComponent<Renderer>().material.color = teamColours[i];                                     //set the VIP's color
        v.tag = "VIP" + i.ToString();                                                                   //set the VIP's tag
        VIPs[i] = v;
        for (int j = 0; j < numTeams; j++) {
//            Debug.Log(j);
            visibleVIPs[j].Add(v, false);
        }
    }

    private void createPlayer(int team, int ID) {
        //create new member of team, within a certain radius of start position
        Vector3 myPosition = teamStartPositions[team];
        myPosition.x += Random.Range(-startRadius, startRadius); //the x and z coordinates are randomized within the circle
        myPosition.z += Random.Range(-startRadius, startRadius);
        Transform p = (Transform)Instantiate(playerPrefab, myPosition, Quaternion.identity); //instantiate the prefab
        p.GetComponent<Renderer>().material.color = teamColours[team]; //set the color depending on team
        p.GetComponent<AILogic>().teamID = team;   //set the teamID so they can identify friendlies
        p.GetComponent<AILogic>().myID = ID;     //set the personal ID, so they do not weight themselves in the attraction to friendlies
        p.tag = "team" + team.ToString();          //set the tag of the new player
        teamRosters[team].Add(ID, p);               //add the player to the gamewide roster
//        Debug.Log("Player " + ID + " created and added to team " + team);
    }

    public int fieldOfViewAngle = 110;
    public int viewDistance = 70;

    private void enemiesInSight() {

        for (int i = 0; i < numTeams; i++) {                                                //for each team
            visibleEnemies[i].Clear();
            foreach (KeyValuePair<int, Transform> player in teamRosters[i]) {               //for each player on that team
                for (int j = 0; j < numTeams && j != i; j++) {                              //for each enemy team
/*                    foreach (Transform enemyVIP in VIPs) {                                  //for each enemy VIP -- currently returning "Reference not set to instance of an object error"
                        myLineOfSight(player.Value, enemyVIP);
                    } */
                    foreach (KeyValuePair<int, Transform> enemy in teamRosters[j]) {         //for each player on the enemy team
                        myLineOfSight(player.Value, enemy.Value);
                    }
                }
            }
        }
    }

    private void myLineOfSight(Transform player, Transform enemy) {
        string enemyTag = enemy.tag;
        int playerTeamID = player.GetComponent<AILogic>().teamID;
        Vector3 direction = player.position - enemy.position;               //calculate the distance to the enemy
        if (direction.magnitude < viewDistance) {                           //if the enemy is within range
            float angle = Vector3.Angle(direction, transform.forward);      //calculate the angle between the way player is looking, and the position of the enemy
            if (angle < fieldOfViewAngle / 2) {                             //if the enemy is within field of view
                RaycastHit hit;                                             //create a raycast
                if (Physics.Raycast(transform.position + transform.up, direction.normalized, out hit, viewDistance)) {  //shoot the ray towards the enemy
                    if (hit.transform.tag.Equals(enemyTag)) {               //if the ray hits the player on the enemy team
                        if (enemy.tag.Contains("VIP")) {
                            visibleVIPs[playerTeamID][enemy] = true;
                            return;
                        } 
                        else { 
                            int key = hit.transform.GetComponent<AILogic>().myID;
                            if (!visibleEnemies[playerTeamID].ContainsKey(key)) {
                                visibleEnemies[playerTeamID].Add(key, hit.transform);
                            }
                        }
                    }
                }
            }
        }
    }
}