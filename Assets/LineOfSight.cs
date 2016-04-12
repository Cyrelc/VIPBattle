using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LineOfSight : MonoBehaviour {

    public AILogic myParent;

    void Start() {
        myParent = transform.parent.GetComponent<AILogic>();
        transform.GetComponent<Collider>().enabled = true;
    }

    void OnTriggerEnter(Collider other) {
        if (other.tag.Equals("Wall")) {
            myParent.wallObjects.Add(other.GetInstanceID(), other.transform);
        }
    }

    void OnTriggerExit(Collider other) {
        if (other.tag.Equals("Wall")) {
            if (myParent.wallObjects.ContainsKey(other.GetInstanceID())) {
                myParent.wallObjects.Remove(other.GetInstanceID());
            }
        }
    }
}