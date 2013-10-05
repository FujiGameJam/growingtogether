using UnityEngine;
using System.Collections;

public class CoinScript : MonoBehaviour {
	
	bool m_collectable = true;
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnTriggerEnter(Collider other)
	{
		if (m_collectable && other.CompareTag ("Player"))
		{
			m_collectable = false;
			animation.CrossFade ("coin_collect", 0.025f);
		}
	}
	
	void RemoveCoin()
	{
		Destroy (gameObject);
	}
}
