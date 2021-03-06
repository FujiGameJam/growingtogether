using UnityEngine;
using System.Collections;

public class cameraScript : MonoBehaviour
{
	public Transform m_water;
	public Transform m_cameraMover;
	public Transform m_cameraGoal;
	public float m_followSpeed = 5.0f;
	public float m_trackSpeed = 10.0f;

	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void FixedUpdate ()
	{
		GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");
		
		Vector3 averagePosition = Vector3.zero;
		
		foreach (GameObject player in players)
		{
			averagePosition += player.transform.position;
		}
		
		averagePosition /= players.Length;
		
		m_cameraMover.position = averagePosition;
		
		transform.position = Vector3.Lerp (transform.position, m_cameraGoal.position, Time.deltaTime * m_followSpeed);
		
		transform.rotation = Quaternion.Slerp (transform.rotation, Quaternion.LookRotation ((m_cameraMover.position + Vector3.up) - transform.position), Time.deltaTime * m_trackSpeed);
		
		Vector3 waterPos = transform.position;
		waterPos.y = 0.0f;
		
		m_water.position = waterPos;
	}
}
