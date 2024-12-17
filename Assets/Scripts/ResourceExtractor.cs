using UnityEngine;
using UnityEditor;
using System.IO;

public class PrefabExtractorWithMesh : MonoBehaviour
{
    [MenuItem("Tools/Extract Materials, Textures, FBX, and Meshes")]
    static void ExtractPrefabDependencies()
    {
        // 获取选中的 Prefab
        GameObject selectedPrefab = Selection.activeGameObject;
        if (selectedPrefab == null)
        {
            Debug.LogError("请先选择一个 Prefab 或 GameObject！");
            return;
        }

        // 定义导出目录
        string materialsFolder = "Assets/Materials";
        string texturesFolder = "Assets/Textures";
        string modelsFolder = "Assets/Models";
        string meshesFolder = "Assets/Meshes";

        // 确保文件夹存在
        EnsureFolderExists(materialsFolder);
        EnsureFolderExists(texturesFolder);
        EnsureFolderExists(modelsFolder);
        EnsureFolderExists(meshesFolder);

        // 遍历 Prefab 的所有子对象，提取材质和纹理
        Renderer[] renderers = selectedPrefab.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            foreach (Material material in renderer.sharedMaterials)
            {
                if (material == null) continue;

                // 提取材质
                string materialPath = $"{materialsFolder}/{material.name}_Extracted.mat";
                Material newMaterial = new Material(material);
                AssetDatabase.CreateAsset(newMaterial, materialPath);

                // 提取纹理
                if (material.mainTexture != null)
                {
                    Texture texture = material.mainTexture;
                    string texturePath = $"{texturesFolder}/{texture.name}_Extracted{Path.GetExtension(AssetDatabase.GetAssetPath(texture))}";
                    AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(texture), texturePath);
                    Texture newTexture = AssetDatabase.LoadAssetAtPath<Texture>(texturePath);

                    // 将新纹理绑定到新材质
                    newMaterial.mainTexture = newTexture;
                }

                Debug.Log($"提取完成：材质 {material.name} 和纹理 {material.mainTexture?.name}");
            }
        }

        // 提取 FBX 文件和独立 Mesh
        ExtractFBXDependencies(selectedPrefab, modelsFolder, meshesFolder);

        // 保存更改
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("所有材质、纹理、FBX 和 Mesh 提取完成！");
    }

    static void ExtractFBXDependencies(GameObject prefab, string modelsFolder, string meshesFolder)
    {
        MeshFilter[] meshFilters = prefab.GetComponentsInChildren<MeshFilter>();
        SkinnedMeshRenderer[] skinnedRenderers = prefab.GetComponentsInChildren<SkinnedMeshRenderer>();
        MeshCollider[] meshColliders = prefab.GetComponentsInChildren<MeshCollider>();

        // 提取 MeshFilter 中的 Mesh
        foreach (MeshFilter meshFilter in meshFilters)
        {
            if (meshFilter.sharedMesh == null) continue;

            ExtractMesh(meshFilter.sharedMesh, meshesFolder, meshFilter.gameObject.name);
        }

        // 提取 SkinnedMeshRenderer 中的 Mesh
        foreach (SkinnedMeshRenderer skinnedRenderer in skinnedRenderers)
        {
            if (skinnedRenderer.sharedMesh == null) continue;

            ExtractMesh(skinnedRenderer.sharedMesh, meshesFolder, skinnedRenderer.gameObject.name);
        }

        // 提取 MeshCollider 中的 Mesh
        foreach (MeshCollider meshCollider in meshColliders)
        {
            if (meshCollider.sharedMesh == null) continue;

            ExtractMesh(meshCollider.sharedMesh, meshesFolder, meshCollider.gameObject.name + "_Collider");
        }
    }

    static void ExtractMesh(Mesh mesh, string meshesFolder, string objectName)
    {
        string meshPath = $"{meshesFolder}/{objectName}_{mesh.name}_Extracted.asset";

        // 检查是否已存在，避免重复
        if (!File.Exists(meshPath))
        {
            Mesh newMesh = Object.Instantiate(mesh);
            AssetDatabase.CreateAsset(newMesh, meshPath);
            Debug.Log($"提取完成：Mesh {mesh.name} 保存为 {meshPath}");
        }
        else
        {
            Debug.Log($"Mesh {mesh.name} 已存在，跳过提取。");
        }
    }

    static void EnsureFolderExists(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string parentFolder = Path.GetDirectoryName(path);
            string folderName = Path.GetFileName(path);
            AssetDatabase.CreateFolder(parentFolder, folderName);
        }
    }
}