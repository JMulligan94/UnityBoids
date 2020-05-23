using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidManager : MonoBehaviour
{
	public static BoidManager Instance;

	public BoxCollider m_bounds;
	
	public int m_numBoids = 10;
	
	[Header("Boid Settings")]	
	public bool m_drawBoidAxis = false;

	public float m_neighbourRadius = 1.0f;
	
	public GameObject m_boidPrefab;
	public bool m_updatePosition = true;

	public float m_minSpeed = 0.1f;
	public float m_maxSpeed = 2.0f;

	[Header( "Separation Settings" )]
	public bool m_enableSeparation = true;
	public bool m_drawSeparationDebugRays = true;
	[Tooltip( "The minimum amount of distance between any two boids" )]
	public float m_separationValue = 1.55f;
	[Tooltip( "How strong the force of separation is" )]
	public float m_separationFactor = 0.05f;

	[Header( "Alignment Settings" )]
	public bool m_enableAlignment = true;
	public bool m_drawAlignmentDebugRays = true;
	public float m_alignmentFactor = 0.05f;

	[Header( "Cohesion Settings" )]
	public bool m_enableCohesion = true;
	public bool m_drawCohesionDebugRays = true;
	public float m_cohesionFactor = 0.05f;

	Boid[] m_boids;
	Vector3 m_minBounds;
	Vector3 m_maxBounds;

	// Start is called before the first frame update
	void Start()
	{
		if ( Instance == null )
			Instance = this;

		m_minBounds = m_bounds.transform.position + m_bounds.center - ( m_bounds.size / 2.0f );
		m_maxBounds = m_bounds.transform.position + m_bounds.center + ( m_bounds.size / 2.0f );

		GenerateBoids();
	}
	
	void FixedUpdate()
	{
		// Find boids neighbours
		PerformNeighbourSearch();

		if ( !m_updatePosition )
			return;
		
		for ( int i = 0; i < m_boids.Length; ++i )
		{
			Boid boid = m_boids[ i ];

			Vector3 newVelocity = Vector3.zero;

			// 1) SEPARATION
			Vector3 separation = Vector3.zero;
			if ( m_enableSeparation )
			{
				separation = CalculateSeparation( i );
				if ( m_drawSeparationDebugRays
					&& separation.sqrMagnitude > 0.0f )
				{
					DrawRedRay( boid, separation );
				}

				newVelocity += separation * m_separationFactor;
			}

			// 2) ALIGNMENT
			Vector3 alignment = Vector3.zero;
			if ( m_enableAlignment )
			{
				alignment = CalculateAlignment( i );
				if ( m_drawAlignmentDebugRays 
					&& alignment.sqrMagnitude > 0.0f )
				{
					DrawRay( boid, alignment, Color.blue );
				}

				newVelocity += alignment * m_alignmentFactor;
			}

			// 3) COHESION
			Vector3 cohesion = Vector3.zero;
			if ( m_enableCohesion )
			{
				cohesion = CalculateCohesion( i );
				if ( m_drawCohesionDebugRays
					&& cohesion.sqrMagnitude > 0.0f )
				{
					DrawRay( boid, cohesion, Color.green );
				}

				newVelocity += cohesion * m_cohesionFactor;
			}


			if ( m_drawBoidAxis )
			{
				boid.DrawDebugAxis();
			}
				
			boid.SetBoidVelocity( newVelocity );
			boid.UpdatePosition();
			boid.ClearNeighbours();
		}
	}

	private void PerformNeighbourSearch()
	{
		float neighbourRadSqr = m_neighbourRadius * m_neighbourRadius;
		for ( int i = 0; i < m_numBoids; ++i )
		{
			Boid boid = m_boids[ i ];
			Vector3 boidPos = boid.transform.position;
			for ( int j = i + 1; j < m_numBoids; ++j )
			{
				float distanceSquared = ( m_boids[ j ].transform.position - boidPos ).sqrMagnitude;
				if ( distanceSquared <= neighbourRadSqr )
				{
					m_boids[ i ].AddNeighbour( m_boids[ j ] );
					m_boids[ j ].AddNeighbour( m_boids[ i ] );
				}
			}
		}
	}

	// SEPARATION: Steering to avoid colliding or crowing with other flockmates
	private Vector3 CalculateSeparation( int boidIndex )
	{
		Boid boid = m_boids[ boidIndex ];
		if ( boid.GetNeighbourCount() == 0 )
			return Vector3.zero;

		Vector3 separationForce = Vector3.zero;

		Vector3 boidPosition = boid.transform.position;
		float separationDistSqr = m_separationValue * m_separationValue;

		foreach ( Boid neighbour in boid.IterateNeighbours() )
		{
			Vector3 difference = boidPosition - neighbour.transform.position;
			float distanceSqr = difference.sqrMagnitude;
			if ( distanceSqr < separationDistSqr )
			{
				// Needs separating
				Vector3 separate = difference;
				separate.Normalize();
				separate = separate / distanceSqr;
				separationForce += separate; 
			}
		}

		return separationForce;
	}

	// ALIGNMENT: Steering to move with the average heading of flockmates
	private Vector3 CalculateAlignment( int boidIndex )
	{
		Boid boid = m_boids[ boidIndex ];
		if ( boid.GetNeighbourCount() == 0 )
			return Vector3.zero;

		Vector3 alignmentForce = Vector3.zero;
		Vector3 sumHeading = Vector3.zero;
		foreach ( Boid neighbour in boid.IterateNeighbours() )
		{
			sumHeading = neighbour.GetBoidVelocity().normalized;
		}

		alignmentForce = sumHeading / boid.GetNeighbourCount();
		return alignmentForce;
	}

	// COHESION: Steering to move towards the average position of flockmates
	private Vector3 CalculateCohesion( int boidIndex )
	{
		Boid boid = m_boids[ boidIndex ];
		if ( boid.GetNeighbourCount() == 0 )
			return Vector3.zero;

		Vector3 cohesionForce = Vector3.zero;
		Vector3 averagePosition = Vector3.zero;
		foreach ( Boid neighbour in boid.IterateNeighbours() )
		{
			averagePosition += neighbour.transform.position;
		}

		averagePosition /= boid.GetNeighbourCount();

		cohesionForce = averagePosition - m_boids[ boidIndex ].transform.position;
		return cohesionForce;
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

	private void DrawRedRay( Boid boid, Vector3 direction )
	{
		DrawRay( boid, direction, Color.red );
	}

	private void DrawRay( Boid boid, Vector3 direction, Color colour )
	{
		Debug.DrawRay( boid.transform.position, direction, colour );
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
		else if ( wrappedPosition.x > m_maxBounds.x )
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
}
