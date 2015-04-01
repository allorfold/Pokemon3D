using UnityEngine;
using System.Collections;

public class groundBallCollision : MonoBehaviour {
	
	public bool isTouchingGround = false;

	void OnCollisionEnter(Collision collisionInfo) {
		if (collisionInfo.collider.name == "middleEarth") {
			isTouchingGround = true;
		}
	}
}
