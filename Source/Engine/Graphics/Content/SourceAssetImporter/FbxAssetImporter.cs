using Duck.Content;
using Duck.Content.SourceAssetImporter;
using Duck.Graphics.Device;
using Duck.Graphics.Mesh;
using Silk.NET.Assimp;
using Silk.NET.Maths;
using Material = Duck.Graphics.Materials.Material;

namespace Duck.Graphics.Content.SourceAssetImporter;

public class FbxAssetImporter : SourceAssetImporterBase<StaticMesh>
{
    private readonly string _contentDirectory;
    private readonly AssetReference<Material> _fallbackMaterial;

    public FbxAssetImporter(string contentDirectory, AssetReference<Material> fallbackMaterial)
    {
        _contentDirectory = contentDirectory;
        _fallbackMaterial = fallbackMaterial;
    }

    public override bool CanImport(string file)
    {
        return Path.GetExtension(file).ToLower() == ".fbx";
    }

    public override unsafe StaticMesh Import(string file)
    {
        Console.WriteLine(Path.Combine(_contentDirectory, file));
        var ai = Assimp.GetApi();
        var scene = ai.ImportFile(Path.Combine(_contentDirectory, file), (uint)(
            PostProcessPreset.TargetRealTimeMaximumQuality
            | PostProcessSteps.Triangulate
            | PostProcessSteps.FlipUVs
            | PostProcessSteps.JoinIdenticalVertices
            // | PostProcessSteps.GenerateNormals
            | PostProcessSteps.OptimizeGraph
            | PostProcessSteps.OptimizeMeshes
            | PostProcessSteps.ImproveCacheLocality
        ));

        if (scene == null) {
            throw new Exception("FIXME: errors");
        }

        // TODO: handle multiple meshes

        var vertices = new List<TexturedVertex>();
        var indices = new List<uint>();

        for (var vertIndex = 0; vertIndex < scene->MMeshes[0]->MNumVertices; vertIndex++) {
            vertices.Add(
                new TexturedVertex(
                    scene->MMeshes[0]->MVertices[vertIndex].ToGeneric(),
                    scene->MMeshes[0]->MNormals[vertIndex].ToGeneric(),
                    new Vector2D<float>()
                )
            );
            // vertices.Add(
            //     new Vertex(
            //         scene->MMeshes[0]->MVertices[vertIndex].ToGeneric(),
            //         scene->MMeshes[0]->MNormals[vertIndex].ToGeneric(),
            //         new Vector2D<float>(
            //             scene->MMeshes[0]->MTextureCoords[vertIndex]->X,
            //             scene->MMeshes[0]->MTextureCoords[vertIndex]->Y
            //         )
            //     )
            // );
        }

        for (var faceIndex = 0; faceIndex < scene->MMeshes[0]->MNumFaces; faceIndex++) {
            for (var idxIndex = 0; idxIndex < scene->MMeshes[0]->MFaces[faceIndex].MNumIndices; idxIndex++) {
                indices.Add(scene->MMeshes[0]->MFaces[faceIndex].MIndices[idxIndex]);
            }
        }

        var b = scene->MMeshes[0]->MAABB;

        return new StaticMesh(
            new AssetImportData(new Uri("file:///" + file)),
            new BufferObject<TexturedVertex>(vertices.ToArray()),
            new BufferObject<uint>(indices.ToArray()),
            _fallbackMaterial
        );
    }
}
