using UnityEngine;
using System.Collections;

public class Butterfly : Vehicle {

	//store the target
	public GameObject target;
	//private obstacles array
	private GameObject[] obstacles;
	
	//get a reference to the GameManager
	private GameManager gm;
	
	//weights for steering forces
	public float seekWt = 75.0f;
	public float avoidWt = 10.0f;
	public float avoidDist = 20.0f;
	//flocking weights
	public float separationWt = 20f;
	public float separationDist = 2f;
	public float cohesionWt = 1f;
	public float alignmentWt = 1f;
	public float boundsWt = 100f;


	public float boundsSize = 48.0f;
	Vector3 center;


	// Use this for initialization
	override public void Start () {
		base.Start();
		obstacles = GameObject.FindGameObjectsWithTag("Obstacle"); // Gets obstacles 
		gm = GameObject.Find("MainGo").GetComponent<GameManager>();
		center = new Vector3(257.7f, 0f, 164.8f);
		center.y = 1f;

	}
	void Update()
	{
		
		CalcSteeringForces();
		
		//update velocity
		velocity += acceleration * Time.deltaTime;
		//later, we can change this to reflect terrain
		velocity.y = 0;

		//C# equivalent of Limit
		velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
		
		//orient the transform to face where I'm going
		if(velocity != Vector3.zero){
			transform.forward = velocity.normalized;
		}
		
		//CharacterController will do the moving for us
		charControl.Move(velocity * Time.deltaTime);
		
		//zero out the acceleration
		acceleration = Vector3.zero;
	
	}

	protected override void CalcSteeringForces() {
		//get a force vector
		Vector3 force = Vector3.zero;
		
		
		//seek the target - may want some weights (hint, hint)
		//force += Arrive(target.transform.position) * seekWt;
		//force += Wander() * 70f;
		//force += pursue (target.transform.position) * seekWt;
		//Debug.Log (target.transform.position);
		
		
		force += Seek (target.transform.position) * seekWt;
		//force += evade (target.transform.position) * seekWt;
		
		//Boundries
		
		if (offStage (boundsSize)) {
			
			//Debug.Log("HERE1");
			
			force += Seek(center) * boundsWt;
			
		}
		
		

		//Avoid Obstacles
		for (int i = 0; i < obstacles.Length; i++ ) {
			force += AvoidObstacle(obstacles[i], avoidDist) * avoidWt;
			
		}
		
		// Flocking
		force += separationWt * Separation(gm.Flock, separationDist);
		force += alignmentWt * Alignment(gm.FlockDirection);
		force += cohesionWt * Cohesion(gm.Centroid);
		
		//limit Force and apply
		force = Vector3.ClampMagnitude(force, maxForce);
		ApplyForce(force); 
		
		//Draw debug lines
		//show force as a blue line pushing the guy like a jet stream
		//Debug.DrawLine(transform.position, transform.position - force, Color.blue);
		//red debug line showing the target
		//Debug.DrawLine(transform.position, target.transform.position, Color.red);
		
	}

	//Check if butterly flying out of the boundries
	protected bool offStage(float dist)
	{

		bool off = false;

		if (transform.position.x > 391.96f) {
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
}
