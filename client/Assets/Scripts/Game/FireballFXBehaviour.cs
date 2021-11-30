using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hats.Game
{
    public class FireballFXBehaviour : MonoBehaviour
    {
        public float Speed = 1;

		[SerializeField]
		private GameObject impactEffect;

        public void End()
        {
			// Create an impact effect
			if(impactEffect)
			{
				Instantiate(impactEffect, transform.position - Vector3.forward, Quaternion.identity);
			}
            Destroy(gameObject);
        }
    }
}