using System.IO;
using _Project.Application.Configs;
using _Project.Core.Drawing;
using _Project.Core.Scoring;
using UnityEditor;
using UnityEngine;

namespace _Project.Editor
{
    public class ReferenceBakeTool
    {
        private const string BakedFolder = "Assets/_Project/Configs/References/Baked";
        private const int ColorMapMaxSide = 256;
        private const int ColorMapChannels = 3;

        [MenuItem("BrushDuel/Bake Reference Data")]
        public static void BakeAll()
        {
            if (AssetDatabase.IsValidFolder(BakedFolder) == false)
            {
                AssetDatabase.CreateFolder("Assets/_Project/Configs/References", "Baked");
            }

            string[] guids = AssetDatabase.FindAssets("t:ReferenceConfig");

            foreach (string guid in guids)
            {
                string configPath = AssetDatabase.GUIDToAssetPath(guid);
                ReferenceConfig config = AssetDatabase.LoadAssetAtPath<ReferenceConfig>(configPath);
                Bake(config);
            }

            AssetDatabase.SaveAssets();
            Debug.Log(nameof(ReferenceBakeTool) + ": baked " + guids.Length + " references");
        }

        private static void Bake(ReferenceConfig config)
        {
            string picturePath = AssetDatabase.GetAssetPath(config.Picture);
            Texture2D source = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            source.LoadImage(File.ReadAllBytes(picturePath));

            Color32[] pixels = source.GetPixels32();
            byte[] rgba = new byte[pixels.Length * CanvasSpec.BytesPerPixel];

            for (int i = 0; i < pixels.Length; i++)
            {
                int offset = i * CanvasSpec.BytesPerPixel;
                rgba[offset] = pixels[i].r;
                rgba[offset + 1] = pixels[i].g;
                rgba[offset + 2] = pixels[i].b;
                rgba[offset + 3] = pixels[i].a;
            }

            PaletteColor[] palette = config.BuildPalette();
            CanvasSize canvasSize = new CanvasSize(source.width, source.height);

            AnalysisGrid grid = new AnalysisGrid(canvasSize);
            ImageSnapshot snapshot = new ImageSnapshot(grid);
            SnapshotBuilder snapshotBuilder = new SnapshotBuilder(new PaletteQuantizer());
            snapshotBuilder.Build(snapshot, rgba, source.width, source.height, palette);

            byte[] gridIndices = new byte[grid.CellCount];

            for (int i = 0; i < grid.CellCount; i++)
            {
                gridIndices[i] = (byte)snapshot.PaletteIndices[i];
            }

            ReferenceCanvasMap map = new ReferenceCanvasMap(canvasSize);
            ReferenceCanvasMapBuilder mapBuilder = new ReferenceCanvasMapBuilder(new PaletteQuantizer(), canvasSize);
            mapBuilder.Build(map, rgba, source.width, source.height, palette);

            string bakedPath = BakedFolder + "/" + config.name + "Baked.asset";
            ReferenceBakedData baked = AssetDatabase.LoadAssetAtPath<ReferenceBakedData>(bakedPath);

            if (baked == null)
            {
                baked = ScriptableObject.CreateInstance<ReferenceBakedData>();
                AssetDatabase.CreateAsset(baked, bakedPath);
            }

            baked.Setup(source.width, source.height, gridIndices, map.Indices);
            BakeColorMap(baked, rgba, source.width, source.height);
            EditorUtility.SetDirty(baked);

            SerializedObject configSo = new SerializedObject(config);
            configSo.FindProperty("_bakedData").objectReferenceValue = baked;
            configSo.ApplyModifiedPropertiesWithoutUndo();

            Object.DestroyImmediate(source);
        }

        private static void BakeColorMap(ReferenceBakedData baked, byte[] rgba, int sourceWidth, int sourceHeight)
        {
            float scale = Mathf.Min(1f, (float)ColorMapMaxSide / Mathf.Max(sourceWidth, sourceHeight));
            int width = Mathf.Max(1, Mathf.RoundToInt(sourceWidth * scale));
            int height = Mathf.Max(1, Mathf.RoundToInt(sourceHeight * scale));
            byte[] colorMap = new byte[width * height * ColorMapChannels];

            for (int y = 0; y < height; y++)
            {
                int sourceY = Mathf.Clamp((int)((y + 0.5f) * sourceHeight / height), 0, sourceHeight - 1);

                for (int x = 0; x < width; x++)
                {
                    int sourceX = Mathf.Clamp((int)((x + 0.5f) * sourceWidth / width), 0, sourceWidth - 1);
                    int sourceOffset = (sourceY * sourceWidth + sourceX) * CanvasSpec.BytesPerPixel;
                    int targetOffset = (y * width + x) * ColorMapChannels;

                    colorMap[targetOffset] = rgba[sourceOffset];
                    colorMap[targetOffset + 1] = rgba[sourceOffset + 1];
                    colorMap[targetOffset + 2] = rgba[sourceOffset + 2];
                }
            }

            baked.SetupColorMap(width, height, colorMap);
        }
    }
}
