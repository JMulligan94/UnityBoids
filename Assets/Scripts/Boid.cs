using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
	[Range(0.5f, 5.0f)]
	public float m_speed = 1.0f; // 1 unit / sec

	public bool m_drawAxis = false;

	// Start is called before the first frame update
	void Start()
	{
		
	}

	// Update is called once per frame
	void Update()
	{
		float movementDelta = m_speed * Time.deltaTime;
		Vector3 newPosition = transform.localPosition + ( transform.forward * movementDelta );

		if ( m_drawAxis )
		{
			Debug.DrawRay( transform.position, transform.forward, Color.blue );
			Debug.DrawRay( transform.position, transform.up, Color.green );
			Debug.DrawRay( transform.position, transform.right, Color.red );
		}

		transform.localPosition = BoidManager.Instance.GetWrappedPosition( newPosition );
	}
}
