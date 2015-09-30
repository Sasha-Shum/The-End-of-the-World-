using UnityEngine;
using System.Collections;
using System.Collections.Generic;


//Automatically adds a CharacterController component to the same GameObject this script is on
//Reminds developer to not delete the CharController comp. 
[RequireComponent(typeof(CharacterController))]

abstract public class Vehicle : MonoBehaviour {

	//fields the Vehicle class needs
	public float maxSpeed = 6.0f;
	public float maxForce = 3.0f;
	public float mass = 1.0f;
	public float radius = 1.0f;
	public float gravity = 20.0f;


	//used for movement
	protected Vector3 velocity;
	protected Vector3 acceleration;
	protected Vector3 desiredVelocity;


	//property for velocity
	public Vector3 Velocity {
		get { return velocity; }
		set { velocity = value; }
	}


	//Wandering
	public float CIRCLE_DISTANCE = 10.0f;
	public float CIRCLE_RADIUS = 50.0f;
	public float wanderAngle = 35.0f;
	public float ANGLE_CHANGE = 100.0f;
	
	
	
	//Arrive
	public float slowingDistance = 10;
	
	//HIS
	//wander
	public float wander_circle_radius = 5;
	public float wander_circle_distance = 5;
	public float rand_jit = 5;
	public int wander_cycle_skip = 10;
	public float avoid_dist_wall_scalar = 0.3f;
	protected Vector3 wander_target = Vector3.zero;
	public Vector3 last_wander_pos;
	int wander_timer = 0;
	
	//LeaderFollowing
	public float LEADER_BEHIND_DIST = 10f;
	public float LEADER_SIGHT_RADIUS = 5f;
	
	//internal reference to the CharacterController component
	protected CharacterController charControl;
	
	//every child class must implement this method
	abstract protected void CalcSteeringForces();

	// Use this for initialization
	public virtual void Start () {

		acceleration = Vector3.zero;
		velocity = transform.forward;
		charControl = gameObject.GetComponent<CharacterController> ();
		Vector3 tempCenter = charControl.center;
		tempCenter.y = 1f;
		charControl.center = tempCenter;
	}
	
	// Update is called once per frame
	virtual public void Update () {

		CalcSteeringForces();
		
		//update velocity
		velocity += acceleration * Time.deltaTime;
		//later, we can change this to reflect terrain
		velocity.y = 0;
		transform.position = new Vector3 (transform.position.x, Terrain.activeTerrain.SampleHeight (transform.position), transform.position.z);
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

	protected void ApplyForce(Vector3 steeringForce)
	{
		acceleration += steeringForce / mass;
	}

	//FUNCTIONS THAT RETURN STEERING FORCES
	protected Vector3 Seek(Vector3 targetPos)
	{
		Debug.DrawLine (transform.position, transform.position - targetPos, Color.green);
		//find desired velocity
		desiredVelocity = targetPos - transform.position;
		//scale the desired by max speed
		desiredVelocity = desiredVelocity.normalized * maxSpeed;
		//desiredVelocity subtract current velocity
		desiredVelocity -= velocity;
		//keep us grounded
		desiredVelocity.y = 0;
		return desiredVelocity;
	}

	//Flee
	protected Vector3 Flee(Vector3 targetPos)
	{
		//find desired velocity
		desiredVelocity = transform.position - targetPos;
		//scale the desired by max speed
		desiredVelocity = desiredVelocity.normalized * maxSpeed;
		//desiredVelocity subtract current velocity
		desiredVelocity -= velocity;
		//keep us grounded
		desiredVelocity.y = 0;
		return desiredVelocity;
	}

	// Pursue - predict future position of the target
	public Vector3 pursue(Vector3 target)
	{
		
		Vector3 toTheTarget = target - transform.position;
		float distance = toTheTarget.magnitude;
		float Tnum = distance / maxSpeed;
		Vector3 targetPosition = target + velocity * Tnum;
		Debug.DrawLine (transform.position, targetPosition, Color.grey);
		return Seek (targetPosition);
	}

	// Evade predict future position of the target
	public Vector3 evade(Vector3 target)
	{
		Vector3 toTheTarget = target - transform.position;
		float distance = toTheTarget.magnitude;
		float Tnum = distance / maxSpeed;
		Vector3 targetPosition = target + velocity * Tnum;
		Debug.DrawLine (transform.position, targetPosition, Color.grey);
		return Flee (targetPosition);
	}

	public Vector3 Wander()
	{
		//temp vectors
		Vector3 centerOfTheCircle = transform.forward;
		Vector3 displacement = Vector3.zero;
		
		//get a random vector from a circle around the vehicle with a radius of 2
		//used a Vector2 because I only need two coordinates on a flat plane
		Vector2 temp = Random.insideUnitCircle * 2;
		displacement.x = temp.x;
		displacement.z = temp.y;
		
		//create a new vector from the center to our random point
		displacement -= centerOfTheCircle;
		
		//add the two vectors together and return
		centerOfTheCircle += displacement;
		
		Debug.DrawLine( transform.position, transform.position + centerOfTheCircle, Color.yellow, 10f);
		
		return centerOfTheCircle;
		
	}

	// Arrive to the specific location
	public Vector3 Arrive(Vector3 target)
	{
		Vector3 targetOffset = target - transform.position;
		float distance = targetOffset.magnitude;
		float rampedSpeed = maxSpeed * (distance / slowingDistance);
		float clippedSpeed = Mathf.Min (rampedSpeed, maxSpeed);
		desiredVelocity = (clippedSpeed / distance) * targetOffset;
		return (desiredVelocity - velocity);
		
	}

	public Vector3 Separation(List<GameObject> flockers, float separationDistance) 
	{
		Vector3 total = Vector3.zero;
		
		//go through all the flockers
		foreach(GameObject f in flockers){
			Vector3 dv = transform.position - f.transform.position;
			float dist = dv.magnitude;
			if(dist > 0 && dist < separationDistance){
				dv *= separationDistance / dist;
				dv.y = 0;
				total += dv;
			}
		}
		
		total = total.normalized * maxSpeed;
		total -= velocity;
		return total;
	}
	
	public Vector3 Alignment(Vector3 alignVector)
	{
		Vector3 dv = alignVector.normalized * maxSpeed;
		dv -= velocity;
		dv.y = 0;
		return dv;
	}
	
	public Vector3 Cohesion(Vector3 cohesionVector) 
	{
		return Seek(cohesionVector);
	}

	// Avoiding the obstalces
	protected Vector3 AvoidObstacle(GameObject obst, float safeDistance) {
		
		desiredVelocity = Vector3.zero;
		//float obRadius = obst.GetComponent<Obstacle>().Radius;
		float obRadius = obst.collider.bounds.size.magnitude / 2;

		Vector3 vecToCenter = obst.transform.position - transform.position;
		vecToCenter.y = 0;
		
		float dist = Mathf.Abs(vecToCenter.magnitude - obRadius - radius);
		dist = Mathf.Max(dist, 0.01f);
		
		if(dist > safeDistance){
			return Vector3.zero;
		}
		
		if(Vector3.Dot(vecToCenter, transform.forward) < 0){
			return Vector3.zero;
		}
		
		float rightDotVTC = Vector3.Dot(vecToCenter, transform.right);
		
		if(Mathf.Abs(rightDotVTC) > radius + obRadius){
			return Vector3.zero;
		}
		
		if(rightDotVTC > 0){
			desiredVelocity = transform.right * -maxSpeed * (safeDistance / dist);
			Debug.DrawLine(transform.position, obst.transform.position, Color.red);
		}
		if (rightDotVTC < 0) {
			desiredVelocity = transform.right * maxSpeed * (safeDistance / dist);
			Debug.DrawLine(transform.position, obst.transform.position, Color.red);
		}
		
		desiredVelocity -= velocity;
		desiredVelocity.y = 0;
		return desiredVelocity;
	}


}
