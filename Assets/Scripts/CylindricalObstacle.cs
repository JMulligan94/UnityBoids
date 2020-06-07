using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CylindricalObstacle : Obstacle
{
	public float m_radius = 0.5f;
	public float m_height = 1.0f;

	public override bool CollidesWith( Vector3 position, out Vector3 avoidanceForce )
	{
		Vector2 xzObstaclePos = new Vector2( position.x, position.z );
		Vector2 xzPosition = new Vector2( transform.position.x, transform.position.z );
		Vector2 xzDiff = xzObstaclePos - xzPosition;
		float distanceXZSquared = xzDiff.SqrMagnitude();
		bool isCollidingWithXZ = distanceXZSquared <= ( m_radius * m_radius );
		if ( isCollidingWithXZ )
		{
			float yDistance = position.y - transform.position.y;
			bool isCollidingWithY = Math.Abs( yDistance ) < m_height / 2.0;
			if ( isCollidingWithY )
			{
				if ( yDistance > ( ( m_height / 2.0f ) * 0.95 ) )
				{
					// Avoid by going above the cylinder
					// Push out from y centre
					avoidanceForce = new Vector3( 0, 1, 0 );
				}
				else if ( yDistance < ( ( -m_height / 2.0 ) * 0.95 ) )
				{
					// Below by going above the cylinder
					// Push out from y centre
					avoidanceForce = new Vector3( 0, -1, 0 );
				}
				else
				{
					// Push out from centre on XZ plane
					// Push out from XZ centre
					avoidanceForce = new Vector3( xzDiff.x, 0.0f, xzDiff.y );
				}
				return true;
			}
		}

		avoidanceForce = Vector3.zero;
		return false;
	}

	public void OnDrawGizmos()
	{
		Gizmos.DrawWireCube( gameObject.transform.position, new Vector3( m_radius, m_height, m_radius ) );
	}
}
