using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Tether : MonoBehaviour
{
	public int numNodes = 5;
	public int solverIterations = 5;
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

	class TetherTransform
	{
		public TetherTransform(GameObject tetherVis, string nodeName, TetherTransform parent)
		{
			t = tetherVis.transform.Find(nodeName);
			this.parent = parent;
		}

		public Transform t;
		public TetherTransform parent;
	}

	Transform blobTransform;
	TetherTransform[] tetherTransforms;

	AvatarScript[] players;

	Vector3[] playerNormals = new Vector3[4];
	Plane playerPlane;

	// Use this for initialization
	void Start()
	{
		nodes = new List<Node>();
		joints = new List<Joint>();

		root = CreateNode(transform.position, 1, null);

		GameObject[] objects = GameObject.FindGameObjectsWithTag("Player");

		players = new AvatarScript[objects.Length];
		foreach (var player in objects)
		{
			AvatarScript avatar = player.GetComponent<AvatarScript>();
			Node tether = CreateTendril(player.transform.position, numNodes);
			avatar.SetTether(tether);

			players[avatar.m_playerID] = avatar;
		}

		string visName = "goo_" + players.Length.ToString();
		GameObject tetherVis = GameObject.Find(visName);

		blobTransform = tetherVis.transform.Find("bone_root");

		tetherTransforms = new TetherTransform[4];
		tetherTransforms[0] = new TetherTransform(tetherVis, "bone_s_001", null);
		tetherTransforms[0] = new TetherTransform(tetherVis, "bone_s_002", tetherTransforms[0]);
		tetherTransforms[0] = new TetherTransform(tetherVis, "bone_s_003", tetherTransforms[0]);
		tetherTransforms[0] = new TetherTransform(tetherVis, "bone_s_004", tetherTransforms[0]);

		tetherTransforms[1] = new TetherTransform(tetherVis, "bone_e_001", null);
		tetherTransforms[1] = new TetherTransform(tetherVis, "bone_e_002", tetherTransforms[1]);
		tetherTransforms[1] = new TetherTransform(tetherVis, "bone_e_003", tetherTransforms[1]);
		tetherTransforms[1] = new TetherTransform(tetherVis, "bone_e_004", tetherTransforms[1]);

		if (players.Length > 2)
		{
			tetherTransforms[2] = new TetherTransform(tetherVis, "bone_n_001", null);
			tetherTransforms[2] = new TetherTransform(tetherVis, "bone_n_002", tetherTransforms[2]);
			tetherTransforms[2] = new TetherTransform(tetherVis, "bone_n_003", tetherTransforms[2]);
			tetherTransforms[2] = new TetherTransform(tetherVis, "bone_n_004", tetherTransforms[2]);
		}

		if (players.Length > 3)
		{
			tetherTransforms[3] = new TetherTransform(tetherVis, "bone_w_001", null);
			tetherTransforms[3] = new TetherTransform(tetherVis, "bone_w_002", tetherTransforms[3]);
			tetherTransforms[3] = new TetherTransform(tetherVis, "bone_w_003", tetherTransforms[3]);
			tetherTransforms[3] = new TetherTransform(tetherVis, "bone_w_004", tetherTransforms[3]);
		}
	}

	float Angle(float a1, float a2)
	{
		if (a2 >= 0.0f)
			return Mathf.Acos(a1);
		else
			return Mathf.PI * 2.0f - Mathf.Acos(a1);
	}

	void SetNodeMatrix(Node n, TetherTransform t, Vector3 nextPos, Vector3 up)
	{
		if(t.parent != null)
			SetNodeMatrix(n.parent, t.parent, n.pos, up);
		t.t.position = n.pos;
		t.t.LookAt(n.pos + (nextPos - n.parent.pos).normalized, up);
	}

	// Update is called once per frame
	void Update()
	{
		foreach (var player in players)
			player.GetTether().pos = player.m_tetherAttachment.position;

		Step();

		transform.position = root.pos;

		// find the order of players in a circle around the goo blob
		switch (players.Length)
		{
			case 2:
			{
				Vector3 diff = players[1].transform.position - players[0].transform.position;
				Vector3 n = Vector3.Cross(diff, Vector3.Cross(diff, Vector3.up));
				if (n.y < 0.0f)
					n = -n;
				playerPlane = new Plane(n.normalized, root.pos);
				break;
			}
			case 3:
			{
				Vector3 n = Vector3.Cross(players[1].transform.position - players[0].transform.position, players[2].transform.position - players[0].transform.position);
				if (n.y < 0.0f)
					n = -n;
				playerPlane = new Plane(n.normalized, root.pos);
				break;
			}
			case 4:
			{
				Vector3 n0 = Vector3.Cross(players[1].transform.position - players[0].transform.position, players[2].transform.position - players[0].transform.position);
				Vector3 n1 = Vector3.Cross(players[2].transform.position - players[0].transform.position, players[3].transform.position - players[0].transform.position);
				if (n0.y < 0.0f) n0 = -n0;
				if (n1.y < 0.0f) n1 = -n1;
				playerPlane = new Plane((n0 + n1).normalized, root.pos);
				break;
			}
			default:
				break;
		}

		// generate coplanar normals for each player
		for (int i = 0; i < players.Length; ++i)
			playerNormals[i] = (players[i].transform.position + playerPlane.normal * -playerPlane.GetDistanceToPoint(players[i].transform.position) - root.pos).normalized;

		Vector3 p0 = Vector3.Cross(Vector3.forward, playerPlane.normal);
		Vector3 p1 = Vector3.Cross(Vector3.right, playerPlane.normal);

		// calculate the angles for each player
		float[] angles = new float[players.Length];
		for (int i = 0; i < players.Length; ++i)
			angles[i] = Angle(Vector3.Dot(playerNormals[i], p0), Vector3.Dot(playerNormals[i], p1));

		// and from that we can conclude the order!
		int[] order = new int[players.Length];
		int[] index = new int[players.Length];
		for (int i = 0; i < players.Length; ++i)
		{
			for (int j = 0; j < players.Length; ++j)
			{
				if (i != j && angles[i] <= angles[j])
					++order[j];
			}
		}

		for (int i = 0; i < players.Length; ++i)
			index[order[i]] = i;

		// rotate the goo blob
		blobTransform.position = root.pos;
		blobTransform.LookAt(root.pos + playerNormals[index[0]], playerPlane.normal);

		// set the tethers
		for (int i = 0; i < players.Length; ++i)
			SetNodeMatrix(players[index[i]].GetTether(), tetherTransforms[i], players[index[i]].GetTether().pos, playerPlane.normal);
	}

	void OnDrawGizmos()
	{
		if (nodes == null)
			return;
/*
		Gizmos.color = Color.blue;
		foreach (var node in nodes)
			Gizmos.DrawSphere(node.pos, sphereSize);

		Gizmos.color = Color.yellow;
		Gizmos.DrawLine(root.pos, root.pos + playerNormals[0]);
		Gizmos.color = Color.blue;
		Gizmos.DrawLine(root.pos, root.pos + playerNormals[1]);
		Gizmos.color = Color.green;
		Gizmos.DrawLine(root.pos, root.pos + playerNormals[2]);
		Gizmos.color = Color.gray;
		Gizmos.DrawLine(root.pos, root.pos + playerNormals[3]);
		Gizmos.color = Color.white;
		Gizmos.DrawLine(root.pos, root.pos + playerPlane.normal);
*/
	}

	Node CreateNode(Vector3 pos, float mass, Node parent)
	{
		pos += Vector3.up;

		Node n = new Node();
		n.prevPos = n.pos = pos;
		n.mass = mass;
		n.parent = parent;
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

	public void Reset()
	{
		// find new root, average of player positions
		root.pos = Vector3.zero;
		foreach (var player in players)
			root.pos += player.transform.position;
		root.pos /= players.Length;
		root.pos += Vector3.up;
		root.prevPos = root.pos;

		// set tether chain nodes
		foreach (var player in players)
		{
			Node n = player.GetTether();

			// tether starts at player position
			Vector3 tether = player.transform.position - root.pos;
			tether /= numNodes;

			// nodes lerp between player position and root position for each node back towards the root
			for (var i = 0; i < numNodes; ++i)
			{
				n.prevPos = n.pos = player.m_tetherAttachment.position + tether * i;
				n = n.parent;
			}
		}

		// run a simulation step
		Step();

		// set the transform to the root pos
		transform.position = root.pos;
	}
}
