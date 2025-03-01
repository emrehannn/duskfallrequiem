// Copyright © Sascha Graeff/13Pixels.

namespace ThirteenPixels.Placr
{
    using UnityEngine;

    public class PlacrModifierRandomRotationOffset : PlacrModifierBase
    {
        public override string title => "Random Rotational Offset";

        [SerializeField, Range(0, 180)]
        private float maxAngle = 10f;

        public override void ApplyTo(GameObject gameObject)
        {
            var angle = Random.value * maxAngle;
            gameObject.transform.rotation *= Quaternion.Euler(Vector3.up * angle);
        }
    }
}
