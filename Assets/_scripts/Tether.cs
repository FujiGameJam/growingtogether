using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Tether : MonoBehaviour
{
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
			return f;
		}
	}

	class Joint
	{
		public Vector3 c1, c2;
		public Node n1, n2;
		public float tension;
	}

	public int numNodes = 5;
	public float gravityScale = 0.2f;
	public float friction = 2.0f;

	List<Node> nodes;
	List<Joint> joints;

	Node root;

	// Use this for initialization
	void Start()
	{
		nodes = new List<Node>();
		joints = new List<Joint>();

		root = CreateNode(transform.position, 1);

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
			avatar.GetTether().pos = avatar.transform.position;
		}

		Step();

		transform.position = root.pos;
	}

	void OnDrawGizmos()
	{
		if (nodes == null)
			return;

		Gizmos.color = Color.blue;
		foreach(var node in nodes)
			Gizmos.DrawCube(node.pos, Vector3.one * 0.1f);
	}

	Node CreateNode(Vector3 pos, float mass, Node parent = null)
	{
		Node n = new Node();
		n.prevPos = n.pos = pos;
		n.mass = mass;
		n.parent = parent;
		nodes.Add(n);
		return n;
	}

	Node CreateTendril(Vector3 target, int numNodes)
	{
		Vector3 diff = target - transform.position;
		diff /= numNodes;

		Node prev = root;
		Node n = null;

		for (var i = 1; i <= numNodes; ++i)
		{
			n = CreateNode(transform.position + diff * i, i == numNodes ? 0 : 1, prev);
			Joint j = new Joint();
			j.n1 = prev;
			j.n2 = n;
			j.tension = 1;
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
	}
}
