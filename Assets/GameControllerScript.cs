using UnityEngine;
using System.Collections;

public class GameControllerScript : MonoBehaviour
{	
	public Transform[] m_players;
	
	public Transform[] m_levelPieces;
	public int m_levelPiecesMin = 6;
	public int m_levelPiecesMax = 8;

	public TextMesh[] m_scoreText;
	
	int[] m_points = new int[4];
	Transform m_checkpoint;

	int numPlayers;
	
	// Use this for initialization
	void Start ()
	{
		int levelPieces = Random.Range (m_levelPiecesMin, m_levelPiecesMax);
		
		int levelCount = 0;
		Transform currentPiece;
		
		currentPiece = Instantiate(m_levelPieces[Random.Range (0, m_levelPieces.Length)]) as Transform;
		SetCheckpoint(currentPiece.GetComponent<LevelPieceScript>().GetCheckpoint ());
		currentPiece.GetComponent<LevelPieceScript>().HideTreasure();
		Respawn();

		numPlayers = 0;
		for (int i = 0; i < 4; ++i)
		{
			if (m_players[i].gameObject.activeInHierarchy)
			{
				++numPlayers;
				m_scoreText[i].gameObject.SetActive(true);
				m_scoreText[i].text = "0";
			}
			else
			{
				m_scoreText[i].gameObject.SetActive(false);
			}
		}

		// layout scores
		float scoreArea = 0.3f;
		float scoreIncrement = scoreArea / (numPlayers - 1);
		for (int i = 0; i < numPlayers; ++i)
		{
			Vector3 pos = m_scoreText[i].transform.localPosition;
			pos.x = -scoreArea * 0.5f + scoreIncrement * i;
			m_scoreText[i].transform.localPosition = pos;
		}

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
			
			if (levelCount < levelPieces)
			{
				currentPiece.GetComponent<LevelPieceScript>().HideTreasure();
			}
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}
	
	public void AddPoint(int _playerID)
	{
		m_points[_playerID]++;

		m_scoreText[_playerID].text = m_points[_playerID].ToString();
	}
	
	public void GetTreasure(int _playerID)
	{
		m_points[_playerID] += 20;
		
		int winningPoints = 0;
		
		foreach (int points in m_points)
		{
			if (points > winningPoints)
			{
				winningPoints = points;
			}
		}
		
		for (int i = 0; i < m_players.Length; i++)
		{
			if (m_players[i].gameObject.activeInHierarchy)
			{
				if (m_points[i] == winningPoints)
				{
					m_players[i].SendMessage("StageClear", true);
				}
				else
				{
					m_players[i].SendMessage("StageClear", false);
				}
			}
		}
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
			
			GameObject.Find ("tether").SendMessage ("Reset");
		}
	}
}
