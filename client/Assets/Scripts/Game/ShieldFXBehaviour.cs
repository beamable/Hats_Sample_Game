using UnityEngine;

namespace Hats.Game
{
    public class ShieldFXBehaviour : MonoBehaviour
    {
        public ParticleSystem[] particles;
        
        public void End()
        {
            foreach (var particle in particles)
            {
                particle.Stop();
            }
        }
    }
}
