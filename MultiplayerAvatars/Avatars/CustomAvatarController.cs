using CustomAvatar.Avatar;
using IPA.Utilities;
using MultiplayerAvatars.Networking;
using MultiplayerAvatars.Providers;
using SiraUtil.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace MultiplayerAvatars.Avatars
{
    internal class CustomAvatarController : MonoBehaviour
    {

        private CustomAvatarPacket _avatarPacket = new();
        private AvatarPrefab? _loadedAvatar;
        private SpawnedAvatar? _spawnedAvatar;

        private AvatarSpawner _avatarSpawner = null!;
        private IConnectedPlayer _connectedPlayer = null!;
        private CustomAvatarManager _customAvatarManager = null!;
        private AvatarProviderService _avatarProvider = null!;
        private AvatarPoseController _poseController = null!;
        private MultiplayerAvatarInput _avatarInput = null!;
        private SiraLog _logger = null!;

        public MultiplayerBigAvatarAnimator? BigAvatarAnimator { get; set; }
        private SpawnedAvatar? _spawnedBigAvatar = null;
        private MultiplayerAvatarInput? _bigAvatarInput = null;

        public bool IsDuel { get; set; } = false;

        [Inject]
        public void Construct(
            AvatarSpawner avatarSpawner,
            IConnectedPlayer connectedPlayer,
            CustomAvatarManager customAvatarManager,
            AvatarProviderService avatarProvider,
            AvatarPoseController poseController,
            SiraLog logger)
        {
            _avatarSpawner = avatarSpawner;
            _avatarProvider = avatarProvider;
            _connectedPlayer = connectedPlayer;
            _customAvatarManager = customAvatarManager;
            _poseController = poseController;
            _logger = logger;

            _avatarInput = new MultiplayerAvatarInput(poseController);
            _bigAvatarInput = new MultiplayerAvatarInput(poseController);

            // Utils.ReadEntireScene(_logger, _poseController.transform);
        }

        public void OnEnable()
        {
            _customAvatarManager.avatarReceived += HandleAvatarReceived;
            _avatarPacket = _customAvatarManager.GetPlayerAvatarPacket(_connectedPlayer.userId);
            HandleAvatarReceived(_connectedPlayer, _avatarPacket);
        }

        public void OnDisable()
        {
            _customAvatarManager.avatarReceived -= HandleAvatarReceived;
        }

        private void HandleAvatarReceived(IConnectedPlayer player, CustomAvatarPacket packet)
        {
            if (player.userId != _connectedPlayer.userId)
                return;
            if (packet.Hash == "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF")
                return;

            _avatarPacket = packet;
            _ = LoadAvatar(packet.Hash); // We need this to run on the main thread
        }

        private async Task LoadAvatar(string hash)
        {
            var avatarPrefab = await _avatarProvider.GetAvatarByHash(hash, CancellationToken.None);
            if (avatarPrefab == null)
            {
                _logger.Warn($"Tried to load avatar and failed: {hash}");
                return;
            }

            HMMainThreadDispatcher.instance.Enqueue(() => CreateAvatar(avatarPrefab));
        }

        private void CreateAvatar(AvatarPrefab avatar)
        {
            _loadedAvatar = avatar;
            if (_spawnedAvatar != null)
                Destroy(_spawnedAvatar);

            _spawnedAvatar = _avatarSpawner.SpawnAvatar(avatar, _avatarInput, _poseController.transform);
            _avatarInput.SetEnabled(true);
            var avatarIk = _spawnedAvatar.GetComponent<AvatarIK>();
            if (avatarIk != null)
                avatarIk.isLocomotionEnabled = true;
            _spawnedAvatar.scale = _avatarPacket.Scale;

            if (IsDuel)
            {
                _logger.Debug("Skipping Big Avatar, as this is a Duel.");
                return;
            }

            if (_bigAvatarInput == null)
            {
                _logger.Error("No Big Avatar Input?");
                return;
            }

            if (BigAvatarAnimator == null)
            {
                if (_connectedPlayer.isMe == false)
                    _logger.Error("Did not have a Big Avatar Animator for remote player!");
                return;
            }

            if (_spawnedBigAvatar != null)
                Destroy(_spawnedBigAvatar);

            var TargetTransform = BigAvatarAnimator.GetField<Transform, MultiplayerBigAvatarAnimator>("_avatarTransform");

            var p = Utils.GetPath(TargetTransform);
            _logger.Debug($"Attempting to spawn Big Avatar '{avatar.descriptor.name}' into '{p}'");

            _spawnedBigAvatar = _avatarSpawner.SpawnAvatar(avatar, _bigAvatarInput, TargetTransform);
            _bigAvatarInput.SetEnabled(true);
            avatarIk = _spawnedBigAvatar.GetComponent<AvatarIK>();
            if (avatarIk != null)
                avatarIk.isLocomotionEnabled = true;
            _spawnedBigAvatar.scale = _avatarPacket.Scale * 5.0f;
        }
    }
}
