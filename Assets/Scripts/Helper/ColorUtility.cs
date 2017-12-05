using UnityEngine;

public class ColorUtility : MonoBehaviour {

    public static string[] skillColors = new string[4] { "red", "green", "blue", "yellow" };

	public static SkillColor GetSkillColorBySyscode(string syscode)
    {
        SkillColor skillColor = SkillColor.Neutral;
        switch (syscode)
        {
            case "red":
                skillColor = SkillColor.Red;
                break;
            case "green":
                skillColor = SkillColor.Green;
                break;
            case "blue":
                skillColor = SkillColor.Blue;
                break;
            case "yellow":
                skillColor = SkillColor.Yellow;
                break;
        }
        return skillColor;
    }
    public static string GetSkillSyscodeBySkillColor(SkillColor color)
    {
        string skillSyscode = "";
        switch (color)
        {
            case SkillColor.Red:
                skillSyscode = "red";
                break;
            case SkillColor.Green:
                skillSyscode = "green";
                break;
            case SkillColor.Blue:
                skillSyscode = "blue";
                break;
            case SkillColor.Yellow:
                skillSyscode = "yellow";
                break;
        }
        return skillSyscode;
    }

    public static SkillColor IntToSkillColor(int skillInt)
    {
        SkillColor skillColor = SkillColor.Neutral;
        switch (skillInt)
        {
            case 0:
                skillColor = SkillColor.Yellow;
                break;
            case 1:
                skillColor = SkillColor.Blue;
                break;
            case 2:
                skillColor = SkillColor.Green;
                break;
            case 3:
                skillColor = SkillColor.Red;
                break;
        }
        return skillColor;
    }

    public static Color GetUnityColorBySkillColor(SkillColor skillColor)
    {
        Color color = new Color();
        switch (skillColor)
        {
            case SkillColor.Red:
                color = Color.red;
                break;
            case SkillColor.Green:
                color = Color.green;
                break;
            case SkillColor.Blue:
                color = Color.blue;
                break;
            case SkillColor.Yellow:
                color = Color.yellow;
                break;
        }
        return color;
    }
}
