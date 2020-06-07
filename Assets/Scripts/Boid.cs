using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
	public float m_minSpeed = 2.0f;
	public float m_maxSpeed = 6.0f;

	public float m_aheadRange = 1.0f;

	public List<Cell> m_cellsToCheck = null;

	private Vector3 m_velocity;
	private List<Boid> m_neighbours = new List<Boid>();

	public void OnStart()
	{
		m_velocity = transform.forward * m_minSpeed;
	}

	public void DrawDebugAxis()
	{
		Debug.DrawRay( transform.position, transform.forward, Color.blue );
		Debug.DrawRay( transform.position, transform.up, Color.green );
		Debug.DrawRay( transform.position, transform.right, Color.red );
	}

	public void UpdatePosition()
	{
		Vector3 normVelocity = m_velocity.normalized;
		Vector3 upAxis = Vector3.Cross( m_velocity, new Vector3( m_velocity.y, m_velocity.z, m_velocity.x ) );

		Quaternion fromRotation = transform.rotation;
		Quaternion toRotation = Quaternion.LookRotation( m_velocity, upAxis.normalized );
		transform.rotation = Quaternion.RotateTowards( fromRotation, toRotation, 500 * Time.fixedDeltaTime );

		float magnitude = m_velocity.magnitude;
		magnitude = Mathf.Clamp( magnitude, m_minSpeed, m_maxSpeed );

		Vector3 heading = transform.forward * magnitude * Time.fixedDeltaTime;
		Vector3 newPosition = transform.position + heading;

		transform.position = newPosition;
	}

	public Vector3 GetBoidVelocity()
	{
		return m_velocity;
	}

	public void SetBoidVelocity( Vector3 velocity )
	{
		m_velocity = velocity;
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
