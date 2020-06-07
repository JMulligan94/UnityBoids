using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell
{
	public Vector3 m_minAABB;
	public Vector3 m_maxAABB;

	private Vector3 m_position;
	private Vector3 m_size;

	private List<Boid> m_boidsInCell;

	public Cell( Vector3 minAABB, Vector3 size )
	{
		m_position = minAABB + ( size / 2.0f );
		m_size = size;
		m_minAABB = minAABB;
		m_maxAABB = minAABB + size;

		m_boidsInCell = new List<Boid>();
	}

	public void OnDrawGizmos()
	{
		float alpha = m_boidsInCell.Count * 0.01f;
		Gizmos.color = new Color( 1, 0, 0, alpha );
		Gizmos.DrawCube( m_position, m_size );
	}

	public void Clear()
	{
		m_boidsInCell.Clear();
	}

}
