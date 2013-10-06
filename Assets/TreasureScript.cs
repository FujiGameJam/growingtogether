using UnityEngine;
using System.Collections;

public class TreasureScript : MonoBehaviour
{
	bool m_collectable = true;

	void OnTriggerEnter(Collider other)
	{
		if (m_collectable && other.CompareTag ("Player"))
		{
			m_collectable = false;
			GameObject.FindGameObjectWithTag ("GameController").GetComponent <GameControllerScript>().GetTreasure(other.GetComponent<AvatarScript>().m_playerID);
		}
	}
}
