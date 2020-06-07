using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class PlaneObstacle : Obstacle
{
	public Vector3 m_normal = new Vector3( 0, 1, 0 );
	public Vector2 m_size = new Vector2(1, 1);

	private Vector3 m_rightAxis;
	private Vector3 m_forwardAxis;

	private Vector3 m_topLeft;
	private Vector3 m_topRight;
	private Vector3 m_bottomLeft;
	private Vector3 m_bottomRight;

	public void OnValidate()
	{
		float halfWidth = m_size.x / 2.0f;
		float halfHeight = m_size.y / 2.0f;
		
		m_rightAxis = new Vector3( m_normal.y, m_normal.z, m_normal.x );
		m_forwardAxis = Vector3.Cross( m_normal, m_rightAxis );

		m_topLeft = gameObject.transform.position;
		m_topLeft -= ( m_rightAxis * halfWidth );
		m_topLeft += ( m_forwardAxis * halfHeight );

		m_topRight = gameObject.transform.position;
		m_topRight += ( m_rightAxis * halfWidth );
		m_topRight += ( m_forwardAxis * halfHeight );

		m_bottomLeft = gameObject.transform.position;
		m_bottomLeft -= ( m_rightAxis * halfWidth );
		m_bottomLeft -= ( m_forwardAxis * halfHeight );

		m_bottomRight = gameObject.transform.position;
		m_bottomRight += ( m_rightAxis * halfWidth );
		m_bottomRight -= ( m_forwardAxis * halfHeight );
	}

	public override bool CollidesWith( Vector3 position, out Vector3 avoidanceForce )
	{
		// Check if position is past plane
		Vector3 normalisedNormal = m_normal.normalized;
		float a = position.x - transform.position.x;
		float b = position.y - transform.position.y;
		float c = position.z - transform.position.z;
		float distanceToPlane = ( a * normalisedNormal.x ) 
			+ ( b * normalisedNormal.y ) 
			+ ( c * normalisedNormal.z );

		avoidanceForce = m_normal;
		return distanceToPlane <= 0;
	}

	public void OnDrawGizmos()
	{
		// Draw axis
		Gizmos.color = UnityEngine.Color.cyan;
		Gizmos.DrawLine( gameObject.transform.position, gameObject.transform.position + m_normal );

		Gizmos.color = UnityEngine.Color.red;
		Gizmos.DrawLine( gameObject.transform.position, gameObject.transform.position + m_rightAxis );

		Gizmos.color = UnityEngine.Color.green;
		Gizmos.DrawLine( gameObject.transform.position, gameObject.transform.position + m_forwardAxis );

		// Draw plane
		Gizmos.color = UnityEngine.Color.white;
		Gizmos.DrawLine( m_topLeft, m_topRight);
		Gizmos.DrawLine( m_topLeft, m_bottomLeft );
		Gizmos.DrawLine( m_bottomRight, m_topRight );
		Gizmos.DrawLine( m_bottomRight, m_bottomLeft );
		Gizmos.DrawLine( m_topLeft, m_bottomRight );


	}
}
