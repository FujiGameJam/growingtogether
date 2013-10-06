using UnityEngine;
using System.Collections;

public class LevelPieceScript : MonoBehaviour
{
	public levelConnectScript[] m_levelNodes;
	
	public Transform m_treasureNode;
	
	public Transform m_checkpoint;
	
	public Transform GetCheckpoint()
	{
		return m_checkpoint;
	}
	
	public void HideTreasure()
	{
		m_treasureNode.gameObject.SetActive(false);
	}
	
	public levelConnectScript GetLevelConnect()
	{
		int index = Random.Range (0, m_levelNodes.Length);
		int count = 0;
		
		bool found = false;
		
		while (!found && count < m_levelNodes.Length)
		{
			if (m_levelNodes[index].m_valid)
			{
				found = true;
			}
			else
			{
				index++;
				
				if (index == m_levelNodes.Length)
				{
					index = 0;
				}
				
				count++;
			}
		}
		
		if (found)
		{
			return m_levelNodes[index];
		}
		else
		{
			return null;
		}
	}
}
