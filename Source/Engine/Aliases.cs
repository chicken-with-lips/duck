#if USE_DOUBLE_PRECISION
global using AScalar = System.Double;
global using AVector2 = Silk.NET.Maths.Vector2D<System.Double>;
global using AVector3 = Silk.NET.Maths.Vector3D<System.Double>;
global using AQuaternion = Silk.NET.Maths.Quaternion<System.Double>;
global using AMatrix3X3 = Silk.NET.Maths.Matrix3X3<System.Double>;
global using AMatrix4X4 = Silk.NET.Maths.Matrix4X4<System.Double>;
global using ABox = Silk.NET.Maths.Box3D<System.Double>;
#else
global using AScalar = System.Single;
global using AVector2 = Silk.NET.Maths.Vector2D<System.Single>;
global using AVector3 = Silk.NET.Maths.Vector3D<System.Single>;
global using AQuaternion = Silk.NET.Maths.Quaternion<System.Single>;
global using AMatrix3X3 = Silk.NET.Maths.Matrix3X3<System.Single>;
global using AMatrix4X4 = Silk.NET.Maths.Matrix4X4<System.Single>;
global using ABox = Silk.NET.Maths.Box3D<System.Single>;
#endif
