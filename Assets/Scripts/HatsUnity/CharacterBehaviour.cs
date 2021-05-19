using UnityEngine;

namespace HatsUnity
{
   public class CharacterBehaviour : MonoBehaviour
   {
		private readonly int MoveTrigger = Animator.StringToHash("Move");
		private readonly int HitTrigger = Animator.StringToHash("Hit");

		[SerializeField]
		private Animator _animator;

		[SerializeField]
		private SpriteRenderer _spriteRenderer;

		public void Move()
		{
			if(_animator)
			{
				_animator.SetTrigger(MoveTrigger);
			}
		}

		public void GetHit()
		{
			if(_animator)
			{
				_animator.SetTrigger(HitTrigger);
			}
		}

		public void SetDirection(bool facingLeft)
		{
			_spriteRenderer.flipX = facingLeft;
		}
   }
}