public class StatUpgrade : Upgrade
{
    public PlayerStat affectedStat;
    public float flatBonus;
    public float percentBonus;

    public override float Modify(PlayerStat targetStat, float value)
    {
        if (targetStat != affectedStat)
            return value;

        value += flatBonus * level;
        value *= 1 + percentBonus * level;

        return value;
    }

}