using UnityEngine;
using System.Collections;

namespace Com.Hypester.DM3
{
    public class TrapPower : MonoBehaviour
    {
        private GameHandler _game;

        public Player ownerPlayer;

        private bool _isPickedUp = false;
        public bool isPickedUp { get { return _isPickedUp; } }

        private BaseTile _overBasetile;
        public BaseTile overBasetile { set { SetBaseTileHover(value); } get { return _overBasetile; } }


        private void Start()
        {
            _game = GameObject.FindWithTag("GameController").GetComponent<GameHandler>();

            ownerPlayer = _game.MyPlayer;
        }

        private void SetBaseTileHover(BaseTile baseTile) {
            if (_overBasetile != null)
                _overBasetile.TrapNotHovered();
            _overBasetile = baseTile;
            baseTile.TrapHovered();
        }


        public void PickUp ()
        {
            _isPickedUp = true;
            Debug.Log("Trap picked up.");
        }

        public void Place()
        {
            if (_isPickedUp && _overBasetile != null) {
                _overBasetile.TrapNotHovered();
                _game.photonView.RPC("RPC_CreateTrapBooster", PhotonTargets.All, _overBasetile.position, ownerPlayer.localID);

                GameObject placeParticle = Instantiate(Resources.Load("ParticleEffects/TrapPlaced")) as GameObject;
                placeParticle.transform.position = _overBasetile.transform.position;
                Destroy(gameObject);

               _game.ResetTimer();
            } else
            {
                _isPickedUp = false;
            }
        }
    }
}