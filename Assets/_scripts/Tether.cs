using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Tether : MonoBehaviour
{
	public int numNodes = 5;
	public int solverIterations = 3;
	public float gravityScale = 0.1f;
	public float friction = 2.0f;
	static public float tensionRamp = 1.5f;
	static public float purchaseDistance = 2.0f;

	float sphereSize = 0.2f;

	public class Node
	{
		public Vector3 pos;
		public Vector3 prevPos;
		public float mass;

		public Node parent;

		public Vector3 Force()
		{
			Vector3 f = Vector3.zero;
			Node n = this;
			while (n.parent != null)
			{
				f += n.parent.pos - n.pos;
				n = n.parent;
			}

			return f.normalized * Mathf.Pow(Mathf.Max(f.magnitude - Tether.purchaseDistance, 0.0f), Tether.tensionRamp);
		}
	}

	class Joint
	{
		public Vector3 c1, c2;
		public Node n1, n2;
		public float tension;
	}

	List<Node> nodes;
	List<Joint> joints;

	Node root;

	// Use this for initialization
	void Start()
	{
		nodes = new List<Node>();
		joints = new List<Joint>();

		root = CreateNode(transform.position, 1, null);

		GameObject[] objects = GameObject.FindGameObjectsWithTag("Player");

		foreach (var player in objects)
		{
			AvatarScript avatar = player.GetComponent<AvatarScript>();
			Node tether = CreateTendril(player.transform.position, numNodes);
			avatar.SetTether(tether);
		}
	}

	// Update is called once per frame
	void Update()
	{
		GameObject[] objects = GameObject.FindGameObjectsWithTag("Player");
		foreach (var player in objects)
		{
			AvatarScript avatar = player.GetComponent<AvatarScript>();
			avatar.GetTether().pos = avatar.transform.position + Vector3.up;
		}

		Step();

		transform.position = root.pos;
	}

	void OnDrawGizmos()
	{
		if (nodes == null)
			return;

		Gizmos.color = Color.blue;
		foreach (var node in nodes)
			Gizmos.DrawSphere(node.pos, sphereSize);
	}

	Node CreateNode(Vector3 pos, float mass, Node parent)
	{
		pos += Vector3.up;

		Node n = new Node();
		n.prevPos = n.pos = pos;
		n.mass = mass;
		n.parent = parent;
		//n.tether = this;
		nodes.Add(n);
		return n;
	}

	Node CreateTendril(Vector3 target, int numNodes)
	{
		target += Vector3.up;

		Vector3 diff = target - root.pos;
		diff /= numNodes;

		Node prev = root;
		Node n = null;

		for (var i = 1; i <= numNodes; ++i)
		{
			// some random function to calculate the tension values (non-linear distribution of nodes)
			//float x = i - 1;
			//x = 1.0f / numNodes * x;
			//x = Mathf.Pow(x, x);
			//x *= x;
			float x = 1;

			// create the joint
			n = CreateNode(transform.position + diff * i, i == numNodes ? 0 : 1, prev);
			Joint j = new Joint();
			j.n1 = prev;
			j.n2 = n;
			j.tension = x;
			joints.Add(j);
			prev = n;
		}

		return n;
	}

	void Step()
	{
		// update
		float delta = Time.deltaTime;
		Vector3 gravity = Physics.gravity * gravityScale;

		// physics the nodes
		foreach (var n in nodes)
		{
			Vector3 velocity = n.pos - n.prevPos;
			n.prevPos = n.pos;

			if (n.mass == 0.0f)
				continue;

			n.pos += velocity * (1.0f - friction * delta) + gravity * delta;
		}

		// calculate joints
		for (int i = 0; i < solverIterations; ++i)
		{
			foreach (var j in joints)
			{
				Vector3 diff = j.n2.pos - j.n1.pos;

				Vector3 dir = diff.normalized;

				float d = diff.magnitude * j.tension;
				float tension = d * 0.5f;

				float totalMass = j.n1.mass + j.n2.mass;
				float n1bias = j.n1.mass / totalMass;
				float n2bias = j.n2.mass / totalMass;
				j.c1 = dir * tension * n1bias;
				j.c2 = -dir * tension * n2bias;
			}

			foreach (var j in joints)
			{
				j.n1.pos += j.c1;
				j.n2.pos += j.c2;
			}

			Collide();
		}
	}

	void Collide()
	{
		foreach (var n in nodes)
		{
			Vector3 d = n.pos - n.prevPos;
			Ray r = new Ray(n.prevPos, d);
			foreach (RaycastHit hit in Physics.SphereCastAll(r, sphereSize, d.magnitude))
			{
				if (hit.transform.CompareTag("Player"))
					continue;

				// slide the sphere along the surface...
				Vector3 perpendicular = Vector3.Cross(r.direction, hit.normal).normalized;
				Vector3 slide = Vector3.Cross(hit.normal, perpendicular);
				float dot = Vector3.Dot(r.direction, slide);
				n.pos = r.origin + r.direction * hit.distance + slide * dot * (d.magnitude - hit.distance) + hit.normal * 0.01f;
			}
		}
	}

	void Teleport()
	{
		// find new root, average of player positions
		root.pos = Vector3.zero;
		GameObject[] objects = GameObject.FindGameObjectsWithTag("Player");
		foreach (var player in objects)
			root.pos += player.transform.position;
		root.pos /= objects.Length;
		root.pos += Vector3.up;

		// set tether chain nodes
		foreach (var player in objects)
		{
			AvatarScript avatar = player.GetComponent<AvatarScript>();
			Node n = avatar.GetTether();

			// tether starts at player position
			Vector3 tether = avatar.transform.position - root.pos;
			tether /= numNodes;

			// nodes lerp between player position and root position for each node back towards the root
			for (var i = 0; i < numNodes; ++i)
			{
				n.prevPos = n.pos = avatar.transform.position + tether * i + Vector3.up;
				n = n.parent;
			}
		}

		// rin a simulation step
		Step();

		// set the transform to the root pos
		transform.position = root.pos;
	}
}
