using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HatsContent;
using HatsCore;
using HatsMultiplayer;
using HatsUnity;
using UnityEngine;

public class PlayerController : GameEventHandler
{
	private const float MovementDuration = 0.25f; // How long it takes to move characters to their new positions
	private const float GhostDelay = 1f; // How long to wait before turning into a ghost

	[Header("Personalization")]
    public CharacterRef CharacterRef;
    public HatRef HatRef;

    [Header("Internal References")]
    public GameObject GhostObject;

    [ReadOnly]
    [SerializeField]
    private CharacterBehaviour CharacterBehaviour;

    [Header("Prefab References")]
    public ShieldFXBehaviour ShieldFXPrefab;
    public FireballFXBehaviour FireballFXPrefab;
    public FireballFXBehaviour ArrowFXPrefab;

    [Header("Runtime Values")]
    [ReadOnly]
    [SerializeField]
    private HatsPlayer _player;

    [ReadOnly]
    [SerializeField]
    private ShieldFXBehaviour _shieldInstance;

    [ReadOnly]
    [SerializeField]
    private Vector3 _targetPosition;
    private Vector3 _currentVel;

    // Start is called before the first frame update
    void Start()
    {
        _targetPosition = transform.localPosition;

    }

    // Update is called once per frame
    void Update()
    {
        // Move the player towards the target position
        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, _targetPosition, ref _currentVel, MovementDuration);
    }

    public async void Setup(GameProcessor gameProcessor, HatsPlayer player)
    {
        _player = player;
        GameProcessor = gameProcessor;
        GameProcessor.EventHandlers.Add(this);

        CharacterRef = await player.GetSelectedCharacter();
        HatRef = await player.GetSelectedHat();
        var characterContent = await CharacterRef.Resolve();
        var hatContent = await HatRef.Resolve();
        var gob = await characterContent.Prefab.SafeResolve();
        CharacterBehaviour = Instantiate(gob, transform);
        await CharacterBehaviour.SetHat(hatContent);
    }

    public override IEnumerator HandleTurnOverEvent(TurnOverEvent evt, Action completeCallback)
    {

        // always wait a bit, so as to the give a bit of rhythm.
        yield return new WaitForSecondsRealtime(.3f);

        // disable any shield FX.
        if (_shieldInstance)
        {
            _shieldInstance.End();
            yield return new WaitForSecondsRealtime(.4f);
            Destroy(_shieldInstance?.gameObject);
            _shieldInstance = null;
        }

        yield return null;
        completeCallback();
    }

    public override IEnumerator HandleShieldEvent(PlayerShieldEvent evt, Action completeCallback)
    {
        if (!Equals(evt.Player, _player))
        {
            completeCallback();
            yield break;
        }

        // spawn shield FX...
        var state = GameProcessor.GetCurrentPlayerState(evt.Player.dbid);
        _shieldInstance = GameProcessor.BattleGridBehaviour.SpawnObjectAtCell(ShieldFXPrefab, state.Position);

        yield return new WaitForSecondsRealtime(.1f);


        completeCallback();
    }

    public override IEnumerator HandleMoveEvent(PlayerMoveEvent evt, Action completeCallback)
    {
        if (!Equals(evt.Player, _player))
        {
            completeCallback();
            yield break;
        }

		CharacterBehaviour.Move();
		var localPosition = GameProcessor.BattleGridBehaviour.Grid.CellToLocal(evt.NewPosition);
		CharacterBehaviour.SetDirection(localPosition.x > _targetPosition.x);
        _targetPosition = localPosition;
        yield return null;
        completeCallback();
    }

    public override IEnumerator HandleAttackEvent(PlayerAttackEvent evt, Action completeCallback)
    {
        completeCallback(); // immediately consume the attack event.

        if (!Equals(evt.Player, _player))
        {
            yield break;
        }

        var state = GameProcessor.GetCurrentPlayerState(evt.Player.dbid);

        // 1. spawn the projectile
        var prefab = evt.Type == PlayerAttackEvent.AttackType.ARROW
            ? ArrowFXPrefab
            : FireballFXPrefab;
        var projectile = GameProcessor.BattleGridBehaviour.SpawnObjectAtCell(prefab, state.Position);
        var dir = evt.Direction.GetRotation();
        projectile.transform.localRotation = dir;

        // 2. move it in direction of travel, until it leaves the board, or it reaches its kill or bounce position
        while (true)
        {
            var currentPosition = GameProcessor.BattleGridBehaviour.Grid.WorldToCell(projectile.transform.position);
            if (!GameProcessor.BattleGridBehaviour.Tilemap.HasTile(currentPosition))
            {
                // the projectile has left the grid;
                break;
            }

            if (evt.DestroyAt.HasValue && !evt.BounceAt.HasValue && currentPosition == evt.DestroyAt.Value)
            {
                // the projectile has reached its target, kaboom
                break;
            }

            if (evt.BounceAt.HasValue && evt.BounceDirection.HasValue && currentPosition == evt.BounceAt.Value)
            {
                dir = evt.BounceDirection.Value.GetRotation();
                evt.BounceAt = null;
                projectile.transform.localRotation = dir;
                // TODO: do bounce animation?
            }

            // actually move the projectile along...
            var vel = projectile.transform.right * projectile.Speed * Time.deltaTime;
            projectile.transform.localPosition += vel;

            yield return null;
        }

        projectile.End();

        // 3. if there are registered kills...
        if (evt.KillsPlayer != null)
        {
            var allPlayers = FindObjectsOfType<PlayerController>();
            var targetPlayerController = allPlayers.First(other => Equals(other._player, evt.KillsPlayer));
            foreach (var progress in targetPlayerController.BecomeGhost())
            {
                yield return progress;
            }
            //targetPlayerController.End();
        }

        yield return null;
    }

    public IEnumerable BecomeGhost()
    {
		CharacterBehaviour.GetHit();

        if (_shieldInstance)
        {
            _shieldInstance.End();
            yield return new WaitForSecondsRealtime(.1f);
            Destroy(_shieldInstance.gameObject);
            _shieldInstance = null;
        }

        yield return new WaitForSecondsRealtime(GhostDelay);

        // TODO: Add a dope animation of the player becoming a ghost...
        GhostObject.SetActive(true);
        CharacterBehaviour.gameObject.SetActive(false);
    }


    public void End()
    {
        // TODO: Sizzle blow up effects? Maybe a little ghosty?
        // TODO: don't actually destroy, let the player continue to wonder around
        if (_shieldInstance)
        {
            _shieldInstance.End();
            Destroy(_shieldInstance.gameObject); // TODO: duplicated delete code feels bad.
        }
        Destroy(gameObject);
    }
}
