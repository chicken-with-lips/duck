using System.Numerics;

namespace Duck.SceneManagement.Components
{
    public struct TransformComponent
    {
        #region Properties

        public Vector3 Translation {
            get => _translation;
            set {
                _translation = value;
                UpdateVectors();
            }
        }

        public Vector3 Scale {
            get => _scale;
            set {
                _scale = value;
            }
        }

        public Quaternion Rotation {
            get => _rotation;
            set {
                _rotation = value;
                UpdateVectors();
            }
        }

        public Vector3 Up => _up;
        public Vector3 Forward => _forward;
        public Vector3 Right => _right;

        #endregion

        #region Members

        private Vector3 _translation;
        private Quaternion _rotation;
        private Vector3 _scale;

        private Vector3 _up;
        private Vector3 _forward;
        private Vector3 _right;

        #endregion

        private void UpdateVectors()
        {
            _right = Vector3.Normalize(Vector3.Cross(_forward, Vector3.UnitY));
            _forward = Vector3.Normalize(Vector3.Transform(-Vector3.UnitZ, _rotation));
            _up = Vector3.Cross(_forward, _right);
        }
    }
}
