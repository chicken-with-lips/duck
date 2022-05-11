using Duck.Serialization;
using Silk.NET.Maths;

namespace Duck.Scene.Components
{
    [AutoSerializable]
    public partial struct TransformComponent
    {
        #region Properties

        public Vector3D<float> Translation {
            get => _translation;
            set {
                _translation = value;
                _isTranslationDirty = true;

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

        public bool IsTranslationDirty => _isTranslationDirty;
        public bool IsRotationDirty => _isRotationDirty;
        public bool IsScaleDirty => _isScaleDirty;

        #endregion

        #region Members

        private Vector3D<float> _translation = default;
        private Quaternion<float> _rotation = default;
        private Vector3D<float> _scale = default;

        private Vector3D<float> _up = default;
        private Vector3D<float> _forward = default;
        private Vector3D<float> _right = default;

        private bool _isTranslationDirty = default;
        private bool _isRotationDirty = default;
        private bool _isScaleDirty = default;

        #endregion

        public void ClearDirtyFlags()
        {
            _isRotationDirty = false;
            _isTranslationDirty = false;
            _isScaleDirty = false;
        }

        private void UpdateVectors()
        {
            _forward = Vector3D.Normalize(Vector3D.Transform(Vector3D<float>.UnitZ, _rotation));
            _right = Vector3D.Normalize(Vector3D.Cross(_forward, Vector3D<float>.UnitY));
            _up = Vector3D.Cross(_forward, _right);
        }
    }
}
