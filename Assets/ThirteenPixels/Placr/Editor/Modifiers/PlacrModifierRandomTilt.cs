// Copyright © Sascha Graeff/13Pixels.

namespace ThirteenPixels.Placr
{
    using UnityEngine;

    public class PlacrModifierRandomTilt : PlacrModifierBase
    {
        public override string title => "Random Tilt";

        [SerializeField, Range(0, 180)]
        private float minAngle = 0f;
        [SerializeField, Range(0, 180)]
        private float maxAngle = 10f;

        public override void ApplyTo(GameObject gameObject)
        {
            var tilt = Random.Range(minAngle, maxAngle);
            var axis2d = Random.insideUnitCircle;
            var axis = axis2d == Vector2.zero ? Vector3.forward : new Vector3(axis2d.x, 0, axis2d.y);
            gameObject.transform.rotation *= Quaternion.AngleAxis(tilt, axis);
        }
    }
}
