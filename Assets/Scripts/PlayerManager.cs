using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Hypester.DM3
{
    public class PlayerManager : MonoBehaviour
    {
        public static PlayerManager instance;

        [SerializeField] GameObject playerPrefab;

        Dictionary<int, Player> players = new Dictionary<int, Player>();

        void Awake()
        {
            if (!instance) { instance = this; }
            else { Destroy(this); }
        }

        private void OnEnable()
        {
            NetworkEvent.OnPhotonPlayerDisconnected += RemovePlayer;
            NetworkEvent.OnPhotonDisconnected += OnPhotonDisconnected;
        }
        private void OnDisable()
        {
            NetworkEvent.OnPhotonPlayerDisconnected -= RemovePlayer;
            NetworkEvent.OnPhotonDisconnected -= OnPhotonDisconnected;
        }

        private void OnPhotonDisconnected()
        {
            ClearPlayerList();
            ResetPlayerCustomProperties(PhotonNetwork.player);
        }

        public void CreatePlayer()
        {
            Player player = (PhotonNetwork.Instantiate(playerPrefab.name, Vector3.zero, Quaternion.identity, 0)).GetComponent<Player>();

            player.SetPlayerId(PhotonNetwork.player.ID);
            player.joinNumber = PhotonNetwork.room.PlayerCount;
            player.localID = PhotonNetwork.isMasterClient || player.joinNumber == 3 ? 0 : 1;
        }

        public Player GetPlayerById(int playerId) { Player player = null; if (players.ContainsKey(playerId)) { player = players[playerId]; } return player; }
        public int GetPlayerIdByPlayer(Player player)
        {
            int id = -404;
            foreach (KeyValuePair<int, Player> kvp in players)
            {
                if (kvp.Value.Equals(player)) { id = kvp.Value.GetPlayerId(); break; }
            }
            return id;
        }
        public Dictionary<int, Player> GetAllPlayers() { return players; }
        public void AddPlayer(int playerId, Player player) { if (!players.ContainsKey(playerId)) { players.Add(playerId, player); } }
        public void RemovePlayer(int playerId) { if (players.ContainsKey(playerId)) { players.Remove(playerId); } }
        public void RemovePlayer(PhotonPlayer photonPlayer) { if (players.ContainsKey(photonPlayer.ID)) { players.Remove(photonPlayer.ID); } }
        public void ClearPlayerList() { players.Clear(); }

        public void ResetPlayerCustomProperties(PhotonPlayer player)
        {
            ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
            if (player.CustomProperties.ContainsKey(PlayerProperty.UserId)) { props.Add(PlayerProperty.UserId, null); }
            if (player.CustomProperties.ContainsKey(PlayerProperty.ProfileImageUrl)) { props.Add(PlayerProperty.ProfileImageUrl, null); }
            if (player.CustomProperties.ContainsKey(PlayerProperty.XpLevel)) { props.Add(PlayerProperty.XpLevel, null); }
            if (player.CustomProperties.ContainsKey(PlayerProperty.State)) { props.Add(PlayerProperty.State, null); }
            if (player.CustomProperties.ContainsKey(PlayerProperty.BlueSkillLevel)) { props.Add(PlayerProperty.BlueSkillLevel, null); }
            if (player.CustomProperties.ContainsKey(PlayerProperty.GreenSkillLevel)) { props.Add(PlayerProperty.GreenSkillLevel, null); }
            if (player.CustomProperties.ContainsKey(PlayerProperty.RedSkillLevel)) { props.Add(PlayerProperty.RedSkillLevel, null); }
            if (player.CustomProperties.ContainsKey(PlayerProperty.YellowSkillLevel)) { props.Add(PlayerProperty.YellowSkillLevel, null); }
            if (props.Count > 0) { PhotonNetwork.player.SetCustomProperties(props); }
        }
    }
}