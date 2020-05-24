using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputeBoid : MonoBehaviour
{
	private Vector3 m_velocity;
	private List<ComputeBoid> m_neighbours = new List<ComputeBoid>();

	public void DrawDebugAxis()
	{
		Debug.DrawRay( transform.position, transform.forward, Color.blue );
		Debug.DrawRay( transform.position, transform.up, Color.green );
		Debug.DrawRay( transform.position, transform.right, Color.red );
	}

	public void UpdatePosition()
	{
		if ( m_velocity == Vector3.zero )
			m_velocity = transform.forward;

		Vector3 normVelocity = m_velocity.normalized;
		Vector3 upAxis = Vector3.Cross( m_velocity, new Vector3( m_velocity.y, m_velocity.z, m_velocity.x ) );

		Quaternion fromRotation = transform.rotation;
		Quaternion toRotation = Quaternion.LookRotation( m_velocity, upAxis.normalized );
		transform.rotation = Quaternion.RotateTowards( fromRotation, toRotation, 50 * Time.fixedDeltaTime );

		float magnitude = m_velocity.magnitude;
		magnitude = Mathf.Clamp( magnitude, ComputeBoidManager.Instance.m_minSpeed, ComputeBoidManager.Instance.m_maxSpeed );

		m_velocity = transform.forward * magnitude;
		Vector3 newPosition = transform.position + ( m_velocity * Time.fixedDeltaTime );

		ComputeBoidManager.Instance.GetWrappedPosition( ref newPosition );
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

	public void AddNeighbour( ComputeBoid neighbour )
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

	public IEnumerable<ComputeBoid> IterateNeighbours()
	{
		foreach ( ComputeBoid neighbour in m_neighbours )
		{
			yield return neighbour;
		}
	}
}
