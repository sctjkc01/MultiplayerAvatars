using IPA.Utilities;
using MultiplayerAvatars.Avatars;
using SiraUtil.Extras;
using SiraUtil.Objects.Multiplayer;
using UnityEngine;
using Zenject;

namespace MultiplayerAvatars.Installers
{
    internal class MpavGameInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.RegisterRedecorator(new ConnectedPlayerRegistration(DecorateConnectedPlayerFacade));
            Container.RegisterRedecorator(new ConnectedPlayerDuelRegistration(DecorateConnectedDuelPlayerFacade));
        }

        private MultiplayerConnectedPlayerFacade DecorateConnectedDuelPlayerFacade(MultiplayerConnectedPlayerFacade original)
        {
            var cac = original.GetComponentInChildren<MultiplayerAvatarPoseController>().gameObject.AddComponent<CustomAvatarController>();
            cac.IsDuel = true;
            GameObject.Destroy(original.GetComponentInChildren<AvatarPoseController>().gameObject.GetComponent<Animator>());
            return original;
        }

        private MultiplayerConnectedPlayerFacade DecorateConnectedPlayerFacade(MultiplayerConnectedPlayerFacade original)
        {
            var cacgo = original.GetComponentInChildren<MultiplayerAvatarPoseController>().gameObject;

            var cac = cacgo.AddComponent<CustomAvatarController>();
            cac.BigAvatarAnimator = original.GetField<MultiplayerBigAvatarAnimator, MultiplayerConnectedPlayerFacade>("_bigAvatarAnimator");

            GameObject.Destroy(original.GetComponentInChildren<AvatarPoseController>().gameObject.GetComponent<Animator>());
            return original;
        }
    }
}
