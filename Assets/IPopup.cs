using UnityEngine;

namespace Com.Hypester.DM3
{
    public interface IPopup
    {
        void Accept();
        void Refuse();
        GameObject GetGameObject();
    }
}