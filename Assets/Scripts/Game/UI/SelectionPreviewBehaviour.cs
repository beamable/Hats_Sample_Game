using Hats.Simulation;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Hats.Game.UI
{
	public class SelectionPreviewBehaviour : MonoBehaviour, IPointerDownHandler
	{
		[SerializeField]
		private SpriteRenderer previewIconSpriteRenderer;

		[Header("Icons")]
		[SerializeField]
		private Sprite walkPreviewSprite;

		[SerializeField]
		private Sprite fireballPreviewSprite;

		[SerializeField]
		private Sprite arrowPreviewSprite;

		[Header("Events")]
		public UnityEvent OnClick;

		public void OnPointerDown(PointerEventData eventData)
		{
			OnClick?.Invoke();
		}

		public void Initialize(Direction moveDirection, HatsPlayerMoveType moveType)
		{
			// Set the icon
			if(previewIconSpriteRenderer)
			{
				switch (moveType)
				{
					case HatsPlayerMoveType.WALK:
						previewIconSpriteRenderer.sprite = walkPreviewSprite;
						break;
					case HatsPlayerMoveType.FIREBALL:
						previewIconSpriteRenderer.sprite = fireballPreviewSprite;
						break;
					case HatsPlayerMoveType.ARROW:
						previewIconSpriteRenderer.sprite = arrowPreviewSprite;
						break;
					default:
						return; // Do nothing and return
				}

				// Set the preview's direction
				previewIconSpriteRenderer.transform.localRotation = moveDirection.GetRotation() * Quaternion.Euler(0, 0, -90);
			}
		}
	}
}