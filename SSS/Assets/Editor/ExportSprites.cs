#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;

public class SpriteExporter : EditorWindow
{
    [MenuItem("Tools/Export All Sprites From Atlas")]
    public static void ExportAllSprites()
    {
        if (Selection.activeObject == null)
        {
            Debug.LogError("No atlas selected!");
            return;
        }


        string assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;

        // Внутри ExportFirstSprite (перед загрузкой спрайтов) — убедитесь, что TextureImporter настроен:
        if (!importer.isReadable || importer.textureCompression != TextureImporterCompression.Uncompressed || importer.mipmapEnabled)
        {
            importer.isReadable = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            // Не трогаем sRGBTexture здесь автоматически — делайте это сознательно если нужно.
            importer.SaveAndReimport();
        }

        if (importer == null || importer.spriteImportMode != SpriteImportMode.Multiple)
        {
            Debug.LogError("Selected object is not a sprite atlas!");
            return;
        }

        // Включаем Read/Write если нужно
        if (!importer.isReadable)
        {
            importer.isReadable = true;
            importer.SaveAndReimport();
        }

        string exportPath = EditorUtility.SaveFolderPanel("Select Export Folder", Application.dataPath, "");
        if (string.IsNullOrEmpty(exportPath))
        {
            Debug.Log("Export cancelled");
            return;
        }

        // Загружаем только первый спрайт
        Sprite[] allSprites = AssetDatabase.LoadAllAssetsAtPath(assetPath)
            .OfType<Sprite>()
            .ToArray();

        if (allSprites.Length == 0)
        {
            Debug.LogError("No sprites found in atlas!");
            return;
        }

        // Берем первый элемент массива
        Sprite firstSprite = allSprites[0];
        firstSprite.name = Selection.activeObject.name;
        for (int i = 0; i < allSprites.Length; i++)
        {
            ExportSingleSprite(allSprites[i], exportPath);

        }

        Debug.Log($"Successfully exported first sprite: {firstSprite.name} to: {exportPath}");
        EditorUtility.RevealInFinder(exportPath);
    }

    // Метод ExportSingleSprite остается без изменений
    static void ExportSingleSpriteL(Sprite sprite, string exportPath)
    {
        try
        {
            RenderTexture rt = RenderTexture.GetTemporary(
                sprite.texture.width,
                sprite.texture.height,
                0,
                RenderTextureFormat.Default,
                RenderTextureReadWrite.sRGB
            );

            Graphics.Blit(sprite.texture, rt);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = rt;

            Texture2D tex = new Texture2D(
                (int)sprite.rect.width,
                (int)sprite.rect.height,
                TextureFormat.RGBA32,
                false
            );

            tex.ReadPixels(new Rect(
                sprite.rect.x,
                sprite.texture.height - sprite.rect.y - sprite.rect.height,
                sprite.rect.width,
                sprite.rect.height
            ), 0, 0);

            tex.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(rt);

            byte[] pngData = tex.EncodeToPNG();
            string filePath = Path.Combine(exportPath, $"{sprite.name}.png");
            File.WriteAllBytes(filePath, pngData);

            Object.DestroyImmediate(tex);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to export sprite {sprite.name}: {e.Message}");
        }
    }

    static void ExportSingleSprite(Sprite sprite, string exportPath)
    {
        if (sprite == null) return;

        try
        {
            Texture2D src = sprite.texture;
            if (!src.isReadable)
            {
                Debug.LogError($"Texture for sprite {sprite.name} is not readable.");
                return;
            }

            int x = Mathf.FloorToInt(sprite.rect.x);
            int y = Mathf.FloorToInt(sprite.rect.y);
            int w = Mathf.FloorToInt(sprite.rect.width);
            int h = Mathf.FloorToInt(sprite.rect.height);

            // GetPixels возвращает массив цветов из заданного прямоугольника (в том же пространстве пикселей, что и texture)
            Color[] pixels = src.GetPixels(x, y, w, h);

            Texture2D outTex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            outTex.SetPixels(pixels);
            outTex.Apply(false, false); // no mipmaps, not marked as readable-only (false -> keep readable so we can Encode)

            byte[] pngData = outTex.EncodeToPNG();
            if (pngData != null && pngData.Length > 0)
            {
                string filePath = Path.Combine(exportPath, $"{sprite.name}.png");
                File.WriteAllBytes(filePath, pngData);
            }
            else
            {
                Debug.LogError($"Failed to encode sprite {sprite.name} to PNG (null/empty).");
            }

            Object.DestroyImmediate(outTex);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to export sprite {sprite.name}: {e.Message}");
        }
    }


}
#endif