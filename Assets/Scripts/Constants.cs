namespace Com.Hypester.DM3
{
    public static class Constants
    {
        //Constants variables. Some things would eventually be tied to the skill tree and thus no longer constants.


        //Stats
        public const int MaxSkillLevel = 6;
        
        //Game variables
        public const int PlayerStartHP = 1000;
        public const int BluePowerReq = 12;
        public const int GreenPowerReq = 9;
        public const int RedPowerReq = 9;
        public const int YellowPowerReq = 12;

        public const float BoosterCollateralDamage = 5f;
        public const int HealPower = 75;
        public const int FireballPower = 90;

        //Grid settings
        public const int gridXsize = 10;
        public const int gridYsize = 8;
        public const float tileWidth = 98f;
        public const float tileHeight = 112f;
        public const float DistanceBetweenTiles = 1.12f;

        public const int AmountOfColors = 4;
        public const int BoosterOneThreshhold = 5;
        public const int BoosterTwoThreshhold = 8;
        public const int BoosterThreeThreshhold = 10;

        public const int BoosterLevel2Vertical = 3;

        //Timing
        public const float TurnTime = 14f;
        public const float TimeBetweenTurns = 2f;
        public const float DelayAfterTileDestruction = 0.05f;
        public const float TileDroppingSpeed = 1400f;
        public const float HealthDroppingSpeed = 100f;
        public const float MinimumTextTime = 3f; //3 seconds
        public const float WigglePause = 4f;
        public const float WiggleDuration = 1f;
        public const float SearchMatchTimeout = 30f;

        //Timeouts
        public const float loginTimeout = 10f;

        public static int GetSkillActivationRequirement(SkillColor skillColor)
        {
            int requirement = 11;
            switch (skillColor)
            {
                case SkillColor.Red:
                    requirement = RedPowerReq;
                    break;
                case SkillColor.Green:
                    requirement = GreenPowerReq;
                    break;
                case SkillColor.Blue:
                    requirement = BluePowerReq;
                    break;
                case SkillColor.Yellow:
                    requirement = YellowPowerReq;
                    break;
            }
            return requirement;
        }
    }
}