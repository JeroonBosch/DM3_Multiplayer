using UnityEngine;
using System.Collections;

namespace Com.Hypester.DM3
{
    public class TrapPower : MonoBehaviour
    {
        public int gameID;
        public Player ownerPlayer;

        private bool _isPickedUp = false;
        public bool isPickedUp { get { return _isPickedUp; } }

        private TileView _overBasetile;
        public TileView overBasetile { set { SetTileViewHover(value); } get { return _overBasetile; } }


        private void Start()
        {
            ownerPlayer = PhotonController.Instance.GameController.MyPlayer;
        }

        private void SetTileViewHover(TileView baseTile) {
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
                PhotonController.Instance.GameController.photonView.RPC("RPC_CreateTrapBooster", PhotonTargets.All, _overBasetile.position, ownerPlayer.localID);

                GameObject placeParticle = Instantiate(Resources.Load("ParticleEffects/TrapPlaced")) as GameObject;
                placeParticle.transform.position = _overBasetile.transform.position;
                Destroy(gameObject);

                PhotonController.Instance.GameController.ResetTimer();
            } else
            {
                _isPickedUp = false;
            }
        }
    }
}