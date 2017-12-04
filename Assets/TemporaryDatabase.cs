using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Hypester.DM3
{
    public class TemporaryDatabase : MonoBehaviour
    {
        public List<PlayerService.Stage> stages = new List<PlayerService.Stage>();
        public List<PlayerService.Skill> skills = new List<PlayerService.Skill>();
    }
}