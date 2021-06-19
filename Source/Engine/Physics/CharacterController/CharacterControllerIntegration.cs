using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuUtilities;
using BepuUtilities.Collections;
using BepuUtilities.Memory;
using Duck.Ecs;
using Duck.Physics.Components;

namespace Duck.Physics.CharacterController
{
    internal class CharacterControllerIntegration
    {
        const int InitialCharacterCapacity = 4096;
        const int InitialBodyHandleCapacity = 4096;

        private BufferPool _bufferPool;
        private Simulation _simulation;

        private Buffer<int> _bodyHandleToCharacterIndex;

        // private QuickList<CharacterControllerComponent> _characters;
        private QuickList<int> _characters;
        private Buffer<ContactCollectionWorkerCache> _contactCollectionWorkerCaches;

        private Buffer<(int Start, int Count)> _boundingBoxExpansionJobs;
        private int _boundingBoxExpansionJobIndex;
        private Action<int> _expandBoundingBoxesWorker;

        private Buffer<AnalyzeContactsWorkerCache> _analyzeContactsWorkerCaches;

        private int _analysisJobIndex;
        private int _analysisJobCount;
        private Buffer<AnalyzeContactsJob> _analysisJobs;
        private Action<int> _analyzeContactsWorker;

        private IWorld _world;
        private int _characterControllerTypeIndex;

        public void Initialize(Simulation simulation, IWorld world)
        {
            _world = world;
            _characterControllerTypeIndex = _world.GetTypeIndexForComponent<CharacterControllerComponent>();

            _simulation = simulation;
            _simulation.Solver.Register<DynamicCharacterMotionConstraint>();
            _simulation.Solver.Register<StaticCharacterMotionConstraint>();
            _simulation.Timestepper.BeforeCollisionDetection += PrepareForContacts;
            _simulation.Timestepper.CollisionsDetected += AnalyzeContacts;

            _bufferPool = _simulation.BufferPool;
            // _characters = new QuickList<CharacterControllerComponent>(InitialCharacterCapacity, _bufferPool);
            _characters = new QuickList<int>(InitialCharacterCapacity, _bufferPool);
            _analyzeContactsWorker = AnalyzeContactsWorker;
            _expandBoundingBoxesWorker = ExpandBoundingBoxesWorker;

            ResizeBodyHandleCapacity(InitialBodyHandleCapacity);
        }

        public void Allocate(int entityId, ref CharacterControllerComponent characterControllerComponent, PhysicsBodyComponent physicsBodyComponent)
        {
            BodyHandle bodyHandle = physicsBodyComponent.BodyHandle;

            Debug.Assert(bodyHandle.Value >= 0 && (bodyHandle.Value >= _bodyHandleToCharacterIndex.Length || _bodyHandleToCharacterIndex[bodyHandle.Value] == -1),
                "Cannot allocate more than one character for the same body handle.");

            if (bodyHandle.Value >= _bodyHandleToCharacterIndex.Length) {
                ResizeBodyHandleCapacity(Math.Max(bodyHandle.Value + 1, _bodyHandleToCharacterIndex.Length * 2));
            }

            int characterIndex = _characters.Count;

            _characters.Add(in entityId, _bufferPool);
            _bodyHandleToCharacterIndex[bodyHandle.Value] = characterIndex;
            characterControllerComponent.BodyHandle = bodyHandle;
        }

        private void ResizeBodyHandleCapacity(int bodyHandleCapacity)
        {
            int oldCapacity = _bodyHandleToCharacterIndex.Length;

            _bufferPool.ResizeToAtLeast(ref _bodyHandleToCharacterIndex, bodyHandleCapacity, _bodyHandleToCharacterIndex.Length);

            if (_bodyHandleToCharacterIndex.Length > oldCapacity) {
                Unsafe.InitBlockUnaligned(ref Unsafe.As<int, byte>(ref _bodyHandleToCharacterIndex[oldCapacity]), 0xFF, (uint)((_bodyHandleToCharacterIndex.Length - oldCapacity) * sizeof(int)));
            }
        }

        private struct SupportCandidate
        {
            public Vector3 OffsetFromCharacter;
            public float Depth;
            public Vector3 OffsetFromSupport;
            public Vector3 Normal;
            public CollidableReference Support;
        }

        private struct ContactCollectionWorkerCache
        {
            public Buffer<SupportCandidate> SupportCandidates;

            public ContactCollectionWorkerCache(int maximumCharacterCount, BufferPool pool)
            {
                pool.Take(maximumCharacterCount, out SupportCandidates);

                for (int i = 0; i < maximumCharacterCount; ++i) {
                    //Initialize the depths to a value that guarantees replacement.
                    SupportCandidates[i].Depth = float.MinValue;
                }
            }

            public void Dispose(BufferPool pool)
            {
                pool.Return(ref SupportCandidates);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool TryReportContacts<TManifold>(CollidableReference characterCollidable, CollidableReference supportCollidable, CollidablePair pair, ref TManifold manifold, int workerIndex) where TManifold : struct, IContactManifold<TManifold>
        {
            if (characterCollidable.Mobility == CollidableMobility.Dynamic && characterCollidable.BodyHandle.Value < _bodyHandleToCharacterIndex.Length) {
                BodyHandle characterBodyHandle = characterCollidable.BodyHandle;
                int characterIndex = _bodyHandleToCharacterIndex[characterBodyHandle.Value];

                if (characterIndex >= 0) {
                    // This is actually a character.
                    // ref CharacterControllerComponent character = ref _characters[characterIndex];
                    ref CharacterControllerComponent character = ref _world.GetComponent<CharacterControllerComponent>(_characters[characterIndex]);

                    // Our job here is to process the manifold into a support representation. That means a single point, normal, and importance heuristic.
                    // Note that we cannot safely pick from the candidates in this function- it is likely executed from a multithreaded context, so all we can do is
                    // output the pair's result into a worker-exclusive buffer.

                    // Contacts with sufficiently negative depth will not be considered support candidates.
                    // Contacts with intermediate depth (above minimum threshold, but still below negative epsilon) may be candidates if the character previously had support.
                    // Contacts with depth above negative epsilon always pass the depth test.

                    // Maximum depth is used to heuristically choose which contact represents the support.
                    // Note that this could be changed to subtly modify the behavior- for example, dotting the movement direction with the support normal and such.
                    // A more careful choice of heuristic could make the character more responsive when trying to 'step' up obstacles.

                    // Note that the body may be inactive during this callback even though it will be activated by new constraints after the narrow phase flushes.
                    // Have to take into account the current potentially inactive location.
                    ref BodyMemoryLocation bodyLocation = ref _simulation.Bodies.HandleToLocation[characterBodyHandle.Value];
                    ref BodySet set = ref _simulation.Bodies.Sets[bodyLocation.SetIndex];
                    ref RigidPose pose = ref set.Poses[bodyLocation.Index];

                    QuaternionEx.Transform(character.LocalUp, pose.Orientation, out var up);

                    // Note that this branch is compiled out- the generic constraints force type specialization.
                    if (manifold.Convex) {
                        ref ConvexContactManifold convexManifold = ref Unsafe.As<TManifold, ConvexContactManifold>(ref manifold);
                        float normalUpDot = Vector3.Dot(convexManifold.Normal, up);

                        // The narrow phase generates contacts with normals pointing from B to A by convention.
                        // If the character is collidable B, then we need to negate the comparison.
                        if ((pair.B.Packed == characterCollidable.Packed ? -normalUpDot : normalUpDot) > character.CosMaximumSlope) {
                            // This manifold has a slope that is potentially supportive.
                            // Can the maximum depth contact be used as a support?
                            float maximumDepth = convexManifold.Contact0.Depth;
                            int maximumDepthIndex = 0;

                            for (int i = 1; i < convexManifold.Count; ++i) {
                                ref float candidateDepth = ref Unsafe.Add(ref convexManifold.Contact0, i).Depth;

                                if (candidateDepth > maximumDepth) {
                                    maximumDepth = candidateDepth;
                                    maximumDepthIndex = i;
                                }
                            }

                            if (maximumDepth >= character.MinimumSupportDepth || (character.Supported && maximumDepth > character.MinimumSupportContinuationDepth)) {
                                ref SupportCandidate supportCandidate = ref _contactCollectionWorkerCaches[workerIndex].SupportCandidates[characterIndex];

                                if (supportCandidate.Depth < maximumDepth) {
                                    // This support candidate should be replaced.
                                    supportCandidate.Depth = maximumDepth;
                                    ref ConvexContact deepestContact = ref Unsafe.Add(ref convexManifold.Contact0, maximumDepthIndex);
                                    Vector3 offsetFromB = deepestContact.Offset - convexManifold.OffsetB;

                                    if (pair.B.Packed == characterCollidable.Packed) {
                                        supportCandidate.Normal = -convexManifold.Normal;
                                        supportCandidate.OffsetFromCharacter = offsetFromB;
                                        supportCandidate.OffsetFromSupport = deepestContact.Offset;
                                    } else {
                                        supportCandidate.Normal = convexManifold.Normal;
                                        supportCandidate.OffsetFromCharacter = deepestContact.Offset;
                                        supportCandidate.OffsetFromSupport = offsetFromB;
                                    }

                                    supportCandidate.Support = supportCollidable;
                                }
                            }
                        }
                    } else {
                        ref NonconvexContactManifold nonconvexManifold = ref Unsafe.As<TManifold, NonconvexContactManifold>(ref manifold);

                        // The narrow phase generates contacts with normals pointing from B to A by convention.
                        // If the character is collidable B, then we need to negate the comparison.
                        // This manifold has a slope that is potentially supportive.
                        // Can the maximum depth contact be used as a support?
                        float maximumDepth = float.MinValue;
                        int maximumDepthIndex = -1;

                        for (int i = 0; i < nonconvexManifold.Count; ++i) {
                            ref NonconvexContact candidate = ref Unsafe.Add(ref nonconvexManifold.Contact0, i);

                            if (candidate.Depth > maximumDepth) {
                                // All the nonconvex candidates can have different normals, so we have to perform the (calibrated) normal test on every single one.
                                float upDot = Vector3.Dot(candidate.Normal, up);

                                if ((pair.B.Packed == characterCollidable.Packed ? -upDot : upDot) > character.CosMaximumSlope) {
                                    maximumDepth = candidate.Depth;
                                    maximumDepthIndex = i;
                                }
                            }
                        }

                        if (maximumDepth >= character.MinimumSupportDepth || (character.Supported && maximumDepth > character.MinimumSupportContinuationDepth)) {
                            ref SupportCandidate supportCandidate = ref _contactCollectionWorkerCaches[workerIndex].SupportCandidates[characterIndex];

                            if (supportCandidate.Depth < maximumDepth) {
                                //This support candidate should be replaced.
                                ref NonconvexContact deepestContact = ref Unsafe.Add(ref nonconvexManifold.Contact0, maximumDepthIndex);
                                supportCandidate.Depth = maximumDepth;
                                Vector3 offsetFromB = deepestContact.Offset - nonconvexManifold.OffsetB;

                                if (pair.B.Packed == characterCollidable.Packed) {
                                    supportCandidate.Normal = -deepestContact.Normal;
                                    supportCandidate.OffsetFromCharacter = offsetFromB;
                                    supportCandidate.OffsetFromSupport = deepestContact.Offset;
                                } else {
                                    supportCandidate.Normal = deepestContact.Normal;
                                    supportCandidate.OffsetFromCharacter = deepestContact.Offset;
                                    supportCandidate.OffsetFromSupport = offsetFromB;
                                }

                                supportCandidate.Support = supportCollidable;
                            }
                        }
                    }

                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Reports contacts about a collision to the character system. If the pair does not involve a character or there are no contacts, does nothing and returns false.
        /// </summary>
        /// <param name="pair">Pair of objects associated with the contact manifold.</param>
        /// <param name="manifold">Contact manifold between the colliding objects.</param>
        /// <param name="workerIndex">Index of the currently executing worker thread.</param>
        /// <param name="materialProperties">Material properties for this pair. Will be modified if the pair involves a character.</param>
        /// <returns>True if the pair involved a character pair and has contacts, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryReportContacts<TManifold>(in CollidablePair pair, ref TManifold manifold, int workerIndex, ref PairMaterialProperties materialProperties) where TManifold : struct, IContactManifold<TManifold>
        {
            Debug.Assert(_contactCollectionWorkerCaches.Allocated && workerIndex < _contactCollectionWorkerCaches.Length && _contactCollectionWorkerCaches[workerIndex].SupportCandidates.Allocated,
                "Worker caches weren't properly allocated; did you forget to call PrepareForContacts before collision detection?");

            if (manifold.Count == 0) {
                return false;
            }

            // It's possible for neither, one, or both collidables to be a character. Check each one, treating the other as a potential support.
            bool aIsCharacter = TryReportContacts(pair.A, pair.B, pair, ref manifold, workerIndex);
            bool bIsCharacter = TryReportContacts(pair.B, pair.A, pair, ref manifold, workerIndex);

            if (aIsCharacter || bIsCharacter) {
                // The character's motion over the surface should be controlled entirely by the horizontal motion constraint.
                // Note- you could use the friction coefficient to change the horizontal motion constraint's maximum force to simulate different environments if you want.
                // That would just require caching a bit more information for the AnalyzeContacts function to use.
                materialProperties.FrictionCoefficient = 0;
                return true;
            }

            return false;
        }

        private unsafe void ExpandBoundingBoxes(int start, int count)
        {
            var end = start + count;

            for (int i = start; i < end; ++i) {
                ref CharacterControllerComponent character = ref _world.GetComponent<CharacterControllerComponent>(_characters[i]);
                BodyReference characterBody = _simulation.Bodies.GetBodyReference(character.BodyHandle);

                if (characterBody.Awake) {
                    _simulation.BroadPhase.GetActiveBoundsPointers(characterBody.Collidable.BroadPhaseIndex, out var min, out var max);
                    QuaternionEx.Transform(character.LocalUp, characterBody.Pose.Orientation, out var characterUp);

                    Vector3 supportExpansion = character.MinimumSupportContinuationDepth * characterUp;

                    *min += Vector3.Min(Vector3.Zero, supportExpansion);
                    *max += Vector3.Max(Vector3.Zero, supportExpansion);
                }
            }
        }

        private void ExpandBoundingBoxesWorker(int workerIndex)
        {
            while (true) {
                int jobIndex = Interlocked.Increment(ref _boundingBoxExpansionJobIndex);

                if (jobIndex < _boundingBoxExpansionJobs.Length) {
                    ref var job = ref _boundingBoxExpansionJobs[jobIndex];
                    Console.WriteLine("BB");
                    ExpandBoundingBoxes(job.Start, job.Count);
                } else {
                    break;
                }
            }
        }

        /// <summary>
        /// Preallocates space for support data collected during the narrow phase. Should be called before the narrow phase executes.
        /// </summary>
        private void PrepareForContacts(float dt, IThreadDispatcher? threadDispatcher = null)
        {
            Debug.Assert(!_contactCollectionWorkerCaches.Allocated, "Worker caches were already allocated; did you forget to call AnalyzeContacts after collision detection to flush the previous frame's results?");

            int threadCount = threadDispatcher?.ThreadCount ?? 1;

            _bufferPool.Take(threadCount, out _contactCollectionWorkerCaches);

            for (int i = 0; i < _contactCollectionWorkerCaches.Length; ++i) {
                _contactCollectionWorkerCaches[i] = new ContactCollectionWorkerCache(_characters.Count, _bufferPool);
            }

            // While the character will retain support with contacts with depths above the MinimumSupportContinuationDepth if there was support in the previous frame,
            // it's possible for the contacts to be lost because the bounding box isn't expanded by MinimumSupportContinuationDepth and the broad phase doesn't see the support collidable.
            // Here, we expand the bounding boxes to compensate.
            if (threadCount == 1 || _characters.Count < 256) {
                ExpandBoundingBoxes(0, _characters.Count);
            } else {
                int jobCount = Math.Min(_characters.Count, threadCount);
                int charactersPerJob = _characters.Count / jobCount;
                int baseCharacterCount = charactersPerJob * jobCount;
                int remainder = _characters.Count - baseCharacterCount;
                int previousEnd = 0;

                _bufferPool.Take(jobCount, out _boundingBoxExpansionJobs);

                for (int jobIndex = 0; jobIndex < jobCount; ++jobIndex) {
                    int charactersForJob = jobIndex < remainder ? charactersPerJob + 1 : charactersPerJob;
                    ref (int Start, int Count) job = ref _boundingBoxExpansionJobs[jobIndex];
                    job.Start = previousEnd;
                    job.Count = charactersForJob;
                    previousEnd += job.Count;
                }

                _boundingBoxExpansionJobIndex = -1;
                threadDispatcher?.DispatchWorkers(_expandBoundingBoxesWorker);
                _bufferPool.Return(ref _boundingBoxExpansionJobs);
            }
        }

        private struct PendingDynamicConstraint
        {
            public int CharacterIndex;
            public DynamicCharacterMotionConstraint Description;
        }

        private struct PendingStaticConstraint
        {
            public int CharacterIndex;
            public StaticCharacterMotionConstraint Description;
        }

        private struct Jump
        {
            // Note that not every jump will contain a support body, so this can waste memory.
            // That's not really a concern- jumps are very rare (relatively speaking), so all we're wasting is capacity, not bandwidth.
            public int CharacterBodyIndex;
            public Vector3 CharacterVelocityChange;
            public int SupportBodyIndex;
            public Vector3 SupportImpulseOffset;
        }

        private struct AnalyzeContactsWorkerCache
        {
            // The solver does not permit multithreaded removals and additions. We handle all of them in a sequential postpass.
            public QuickList<ConstraintHandle> ConstraintHandlesToRemove;
            public QuickList<PendingDynamicConstraint> DynamicConstraintsToAdd;
            public QuickList<PendingStaticConstraint> StaticConstraintsToAdd;
            public QuickList<Jump> Jumps;

            public AnalyzeContactsWorkerCache(int maximumCharacterCount, BufferPool pool)
            {
                ConstraintHandlesToRemove = new QuickList<ConstraintHandle>(maximumCharacterCount, pool);
                DynamicConstraintsToAdd = new QuickList<PendingDynamicConstraint>(maximumCharacterCount, pool);
                StaticConstraintsToAdd = new QuickList<PendingStaticConstraint>(maximumCharacterCount, pool);
                Jumps = new QuickList<Jump>(maximumCharacterCount, pool);
            }

            public void Dispose(BufferPool pool)
            {
                ConstraintHandlesToRemove.Dispose(pool);
                DynamicConstraintsToAdd.Dispose(pool);
                StaticConstraintsToAdd.Dispose(pool);
                Jumps.Dispose(pool);
            }
        }

        private void AnalyzeContactsForCharacterRegion(int start, int exclusiveEnd, int workerIndex)
        {
            ref AnalyzeContactsWorkerCache analyzeContactsWorkerCache = ref _analyzeContactsWorkerCaches[workerIndex];

            for (int characterIndex = start; characterIndex < exclusiveEnd; ++characterIndex) {
                // Note that this iterates over both active and inactive characters rather than segmenting inactive characters into their own collection.
                // This demands branching, but the expectation is that the vast majority of characters will be active, so there is less value in copying them into stasis.
                ref CharacterControllerComponent character = ref _world.GetComponent<CharacterControllerComponent>(_characters[characterIndex]);
                ref BodyMemoryLocation bodyLocation = ref _simulation.Bodies.HandleToLocation[character.BodyHandle.Value];

                if (bodyLocation.SetIndex == 0) {
                    SupportCandidate supportCandidate = _contactCollectionWorkerCaches[0].SupportCandidates[characterIndex];

                    for (int j = 1; j < _contactCollectionWorkerCaches.Length; ++j) {
                        ref SupportCandidate workerCandidate = ref _contactCollectionWorkerCaches[j].SupportCandidates[characterIndex];

                        if (workerCandidate.Depth > supportCandidate.Depth) {
                            supportCandidate = workerCandidate;
                        }
                    }

                    // We need to protect against one possible corner case: if the body supporting the character was removed, the associated motion constraint was also removed.
                    // Arbitrarily un-support the character if we detect this.
                    if (character.Supported) {
                        // If the constraint no longer exists at all,
                        if (!_simulation.Solver.ConstraintExists(character.MotionConstraintHandle) ||
                            // or if the constraint does exist but is now used by a different constraint type,
                            (_simulation.Solver.HandleToConstraint[character.MotionConstraintHandle.Value].TypeId != DynamicCharacterMotionTypeProcessor.BatchTypeId &&
                             _simulation.Solver.HandleToConstraint[character.MotionConstraintHandle.Value].TypeId != StaticCharacterMotionTypeProcessor.BatchTypeId)) {
                            // then the character isn't actually supported anymore.
                            character.Supported = false;
                        }
                        // Note that it's sufficient to only check that the type matches the dynamic motion constraint type id because no other systems ever create dynamic character motion constraints.
                        // Other systems may result in the constraint's removal, but no other system will ever *create* it.
                        // Further, during this analysis loop, we do not create any constraints. We only set up pending additions to be processed after the multithreaded analysis completes.
                    }

                    // The body is active. We may need to remove the associated constraint from the solver. Remove if any of the following hold:
                    // 1) The character was previously supported but is no longer.
                    // 2) The character was previously supported by a body, and is now supported by a different body.
                    // 3) The character was previously supported by a static, and is now supported by a body.
                    // 4) The character was previously supported by a body, and is now supported by a static.
                    bool shouldRemove = character.Supported && (character.TryJump || supportCandidate.Depth == float.MinValue || character.Support.Packed != supportCandidate.Support.Packed);

                    if (shouldRemove) {
                        //Mark the constraint for removal.
                        analyzeContactsWorkerCache.ConstraintHandlesToRemove.AllocateUnsafely() = character.MotionConstraintHandle;
                    }

                    //If the character is jumping, don't create a constraint.
                    if (supportCandidate.Depth > float.MinValue && character.TryJump) {
                        QuaternionEx.Transform(character.LocalUp, _simulation.Bodies.ActiveSet.Poses[bodyLocation.Index].Orientation, out var characterUp);
                        // Note that we assume that character orientations are constant. This isn't necessarily the case in all uses, but it's a decent approximation.
                        float characterUpVelocity = Vector3.Dot(_simulation.Bodies.ActiveSet.Velocities[bodyLocation.Index].Linear, characterUp);

                        // We don't want the character to be able to 'superboost' by simply adding jump speed on top of horizontal motion.
                        // Instead, jumping targets a velocity change necessary to reach character.JumpVelocity along the up axis.
                        if (character.Support.Mobility != CollidableMobility.Static) {
                            ref BodyMemoryLocation supportingBodyLocation = ref _simulation.Bodies.HandleToLocation[character.Support.BodyHandle.Value];

                            Debug.Assert(supportingBodyLocation.SetIndex == 0, "If the character is active, any support should be too.");

                            ref BodyVelocity supportVelocity = ref _simulation.Bodies.ActiveSet.Velocities[supportingBodyLocation.Index];
                            Vector3 wxr = Vector3.Cross(supportVelocity.Angular, supportCandidate.OffsetFromSupport);
                            Vector3 supportContactVelocity = supportVelocity.Linear + wxr;
                            float supportUpVelocity = Vector3.Dot(supportContactVelocity, characterUp);

                            // If the support is dynamic, apply an opposing impulse. Note that velocity changes cannot safely be applied during multithreaded execution;
                            // characters could share support bodies, and a character might be a support of another character.
                            // That's really not concerning from a performance perspective- characters don't jump many times per frame.
                            ref Jump jump = ref analyzeContactsWorkerCache.Jumps.AllocateUnsafely();
                            jump.CharacterBodyIndex = bodyLocation.Index;
                            jump.CharacterVelocityChange = characterUp * MathF.Max(0, character.JumpVelocity - (characterUpVelocity - supportUpVelocity));

                            if (character.Support.Mobility == CollidableMobility.Dynamic) {
                                jump.SupportBodyIndex = supportingBodyLocation.Index;
                                jump.SupportImpulseOffset = supportCandidate.OffsetFromSupport;
                            } else {
                                // No point in applying impulses to kinematics.
                                jump.SupportBodyIndex = -1;
                            }
                        } else {
                            // Static bodies have no velocity, so we don't have to consider the support.
                            ref Jump jump = ref analyzeContactsWorkerCache.Jumps.AllocateUnsafely();
                            jump.CharacterBodyIndex = bodyLocation.Index;
                            jump.CharacterVelocityChange = characterUp * MathF.Max(0, character.JumpVelocity - characterUpVelocity);
                            jump.SupportBodyIndex = -1;
                        }

                        character.Supported = false;
                    } else if (supportCandidate.Depth > float.MinValue) {
                        // If a support currently exists and there is still an old constraint, then update it.
                        // If a support currently exists and there is not an old constraint, add the new constraint.

                        // Project the view direction down onto the surface as represented by the contact normal.
                        Matrix3x3 surfaceBasis;
                        surfaceBasis.Y = supportCandidate.Normal;

                        // Note negation: we're using a right handed basis where -Z is forward, +Z is backward.
                        QuaternionEx.Transform(character.LocalUp, _simulation.Bodies.ActiveSet.Poses[bodyLocation.Index].Orientation, out Vector3 up);

                        var rayDistance = Vector3.Dot(character.ViewDirection, surfaceBasis.Y);
                        var rayVelocity = Vector3.Dot(up, surfaceBasis.Y);

                        Debug.Assert(rayVelocity > 0,
                            "The calibrated support normal and the character's up direction should have a positive dot product if the maximum slope is working properly. Is the maximum slope >= pi/2?");

                        surfaceBasis.Z = up * (rayDistance / rayVelocity) - character.ViewDirection;
                        float zLengthSquared = surfaceBasis.Z.LengthSquared();

                        if (zLengthSquared > 1e-12f) {
                            surfaceBasis.Z /= MathF.Sqrt(zLengthSquared);
                        } else {
                            QuaternionEx.GetQuaternionBetweenNormalizedVectors(Vector3.UnitY, surfaceBasis.Y, out Quaternion rotation);
                            QuaternionEx.TransformUnitZ(rotation, out surfaceBasis.Z);
                        }

                        surfaceBasis.X = Vector3.Cross(surfaceBasis.Y, surfaceBasis.Z);
                        QuaternionEx.CreateFromRotationMatrix(surfaceBasis, out Quaternion surfaceBasisQuaternion);

                        if (supportCandidate.Support.Mobility != CollidableMobility.Static) {
                            //The character is supported by a body.
                            DynamicCharacterMotionConstraint motionConstraint = new DynamicCharacterMotionConstraint {
                                MaximumHorizontalForce = character.MaximumHorizontalForce,
                                MaximumVerticalForce = character.MaximumVerticalForce,
                                OffsetFromCharacterToSupportPoint = supportCandidate.OffsetFromCharacter,
                                OffsetFromSupportToSupportPoint = supportCandidate.OffsetFromSupport,
                                SurfaceBasis = surfaceBasisQuaternion,
                                TargetVelocity = character.TargetVelocity,
                                Depth = supportCandidate.Depth
                            };

                            if (character.Supported && !shouldRemove) {
                                //Already exists, update it.
                                _simulation.Solver.ApplyDescriptionWithoutWaking(character.MotionConstraintHandle, ref motionConstraint);
                            } else {
                                //Doesn't exist, mark it for addition.
                                ref PendingDynamicConstraint pendingConstraint = ref analyzeContactsWorkerCache.DynamicConstraintsToAdd.AllocateUnsafely();
                                pendingConstraint.Description = motionConstraint;
                                pendingConstraint.CharacterIndex = characterIndex;
                            }
                        } else {
                            // The character is supported by a static.
                            StaticCharacterMotionConstraint motionConstraint = new StaticCharacterMotionConstraint {
                                MaximumHorizontalForce = character.MaximumHorizontalForce,
                                MaximumVerticalForce = character.MaximumVerticalForce,
                                OffsetFromCharacterToSupportPoint = supportCandidate.OffsetFromCharacter,
                                SurfaceBasis = surfaceBasisQuaternion,
                                TargetVelocity = character.TargetVelocity,
                                Depth = supportCandidate.Depth
                            };

                            if (character.Supported && !shouldRemove) {
                                // Already exists, update it.
                                _simulation.Solver.ApplyDescriptionWithoutWaking(character.MotionConstraintHandle, ref motionConstraint);
                            } else {
                                //Doesn't exist, mark it for addition.
                                ref PendingStaticConstraint pendingConstraint = ref analyzeContactsWorkerCache.StaticConstraintsToAdd.AllocateUnsafely();
                                pendingConstraint.Description = motionConstraint;
                                pendingConstraint.CharacterIndex = characterIndex;
                            }
                        }

                        character.Supported = true;
                        character.Support = supportCandidate.Support;
                    } else {
                        character.Supported = false;
                    }
                }

                //The TryJump flag is always reset even if the attempt failed.
                character.TryJump = false;
            }
        }

        private struct AnalyzeContactsJob
        {
            public int Start;
            public int ExclusiveEnd;
        }

        private void AnalyzeContactsWorker(int workerIndex)
        {
            int jobIndex;

            while ((jobIndex = Interlocked.Increment(ref _analysisJobIndex)) < _analysisJobCount) {
                ref AnalyzeContactsJob job = ref _analysisJobs[jobIndex];
                AnalyzeContactsForCharacterRegion(job.Start, job.ExclusiveEnd, workerIndex);
            }
        }


        /// <summary>
        /// Updates all character support states and motion constraints based on the current character goals and all the contacts collected since the last call to AnalyzeContacts.
        /// Attach to a simulation callback where the most recent contact is available and before the solver executes.
        /// </summary>
        private void AnalyzeContacts(float dt, IThreadDispatcher? threadDispatcher)
        {
            Debug.Assert(_contactCollectionWorkerCaches.Allocated, "Worker caches weren't properly allocated; did you forget to call PrepareForContacts before collision detection?");

            if (threadDispatcher == null) {
                _bufferPool.Take(1, out _analyzeContactsWorkerCaches);
                _analyzeContactsWorkerCaches[0] = new AnalyzeContactsWorkerCache(_characters.Count, _bufferPool);

                AnalyzeContactsForCharacterRegion(0, _characters.Count, 0);
            } else {
                _analysisJobCount = Math.Min(_characters.Count, threadDispatcher.ThreadCount * 4);

                if (_analysisJobCount > 0) {
                    _bufferPool.Take(threadDispatcher.ThreadCount, out _analyzeContactsWorkerCaches);
                    _bufferPool.Take(_analysisJobCount, out _analysisJobs);

                    for (int i = 0; i < threadDispatcher.ThreadCount; ++i) {
                        _analyzeContactsWorkerCaches[i] = new AnalyzeContactsWorkerCache(_characters.Count, _bufferPool);
                    }

                    int baseCount = _characters.Count / _analysisJobCount;
                    int remainder = _characters.Count - baseCount * _analysisJobCount;
                    int previousEnd = 0;

                    for (int i = 0; i < _analysisJobCount; ++i) {
                        ref AnalyzeContactsJob job = ref _analysisJobs[i];
                        job.Start = previousEnd;
                        job.ExclusiveEnd = job.Start + (i < remainder ? baseCount + 1 : baseCount);
                        previousEnd = job.ExclusiveEnd;
                    }

                    _analysisJobIndex = -1;
                    threadDispatcher.DispatchWorkers(_analyzeContactsWorker);
                    _bufferPool.Return(ref _analysisJobs);
                }
            }

            // We're done with all the contact collection worker caches.
            for (int i = 0; i < _contactCollectionWorkerCaches.Length; ++i) {
                _contactCollectionWorkerCaches[i].Dispose(_bufferPool);
            }

            _bufferPool.Return(ref _contactCollectionWorkerCaches);

            if (_analyzeContactsWorkerCaches.Allocated) {
                // Flush all the worker caches. Note that we perform all removals before moving onto any additions to avoid unnecessary constraint batches
                // caused by the new and old constraint affecting the same bodies.
                for (int threadIndex = 0; threadIndex < _analyzeContactsWorkerCaches.Length; ++threadIndex) {
                    ref AnalyzeContactsWorkerCache cache = ref _analyzeContactsWorkerCaches[threadIndex];

                    for (int i = 0; i < cache.ConstraintHandlesToRemove.Count; ++i) {
                        _simulation.Solver.Remove(cache.ConstraintHandlesToRemove[i]);
                    }
                }

                for (int threadIndex = 0; threadIndex < _analyzeContactsWorkerCaches.Length; ++threadIndex) {
                    ref AnalyzeContactsWorkerCache workerCache = ref _analyzeContactsWorkerCaches[threadIndex];

                    for (int i = 0; i < workerCache.StaticConstraintsToAdd.Count; ++i) {
                        ref PendingStaticConstraint pendingConstraint = ref workerCache.StaticConstraintsToAdd[i];
                        ref CharacterControllerComponent character = ref _world.GetComponent<CharacterControllerComponent>(_characters[pendingConstraint.CharacterIndex]);

                        Debug.Assert(character.Support.Mobility == CollidableMobility.Static);

                        character.MotionConstraintHandle = _simulation.Solver.Add(character.BodyHandle, ref pendingConstraint.Description);
                    }

                    for (int i = 0; i < workerCache.DynamicConstraintsToAdd.Count; ++i) {
                        ref PendingDynamicConstraint pendingConstraint = ref workerCache.DynamicConstraintsToAdd[i];
                        ref CharacterControllerComponent character = ref _world.GetComponent<CharacterControllerComponent>(_characters[pendingConstraint.CharacterIndex]);

                        Debug.Assert(character.Support.Mobility != CollidableMobility.Static);

                        character.MotionConstraintHandle = _simulation.Solver.Add(character.BodyHandle, character.Support.BodyHandle, ref pendingConstraint.Description);
                    }

                    ref BodySet activeSet = ref _simulation.Bodies.ActiveSet;
                    for (int i = 0; i < workerCache.Jumps.Count; ++i) {
                        ref Jump jump = ref workerCache.Jumps[i];
                        activeSet.Velocities[jump.CharacterBodyIndex].Linear += jump.CharacterVelocityChange;

                        if (jump.SupportBodyIndex >= 0) {
                            BodyReference.ApplyImpulse(_simulation.Bodies.ActiveSet, jump.SupportBodyIndex, jump.CharacterVelocityChange / -activeSet.LocalInertias[jump.CharacterBodyIndex].InverseMass, jump.SupportImpulseOffset);
                        }
                    }

                    workerCache.Dispose(_bufferPool);
                }

                _bufferPool.Return(ref _analyzeContactsWorkerCaches);
            }
        }

        /// <summary>
        /// Ensures that the internal structures of the character controllers system can handle the given number of characters and body handles, resizing if necessary.
        /// </summary>
        /// <param name="characterCapacity">Minimum character capacity to require.</param>
        /// <param name="bodyHandleCapacity">Minimum number of body handles to allocate space for.</param>
        public void EnsureCapacity(int characterCapacity, int bodyHandleCapacity)
        {
            _characters.EnsureCapacity(characterCapacity, _bufferPool);

            if (_bodyHandleToCharacterIndex.Length < bodyHandleCapacity) {
                ResizeBodyHandleCapacity(bodyHandleCapacity);
            }
        }

        /// <summary>
        /// Resizes the internal structures of the character controllers system for the target sizes. Will not shrink below the currently active data size.
        /// </summary>
        /// <param name="characterCapacity">Target character capacity to allocate space for.</param>
        /// <param name="bodyHandleCapacity">Target number of body handles to allocate space for.</param>
        public void Resize(int characterCapacity, int bodyHandleCapacity)
        {
            int lastOccupiedIndex = -1;

            for (int i = _bodyHandleToCharacterIndex.Length - 1; i >= 0; --i) {
                if (_bodyHandleToCharacterIndex[i] != -1) {
                    lastOccupiedIndex = i;
                    break;
                }
            }

            int targetHandleCapacity = BufferPool.GetCapacityForCount<int>(Math.Max(lastOccupiedIndex + 1, bodyHandleCapacity));

            if (targetHandleCapacity != _bodyHandleToCharacterIndex.Length) {
                ResizeBodyHandleCapacity(targetHandleCapacity);
            }

            int targetCharacterCapacity = BufferPool.GetCapacityForCount<int>(Math.Max(_characters.Count, characterCapacity));

            if (targetCharacterCapacity != _characters.Span.Length) {
                _characters.Resize(targetCharacterCapacity, _bufferPool);
            }
        }
    }
}
