using System.Diagnostics;
using ADyn;
using ADyn.Collision;
using Arch.Core;
using Arch.LowLevel;
using Duck.Graphics.Components;
using Duck.Physics.Events;
using Duck.Platform;
using Duck.ServiceBus;
using Silk.NET.Maths;

namespace Duck.Physics;

public class PhysicsScene : IPhysicsScene
{
    #region Properties

    public Vector3D<float> Gravity {
        get {
            Console.WriteLine("TODO: PhysicsWorld.GetGravity");
            return Vector3D<float>.Zero;
        }

        set { Console.WriteLine("TODO: PhysicsWorld.SetGravity"); }
    }

    public bool IsPaused {
        get => _simulation.IsPaused;
        set => _simulation.IsPaused = value;
    }

    public Simulation Simulation => _simulation;

    #endregion

    #region Members

    private readonly World _world;
    private readonly Simulation _simulation;

    private UnsafeList<PhysicsCollision> _collisionEvents = new(8);

    #endregion

    #region Methods

    public PhysicsScene(World world)
    {
        _world = world;
        _simulation = new Simulation(world, SimulationConfiguration.Default);

        _simulation.ContactEventEmitter.ContactPointCreated += OnContactPointCreated;
    }

    ~PhysicsScene()
    {
        Dispose(false);
    }

    public void EmitEvents(IEventBus eventBus)
    {
        ThrowIfDisposed();

        foreach (var e in _collisionEvents) {
            _world.Create(e);
        }

        _collisionEvents.Clear();
    }

    public bool Overlaps(IBoundingVolume volume, Vector3D<float> position, Quaternion<float> rotation)
    {
        ThrowIfDisposed();

        /*var transform = new PxTransform(rotation.ToSystem(), position.ToSystem());
        var flags = PxQueryFlag.AnyHit | PxQueryFlag.Dynamic | PxQueryFlag.Static;

        if (volume is BoundingBoxComponent boxVolume) {
            return _scene.Overlap(
                PhysXHelper.CreateBoxGeometry(boxVolume.Box, Vector3D<float>.One),
                transform,
                flags
            );
        } else if (volume is BoundingSphereComponent sphereVolume) {
            return _scene.Overlap(
                PhysXHelper.CreateSphereGeometry(sphereVolume, Vector3D<float>.One),
                transform,
                flags
            );
        }

        throw new ApplicationException("TODO: errors");*/

        Console.WriteLine("TODO: PhysicsWorld.Overlaps");

        return false;
    }

    public void Step(float timeStep)
    {
        ThrowIfDisposed();
Console.WriteLine("STEP");
        _simulation.Update(Time.Elapsed);
    }

    private void OnContactPointCreated(Entity entity, byte contactPointIndex)
    {
        ThrowIfDisposed();

        var manifold = _world.Get<ContactManifold>(entity);

        if (manifold.BodyA.HasValue && manifold.BodyB.HasValue) {
            _collisionEvents.Add(new PhysicsCollision { A = manifold.BodyA!.Value, B = manifold.BodyB!.Value, });

            _collisionEvents.Add(new PhysicsCollision { A = manifold.BodyB!.Value, B = manifold.BodyA!.Value, });
        }
    }

    #endregion

    #region IDisposable implementation

    private bool _isDisposed = false;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_isDisposed) {
            return;
        }

        _simulation.Dispose();
        _isDisposed = true;
    }

    [Conditional("DEBUG")]
    private void ThrowIfDisposed()
    {
        if (_isDisposed) {
            throw new ObjectDisposedException("PhysicsScene");
        }
    }

    #endregion
}