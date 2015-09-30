using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class Fleer : Vehicle {

	//store the target
	public GameObject target;
	//private obstacles array
	private GameObject[] obstacles;
	
	//get a reference to the GameManager
	private GameManager gm;
	
	//weights for steering forces
	public float seekWt = 75.0f;
	public float fleeWt = 90.0f;
	public float avoidWt = 10.0f;
	public float avoidDist = 20.0f;
	//flocking weights
	public float separationWt = 20f;
	public float separationDist = 2f;
	public float cohesionWt = 1f;
	public float alignmentWt = 1f;
	public float boundsWt = 100f;

	Vector3 center;

	bool shouldIWander = true;

	bool readyToEscape = false;

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
		hit = new RaycastHit ();

	}
	
	protected override void CalcSteeringForces() {
		//get a force vector
		Vector3 force = Vector3.zero;
		Debug.DrawLine (transform.position, transform.position + transform.forward * 10, Color.red);


		if (shouldIWander == true)
		{
			force += Wander() * 70f;
		}
		Vector3 rayDirection = target.transform.position - transform.position;
		rayDirection.y = 2f;
		if ((Vector3.Angle (rayDirection, transform.forward)) <= ViewDegree) {
			if(Physics.Raycast(transform.position, rayDirection, out hit, rayCastingHitDistance))
			{ 
				if(hit.transform.gameObject.tag == "Dead")
				{

					shouldIWander = false;
					Debug.Log("RUN AWAY");
					force += evade (target.transform.position) * fleeWt;
					Debug.DrawRay(transform.position, rayDirection, Color.magenta);


				}
				else
				{
					shouldIWander = true;
				}

			}
		}

		if (Doors.doorOpen == true)
		{
			shouldIWander = false;
			force += Arrive(GameObject.FindGameObjectWithTag("SaveZone").transform.position) * seekWt;
		}
	
	

		
		//Boundries

		Debug.Log ("OffStage: " + offStage () + "| ReadyToEscape:" + readyToEscape);
		if (offStage () && readyToEscape == false) {
			
			//Debug.Log("HERE1");
			
			force += Seek (center) * boundsWt;
			Debug.Log("HERE");
			
		} 
		if (offStage () && readyToEscape == true && Doors.doorOpen == false) {
			force += Seek(center) * boundsWt;
		}

	
		//ObstacleAvoidance	
		for (int i = 0; i < obstacles.Length; i++ ) {
			force += AvoidObstacle(obstacles[i], avoidDist) * avoidWt;
			
		}

		//limit Force and apply
		force = Vector3.ClampMagnitude(force, maxForce);
		ApplyForce(force); 
		
	
	}
	// Gets the closest zombie
	public GameObject getClosestTarget()
	{
		float distance = 100000f;
		GameObject closeZombie = null;
		List<GameObject> zom = gm.Zombies;
		foreach (GameObject zombi in zom)
		{
			float tempDistance = Vector3.Distance(zombi.transform.position, transform.position);
			if(tempDistance < distance)
			{
				distance = tempDistance;
				closeZombie = zombi;
			}
		}
		return closeZombie;
	}
	// checks if you still in the stage
	protected bool offStage()
	{

		bool off = false;


		if (transform.position.x > 393.56f) {
			off = true;
		} else if (transform.position.x < 98f) {
			off = true;
		} else if (transform.position.z > 294.08f) {
			off = true;
		} else if (transform.position.z < 6f) {
			off = true;
		}
		return off;
		
		
	}


	//Detect if you enter the safe spot 
	void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.tag == "SafePointStartHere") {
			readyToEscape = true;
		}
	}
	
}
