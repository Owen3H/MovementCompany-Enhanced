using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MovementCompany.Component {
    internal class MovementScript : MonoBehaviour {
        public PlayerControllerB myPlayer;
        bool inAir;
        public static Vector3 wantedVelToAdd;
        public float jumpTime;

        private Vector3 previousForward;
        
        public void Update() {
            if (myPlayer.playerBodyAnimator.GetBool("Jumping") && jumpTime < 0.1f) {
                myPlayer.fallValue = myPlayer.jumpForce;
                jumpTime += Time.deltaTime * 10f;
            }

            myPlayer.sprintMeter = 100;

            if (!myPlayer.thisController.isGrounded && !myPlayer.isClimbingLadder)
            {
                if (!inAir) {
                    inAir = true;

                    Vector3 vel = myPlayer.thisController.velocity;
                    vel.y = 0;
                    
                    wantedVelToAdd += 0.006f * vel;
                }

                Vector3 currentForward = myPlayer.gameObject.transform.forward;
                
                wantedVelToAdd.y = 0;
                myPlayer.thisController.Move(currentForward * wantedVelToAdd.magnitude);

                Vector3 forwardChange = currentForward - previousForward;
                float rotationThreshold = 0.01f;

                if (forwardChange.magnitude > rotationThreshold)
                    wantedVelToAdd += new Vector3(0.0005f, 0.0005f, 0.0005f);

                previousForward = currentForward;
            }
            else
            {
                wantedVelToAdd = Vector3.Lerp(wantedVelToAdd, Vector3.zero, Time.deltaTime * 4.2f);
                inAir = false;
                jumpTime = 0;
            }
        }
    }
}
