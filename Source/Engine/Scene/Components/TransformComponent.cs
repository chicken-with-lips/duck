using Silk.NET.Maths;

namespace Duck.Scene.Components
{
    public struct TransformComponent
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

        private Vector3D<float> _translation;
        private Quaternion<float> _rotation;
        private Vector3D<float> _scale;

        private Vector3D<float> _up;
        private Vector3D<float> _forward;
        private Vector3D<float> _right;

        private bool _isTranslationDirty;
        private bool _isRotationDirty;
        private bool _isScaleDirty;

        #endregion

        public void ClearDirtyFlags()
        {
            _isRotationDirty = false;
            _isTranslationDirty = false;
            _isScaleDirty = false;
        }

        private void UpdateVectors()
        {
            _forward = Vector3D.Normalize(Vector3D.Transform(-Vector3D<float>.UnitZ, _rotation));
            _right = Vector3D.Normalize(Vector3D.Cross(_forward, Vector3D<float>.UnitY));
            _up = Vector3D.Cross(_forward, _right);
        }
    }
}
