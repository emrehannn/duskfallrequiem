// Copyright © Sascha Graeff/13Pixels.

namespace ThirteenPixels.Placr
{
    using UnityEngine;

    public class PlacrModifierRandomScale : PlacrModifierBase
    {
        public override string title => "Random Scale";

        [SerializeField]
        private Vector3 min = new Vector3(0.9f, 0.9f, 0.9f);
        [SerializeField]
        private Vector3 max = new Vector3(1.1f, 1.1f, 1.1f);
        [SerializeField]
        private bool sameHorizontalValue = true;

        public override void ApplyTo(GameObject gameObject)
        {
            var x = Random.Range(min.x, max.x);
            var y = Random.Range(min.y, max.y);
            var z = sameHorizontalValue ? x : Random.Range(min.z, max.z);
            var scale = new Vector3(x, y, z);
            gameObject.transform.localScale = scale;
        }
    }
}
