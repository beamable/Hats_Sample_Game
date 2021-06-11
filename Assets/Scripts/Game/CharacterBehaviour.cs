using System.Threading.Tasks;
using Beamable.UI.Scripts;
using Hats.Simulation;
using Hats.Content;
using UnityEngine;

namespace Hats.Game
{
	public class CharacterBehaviour : MonoBehaviour
	{
		private readonly int MoveTrigger = Animator.StringToHash("Move");
		private readonly int HitTrigger = Animator.StringToHash("Hit");
		private readonly int AttackTrigger = Animator.StringToHash("Attack");

		[Header("Prefab References")]
		public SpriteRenderer HatRendererPrefab;

		[Header("Internal References")]
		public Transform HatAnchor;

		[Header("Internals")]
		[ReadOnly]
		[SerializeField]
		private Sprite HatSprite;

		[ReadOnly]
		[SerializeField]
		private HatContent HatContent;

		[SerializeField]
		private Animator _animator;

		[SerializeField]
		private SpriteRenderer _spriteRenderer;

		public async Task SetHat(HatContent hatContent)
		{
			HatSprite = await hatContent.icon.LoadSprite();

			ClearHat();
			var hat = Instantiate(HatRendererPrefab, HatAnchor);
			hat.sprite = HatSprite;
			hat.transform.localPosition = new Vector3(hatContent.Offset.x + .01f, hatContent.Offset.y + .053f, 0);
			hat.transform.localEulerAngles = new Vector3(0, 0, hatContent.Rotation + -8.58f);
		}

		public void ClearHat()
		{
			for (var i = 0; i < HatAnchor.childCount; i++)
			{
				Destroy(HatAnchor.GetChild(i).gameObject);
			}
		}

		public void Move()
		{
			if (_animator)
			{
				_animator.SetTrigger(MoveTrigger);
			}
		}

		public void GetHit()
		{
			if (_animator)
			{
				_animator.SetTrigger(HitTrigger);
			}
		}

		public void Attack()
		{
			if(_animator)
			{
				_animator.SetTrigger(AttackTrigger);
			}
		}

		public void SetDirection(bool facingLeft)
		{
			transform.localScale = new Vector3(facingLeft ? -1f : 1f, 1f, 1f);
		}
	}
}