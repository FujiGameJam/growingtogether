using UnityEngine;
using System.Collections;

public class CheckpointScript : MonoBehaviour
{

	void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag ("Player"))
		{
			GameObject.FindGameObjectWithTag ("GameController").GetComponent<GameControllerScript>().SetCheckpoint(transform);
		}
	}
}
