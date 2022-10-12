using Duck.Graphics.Device;
using Silk.NET.OpenGL;

namespace Duck.Graphics.OpenGL;

public class OpenGLIndexBuffer<TDataType> : OpenGLBufferBase<TDataType>, IIndexBuffer<TDataType>
    where TDataType : unmanaged
{
    #region Methods

    internal OpenGLIndexBuffer(OpenGLGraphicsDevice graphicsDevice, GL api, BufferUsageARB usage)
        : base(graphicsDevice, api, usage, BufferTargetARB.ElementArrayBuffer)
    {
    }

    #endregion
}
