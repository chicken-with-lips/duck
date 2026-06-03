#if USE_DOUBLE_PRECISION
global using AScalar = System.Double;
global using AVector2 = Silk.NET.Maths.Vector2D<System.Double>;
global using AVector3 = Silk.NET.Maths.Vector3D<System.Double>;
global using AQuaternion = Silk.NET.Maths.Quaternion<System.Double>;
global using AMatrix3X3 = Silk.NET.Maths.Matrix3X3<System.Double>;
global using AMatrix4X4 = Silk.NET.Maths.Matrix4X4<System.Double>;
global using ABox = Silk.NET.Maths.Box3D<System.Double>;
#else
global using DScalar = float;
global using DVector2 = Silk.NET.Maths.Vector2D<float>;
global using DVector3 = Silk.NET.Maths.Vector3D<float>;
global using DQuaternion = Silk.NET.Maths.Quaternion<float>;
global using DMatrix3X3 = Silk.NET.Maths.Matrix3X3<float>;
global using DMatrix4X4 = Silk.NET.Maths.Matrix4X4<float>;
global using DBox = Silk.NET.Maths.Box3D<float>;
#endif