using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputeBoidManager : MonoBehaviour
{
	struct BoidPairing
	{
		public Vector3 position;
		public Vector3 velocity;
	}

	public static ComputeBoidManager Instance;

	public BoxCollider m_bounds;

	public ComputeShader m_computeShader;
	
	public int m_numBoids = 10;
	
	[Header("Compute Boid Settings")]	
	public bool m_drawBoidAxis = false;

	public float m_neighbourRadius = 10.0f;
	
	public GameObject m_boidPrefab;
	public bool m_updatePosition = true;
	public bool m_wrapPositionEnabled = true;

	public float m_minSpeed = 0.5f;
	public float m_maxSpeed = 2.0f;

	[Header( "Separation Settings" )]
	public bool m_enableSeparation = true;
	public bool m_drawSeparationDebugRays = true;
	[Tooltip( "The minimum amount of distance between any two boids" )]
	public float m_separationValue = 2.0f;
	[Tooltip( "How strong the force of separation is" )]
	public float m_separationFactor = 1.0f;

	[Header( "Alignment Settings" )]
	public bool m_enableAlignment = true;
	public bool m_drawAlignmentDebugRays = true;
	public float m_alignmentFactor = 50.0f;

	[Header( "Cohesion Settings" )]
	public bool m_enableCohesion = true;
	public bool m_drawCohesionDebugRays = true;
	public float m_cohesionFactor = 0.01f;

	ComputeBoid[] m_boids;
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
			ComputeBoid boid = m_boids[ i ];

			Vector3 newVelocity = Vector3.zero;

			Vector3 separation = Vector3.zero;
			Vector3 alignment = Vector3.zero;
			Vector3 cohesion = Vector3.zero;
			CalculateRules( i, ref separation, ref alignment, ref cohesion );

			// 1) SEPARATION
			if ( m_enableSeparation )
			{
				//separation = CalculateSeparation( i );
				if ( m_drawSeparationDebugRays
					&& separation.sqrMagnitude > 0.0f )
				{
					DrawRedRay( boid, separation );
				}

				newVelocity += separation * m_separationFactor;
			}

			// 2) ALIGNMENT
			if ( m_enableAlignment )
			{
				//alignment = CalculateAlignment( i );
				if ( m_drawAlignmentDebugRays 
					&& alignment.sqrMagnitude > 0.0f )
				{
					DrawRay( boid, alignment, Color.blue );
				}

				newVelocity += alignment * m_alignmentFactor;
			}

			// 3) COHESION
			if ( m_enableCohesion )
			{
				//cohesion = CalculateCohesion( i );
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
		int kernel = m_computeShader.FindKernel( "FindNeighbour" );

		// Boid count
		m_computeShader.SetInt( "boidCount", m_numBoids );

		// Neighbour Radius Squared
		m_computeShader.SetFloat( "neighbourRadiusSqr", m_neighbourRadius * m_neighbourRadius );

		// Boid buffer
		BoidPairing[] boidData = new BoidPairing[ m_numBoids ];
		for ( int i = 0; i < m_numBoids; ++i )
		{
			boidData[ i ] = new BoidPairing() { position = m_boids[ i ].transform.position, velocity = m_boids[ i ].GetBoidVelocity() };
		}
		ComputeBuffer boidBuffer = new ComputeBuffer( m_numBoids, 24 );
		boidBuffer.SetData( boidData );
		m_computeShader.SetBuffer( kernel, "boidBuffer", boidBuffer );

		int neighbourBufferSize = GetRunningTotal( m_numBoids );

		//Boid count 
		m_computeShader.SetInt( "neighbourBufferSize", neighbourBufferSize );

		// Neighbour Indices
		int[] neighbourIndices = new int[ neighbourBufferSize ];
		ComputeBuffer neighboursBuffer = new ComputeBuffer( neighbourBufferSize, 4 );
		neighboursBuffer.SetData( neighbourIndices );
		m_computeShader.SetBuffer( kernel, "neighboursBuffer", neighboursBuffer );

		m_computeShader.Dispatch( kernel, m_numBoids, m_numBoids, 1 );

		int[] output = new int[ neighbourBufferSize ];
		neighboursBuffer.GetData( output );
		neighboursBuffer.Release();
		boidBuffer.Release();

		int neighbourBufferIndex = 0;
		for ( int i = 0; i < m_numBoids; ++i )
		{
			for ( int j = i + 1; j < m_numBoids; ++j )
			{
				if ( output[ neighbourBufferIndex ] == 1 )
				{
					m_boids[ i ].AddNeighbour( m_boids[ j ] );
					m_boids[ j ].AddNeighbour( m_boids[ i ] );
				}
				neighbourBufferIndex++;
			}

		}
	}

	private void CalculateRules( int boidIndex, ref Vector3 sepForce, ref Vector3 alignForce, ref Vector3 cohForce )
	{
		ComputeBoid boid = m_boids[ boidIndex ];
		if ( boid.GetNeighbourCount() == 0 )
			return;

		Vector3 boidPosition = boid.transform.position;

		sepForce = Vector3.zero;
		alignForce = Vector3.zero;
		cohForce = Vector3.zero;

		float separationDistSqr = m_separationValue * m_separationValue;
		Vector3 sumHeading = Vector3.zero;
		Vector3 averagePosition = Vector3.zero;

		foreach ( ComputeBoid neighbour in boid.IterateNeighbours() )
		{
			sumHeading = neighbour.GetBoidVelocity().normalized;
			averagePosition += neighbour.transform.position;

			Vector3 difference = boidPosition - neighbour.transform.position;
			float distanceSqr = difference.sqrMagnitude;
			if ( distanceSqr < separationDistSqr )
			{
				// Needs separating
				Vector3 separate = difference;
				separate.Normalize();
				separate = separate / distanceSqr;
				sepForce += separate;

			}
		}

		alignForce = sumHeading / boid.GetNeighbourCount();

		averagePosition /= boid.GetNeighbourCount();
		cohForce = averagePosition - m_boids[ boidIndex ].transform.position;
	}

	// SEPARATION: Steering to avoid colliding or crowing with other flockmates
	private Vector3 CalculateSeparation( int boidIndex )
	{
		ComputeBoid boid = m_boids[ boidIndex ];
		if ( boid.GetNeighbourCount() == 0 )
			return Vector3.zero;

		Vector3 separationForce = Vector3.zero;

		Vector3 boidPosition = boid.transform.position;
		float separationDistSqr = m_separationValue * m_separationValue;

		foreach ( ComputeBoid neighbour in boid.IterateNeighbours() )
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
		ComputeBoid boid = m_boids[ boidIndex ];
		if ( boid.GetNeighbourCount() == 0 )
			return Vector3.zero;

		Vector3 alignmentForce = Vector3.zero;
		Vector3 sumHeading = Vector3.zero;
		foreach ( ComputeBoid neighbour in boid.IterateNeighbours() )
		{
			sumHeading = neighbour.GetBoidVelocity().normalized;
		}

		alignmentForce = sumHeading / boid.GetNeighbourCount();
		return alignmentForce;
	}

	// COHESION: Steering to move towards the average position of flockmates
	private Vector3 CalculateCohesion( int boidIndex )
	{
		ComputeBoid boid = m_boids[ boidIndex ];
		if ( boid.GetNeighbourCount() == 0 )
			return Vector3.zero;

		Vector3 cohesionForce = Vector3.zero;
		Vector3 averagePosition = Vector3.zero;
		foreach ( ComputeBoid neighbour in boid.IterateNeighbours() )
		{
			averagePosition += neighbour.transform.position;
		}

		averagePosition /= boid.GetNeighbourCount();

		cohesionForce = averagePosition - m_boids[ boidIndex ].transform.position;
		return cohesionForce;
	}

	private void GenerateBoids()
	{
		m_boids = new ComputeBoid[ m_numBoids ];
		for ( int i = 0; i < m_numBoids; ++i )
		{
			Vector3 randomPosition = new Vector3( Random.Range( m_minBounds.x, m_maxBounds.x ),
								Random.Range( m_minBounds.y, m_maxBounds.y ),
								Random.Range( m_minBounds.z, m_maxBounds.z ) );

			Quaternion randomRotation = Quaternion.Euler( Random.Range( 0, 360 ), Random.Range( 0, 360 ), Random.Range( 0, 360 ) );

			GameObject boidObj = Instantiate( m_boidPrefab, randomPosition, randomRotation, transform );
			boidObj.name = "Boid_" + i;
			m_boids[ i ] = boidObj.GetComponent<ComputeBoid>();
		}
	}

	private void DrawRedRay( ComputeBoid boid, Vector3 direction )
	{
		DrawRay( boid, direction, Color.red );
	}

	private void DrawRay( ComputeBoid boid, Vector3 direction, Color colour )
	{
		Debug.DrawRay( boid.transform.position, direction, colour );
	}


	public void GetWrappedPosition( ref Vector3 worldPosition )
	{
		if ( !m_wrapPositionEnabled )
			return;

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

		worldPosition = wrappedPosition;
	}

	private int GetRunningTotal( int total )
	{
		return ( total / 2 ) * ( 1 + total ) - total;
	}
}
