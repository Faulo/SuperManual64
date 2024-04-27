namespace SuperManual64.Player {
    static class MathUtil {
        public static int ApproachInt(int current, int target, int inc, int dec) {
            if (current < target) {
                current += inc;
                if (current > target) {
                    current = target;
                }
            } else {
                current -= dec;
                if (current < target) {
                    current = target;
                }
            }

            return current;
        }

        public static float ApproachFloat(float current, float target, float inc, float dec) {
            if (current < target) {
                current += inc;
                if (current > target) {
                    current = target;
                }
            } else {
                current -= dec;
                if (current < target) {
                    current = target;
                }
            }

            return current;
        }
    }
}
