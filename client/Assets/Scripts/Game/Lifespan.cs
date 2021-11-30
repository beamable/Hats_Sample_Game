using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Destroy the GameObject after some amount of time.
/// </summary>
public class Lifespan : MonoBehaviour
{
	[Min(0f)]
	public float lifespan = 1f;

	private void Awake()
	{
		Destroy(gameObject, lifespan);
	}
}
