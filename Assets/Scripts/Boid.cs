using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
	private Vector3 m_velocity;
	private Vector3 m_upAxis;

	private List<Boid> m_neighbours = new List<Boid>();

	public void DrawDebugAxis()
	{
		Debug.DrawRay( transform.position, transform.forward, Color.blue );
		Debug.DrawRay( transform.position, transform.up, Color.green );
		Debug.DrawRay( transform.position, transform.right, Color.red );
	}

	public void UpdatePosition()
	{
		Quaternion fromRotation = transform.rotation;
		Quaternion toRotation = Quaternion.LookRotation( m_velocity, m_upAxis );
		transform.rotation = Quaternion.RotateTowards( fromRotation, toRotation, 500 * Time.fixedDeltaTime );

		float magnitude = m_velocity.magnitude;
		magnitude = Mathf.Clamp( magnitude, BoidManager.Instance.m_minSpeed, BoidManager.Instance.m_maxSpeed );

		Vector3 heading = transform.forward * magnitude * Time.fixedDeltaTime;
		Vector3 newPosition = transform.position + heading;

		transform.position = newPosition;
		transform.position = BoidManager.Instance.GetWrappedPosition( transform.position );
	}

	public Vector3 GetBoidVelocity()
	{
		return m_velocity;
	}

	public void SetBoidVelocity( Vector3 velocity )
	{
		m_velocity = velocity;
		Vector3 normVelocity = m_velocity.normalized;
		m_upAxis = Vector3.Cross( m_velocity, new Vector3( normVelocity.y, normVelocity.z, normVelocity.x ) );
	}

	public void AddNeighbour( Boid neighbour )
	{
		m_neighbours.Add( neighbour );
	}

	public void ClearNeighbours()
	{
		m_neighbours.Clear();
	}

	public int GetNeighbourCount()
	{
		return m_neighbours.Count;
	}

	public IEnumerable<Boid> IterateNeighbours()
	{
		foreach ( Boid neighbour in m_neighbours )
		{
			yield return neighbour;
		}
	}
}
