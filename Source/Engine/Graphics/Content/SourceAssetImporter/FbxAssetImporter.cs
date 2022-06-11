using Duck.Content;
using Duck.Content.SourceAssetImporter;
using Duck.Graphics.Device;
using Duck.Graphics.Mesh;
using Duck.Graphics.Shaders;
using Silk.NET.Assimp;
using Silk.NET.Maths;

namespace Duck.Graphics.Content.SourceAssetImporter;

public class FbxAssetImporter : SourceAssetImporterBase<StaticMesh>
{
    private readonly AssetReference<ShaderProgram> _defaultShader;

    public FbxAssetImporter(AssetReference<ShaderProgram> defaultShader)
    {
        _defaultShader = defaultShader;
    }

    public override bool CanImport(string file)
    {
        return Path.GetExtension(file).ToLower() == ".fbx";
    }

    public override unsafe StaticMesh? Import(string file)
    {
        var ai = Assimp.GetApi();
        var scene = ai.ImportFile(file, (uint)(PostProcessPreset.TargetRealTimeMaximumQuality | PostProcessSteps.Triangulate | PostProcessSteps.FlipUVs));

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
            new AssetImportData(new Uri("memory://" + file)),
            new BufferObject<TexturedVertex>(vertices.ToArray()),
            new BufferObject<uint>(indices.ToArray()),
            _defaultShader
        );
    }
}
