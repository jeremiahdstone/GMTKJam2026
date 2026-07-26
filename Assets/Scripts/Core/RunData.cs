using UnityEngine;

public class RunData
{
    public int day;
    public int enemiesKilled;
    public int bloodCount;
    public int maxBloodCount;
    public int trapsBought;
    public int upgradesBought;

    public PlayerStats playerStats;

    public RunData()
    {
        day = 0;
        bloodCount = 100;
        maxBloodCount = 100;
        enemiesKilled = 0;
        upgradesBought = 0;
        trapsBought = 0;

        playerStats = null;
    }
}
