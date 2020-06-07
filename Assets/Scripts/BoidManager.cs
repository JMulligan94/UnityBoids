using System.Collections.Generic;
using UnityEngine;

public class BoidManager : MonoBehaviour
{
	[Header( "Boundary Settings" )]
	public BoxCollider m_bounds;

	public Vector3 m_numCells = new Vector3( 5, 5, 5 );
	private Cell[] m_cells;
	
	public int m_numBoids = 200;
	
	[Header("Boid Settings")]	
	public bool m_drawBoidAxis = false;

	public float m_neighbourRadius = 10.0f;
	
	public GameObject m_boidPrefab;
	public bool m_updatePosition = true;

	public float m_minSpeed = 2.0f;
	public float m_maxSpeed = 6.0f;

	[Header( "Separation Settings" )]
	public bool m_enableSeparation = true;
	public bool m_drawSeparationDebugRays = false;
	[Tooltip( "The minimum amount of distance between any two boids" )]
	public float m_separationValue = 2.0f;
	[Tooltip( "How strong the force of separation is" )]
	public float m_separationFactor = 0.1f;

	[Header( "Alignment Settings" )]
	public bool m_enableAlignment = true;
	public bool m_drawAlignmentDebugRays = false;
	public float m_alignmentFactor = 50.0f;

	[Header( "Cohesion Settings" )]
	public bool m_enableCohesion = true;
	public bool m_drawCohesionDebugRays = false;
	public float m_cohesionFactor = 0.01f;

	[Header( "Avoidance Settings" )]
	public bool m_enableAvoidance = true;
	public bool m_drawAvoidanceDebugRays = false;
	public float m_avoidanceFactor = 1.0f;
	public float m_avoidanceDistance = 0.1f;

	Boid[] m_boids;
	Vector3 m_minBounds;
	Vector3 m_maxBounds;

	[Header( "Obstacles" )]
	public Obstacle[] m_obstacles; 


	void Awake()
	{
		m_minBounds = m_bounds.transform.position + m_bounds.center - ( m_bounds.size / 2.0f );
		m_maxBounds = m_bounds.transform.position + m_bounds.center + ( m_bounds.size / 2.0f );

		GenerateBoids();
		GenerateCells();
	}
	
	void FixedUpdate()
	{
		if ( !m_updatePosition )
			return;

		// Sort boids into cells
		SortBoidsIntoCells();

		// Find boids neighbours
		PerformNeighbourSearch();
		
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

			// 4) AVOIDANCE
			if ( m_obstacles.Length > 0 )
			{
				Vector3 avoidance = Vector3.zero;
				if ( m_enableAvoidance )
				{
					avoidance = CalculateAvoidance( i );
					if ( m_drawAvoidanceDebugRays
						&& avoidance.sqrMagnitude > 0.0f )
					{
						DrawRay( boid, avoidance, Color.yellow );
					}

					newVelocity += avoidance * m_avoidanceFactor;
				}
			}

			if ( m_drawBoidAxis )
			{
				boid.DrawDebugAxis();
			}

			if ( newVelocity.sqrMagnitude > 0 )
			{
				boid.SetBoidVelocity( newVelocity );
			}

			boid.UpdatePosition();
			boid.ClearNeighbours();
		}
	}

	private void SortBoidsIntoCells()
	{
		return;
		// Clear all boids from their cells
		foreach ( Cell cell in m_cells )
		{
			cell.Clear();
		}

		List<int> integers = new List<int>();
		int boidIndex = 0;
		foreach ( Boid boid in m_boids )
		{
			boid.m_cellsToCheck.Clear();
			integers.Add( boidIndex++ );
		}

		// Sort boids by axes 
		int[] sortedByX = integers.ToArray();
		int[] sortedByY = integers.ToArray();
		int[] sortedByZ = integers.ToArray();

		System.Array.Sort( sortedByX, ( delegate ( int x, int y )
		{
			return m_boids[ x ].transform.position.x.CompareTo( m_boids[ y ].transform.position.x );
		} ) );
		System.Array.Sort( sortedByY, ( delegate ( int x, int y )
		{
			return m_boids[ x ].transform.position.y.CompareTo( m_boids[ y ].transform.position.y );
		} ) );
		System.Array.Sort( sortedByZ, ( delegate ( int x, int y )
		{
			return m_boids[ x ].transform.position.z.CompareTo( m_boids[ y ].transform.position.z );
		} ) );

		int cellIndex = 0;
		for ( int x = 0; x < ( int )m_numCells.x; ++x )
		{
			for ( int y = 0; y < ( int )m_numCells.y; ++y )
			{
				for ( int z = 0; z < ( int )m_numCells.z; ++z )
				{
					Cell cell = m_cells[ cellIndex ];

					cellIndex++;
				}
			}
		}

		//foreach ( Boid boid in m_boids )
		//{

		//	Vector3 boidPos = boid.transform.position;

		//	Vector3 boidMin = boidPos - new Vector3( m_neighbourRadius, m_neighbourRadius, m_neighbourRadius );
		//	Vector3 boidMax = boidPos + new Vector3( m_neighbourRadius, m_neighbourRadius, m_neighbourRadius );

		//	// Check aligned to x axis
			
			
		//}
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

	// AVOIDANCE: Steering to avoid any obstacles in the scene
	private Vector3 CalculateAvoidance( int boidIndex )
	{
		Boid boid = m_boids[ boidIndex ];

		Vector3 aheadVector = boid.GetBoidVelocity().normalized * m_avoidanceDistance;
		Vector3 aheadPosition = boid.transform.position + aheadVector;

		Vector3 avoidanceForce = Vector3.zero;
		foreach ( Obstacle obstacle in m_obstacles )
		{
			Vector3 obstacleAvoidance = Vector3.zero;
			if ( obstacle.CollidesWith( aheadPosition, out obstacleAvoidance ) )
			{
				avoidanceForce += obstacleAvoidance;
			}
		}
		avoidanceForce.Normalize();
		return avoidanceForce;
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

	private void GenerateCells()
	{
		int arraySize = ( int )m_numCells.x * ( int )m_numCells.y * ( int )m_numCells.z;
		m_cells = new Cell[ arraySize ];

		float xStartValue = m_minBounds.x;
		float xInterval = ( m_maxBounds.x - m_minBounds.x ) / m_numCells.x;

		float yStartValue = m_minBounds.y;
		float yInterval = ( m_maxBounds.y - m_minBounds.y ) / m_numCells.y;

		float zStartValue = m_minBounds.z;
		float zInterval = ( m_maxBounds.z - m_minBounds.z ) / m_numCells.z;

		float xCurrentValue = xStartValue;
		float yCurrentValue = yStartValue;
		float zCurrentValue = zStartValue;

		int cellNumber = 0;
		for ( int x = 0; x < ( int )m_numCells.x; ++x )
		{
			yCurrentValue = yStartValue;
			for ( int y = 0; y < ( int )m_numCells.y; ++y )
			{
				zCurrentValue = zStartValue;
				for ( int z = 0; z < ( int )m_numCells.z; ++z )
				{
					m_cells[ cellNumber ] = new Cell( new Vector3( xCurrentValue, yCurrentValue, zCurrentValue ), new Vector3( xInterval, yInterval, zInterval ) );

					cellNumber++;
					zCurrentValue += zInterval;
				}
				yCurrentValue += yInterval;
			}
			xCurrentValue += xInterval;
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

	void OnDrawGizmos()
	{
		if ( m_cells == null )
			return;

		foreach ( Cell cell in m_cells )
		{
			cell.OnDrawGizmos();
		}
	}
}
