using UnityEngine;
using System.Collections;

public class FSM1 : MonoBehaviour {
	/* Since these values are going to be multiplied against Vector3's and floats, 2 is going to mean a very strong influence, 1 is no influence
	 * and less than 1 means a smaller influence.
	 * 
	 * 
	 * 
	 * 
	 */ 
	public bool isCalled, isFriendVIP, isEnemyVIP, isAgressive, isDefensive, isBad, isPatrol; // These booleans define the transitions between our states.
	public float enemyVIP, ourVIP, enemySoldier, friend, health, distance; // These are the floats that will be applied to the Vector3's that swarm and FuSM will give
	public Vector3 caller; // This is the only vector that is dependent entirely upon a state. it is the postision of the boid calling for help
	private Animator anim;

	public bool isTesting;

	void Start(){
		anim = GetComponent<Animator> ();
		caller = Vector3.zero;
		isCalled = false;
		isFriendVIP = false;
		isEnemyVIP = false;
		isAgressive = false;
		isDefensive = false;
		isBad = false;
		isPatrol = false;
		isTesting = false;

	}
	void Update(){
		AnimatorStateInfo state = anim.GetCurrentAnimatorStateInfo (0);

		if (state.IsName("Patrol"))
			PatrolHandler();
		if (state.IsName ("Defensive"))
			DefenseHandler ();
		if (state.IsName ("Agressive"))
			AgressiveHandler ();
		if (state.IsName ("Helping"))
			HelpHandler ();
		if (state.IsName ("DefendVIP"))
			DefendVIPHandler ();
		if (state.IsName ("AttackVIP"))
			EnemyVIPHandler ();
		if (state.IsName ("Broken"))
			DefeatedHandler ();
		anim.SetBool ("isDefensive", isDefensive);
		anim.SetBool ("isCalled", isCalled);
		anim.SetBool ("isFriendVIP", isFriendVIP);
		anim.SetBool ("isAgressive", isAgressive);
		anim.SetBool ("isBad", isBad);
		anim.SetBool ("isEnemyVIP", isEnemyVIP);
		anim.SetBool ("isPatrol", isPatrol);
			
	}


	public void DefenseHandler(){
		if (isTesting)
			Debug.Log ("Defense state");
		enemyVIP = 0.25f;
		ourVIP = 1.5f;
		enemySoldier = 0.95f;
		friend = 1.9f;
		health = 1f;
		distance = 1.2f;
		caller = Vector3.zero;
	}
	public void AgressiveHandler(){
		if (isTesting)
			Debug.Log ("Agressive state");
		enemyVIP = 1.6f;
		ourVIP = 0.75f;
		enemySoldier = 1.8f;
		friend = 1.25f;
		health = 0.95f;
		distance = 1.0f;
		caller = Vector3.zero;
	}
	public void EnemyVIPHandler(){
		if (isTesting)
			Debug.Log ("Enemy VIP state");
		enemyVIP = 2f;
		ourVIP = 0.5f;
		enemySoldier = 1.1f;
		friend = 1.1f;
		health = 1.0f;
		distance = 1.0f;
		caller = Vector3.zero;
	}
	public void HelpHandler(){
		//need to return vector3 of allied soldier
		if (isTesting)
			Debug.Log ("Helping state");
		caller = gameObject.transform.position;
		enemyVIP = 1.0f;
		ourVIP = 1.0f;
		enemySoldier = 1.0f;
		friend = 1.0f;
		health = 1.0f;
		distance = 1.0f;
	}
	public void DefendVIPHandler(){
		if (isTesting)
			Debug.Log ("Defend VIP state");
		enemyVIP = 0.25f;
		ourVIP = 2.0f;
		enemySoldier = 1.25f;
		friend = 0.95f;
		health = 1.0f;
		distance = 1.5f;
		caller = Vector3.zero;
	}
	public void DefeatedHandler(){
		if (isTesting)
			Debug.Log ("Defeated State");
		enemyVIP = 0.25f;
		ourVIP = 0.25f;
		enemySoldier = 2.0f;
		friend = 0.25f;
		health = 2.0f;
		distance = 1.75f;
		caller = Vector3.zero;
	}
	public void PatrolHandler(){
		if (isTesting)
			Debug.Log ("Patrol State");
		enemyVIP = 1.0f;
		ourVIP = 1.0f;
		enemySoldier = 1.0f;
		friend = 1.0f;
		health = 1.0f;
		health = 1.0f;
		distance = 1.0f;
		caller = Vector3.zero;
	}
	
}