using UnityEngine;
using System.Collections;

public class GameControllerScript : MonoBehaviour
{	
	public Transform[] m_players;
	
	public Transform[] m_levelPieces;
	public int m_levelPiecesMin = 6;
	public int m_levelPiecesMax = 8;
	
	int[] m_points = new int[4];
	Transform m_checkpoint;
	
	// Use this for initialization
	void Start ()
	{
		int levelPieces = Random.Range (m_levelPiecesMin, m_levelPiecesMax);
		
		int levelCount = 0;
		Transform currentPiece;
		
		currentPiece = Instantiate(m_levelPieces[Random.Range (0, m_levelPieces.Length)], Vector3.zero, Quaternion.identity) as Transform;
		SetCheckpoint(currentPiece.GetComponent<LevelPieceScript>().GetCheckpoint ());
		Respawn();
		
		while (levelCount < levelPieces)
		{
			levelConnectScript levelConnect = currentPiece.GetComponent<LevelPieceScript>().GetLevelConnect();
			
			if (levelConnect)
			{
				currentPiece = Instantiate(levelConnect.m_prefab, levelConnect.transform.position, levelConnect.transform.rotation) as Transform;
			}
			else
			{
				Debug.Log ("no level connect");
			}
			
			levelCount++;
		}
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
		if (m_checkpoint)
		{
			for (int i = 0; i < m_players.Length; i++)
			{
				if (m_players[i].gameObject.activeInHierarchy)
				{
					float angle = (45.0f + (90.0f * i)) * Mathf.Deg2Rad;
					m_players[i].position = m_checkpoint.position + new Vector3(Mathf.Sin (angle), 0.0f, Mathf.Cos (-angle)) * 2.0f;
					m_players[i].rotation = m_checkpoint.rotation;
					
					m_players[i].SendMessage("Reset");
				}
			}
		}
	}
}
