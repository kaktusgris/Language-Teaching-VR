// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PhotonVoiceLagSimulationGui.cs" company="Exit Games GmbH">
//   Part of: Photon Voice Utilities for Unity - Copyright (C) 2018 Exit Games GmbH
// </copyright>
// <summary>
// This MonoBehaviour is a basic GUI for the Photon Voice client's network-simulation feature.
// It can modify lag (fixed delay), jitter (random lag), packet loss and audio frames percentage loss.
// </summary>
// <author>developer@exitgames.com</author>
// --------------------------------------------------------------------------------------------------------------------

using UnityEngine;
using ExitGames.Client.Photon;

namespace Photon.Voice.Unity.UtilityScripts
{
    [RequireComponent(typeof(VoiceConnection))]
    public class PhotonVoiceLagSimulationGui : MonoBehaviour
    {
        private VoiceConnection voiceConnection;

        /// <summary>Positioning rect for window.</summary>
        private Rect windowRect = new Rect(0, 100, 200, 100);

        /// <summary>Unity GUI Window ID (must be unique or will cause issues).</summary>
        private int windowId = 201;

        /// <summary>Shows or hides GUI (does not affect settings).</summary>
        private bool visible = true;

        /// <summary>The peer currently in use (to set the network simulation).</summary>
        private PhotonPeer peer;

        public void OnEnable()
        {
            VoiceConnection[] voiceConnections = this.GetComponents<VoiceConnection>();
            if (voiceConnections == null || voiceConnections.Length == 0)
            {
                Debug.LogError("No VoiceConnection component found, PhotonVoiceStatsGui disabled", this);
                this.enabled = false;
            }
            if (voiceConnections.Length > 1)
            {
                Debug.LogWarningFormat(this, "Multiple VoiceConnection components found, using first occurrence attached to GameObject {0}", voiceConnections[0].name);
            }
            this.voiceConnection = voiceConnections[0];
            this.peer = this.voiceConnection.Client.LoadBalancingPeer;
        }

        private void OnGUI()
        {
            if (!this.visible)
            {
                return;
            }

            if (this.peer == null)
            {
                this.windowRect = GUILayout.Window(this.windowId, this.windowRect, this.NetSimHasNoPeerWindow,
                    "Voice Network Simulation");
            }
            else
            {
                this.windowRect = GUILayout.Window(this.windowId, this.windowRect, this.NetSimWindow, "Voice Network Simulation");
            }
        }

        private void NetSimHasNoPeerWindow(int windowId)
        {
            GUILayout.Label("No voice peer to communicate with. ");
        }

        private void NetSimWindow(int windowId)
        {
            GUILayout.Label(string.Format("Rtt:{0,4} +/-{1,3}", this.peer.RoundTripTime,
                this.peer.RoundTripTimeVariance));

            bool simEnabled = this.peer.IsSimulationEnabled;
            bool newSimEnabled = GUILayout.Toggle(simEnabled, "Simulate");
            if (newSimEnabled != simEnabled)
            {
                this.peer.IsSimulationEnabled = newSimEnabled;
            }

            float inOutLag = this.peer.NetworkSimulationSettings.IncomingLag;
            GUILayout.Label("Lag " + inOutLag);
            inOutLag = GUILayout.HorizontalSlider(inOutLag, 0, 500);

            this.peer.NetworkSimulationSettings.IncomingLag = (int)inOutLag;
            this.peer.NetworkSimulationSettings.OutgoingLag = (int)inOutLag;

            float inOutJitter = this.peer.NetworkSimulationSettings.IncomingJitter;
            GUILayout.Label("Jit " + inOutJitter);
            inOutJitter = GUILayout.HorizontalSlider(inOutJitter, 0, 100);

            this.peer.NetworkSimulationSettings.IncomingJitter = (int)inOutJitter;
            this.peer.NetworkSimulationSettings.OutgoingJitter = (int)inOutJitter;

            float loss = this.peer.NetworkSimulationSettings.IncomingLossPercentage;
            GUILayout.Label("Loss " + loss);
            loss = GUILayout.HorizontalSlider(loss, 0, 10);

            this.peer.NetworkSimulationSettings.IncomingLossPercentage = (int)loss;
            this.peer.NetworkSimulationSettings.OutgoingLossPercentage = (int)loss;

            float debugLostPercent = this.voiceConnection.VoiceClient.DebugLostPercent;
            GUILayout.Label(string.Format("Lost Audio Frames {0}%", (int)debugLostPercent));
            debugLostPercent = GUILayout.HorizontalSlider(debugLostPercent, 0, 100);
            this.voiceConnection.VoiceClient.DebugLostPercent = (int)debugLostPercent;

            // if anything was clicked, the height of this window is likely changed. reduce it to be layouted again next frame
            if (GUI.changed)
            {
                this.windowRect.height = 100;
            }

            GUI.DragWindow();
        }
    }
}