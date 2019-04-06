using UnityEngine;

namespace Mirror
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Network/NetworkTransform")]
    public class NetworkTransform : NetworkTransformBase
    {
        public float warpDistance = 5f;
        public NetworkTransform() : base()
        {
            base._warpDistance = warpDistance;
        }
        protected override Transform targetComponent { get { return transform; } }
    }
}