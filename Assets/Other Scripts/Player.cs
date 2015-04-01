using UnityEngine;
using System.Collections;

using UnityEngine.UI;
using System.IO;

using System.Text.RegularExpressions;
using Pose = Thalmic.Myo.Pose;

public class Player : MonoBehaviour {

	public GameObject meowthTacklePrefab;
	public GameObject tacklePrefab;
	public GameObject thunderAttackPrefab;
	public GameObject fireAttackPrefab;

	public int theDamages;
	public int friendlyHP = 100;
	public int currentFriendlyHP = 100;
	public int enemyHP = 100;
	public int currentEnemyHP = 100;

	public float ballForce = 10000;
	public bool doesBallExist = true;

	public bool moveSelected = false;
	public bool isPickedUp = false;
	public bool isBallToPickUp = true;
	public bool isBallThrowable = false;

	public GameObject myo = null;
	protected ThalmicMyo myoController = null;


	public Transform pokeball;
	public Transform pokeball_pika;
	public Transform pokeball_squirt;
	public Transform pokeball_char;
	public Transform pokeball_bulb;
	
	//////////////////
	public bool fore = false;
	public bool isThrowing;

	public Vector3[] aggz;
	public Vector3[] jerks;
	
	public double t2;
	
	public double grav = 5;
	
	public Vector3 aggtest;

	public int aggtime = 10;
	public double aggmag = 2;
	
	private float projectileTimer = 0;

	public bool hasBeenThrown = false;

	public bool thrownBallScriptAttached = false;

	public GameObject pokeLight;

	public bool pokeballAnimating = false;

	public bool locked = false;
	public float gameTime;
	public float timeOfAniStart = -99F;

	public GameObject pokeArea;


	public GameObject pikachu;
	public GameObject squirtle;
	public GameObject charmander;
	public GameObject bulbasaur;
	public GameObject meowth;

	public int numberOfPokeballs = 4;
	public Transform[] pokeballs; 

	public Transform pokemonChosen;


	public bool isBattling = false;
	public bool justStartedBattling = true;

	public bool canAttack = true;
	private Pose _lastPose = Pose.Unknown;

	public bool isRightHighlighted = true;
	public bool isLeftHighlighted = false;

	public string specialAttack;


	// Use this for initialization
	void Start () {
		if(!isBattling) 
		aggz = new Vector3[aggtime];
		jerks = new Vector3[aggtime];

		pokeballs = new Transform[4];
		pokeballs [0] = pokeball_bulb;
		pokeballs [1] = pokeball_squirt;
		pokeballs [2] = pokeball_char; 
		pokeballs [3] = pokeball_pika;

		myoController = myo.GetComponent<ThalmicMyo> ();

		for(int pBall = 0; pBall < numberOfPokeballs; pBall++) {
			pokeballs[pBall].gameObject.AddComponent<handBallCollision>();			
		}

	}
	
	public int quetzelcoatl = 0;

	// Update is called once per frame
	void Update () {
		if (locked) {
			return;
		}
		if (!isBattling) {
			fore = false;
			isThrowing = false;

			Vector3 accl = myoController.accelerometer;
			int prev = quetzelcoatl;
			quetzelcoatl++;
			if (quetzelcoatl >= aggtime)
				quetzelcoatl = 0;
			aggz [quetzelcoatl] = accl;
			
			Vector3 aggdif = aggz [quetzelcoatl] - aggz [prev];
			Vector3 jerk = aggdif / Time.deltaTime;
			
			jerks [quetzelcoatl] = jerk;
			
			Vector3 deltaJ = Vector3.zero;
			Vector3 deltaJ2 = Vector3.zero;
			
			for (int i=0; i<aggtime; i++) {
				deltaJ += jerks [i];
			}
			
			deltaJ = deltaJ / aggtime;
			aggtest = deltaJ;
			
			t2 = Mathf.Sqrt ((deltaJ.x * deltaJ.x) + (deltaJ.y * deltaJ.y));
			
			if (deltaJ.z > aggmag && Mathf.Sqrt ((deltaJ.x * deltaJ.x) + (deltaJ.y * deltaJ.y)) < grav) {
				fore = true;
			}
			isThrowing = fore;

			if (isBallToPickUp) {
				for (int pBall = 0; pBall < numberOfPokeballs; pBall++) {
					if (isBallToPickUp) {
						if (pokeballs [pBall].gameObject.GetComponent<handBallCollision> ().pointsOfContact >= 1) {
							isBallToPickUp = false;
							isPickedUp = true;
							pokemonChosen = pokeballs [pBall];
							if (pokemonChosen.name == "MainPokeball_squirtle") {
								pokemonChosen = squirtle.transform;
							} else if (pokemonChosen.name == "MainPokeball_charmander") {
								pokemonChosen = charmander.transform;
							} else if (pokemonChosen.name == "MainPokeball_pikachu") {
								pokemonChosen = pikachu.transform;
							} else if (pokemonChosen.name == "MainPokeball_bulbasaur") {
								pokemonChosen = bulbasaur.transform;
							} else {
								Debug.Log (pokemonChosen.name);
							}
							createBallInHand ();
						}
					}
				}
			}

			if (isPickedUp && !hasBeenThrown) {
				GameObject palm = GameObject.FindGameObjectWithTag ("palm");
				if (palm == null) {
					doesBallExist = false;
				} else if (palm != null && !doesBallExist) {
					recreateBallInHand ();
					doesBallExist = true;
					if (isBallThrowable) {
						GameObject ball = GameObject.FindGameObjectWithTag ("pokeball");
						ball.transform.localScale = new Vector3 (.05F, .05F, .05F);
					}
				}
			
				// Myo stuff (gesture)
				if (!myo && !myoController)
					return;

				if (myoController.pose != _lastPose) {
					_lastPose = myoController.pose;
				
					if (myoController.pose == Pose.DoubleTap && doesBallExist) {
						GameObject ball = GameObject.FindGameObjectWithTag ("pokeball");
						if (!isBallThrowable) {
							// Increase ball size
							ball.transform.localScale = new Vector3 (.05F, .05F, .05F);
							isBallThrowable = true;
						} else if (isBallThrowable) {
							// Decrease ball size
							ball.transform.localScale = new Vector3 (.022F, .022F, .022F);
							isBallThrowable = false;
						}
					}
				}

				if (isBallThrowable && !doesBallExist && isThrowing) {
					throwBall ();
					hasBeenThrown = true;
					isBallThrowable = false;
				}
			}

			GameObject thrownBall = GameObject.FindGameObjectWithTag ("thrownPokeball");
			if (thrownBall != null) {
				if (!thrownBallScriptAttached) {
					thrownBallScriptAttached = true;
					thrownBall.AddComponent<groundBallCollision> ();
				} else { 
					if (thrownBall.GetComponent<groundBallCollision> ().isTouchingGround) {
						// Do stuff
						createBallOnGround ();
					}
				}
			}

			// Check time
			gameTime = Time.time;
			if (pokeballAnimating) {
				if (timeOfAniStart == -99F) {
					timeOfAniStart = gameTime;
				} else if (timeOfAniStart < gameTime - 2) {
					destroyBallAndLight ();
					pokeballAnimating = false;
				}
			}
		} else {

			if (!canAttack && currentFriendlyHP > 0) {
				aiplay();
			}
			if (canAttack && (currentFriendlyHP > 0)) {
				if (justStartedBattling) {
					
					GameObject.FindGameObjectWithTag ("hpText").GetComponent<Text> ().text = "Your Pokemon's HP:";
					GameObject.FindGameObjectWithTag ("hp").GetComponent<Text> ().text = currentFriendlyHP.ToString ();
					GameObject.FindGameObjectWithTag ("enemyhpText").GetComponent<Text> ().text = "Enemy Pokemon's HP:";
					GameObject.FindGameObjectWithTag ("enemyhp").GetComponent<Text> ().text = currentEnemyHP.ToString ();
					GameObject.Find ("rightBut").GetComponent<SpriteRenderer> ().sprite = Resources.Load<Sprite> ("tackleButton");
					if (pokemonChosen == bulbasaur.transform) {
						specialAttack = "solarbeam";
						GameObject.Find ("leftBut").GetComponent<SpriteRenderer> ().sprite = Resources.Load<Sprite> ("solarbeamButton");
					} else if (pokemonChosen == pikachu.transform) {
						specialAttack = "thunderbolt";
						GameObject.Find ("leftBut").GetComponent<SpriteRenderer> ().sprite = Resources.Load<Sprite> ("thunderboltButton");
					} else if (pokemonChosen == charmander.transform) {
						specialAttack = "flamethrower";
						GameObject.Find ("leftBut").GetComponent<SpriteRenderer> ().sprite = Resources.Load<Sprite> ("flamethrowerButton");
					} else if (pokemonChosen == squirtle.transform) {				
						specialAttack = "hydropump";
						GameObject.Find ("leftBut").GetComponent<SpriteRenderer> ().sprite = Resources.Load<Sprite> ("hydroPumpButton");
					}
				}
				play ();
			}
		}
	}

	void nullifyLRbuts() {
		GameObject.Find ("centerOutline").GetComponent<SpriteRenderer> ().sprite = Resources.Load<Sprite> ("outlineBox");
		GameObject.Find("outlineLeft").GetComponent<SpriteRenderer>().sprite = null;
		GameObject.Find("outlineRight").GetComponent<SpriteRenderer>().sprite = null;
		GameObject.Find("leftBut").GetComponent<SpriteRenderer>().sprite = null;
		GameObject.Find ("rightBut").GetComponent<SpriteRenderer> ().sprite = null;
	}

	void destroyBallAndLight() {
		GameObject pLight = GameObject.FindGameObjectWithTag ("pokeLight");
		GameObject gBall = GameObject.FindGameObjectWithTag ("groundBall");
		DestroyImmediate(gBall.gameObject, true);
		DestroyImmediate(pLight.gameObject, true);
		isBattling = true;
	}

	// ignore for now
	void throwBall(){
		GameObject throwBall = (GameObject)Instantiate (pokeball.gameObject, GameObject.Find("ballSpawn").transform.position, Quaternion.identity);
		throwBall.transform.localScale = new Vector3(.07F, .07F, .07F);
		throwBall.GetComponent<Animation>().enabled = false;
		throwBall.AddComponent<Rigidbody>();
		throwBall.GetComponent<Rigidbody> ().AddForce (ballForce * GameObject.Find("CenterEyeAnchor").transform.forward);
		throwBall.AddComponent<SphereCollider> ();
		throwBall.GetComponent<SphereCollider> ().center = new Vector3 (0, .75f, 0);
		throwBall.GetComponent<SphereCollider> ().radius = .75F;
		throwBall.tag = "thrownPokeball";
	}

	void createBallInHand() {	
		GameObject palm = GameObject.FindGameObjectWithTag ("palm");
		if (palm != null) {
			GameObject ballTemp = (GameObject)Instantiate (pokeball.gameObject,GameObject.Find("pokeballHandSpawnPoint").transform.position, Quaternion.identity);
			ballTemp.gameObject.tag = "pokeball";
			ballTemp.GetComponent<Animation>().playAutomatically = false;
			ballTemp.transform.parent = palm.transform;
			ballTemp.AddComponent<Rigidbody>();
			ballTemp.GetComponent<Rigidbody>().useGravity = false;
			ballTemp.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
			ballTemp.GetComponent<Rigidbody>().angularDrag = 0;
			ballTemp.transform.localScale = new Vector3(.022F, .022F, .022F);
			ballTemp.transform.position += new Vector3(0, -.055F, .01F);
			ballTemp.GetComponent<Animation>().enabled = false;
			ballTemp.GetComponent<Animation>().playAutomatically = false;

			DestroyImmediate(GameObject.Find("charmander_stand").gameObject, true);
			DestroyImmediate(GameObject.Find("pika_stand").gameObject, true);
			DestroyImmediate(GameObject.Find("squirtle_stand").gameObject, true);
			DestroyImmediate(GameObject.Find("bulba_stand").gameObject, true);

			DestroyImmediate(GameObject.Find("BALLS").gameObject, true);

			
			GameObject.Find ("middleEarth").transform.position = new Vector3(.25F, 0, -21.95F);

		}
	}

	void recreateBallInHand() {
		GameObject palm = GameObject.FindGameObjectWithTag ("palm");
		if (palm != null) {
			GameObject ballTemp = (GameObject)Instantiate (pokeball.gameObject,GameObject.Find("pokeballHandSpawnPoint").transform.position,Quaternion.identity);
			ballTemp.gameObject.tag = "pokeball";
			ballTemp.GetComponent<Animation>().playAutomatically = false;
			ballTemp.transform.parent = palm.transform;
			ballTemp.AddComponent<Rigidbody>();
			ballTemp.GetComponent<Rigidbody>().useGravity = false;
			ballTemp.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
			ballTemp.GetComponent<Rigidbody>().angularDrag = 0;
			ballTemp.transform.localScale = new Vector3(.022F, .022F, .022F);
			ballTemp.GetComponent<Animation>().enabled = false;
			ballTemp.GetComponent<Animation>().playAutomatically = false;
		}
	}

	void createBallOnGround() {

		GameObject.Find("Audio").GetComponent<AudioSource>().enabled = false;
		GameObject.Find("Audio2").GetComponent<AudioSource>().Play();

		GameObject thrownBall = GameObject.FindGameObjectWithTag ("thrownPokeball");
		GameObject groundBall = (GameObject)Instantiate (pokeball.gameObject,GameObject.FindGameObjectWithTag("thrownPokeball").transform.position, Quaternion.identity);
		DestroyImmediate(thrownBall.gameObject, true);
		groundBall.tag = "groundBall";
		groundBall.transform.position += new Vector3 (0, .5F, 0);
		groundBall.transform.forward = groundBall.transform.position - GameObject.Find ("ballSpawn").transform.position;
		groundBall.transform.localScale = new Vector3(.07F, .07F, .07F);
		GameObject light = (GameObject)Instantiate (pokeLight.gameObject, groundBall.transform.position, Quaternion.identity);
		light.GetComponent<ParticleSystem> ().loop = false;
		light.tag = "pokeLight";
		pokeballAnimating = true;
		GameObject pokemonArea = (GameObject)Instantiate (pokeArea.gameObject, new Vector3(groundBall.transform.position.x, .05F, groundBall.transform.position.z), Quaternion.identity);

		GameObject MR_GAMENWATCH = (GameObject)Instantiate (pokemonChosen.gameObject, new Vector3(groundBall.transform.position.x, 0, groundBall.transform.position.z), Quaternion.identity);
		MR_GAMENWATCH.transform.forward = GameObject.Find ("CenterEyeAnchor").transform.forward; 
		MR_GAMENWATCH.transform.position += new Vector3 (0, .31F, 0);
		GameObject MEOWTH_THATS_RIGHT = (GameObject)Instantiate (meowth.gameObject, new Vector3(groundBall.transform.position.x, 0, groundBall.transform.position.z) + new Vector3(groundBall.transform.position.x, 0, groundBall.transform.position.z) - new Vector3(GameObject.Find("OVRPlayerController").transform.position.x, 0, GameObject.Find("OVRPlayerController").transform.position.z), Quaternion.identity);
		MEOWTH_THATS_RIGHT.tag = "MR_GAMENWATCH";
		GameObject.Find ("BR_Squirtle").GetComponent<AudioSource> ().Play ();
		MR_GAMENWATCH.tag = "charmander";

		MEOWTH_THATS_RIGHT.transform.position += new Vector3 (0, 0.373F, 0);
		GameObject meowthArea = (GameObject)Instantiate (pokeArea.gameObject, new Vector3(MEOWTH_THATS_RIGHT.transform.position.x, .05F, MEOWTH_THATS_RIGHT.transform.position.z), Quaternion.identity);

		MEOWTH_THATS_RIGHT.transform.forward = GameObject.Find ("CenterEyeAnchor").transform.forward ; //new Vector3(GameObject.Find ("ballSpawn").transform.position.x, 0, GameObject.Find ("ballSpawn").transform.position.z) - new Vector3(MR_GAMENWATCH.transform.position.x, 0, MR_GAMENWATCH.transform.position.z);
		MEOWTH_THATS_RIGHT.transform.RotateAround (Vector3.up, Mathf.PI);
	}

	
	
	int damageCalculation(int attackType) {
		int damageDealt = 0;
		int power = Random.Range (1, 4);
		int power2 = Random.Range(1,3);
		if(attackType == 1) {;
			if(power == 1) {
				damageDealt = 15;
			}
			else if(power == 2) {
				damageDealt = 25;
			}
			else if(power == 3){
				damageDealt = 30;
			}
		}
		else if(attackType == 2) {
			if(power2 == 1) {
				damageDealt = 15;
			}
			else if(power2 == 2) {
				damageDealt = 30;
			}
		}
		else {
			return 15;
		}
		return damageDealt;
	}
	
	
	int aiDamage() {
		int damageDealt = 0;
		int power = Random.Range (1, 4);
		if (power == 1) {
			damageDealt = 15;
		}
		else if (power == 2) {
			damageDealt = 20;
		}
		else if(power == 3) {
			damageDealt = 25;
		}
		return damageDealt;
	}
	
	int updateMyHealth(int health, int damage) {
		return health - damage;
	}

	void endGame(int winner) {
		if (winner == 1) {
		} else if (winner == 2) {
		}
		Application.LoadLevel (1);
	}

	public IEnumerator takeFive() {
		locked = true;
		yield return new WaitForSeconds (5);
		GameObject.Find ("centerBut").GetComponent<SpriteRenderer> ().sprite = null;
		GameObject.Find ("centerOutline").GetComponent<SpriteRenderer> ().sprite = null;
		locked = false;
	}

	void play() {
			if(isLeftHighlighted) {
				GameObject.Find("outlineLeft").GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("outlineBox");
				GameObject.Find("outlineRight").GetComponent<SpriteRenderer>().sprite = null;
			} else if(isRightHighlighted) {
				GameObject.Find("outlineRight").GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("outlineBox");
				GameObject.Find("outlineLeft").GetComponent<SpriteRenderer>().sprite = null;
			}
			
			if(myoController.pose == Pose.WaveIn && isRightHighlighted) {
				isLeftHighlighted = true;
				isRightHighlighted = false;
			} else if(myoController.pose == Pose.WaveOut && isLeftHighlighted) {
				isLeftHighlighted = false;
				isRightHighlighted = true;
			} else if(myoController.pose == Pose.DoubleTap && moveSelected == false) {
				moveSelected = true;
				locked = true;
				myoController.pose = Pose.Fist;
				if(isLeftHighlighted && canAttack) {
					GameObject.Find ("centerBut").GetComponent<SpriteRenderer>().sprite = GameObject.Find ("leftBut").GetComponent<SpriteRenderer>().sprite;
					nullifyLRbuts();
					theDamages = damageCalculation(1);
					currentEnemyHP = currentEnemyHP - theDamages;
					if(currentEnemyHP < 0) {
						currentEnemyHP = 0;
					}
					GameObject.FindGameObjectWithTag ("enemyhp").GetComponent<Text>().text = currentEnemyHP.ToString();
					theDamages = 0;
					StartCoroutine(takeFive());
					canAttack = false;
					if(currentEnemyHP <= 0) {
						endGame(1);
					}

					if(specialAttack == "thunderbolt") {
						GameObject thunda = (GameObject)Instantiate(thunderAttackPrefab, GameObject.FindGameObjectWithTag("MR_GAMENWATCH").transform.position, Quaternion.identity);
						thunda.tag = "thunda";
					} else if(specialAttack == "flamethrower") {
					GameObject flamer = (GameObject)Instantiate(fireAttackPrefab, GameObject.FindGameObjectWithTag("charmander").transform.position, Quaternion.identity);
					flamer.transform.forward = -GameObject.FindGameObjectWithTag("MR_GAMENWATCH").transform.forward;
					//flamer.tag = "flamer";
				}
				
					
				} else if(isRightHighlighted && canAttack) {
					GameObject.Find ("centerBut").GetComponent<SpriteRenderer>().sprite = GameObject.Find ("rightBut").GetComponent<SpriteRenderer>().sprite;
					nullifyLRbuts();
					theDamages = damageCalculation(2);
					currentEnemyHP = currentEnemyHP - theDamages;
					if(currentEnemyHP < 0) {
						currentEnemyHP = 0;
					}
					GameObject.FindGameObjectWithTag ("enemyhp").GetComponent<Text>().text = currentEnemyHP.ToString ();
					theDamages = 0;
					StartCoroutine(takeFive());
					canAttack = false;
					if(currentEnemyHP <= 0) {
						endGame(1);
					}

					GameObject tackle = (GameObject)Instantiate(tacklePrefab, GameObject.FindGameObjectWithTag("MR_GAMENWATCH").transform.position, Quaternion.identity);
				}
			}
	 else {
			if(currentEnemyHP<=0) {
				endGame (1);
			}
			// Do something i guess
		}


	}

	void aiplay() {
		if (GameObject.FindGameObjectWithTag ("thunda") != null) {
			GameObject.FindGameObjectWithTag ("thunda").SetActive (false);
		}
		locked = true;
		theDamages = aiDamage();
		currentFriendlyHP	= currentFriendlyHP - theDamages;
		if(currentFriendlyHP < 0) {
			currentFriendlyHP = 0;
		}
		GameObject.FindGameObjectWithTag ("hp").GetComponent<Text>().text = currentFriendlyHP.ToString();
		theDamages = 0;
		StartCoroutine(takeFive());
		moveSelected = false;
		GameObject mtackle = (GameObject)Instantiate (meowthTacklePrefab, GameObject.FindGameObjectWithTag ("charmander").transform.position, Quaternion.identity);
		canAttack = true;
		if(currentFriendlyHP<=0) {
			endGame (2);
		} else {
			if (currentFriendlyHP<=0) {
				endGame (2);
		}
		}
	}

	

}






