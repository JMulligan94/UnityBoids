using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidManager : MonoBehaviour
{
	public static BoidManager Instance;

	public BoxCollider m_bounds;

	[Range( 2, 40 )]
	public int m_numBoids = 10;

	public GameObject m_boidPrefab;

	Boid[] m_boids;
	Vector3 m_minBounds;
	Vector3 m_maxBounds;

	private void OnEnable()
	{
		if ( Instance == null )
		{
			Instance = this;
		}
	}

	// Start is called before the first frame update
	void Start()
	{
		m_minBounds = m_bounds.transform.position + m_bounds.center - ( m_bounds.size / 2.0f );
		m_maxBounds = m_bounds.transform.position + m_bounds.center + ( m_bounds.size / 2.0f );

		GenerateBoids();
	}

	// Update is called once per frame
	void Update()
	{
		
	}

	public Vector3 GetWrappedPosition( Vector3 worldPosition )
	{
		float dist = 0.0f;
		Vector3 wrappedPosition = worldPosition;

		// Wrap X 
		if ( wrappedPosition.x < m_minBounds.x )
		{
			dist = m_minBounds.x - wrappedPosition.x;
			wrappedPosition.x = m_maxBounds.x - dist;
		}
		else if (wrappedPosition.x > m_maxBounds.x)
		{
			dist = wrappedPosition.x - m_maxBounds.x;
			wrappedPosition.x = m_minBounds.x + dist;
		}

		// Wrap Y
		if ( wrappedPosition.y < m_minBounds.y )
		{
			dist = m_minBounds.y - wrappedPosition.y;
			wrappedPosition.y = m_maxBounds.y - dist;
		}
		else if ( wrappedPosition.y > m_maxBounds.y )
		{
			dist = wrappedPosition.y - m_maxBounds.y;
			wrappedPosition.y = m_minBounds.y + dist;
		}

		// Wrap Z 
		if ( wrappedPosition.z < m_minBounds.z )
		{
			dist = m_minBounds.z - wrappedPosition.z;
			wrappedPosition.z = m_maxBounds.z - dist;
		}
		else if ( wrappedPosition.z > m_maxBounds.z )
		{
			dist = wrappedPosition.z - m_maxBounds.z;
			wrappedPosition.z = m_minBounds.z + dist;
		}

		return wrappedPosition;
	}

	private void GenerateBoids()
	{
		m_boids = new Boid[ m_numBoids ];
		for ( int i = 0; i < m_numBoids; ++i )
		{
			Vector3 randomPosition = new Vector3( Random.Range( m_minBounds.x, m_maxBounds.x ),
								Random.Range( m_minBounds.y, m_maxBounds.y ),
								Random.Range( m_minBounds.z, m_maxBounds.z ) );

			Quaternion randomRotation = Quaternion.Euler( Random.Range( 0, 360 ), Random.Range( 0, 360 ), Random.Range( 0, 360 ) );

			GameObject boidObj = Instantiate( m_boidPrefab, randomPosition, randomRotation, transform );
			boidObj.name = "Boid_" + i;
			m_boids[ i ] = boidObj.GetComponent<Boid>();
		}
	}
}
