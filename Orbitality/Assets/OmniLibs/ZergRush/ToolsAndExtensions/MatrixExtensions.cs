using UnityEngine;

namespace ZergRush
{
    public static class MatrixExtensions
    {
        public static Matrix4x4 ClearScale(this Matrix4x4 matrix)
        {
            var scale = matrix.ExtractScale();
            var res = matrix * Matrix4x4.Scale(new Vector3(1 / scale.x, 1 / scale.y, 1 / scale.z));
            var resScale = res.ExtractScale();
            return res;
            //return Matrix4x4.TRS(matrix.ExtractPosition(), matrix.ExtractRotation(), Vector3.one);
        }

        public static Quaternion ExtractRotation(this Matrix4x4 matrix) => ((ServerEngine.Matrix4x4)matrix).ExtractRotation();
            public static Vector3 ExtractPosition(this Matrix4x4 matrix) => ((ServerEngine.Matrix4x4)matrix).ExtractPosition();

        public static Vector3 up(this Matrix4x4 matrix)
        {
            Vector3 position;
            position.x = matrix.m01;
            position.y = matrix.m11;
            position.z = matrix.m21;
            return position;
        }

        public static Vector3 forward(this Matrix4x4 matrix)
        {
            Vector3 position;
            position.x = matrix.m02;
            position.y = matrix.m12;
            position.z = matrix.m22;
            return position;
        }

        public static Vector3 right(this Matrix4x4 matrix)
        {
            Vector3 position;
            position.x = matrix.m00;
            position.y = matrix.m10;
            position.z = matrix.m20;
            return position;
        }

        public static Vector3 ExtractScale(this Matrix4x4 matrix)
        {
            Vector3 scale;
            scale.x = new Vector4(matrix.m00, matrix.m10, matrix.m20, matrix.m30).magnitude;
            scale.y = new Vector4(matrix.m01, matrix.m11, matrix.m21, matrix.m31).magnitude;
            scale.z = new Vector4(matrix.m02, matrix.m12, matrix.m22, matrix.m32).magnitude;
            return scale;
        }
    }
}