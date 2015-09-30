using UnityEngine;
using System.Collections;


//An Obstacle is a GameObject we need to avoid
//All Obstacles should return their radius and be tagged "Obstacle"
public class Obstacle : MonoBehaviour {
	
	public float radius = 1.414f;
	
	public float Radius {
		get {
			return radius;
		}
	}
}
