using UnityEngine;

public class DayCycle : MonoBehaviour
{
    // 24-hour day-night cycle squeezed into 60 minutes
    // This script controls the position of the "Sun" and the "Moon"
    // Basically there is a Sun and Moon placed in the opposite side within a radius
    // The Sun rises (becomes active) from the east at Time 06:00
    // The Sun is directly in the middle of the sky at Time 12:00
    // The Sun sets (becomes inactive) frpm the west at Time 18:00
    // The Moon follows in the same fashion
}
