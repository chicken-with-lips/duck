using Duck.Serialization;
using Silk.NET.Maths;

namespace Duck.Scene.Components;

[AutoSerializable]
public partial struct TransformComponent
{
    #region Properties

    public Vector3D<float> Position {
        get => _position;
        set {
            _position = value;
            _isPositionDirty = true;

            UpdateVectors();
        }
    }

    public Vector3D<float> Scale {
        get => _scale;
        set {
            _scale = value;
            _isScaleDirty = true;
        }
    }

    public Quaternion<float> Rotation {
        get => _rotation;
        set {
            _rotation = value;
            _isRotationDirty = true;

            UpdateVectors();
        }
    }

    public Vector3D<float> Up => _up;
    public Vector3D<float> Forward => _forward;
    public Vector3D<float> Right => _right;

    public bool IsPositionDirty => _isPositionDirty;
    public bool IsRotationDirty => _isRotationDirty;
    public bool IsScaleDirty => _isScaleDirty;

    #endregion

    #region Members

    private Vector3D<float> _position = default;
    private Quaternion<float> _rotation = default;
    private Vector3D<float> _scale = Vector3D<float>.One;

    private Vector3D<float> _up = default;
    private Vector3D<float> _forward = default;
    private Vector3D<float> _right = default;

    private bool _isPositionDirty = default;
    private bool _isRotationDirty = default;
    private bool _isScaleDirty = default;

    #endregion

    #region Methods

    public TransformComponent()
    {
    }

    public void ClearDirtyFlags()
    {
        _isRotationDirty = false;
        _isPositionDirty = false;
        _isScaleDirty = false;
    }

    private void UpdateVectors()
    {
        _forward = Vector3D.Normalize(Vector3D.Transform(Vector3D<float>.UnitZ, _rotation));
        _right = Vector3D.Normalize(Vector3D.Cross(Vector3D<float>.UnitY, _forward));
        _up = Vector3D.Cross(_forward, _right);
    }

    #endregion
}
