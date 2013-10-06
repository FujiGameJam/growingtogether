using UnityEngine;
using System.Collections;

public class GameControllerScript : MonoBehaviour
{	
	public Transform[] m_players;
	
	public Transform[] m_levelPieces;
	public int m_levelPiecesMin = 6;
	public int m_levelPiecesMax = 8;

	public TextMesh[] m_scoreText;

	public Font m_activeText;
	public Font m_inactiveText;
	public TextMesh m_againText;
	public TextMesh m_exitText;
	public GameObject m_booty;

	int[] m_points = new int[4];
	Transform m_checkpoint;

	int numPlayers;

	int menuSelection;
	
	public AudioClip m_coinSound;
	public AudioClip m_failSound;
	
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

		m_againText.gameObject.SetActive(false);
		m_exitText.gameObject.SetActive(false);
		m_booty.SetActive(false);
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}
	
	public void AddPoint(int _playerID)
	{
		m_points[_playerID]++;

		m_scoreText[_playerID].text = m_points[_playerID].ToString();
		AudioSource.PlayClipAtPoint(m_coinSound, m_players[_playerID].position);
	}
	
	public void LosePoints(int _playerID)
	{
		m_points[_playerID] = Mathf.Max(0, m_points[_playerID] - 10);
		AudioSource.PlayClipAtPoint(m_failSound, Vector3.zero);
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

		m_booty.SetActive(true);

		menuSelection = 0;

		UpdateText();
	}

	void UpdateText()
	{
		m_againText.gameObject.SetActive(true);
		m_againText.font = m_activeText;
		m_againText.characterSize = 1.5f;
		m_exitText.gameObject.SetActive(true);
		m_exitText.font = m_inactiveText;
		m_exitText.characterSize = 1.0f;
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
					m_players[i].position = m_checkpoint.position + new Vector3(Mathf.Sin (angle), 5.0f, Mathf.Cos (-angle)) * 2.0f;
					m_players[i].rotation = m_checkpoint.rotation;
					
					m_players[i].SendMessage("Reset");
				}
			}
			
			GameObject.Find ("tether").SendMessage ("Reset");
		}
	}
}
