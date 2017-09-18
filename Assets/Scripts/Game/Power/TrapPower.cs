using UnityEngine;
using System.Collections;

namespace Com.Hypester.DM3
{
    public class TrapPower : MonoBehaviour
    {

        public Player ownerPlayer;

        private bool _isPickedUp = false;
        public bool isPickedUp { get { return _isPickedUp; } }

        private BaseTile _overBasetile;
        public BaseTile overBasetile { set { SetBaseTileHover(value); } get { return _overBasetile; } }


        private void Start()
        {
            ownerPlayer = PhotonConnect.Instance.GameController.MyPlayer;
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
                PhotonConnect.Instance.GameController.photonView.RPC("RPC_CreateTrapBooster", PhotonTargets.All, _overBasetile.position, ownerPlayer.localID);

                GameObject placeParticle = Instantiate(Resources.Load("ParticleEffects/TrapPlaced")) as GameObject;
                placeParticle.transform.position = _overBasetile.transform.position;
                Destroy(gameObject);

                PhotonConnect.Instance.GameController.ResetTimer();
            } else
            {
                _isPickedUp = false;
            }
        }
    }
}