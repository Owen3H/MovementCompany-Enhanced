using GameNetcodeStuff;
using UnityEngine;

using Plugin = MovementCompany.Core.MovementCompany;

namespace MovementCompany.Component {
    internal class MovementAdder : MonoBehaviour {
        public void Update() {
            PlayerControllerB[] players = FindObjectsOfType<PlayerControllerB>();

            int playersLen = players.Length;
            for (int i = 0; i < playersLen; i++) {
                PlayerControllerB player = players[i];
                if (player == null) return;
                if (player.GetComponentInChildren<MovementScript>() != null) return;

                if (player.IsOwner && player.isPlayerControlled) {
                    player.gameObject.AddComponent<MovementScript>().myPlayer = player;
                    Plugin.Logger.LogMessage("Gave player the movement script");
                }
            }
        }
    }
}
