using UnityEngine;

namespace Hats.Game
{
    public class ShieldFXBehaviour : MonoBehaviour
    {
        public ParticleSystem[] particles;


        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void End()
        {
            foreach (var particle in particles)
            {
                particle.Stop();
            }
        }
    }
}
