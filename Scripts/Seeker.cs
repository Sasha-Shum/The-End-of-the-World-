using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class Seeker : Vehicle {

	//store the target
	public GameObject target;
	//private obstacles array
	private GameObject[] obstacles;
	
	//get a reference to the GameManager
	private GameManager gm;
	
	//weights for steering forces
	public float seekWt = 75.0f;
	public float avoidWt = 10.0f;
	public float avoidDist = 5.0f;
	//flocking weights

	public float boundsWt = 100f;


	bool shouldWander = true; // check if zombie should still wander

	Vector3 center; // center of my map




	// Raycasting. Used to detect the target
	public float rayCastingHitDistance = 40f;
	public float ViewDegree = 90f;
	private bool foundHit = false;
	RaycastHit hit;



	// Use this for initialization
	override public void Start () {
		base.Start();
		obstacles = GameObject.FindGameObjectsWithTag("Obstacle");
		gm = GameObject.Find("MainGo").GetComponent<GameManager>();
		center = new Vector3(257.7f, 0f, 164.8f);
		center.y = 1f;
	}
	
	protected override void CalcSteeringForces() {
		//get a force vector
		Vector3 force = Vector3.zero;


		// Wander if you don't see anyone
		if (shouldWander == true)
		{
			force += Wander() * 70f;
		}
		this.DetectCollistion (); // detect if you collide with someone
		// Search for the target. 
		if(target != null)
		{
			Vector3 rayDirection = target.transform.position - transform.position;
			rayDirection.y = 2f;
			if ((Vector3.Angle (rayDirection, transform.forward)) <= ViewDegree / 2f)
			{
				if(Physics.Raycast(transform.position, rayDirection, out hit, rayCastingHitDistance))
				{ 
					if(hit.transform.gameObject.tag == "Humanoid")
					{
						Debug.DrawRay(transform.position, rayDirection, Color.magenta);
						shouldWander = false;
						force += pursue (target.transform.position) * seekWt; 
						Debug.Log("I SEE THE HUMAN. EAT");
					}
					if(hit.transform.gameObject.tag == "Player")
					{
						shouldWander = false;
						force += pursue(target.transform.position) * seekWt;
						Debug.Log ("I See the player");
			
					}
					else
					{
						shouldWander = true;
					
					}

					
				}
			}
		}

	
		
		//Boundries

		if (offStage ()) {
			

			
			force += Seek(center) * boundsWt;
			
		}



		//WILL AVOID OBSTACLES AND ADD THAT TO FORCE
		//DO STUFF HERE!!!one!!1

		for (int i = 0; i < obstacles.Length; i++ ) {
			force += AvoidObstacle(obstacles[i], avoidDist) * avoidWt;

		}


		//limit Force and apply
		force = Vector3.ClampMagnitude(force, maxForce);
		ApplyForce(force); 


		//Draw debug lines
		//show force as a blue line pushing the guy like a jet stream
		//Debug.DrawLine(transform.position, transform.position - force, Color.blue);
		//red debug line showing the target
		//Debug.DrawLine(transform.position, target.transform.position, Color.red);

	}

	// Get's the closest target
	public GameObject getClosestTarget()
	{
		float distance = 100000f;
		GameObject closeHuman = null;
		List<GameObject> hum = gm.Humans;
		foreach (GameObject humAn in hum)
		{
			float tempDistance = Vector3.Distance(humAn.transform.position, transform.position);
			float tempPlayerDistance = Vector3.Distance(GameObject.FindGameObjectWithTag("Player").transform.position, transform.position);
			if(tempDistance < distance)
			{
				if(tempPlayerDistance < tempDistance)
				{
					distance = tempPlayerDistance;
					closeHuman = GameObject.FindGameObjectWithTag("Player");
				}
				else
				{
					distance = tempDistance;
					closeHuman = humAn;
				}

			}
		}

		return closeHuman;
	}

	// Detects the collision 
 void DetectCollistion()
	{

		if (Vector3.Distance (transform.position,GameObject.FindGameObjectWithTag("Player").transform.position) < 2) 
		{
			Application.LoadLevel(Application.loadedLevelName);
		}


			foreach (GameObject human in new List<GameObject>(GameManager.humans)) 
			{
				if (Vector3.Distance (transform.position, human.transform.position) < 3) 
				{
					Destroy (human);
					GameManager.humans.Remove (human);
				}
			}


	}
	// Check if zombie is out off Stage
	protected bool offStage()
	{
		//GameObject floor = GameObject.FindGameObjectWithTag ("Floor");
		bool off = false;


		if (transform.position.x > 393.56f) {
			off = true;
		} else if (transform.position.x < 98f) {
			off = true;
		} else if (transform.position.z > 294.08f) {
			off = true;
		} else if (transform.position.z < 9f) {
			off = true;
		}
		return off;

	}



}
