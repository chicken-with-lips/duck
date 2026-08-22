# Code Review Findings — Serialization & Render System Scaffold

Findings from a review of the `v2` branch diff (`origin/v2...HEAD`, ~7,813 lines):
the custom binary serialization system + Roslyn generators, engine core
(module management, scene/ECS), and the Godot/Vulkan render backends. All
items below were confirmed directly against source, not just reported by a
reviewer.

Checkboxes are for tracking as these get fixed.

## Source generator — emits code that doesn't compile

The generator (`Source/SerializationGenerators/SerializerGenerator.cs`)
references APIs that don't exist anywhere in the codebase. None of these
trigger today because no current `[DuckSerializable]` type uses these shapes
(list fields, nested reference fields, class types, unconstrained generics) —
but the moment one does, the build breaks.

- [ ] **Class (non-struct) `[DuckSerializable]` types don't compile** — `SerializerGenerator.cs:156`. `GenerateDeserializerMethod` emits `context.ObjectId`/`context.AddObject(...)` for any non-value-type, but the generated `Deserialize(GraphReader reader)` method has no `context` in scope. CS0103.
- [ ] **`List<T>` field serialize doesn't compile** — `SerializerGenerator.cs:132`. Emits `writer.Write(name, array, typeNameString)`, a 3-arg overload `GraphWriter` doesn't have (only `Write<T>(string, in IList<T>)` exists). CS1501.
- [ ] **`List<T>` field deserialize doesn't compile** — `SerializerGenerator.cs:201`. Calls `reader.ReadObjectList<...>(lambda, typeName, offset)` on a `GraphReader`, but `ReadObjectList` only exists on `Reader` with a totally different signature (`ref TContainerType dest`, no lambda). Also calls `Instanciator.Create<T>(...)` — `Instanciator` is never defined anywhere in the repo. CS1061 + CS0246.
- [ ] **Nested-serializable reference field deserialize doesn't compile** — `SerializerGenerator.cs:224`. Assigns to a bare `{symbolName}` missing the `ret.` prefix every other branch uses, and calls `ReadObjectReference<T>(...)`, which doesn't exist on `GraphReader` or `Reader`. CS0103 + CS1061.
- [ ] **Unconstrained generic types produce a syntax error** — `Util.cs:66` (`MakeGenericConstraintString`). When a generic type's parameters have no constraints, the method still emits `where` with nothing after it, e.g. `Serialize<T>(in Box<T> value, GraphWriter writer) where`.

## Source generator — silent data bugs (no compile error)

- [ ] **Auto-implemented properties are silently dropped from serialization** — `Util.cs:124` (`IsFieldSerializable`). Rejects any field whose `DeclaredAccessibility != Public`; auto-property backing fields are always compiler-generated `private`, so every `public int Foo { get; set; }` field is excluded before the `AssociatedSymbol` handling (added specifically to support this case) is ever reached.
- [ ] **`[Ignore]` attribute has no effect** — `Util.cs:122` (`IsFieldSerializable`). Only checks `symbol.Type.GetAttributes()` (the field's type's attributes), never `symbol.GetAttributes()` (the field's own attributes) — so `[Ignore]` on a field is never seen, and the field is serialized anyway.

## Serialization runtime — dispatch is broken

- [ ] **Generic types (including all Silk.NET math types) can never be found by the serializer** — `Serializer.cs:98` (`GetFormattedFullName`). Three different type-name formats are used and never agree: `Serialize` uses a short-name format (`"Vector3D<Single>"`), `Deserialize<T>` uses `typeof(T).FullName` (CLR-mangled), and factories' `Supports()` check a namespace-qualified literal-`T` format (`"Silk.NET.Maths.Vector3D<T>"`). Always throws `SerializationException`.
- [ ] **`PrimitiveSerializationFactory` / `SilkMathSerializationFactory` always throw** — `PrimitiveSerializationFactory.cs:93` and `SilkMathSerializationFactory.cs:46,52`. `Serialize`/`Deserialize` unconditionally `throw new NotImplementedException()` even though `Supports()` advertises full support for `int`, `string`, `bool`, `Guid`, and every listed math type. Any `List<int>`/`List<Vector3D<float>>` field crashes serialization.

## Engine / scene

- [ ] **Scenes never activate — cameras never sync** — `Scene.cs:32` + `SceneModule.cs:101`. `Scene` defaults `IsActive = false`; `SceneModule.CreateScene` never passes `true`, and nothing else in the diff ever sets `scene.IsActive = true`. Every tick method early-returns on `!IsActive`, so `CameraSystem` (registered into `LateSimulationGroup`) never runs for any normally-created scene — the Godot viewport never gets a camera attached.
- [ ] **Hot reload crashes on first use** — `ExternalModuleManager.cs:86` (`PumpUnload`). Calls `GC.WaitForFullGCComplete()` without ever calling `GC.RegisterForFullGCNotification()` (required first, or it throws `InvalidOperationException`). `PumpUnload` runs every tick of `Application.Run()` once a reload is queued, so the first time a watched module DLL changes on disk, the app crashes on the next tick.
- [ ] **`GodotCameraHandle` isn't serializable — crashes hot reload** — `Components/GodotCameraHandle.cs:5`. Not marked `[DuckSerializable]`, no registered factory. A hot-reload world save walks every component via `Serializer.Serialize`, which throws as soon as it hits a camera entity's `GodotCameraHandle`.
- [ ] **Deserialized entities get the wrong `WorldId`** — `ArchReaderExtensions.cs:14` (`ReadEntity`). Hardcodes `WorldId` to `0` instead of using the world actually being deserialized into (contrast `WorldSerializationFactory.DeserializeEntity`, which does this correctly). Breaks as soon as more than one `World` exists in the process.

## Godot render system

- [ ] **Double-free on failed `Initialize()`** — `GodotPlatform.cs:45,56,64`. All three error paths call `libgodot_destroy_godot_instance` then throw, without resetting `_godotInstancePtr` to `Zero`. If a caller catches the exception and calls `Shutdown()` for cleanup, it destroys the same pointer a second time.
- [ ] **Undocumented module-registration ordering dependency** — `GodotPlatform.cs:75`. `Initialize()` calls `app.GetModule<GodotRenderModule>()`, which throws unless `GodotRenderModule` was already registered via `AddModule` first. Nothing enforces or documents this ordering requirement.

---

*Generated from an `/code-review` pass at xhigh effort (2026-07-11). Re-run
`/code-review` after fixes land to confirm.*
