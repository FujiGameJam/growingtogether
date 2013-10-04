using UnityEngine;
using System.Collections;

public class AvatarScript : MonoBehaviour
{
	public enum PlayerID
	{
		player1,
		player2,
		player3,
		player4
	}

	public PlayerID m_playerID = PlayerID.player1;

	public float m_movementSpeed = 5.0f;
	public float m_runSpeed = 8.0f;
	public float m_turnSpeed = 5.0f;

	float verticalForce = 0.0f;

	Tether.Node tether;

	public void SetTether(Tether.Node n)
	{
		tether = n;
	}

	public Tether.Node GetTether()
	{
		return tether;
	}

	string Player ()
	{
		string result = "";

		switch ( m_playerID )
		{
			case PlayerID.player1:
				result = "p1";
				break;
			case PlayerID.player2:
				result = "p2";
				break;
			case PlayerID.player3:
				result = "p3";
				break;
			case PlayerID.player4:
				result = "p4";
				break;
		}

		return result;
	}

	// Use this for initialization
	void Start ()
	{

	}

	// Update is called once per frame
	void Update ()
	{
		Vector3 inputVector = new Vector3(Input.GetAxis ( Player() + "Horizontal" ), 0.0f, Input.GetAxis ( Player() + "Vertical" ));
		bool running = false;

		if (inputVector.magnitude > 0.1)
		{
			transform.rotation = Quaternion.Slerp (transform.rotation, Quaternion.LookRotation (inputVector), Time.deltaTime * m_turnSpeed);
		}

		if (verticalForce <= 0.0 && Physics.SphereCast (new Ray(transform.position, -Vector3.up), 0.5f, 0.5f))
		{
			if (Input.GetButtonDown (Player() + "Jump"))
			{
				verticalForce = 4.0f;
			}
			else
			{
				verticalForce = 0.0f;
				
				if (Input.GetAxis (Player () + "Anchor") > 0.1f)
				{
					inputVector = Vector3.zero;
				}
				else if (Input.GetAxis (Player () + "Run") > 0.1f)
				{
					running = true;
				}
			}
		}
		else
		{
			verticalForce -= Time.deltaTime * 9.8f;
		}

		inputVector.y = verticalForce;

		rigidbody.velocity = inputVector * (running ? m_runSpeed : m_movementSpeed);

		rigidbody.velocity += tether.Force();
	}
}
