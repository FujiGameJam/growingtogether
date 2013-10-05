using UnityEngine;
using System.Collections;

public class GameControllerScript : MonoBehaviour
{
	public Transform[] m_players;
	
	int[] m_points = new int[4];
	Transform m_checkpoint;
	
	// Use this for initialization
	void Start ()
	{
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}
	
	public void AddPoint(int _playerID)
	{
		m_points[_playerID]++;
	}
	
	public void SetCheckpoint(Transform _checkpoint)
	{
		m_checkpoint = _checkpoint;
	}
	
	public void Respawn()
	{
//		if (m_checkpoint)
//		{
//			for (
//		}
	}
}
