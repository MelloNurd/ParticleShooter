using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using static Unity.Entities.SystemBaseDelegates;
using UnityEngine.UIElements;

namespace NaughtyAttributes
{
    public class SimParticle : MonoBehaviour
    {
        [ShowNativeProperty] public int Type { get; set; } // Determines the type of the particle, as well as the color
        [ShowNativeProperty] public int Id { get; set; } // Can be used to single out specific particles
    }
}