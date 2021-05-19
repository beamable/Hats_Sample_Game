using System.Threading.Tasks;
using Beamable.UI.Scripts;
using HatsCore;
using HatsContent;
using UnityEngine;

namespace HatsUnity
{
   public class CharacterBehaviour : MonoBehaviour
   {
	   private readonly int MoveTrigger = Animator.StringToHash("Move");
	   private readonly int HitTrigger = Animator.StringToHash("Hit");

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