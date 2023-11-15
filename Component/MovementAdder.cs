using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MovementCompany.Component {
    internal class MovementAdder : MonoBehaviour {
        public void Update() {
            PlayerControllerB[] players = GameObject.FindObjectsOfType<PlayerControllerB>();

            foreach (PlayerControllerB player in players) {
                if (player == null) return
                
                if (player.gameObject.GetComponentInChildren<MovementScript>() == null) {
                    if (player.IsOwner && player.isPlayerControlled) {
                        player.gameObject.AddComponent<MovementScript>().myPlayer = player;
                        Plugin.Log.LogMessage("Gave player the movement script");
                    }
                }
            }
        }
    }
}
