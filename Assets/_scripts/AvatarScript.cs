using UnityEngine;
using System.Collections;

public class AvatarScript : MonoBehaviour
{
	public enum ControlID
	{
		player1,
		player2,
		player3,
		player4
	}
	
	enum ControlState
	{
		Walking,
		Running,
		Jumping,
		Falling,
		Anchored
	}
	
	public int m_playerID;
	public ControlID m_controlID = ControlID.player1;

	public float m_movementSpeed = 5.0f;
	public float m_runSpeed = 8.0f;
	public float m_turnSpeed = 10.0f;
	public float m_jumpVelocity = 20.0f;
	public float m_playerGravity = -50.0f;
	public float m_airTensionMultiplier = 3.0f;
	public float m_waterDamping = 10.0f;

	Vector3 m_velocity = Vector3.zero;
	
	float m_stamina = 1.0f;

	Tether.Node tether;
	
	ControlState controlState = ControlState.Walking;

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

		switch ( m_controlID )
		{
			case ControlID.player1:
				result = "p1";
				break;
			case ControlID.player2:
				result = "p2";
				break;
			case ControlID.player3:
				result = "p3";
				break;
			case ControlID.player4:
				result = "p4";
				break;
		}
		
		if (Input.GetKey(KeyCode.Space))
		{
			result = "p1";
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

		// detect is the player is on the ground
		bool grounded = false;
		if (m_velocity.y <= 0.0)
		{
			foreach (RaycastHit hit in Physics.SphereCastAll (new Ray(transform.position + new Vector3(0.0f, 0.9f, 0.0f), -Vector3.up), 0.5f, 0.5f))
			{
				if (!hit.transform.CompareTag ("Player"))
				{
					grounded = true;
					break;
				}
			}
		}

		// detect state change
		if(grounded)
		{
			if (Input.GetButtonDown (Player() + "Jump"))
			{
				controlState = ControlState.Jumping;
				grounded = false;
				
				// set initialvelocity
				m_velocity = inputVector * (controlState == ControlState.Running ? m_runSpeed : m_movementSpeed);
				m_velocity.y = m_jumpVelocity;
			}
			else if (Input.GetAxis (Player() + "Anchor") >= 0.1f)
			{
				m_velocity = Vector3.zero;
				controlState = ControlState.Anchored;
			}
			else if (Input.GetAxis (Player() + "Run") >= 0.1f)
			{
				controlState = ControlState.Running;
			}
			else
			{
				controlState = ControlState.Walking;
			}
		}

		if (!grounded && controlState != ControlState.Jumping)
			controlState = ControlState.Falling;

		// update player for state
		switch(controlState)
		{
			case ControlState.Walking:
			case ControlState.Running:
				if (inputVector.magnitude > 0.1)
				{
					transform.rotation = Quaternion.Slerp (transform.rotation, Quaternion.LookRotation (inputVector), Time.deltaTime * m_turnSpeed);
				}
			
				m_velocity = inputVector * (controlState == ControlState.Running ? m_runSpeed : m_movementSpeed);
				m_velocity += tether.Force();
				break;
			case ControlState.Anchored:
				break;
			case ControlState.Jumping:
			case ControlState.Falling:
				transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(inputVector), Time.deltaTime * m_turnSpeed);

				Vector3 acc = inputVector * (controlState == ControlState.Running ? m_runSpeed : m_movementSpeed);
				acc.y = m_playerGravity;

				Vector3 force = tether.Force();
				float dir = Vector3.Dot(inputVector, force);

				m_velocity += acc*Time.deltaTime;
				m_velocity += tether.Force()*(dir > 0.0f? m_airTensionMultiplier*60.0f : 1.0f)*Time.deltaTime;

				if(transform.position.y < 0.0f)
				{
					m_velocity.y += -m_playerGravity*-transform.position.y*0.7f*Time.deltaTime;
					m_velocity *= 1.0f-m_waterDamping*Time.deltaTime;
				}
				break;
		}

		rigidbody.velocity = m_velocity;
	}
}
