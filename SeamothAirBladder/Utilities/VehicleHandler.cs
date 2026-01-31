using UnityEngine;

namespace SeamothAirBladder.Utilities
{
    public static class VehicleHandler
    {
        public static void ApplyBuoyancy(Rigidbody rigidBody, float buoyancyForce)
        {
            rigidBody?.AddForce(Vector3.up * buoyancyForce, ForceMode.Force);
        }
    }
}
