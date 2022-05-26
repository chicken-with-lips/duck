using Silk.NET.Maths;

namespace Duck.Physics.Components;

public struct CharacterControllerComponent
{
    /// <summary>
    /// Direction the character is looking in world space. Defines the forward direction for movement.
    /// </summary>
    public Vector3D<float> ViewDirection;

    /// <summary>
    /// Target horizontal velocity.
    /// X component refers to desired velocity along the strafing direction (perpendicular to the view direction projected down to the surface),
    /// Y component refers to the desired velocity along the forward direction (aligned with the view direction projected down to the surface).
    /// </summary>
    public Vector2D<float> TargetVelocity;

    /// <summary>
    /// If true, the character will try to jump on the next time step. Will be reset to false after being processed.
    /// </summary>
    public bool TryJump;

    /// <summary>
    /// Character's up direction in the local space of the character's body.
    /// </summary>
    public Vector3D<float> LocalUp;

    /// <summary>
    /// Velocity at which the character pushes off the support during a jump.
    /// </summary>
    public float JumpVelocity;

    /// <summary>
    /// Maximum force the character can apply tangent to the supporting surface to move.
    /// </summary>
    public float MaximumHorizontalForce;

    /// <summary>
    /// Maximum force the character can apply to glue itself to the supporting surface.
    /// </summary>
    public float MaximumVerticalForce;

    /// <summary>
    /// Cosine of the maximum slope angle that the character can treat as a support.
    /// </summary>
    public float CosMaximumSlope;

    /// <summary>
    /// Depth threshold beyond which a contact is considered a support if it the normal allows it.
    /// </summary>
    public float MinimumSupportDepth;

    /// <summary>
    /// Depth threshold beyond which a contact is considered a support if the previous frame had support, even if it isn't deep enough to meet the MinimumSupportDepth.
    /// </summary>
    public float MinimumSupportContinuationDepth;

    /// <summary>
    /// Flag that the physics body should awake.
    /// </summary>
    public bool AwakeBody;
}
