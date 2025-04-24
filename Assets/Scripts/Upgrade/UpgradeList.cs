using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public enum UpgradeType
{
    //Player Stats
    Health,
    Energy,
    Boost,
    Speed,

    //Abilties
    Move_Backwards,
    Dash,

    //Weapon Stuff
    Weapon
}

public class UpgradeList : MonoBehaviour
{
    public static List<int> HealthValues = new List<int>(){ 100, 250, 400, 550, 700, 850, 1000 };
    public static List<int> EnergyValues = new List<int>() { 100, 250, 400, 550, 700, 850, 1000 };
    public static List<int> BoostValues = new List<int>() { 100, 250, 400, 550, 700, 850, 1000 };
    public static List<int> SpeedValues = new List<int>() { 3, 5, 7, 10 };

    public static int CanMoveBackwards = 0;
    public static List<int> DashValues = new List<int>() { 0, 10, 7, 5, 3 };

}
