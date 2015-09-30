using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/*
Code for the camera was taken from the myCourses. Thank you Darren
 */

public class GameManager : MonoBehaviour {

	public GameObject HumansPrefab;
	public GameObject ZombiePrefab;
	public GameObject BatterflyPrefab;
	public GameObject ObstaclePrefab;
	public GameObject TARGET;

	public Camera[] cameras;
	private int currentCameraIndex;


	//--------- FLOCKERS-----------//
	//list of flockers dudes
	private List<GameObject> flock;
	private GameObject[] pointsToFly;
	public List<GameObject> Flock
	{
		get { return flock; }
	}

	private Vector3 flockDirection;
	public Vector3 FlockDirection
	{
		get { return flockDirection; }
	}

	private Vector3 centroid;
	public Vector3 Centroid 
	{
		get { return centroid; }
	}
	public int numFlockers;


	//----------ZOMBIE---------//
	private List<GameObject> zombies;
	public int numberOfZombies;
	public List<GameObject> Zombies
	{
		get {return zombies;}
	}


	//---------HUMANS--------//
	static public List<GameObject> humans;
	public int numberOfHumans;
	public List<GameObject> Humans
	{
		get {return humans;}
	}


	//-----------FLOCKERS POSITION-------//
	Vector3 flockStartPosition;

	//-----------LIST OF ZOMBIE WAYPOINTS-------//
	public GameObject[] waypointsSpawn;
	private int wayPointNumber;

	//-----------LIST OF HUMAN WAYPOINTS-------//
	public GameObject[] waypointsSpawnHuman;
	private int wayPointNumberHuman;





	// Use this for initialization
	void Start () {
	 
		currentCameraIndex = 0;
		foreach (Camera cam in cameras)
		{
			cam.gameObject.SetActive(false);
		}
		if (cameras.Length > 0)
		{
			cameras[0].gameObject.SetActive(true);
		}

		flockStartPosition = new Vector3 (116.3f, 3.7f, 145.15f);

		pointsToFly = GameObject.FindGameObjectsWithTag("FlyingPoints");

		Vector3 position = Vector3.zero;


		zombies = new List<GameObject> ();
		humans = new List<GameObject> ();
		flock = new List<GameObject> ();


		waypointsSpawn = GameObject.FindGameObjectsWithTag("ZombieSwapnWayPoints");

		waypointsSpawnHuman = GameObject.FindGameObjectsWithTag ("HumansSpawnWayPoints");


		//----------CREATING HUMANS-----------//
		for (int y = 0; y < numberOfHumans; y++)
		{
			int wayPointNumberHuman = Random.Range(0, waypointsSpawnHuman.Length);
			position = new Vector3(waypointsSpawnHuman[wayPointNumberHuman].transform.position.x, 0f,waypointsSpawnHuman[wayPointNumberHuman].transform.position.z);
			GameObject hum = (GameObject)GameObject.Instantiate(HumansPrefab,position, Quaternion.identity);
			hum.name = "human" + y;
			hum.GetComponent<Fleer>();
			humans.Add(hum);
		}
		
		
		//------------CREATING ZOMBIES------------//
		for (int i = 0; i < numberOfZombies; i++)
		{
			int wayPointNumber = Random.Range(0, waypointsSpawn.Length);
			position = new Vector3(waypointsSpawn[wayPointNumber].transform.position.x, 0f, waypointsSpawn[wayPointNumber].transform.position.z);
			GameObject zomb = (GameObject)GameObject.Instantiate(ZombiePrefab,position, Quaternion.identity);
			zomb.name = "zombie" + i;
			zomb.GetComponent<Seeker>() ;
			zombies.Add(zomb);
		}
	 	

		//------------CREATING FLOCK------------//
		for (int z = 0; z < numFlockers; z++)
		{
			position = new Vector3(Random.Range(flockStartPosition.x-10f,flockStartPosition.x+10f), 5f, Random.Range(flockStartPosition.z - 10f,flockStartPosition.z + 10f));
			GameObject butterfly = (GameObject)GameObject.Instantiate(BatterflyPrefab,position, Quaternion.identity);
			butterfly.name = "butterfly" + z;
			butterfly.GetComponent<Butterfly>().target = TARGET;
			flock.Add(butterfly);
		}




	}
	
	// Update is called once per frame
	void Update () {

		if (Input.GetKeyDown (KeyCode.C))
		{
			currentCameraIndex ++;
			Debug.Log ("C button has been pressed. Switching to the next camera");
			if (currentCameraIndex < cameras.Length)
			{
				cameras[currentCameraIndex-1].gameObject.SetActive(false);
				cameras[currentCameraIndex].gameObject.SetActive(true);
				Debug.Log ("Camera with name: " + cameras [currentCameraIndex].camera.name + ", is now enabled");
			}
			else
			{
				cameras[currentCameraIndex-1].gameObject.SetActive(false);
				currentCameraIndex = 0;
				cameras[currentCameraIndex].gameObject.SetActive(true);
				Debug.Log ("Camera with name: " + cameras [currentCameraIndex].camera.name + ", is now enabled");
			}
		}


		//Setting targets
		foreach (GameObject f in flock)
		{
			if(Vector3.Distance(f.transform.position, TARGET.transform.position) < 4.1f)
			{
				int randomNumber = (int)Random.Range (0, pointsToFly.Length);
				TARGET.transform.position = pointsToFly[randomNumber].transform.position;
			}
		}

		if (zombies.Count > 0)
		{
			foreach(GameObject zz in zombies)
			{
				zz.GetComponent<Seeker>().target = zz.GetComponent<Seeker>().getClosestTarget();
			}
		}

		if (humans.Count > 0)
		{
			foreach(GameObject hh in humans)
			{
				hh.GetComponent<Fleer>().target = hh.GetComponent<Fleer>().getClosestTarget();
			}
		}



	}

}
