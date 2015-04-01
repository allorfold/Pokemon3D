using UnityEngine;
using System.Collections;

public class handBallCollision : MonoBehaviour {

	public bool isHandTouching = false;
	public int pointsOfContact = 0;

	void OnCollisionEnter(Collision collisionInfo) {
		if (collisionInfo.collider.name == "bone3" || collisionInfo.collider.name == "bone2") {
			isHandTouching = true;
			pointsOfContact++;
		}
	}
		
	void OnCollisionExit(Collision collisionInfo) {
		if (collisionInfo.collider.name == "bone3" || collisionInfo.collider.name == "bone2") {
			isHandTouching = false;
			pointsOfContact--;
		}
	}
}
