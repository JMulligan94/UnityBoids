using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Obstacle : MonoBehaviour
{
	public abstract bool CollidesWith( Vector3 position, out Vector3 avoidanceForce );
}
