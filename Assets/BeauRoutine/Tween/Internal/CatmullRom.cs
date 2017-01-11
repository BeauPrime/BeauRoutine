using UnityEngine;

namespace BeauRoutine.Internal
{
    static internal class CatmullRom
    {
        // http://www.iquilezles.org/www/articles/minispline/minispline.htm
        static public Vector3 Evaluate(Vector3 inP0, Vector3 inP1, Vector3 inP2, Vector3 inP3, float inT)
        {
            Vector3 a = 2 * inP1;
            Vector3 b = inP2 - inP0;
            Vector3 c = (2f * inP0) - (5f * inP1) + (4f * inP2) - inP3;
            Vector3 d = -inP0 + (3f * inP1) - (3f * inP2) + inP3;

            return 0.5f * (a + (b * inT) + (c * inT * inT) + (d * inT * inT * inT));
        }
    }
}
