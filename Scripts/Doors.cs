using UnityEngine;
using System.Collections;

public class Doors : MonoBehaviour {
	Animator animator;
	static public bool doorOpen;
	// Use this for initialization
	void Start () {
		doorOpen = false;
		animator = GetComponent<Animator> ();
	}

	void OnTriggerEnter(Collider col)
	{
		if (col.gameObject.tag == "Player" && doorOpen == false)
		{
			doorOpen = true;
			DoorControl("Open");

		}
	}

	void DoorControl(string state)
	{
		animator.SetTrigger(state);
	}

	void OnTriggerExit(Collider col)
	{
		if (doorOpen)
		{
			doorOpen = false;
			DoorControl("Close");
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
