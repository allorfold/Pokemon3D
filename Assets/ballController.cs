using UnityEngine;
using System.Collections;

public class ballController : MonoBehaviour {
	public static GameObject pokeball;
	public bool isPickedUp = false;
	public bool isBallToPickUp;
	public Transform ballInit;

	// Use this for initialization
	void Start () {
		pokeball = GameObject.FindGameObjectWithTag ("pokeball");
		pokeball.gameObject.AddComponent<handBallCollision>();
		isBallToPickUp = true;
	}
	
	// Update is called once per frame
	void Update () {
		if (pokeball.GetComponent<handBallCollision> ().pointsOfContact >= 3) {
			isPickedUp = true;
		}



		if (isPickedUp && isBallToPickUp) {
			createBallInHand();
		}
	}

	void createBallInHand() {
		GameObject palm = GameObject.FindGameObjectWithTag ("palm");
		if (palm != null) {
			GameObject ballTemp = (GameObject)Instantiate (ballInit.gameObject,new Vector3(0,0,0),Quaternion.identity);
			ballTemp.transform.parent = GameObject.Find("Wrist_00").transform;
			ballTemp.transform.position = new Vector3((float)-0.3, (float)-0.4,(float) -0.5);
			isBallToPickUp = false;
			Destroy(this.gameObject);
		}
	}
}

// 