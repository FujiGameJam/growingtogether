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
		Anchored,
		StageClear
	}

	public int m_playerID;
	public ControlID m_controlID = ControlID.player1;

	public Animation m_anim;

	public float m_movementSpeed = 5.0f;
	public float m_runSpeed = 8.0f;
	public float m_turnSpeed = 10.0f;
	public float m_jumpVelocity = 20.0f;
	public float m_playerGravity = -50.0f;
	public float m_airTensionMultiplier = 1.0f;
	public float m_waterDamping = 10.0f;

	public Transform m_tetherAttachment;

	Vector3 m_velocity = Vector3.zero;

	float m_stamina = 1.0f;
	float m_timeInWater = 0.0f;

	Tether.Node tether;

	ControlState controlState = ControlState.Walking;
	
	public AudioClip m_splashSound;
	
	public void Reset()
	{
		rigidbody.velocity = Vector3.zero;
		m_velocity = Vector3.zero;
		m_timeInWater = 0.0f;
		
		controlState = ControlState.Falling;
	}

	public void SetTether(Tether.Node n)
	{
		tether = n;
	}

	public Tether.Node GetTether()
	{
		return tether;
	}
	
	public void StageClear(bool _winner)
	{
		controlState = ControlState.StageClear;
		
		if (_winner)
		{
			m_anim.Play ("celebrate_flex");
		}
		else
		{
			m_anim.Play ("celebrate");
		}
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
		
		if (Input.GetKey(KeyCode.Joystick1Button1))
		{
			result = "p1";
		}

		return result;
	}

	// Use this for initialization
	void Start ()
	{
		m_anim["celebrate"].speed = Random.Range (0.9f, 1.1f);
		m_anim["jog"].speed = Random.Range (0.9f, 1.1f);
		m_anim["run"].speed = Random.Range (0.9f, 1.1f);
	}

	// Update is called once per frame
	void Update ()
	{
		Vector3 inputVector = new Vector3(Input.GetAxis ( Player() + "Horizontal" ), 0.0f, Input.GetAxis ( Player() + "Vertical" ));

		// detect is the player is on the ground
		bool grounded = false;
		if (controlState != ControlState.Jumping)
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
		
		if (controlState == ControlState.StageClear)
		{
			if (grounded)
			{
				rigidbody.velocity = Vector3.zero;
			}
			else
			{
				m_velocity.y += m_playerGravity * Time.deltaTime;
				
				rigidbody.velocity = m_velocity;
			}
			
			Vector3 cameraDirection = Camera.main.transform.position - transform.position;
			cameraDirection.y = 0.0f;
			
			transform.rotation = Quaternion.Slerp (transform.rotation, Quaternion.LookRotation (cameraDirection), Time.deltaTime * 3.0f);
			
			return;
		}
		
		// detect state change
		if(grounded)
		{
			if (m_anim.IsPlaying ("fall"))
			{
				m_anim.Play ("land");
			}
			
			if (Input.GetButtonDown (Player() + "Jump"))
			{
				controlState = ControlState.Jumping;
				grounded = false;

				// set initialvelocity
				m_velocity = inputVector * (controlState == ControlState.Running ? m_runSpeed : m_movementSpeed);
				m_velocity.y = m_jumpVelocity;
				
				m_anim.Play ("jump");
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

		if(!grounded && controlState != ControlState.Jumping)
				controlState = ControlState.Falling;

		switch(controlState)
		{
			case ControlState.Walking:
			case ControlState.Running:
				if (inputVector.magnitude > 0.1f)
				{
					transform.rotation = Quaternion.Slerp (transform.rotation, Quaternion.LookRotation (inputVector), Time.deltaTime * m_turnSpeed);
					
					if (controlState == ControlState.Walking)
					{
						m_anim.CrossFade ("jog", 0.125f);
					}
					else
					{
						if (tether.Force().magnitude > 7.0f)
						{
							m_anim.CrossFade ("pull_run", 0.3f);
						}
						else
						{
							m_anim.CrossFade ("run", 0.3f);
						}
					}
				}
				else
				{
					if (!m_anim.IsPlaying ("land") || m_anim["land"].time > m_anim["land"].length - 0.25f)
					{
						m_anim.CrossFade ("idle", 0.25f);
					}
				}

				m_velocity = inputVector * (controlState == ControlState.Running ? m_runSpeed : m_movementSpeed);
				m_velocity += tether.Force();
				break;
			case ControlState.Anchored:
				m_anim.CrossFade ("anchor_idle", 0.1f);
				break;
			case ControlState.Jumping:
			case ControlState.Falling:
				transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(inputVector), Time.deltaTime * m_turnSpeed);

				Vector3 acc = inputVector * (controlState == ControlState.Running ? m_runSpeed : m_movementSpeed);
				acc.y = m_playerGravity;

				m_velocity += acc*Time.deltaTime;

				if (m_velocity.y <= 0.0f)
					controlState = ControlState.Falling;

				Vector3 force = tether.Force();
				float dir = Vector3.Dot(inputVector, force.normalized);

				// handle sling-shot manoeuvre
				float extraAction = (dir > 0.0f ? dir * m_airTensionMultiplier * 60.0f : 1.0f);

				// handle tether force
				m_velocity += force * extraAction * Time.deltaTime;

				// if they're in the water, apply bouyancy
				if(transform.position.y < 0.0f)
				{
					m_velocity.y += -m_playerGravity*-transform.position.y*0.7f*Time.deltaTime;
					m_velocity *= 1.0f-m_waterDamping*Time.deltaTime;
				
					if (m_timeInWater == 0.0f)
					{
						AudioSource.PlayClipAtPoint(m_splashSound, transform.position);
					}
				
					m_anim.CrossFade ("swim", 0.25f);
				
					m_timeInWater += Time.deltaTime;
				
					if (m_timeInWater > 1.5f)
					{
						GameControllerScript gameController = GameObject.FindGameObjectWithTag ("GameController").GetComponent<GameControllerScript>();
					
						gameController.LosePoints(m_playerID);
						gameController.Respawn ();
					}
				}
				else
				{
					if (!m_anim.IsPlaying ("jump"))
					{
						m_anim.CrossFade ("fall", 0.125f);
					}
				
					m_timeInWater = 0.0f;
				}
				break;
		}

		rigidbody.velocity = m_velocity;

		// try casting a capcule along the velocity vector
		foreach (RaycastHit hit in Physics.CapsuleCastAll(transform.position + new Vector3(0, 0.5f, 0), transform.position + new Vector3(0, 1.5f, 0), 0.5f, m_velocity.normalized, m_velocity.magnitude * Time.deltaTime))
		{
			if (hit.transform.CompareTag ("Player"))
				continue;

			// if we hit a wall, redirect the velocity.
			int x = 0;

		}
	}
}
