namespace LEDPiLib.Modules.Helper
{
    class MathHelper
    {
        public static float Constrain(float constrainValue, float constrainMin, float constrainMax)
        {
            return constrainValue < constrainMin
                ? constrainMin
                : (constrainValue > constrainMax ? constrainMax : constrainValue);
        }

        public static float Map(float s, float a1, float a2, float b1, float b2)
        {
            return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
        }

        public static float Lerp(float a, float b, float x)
        {
            return a + x * (b - a);
        }
    }
}
