using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using _Project.Application.Configs;
using UnityEditor;
using UnityEngine;

namespace _Project.Editor
{
    public static class PaletteExtractor
    {
        public class Result
        {
            public List<Color32> Colors = new List<Color32>();
            public List<float> Shares = new List<float>();
        }

        private const int KMeansIterations = 15;
        private const int SeedCandidates = 800;
        private const float MergeDistance = 9f;
        private const float MinSharePercent = 0.4f;
        private const float ShowMinDistance = 16f;

        public static void ExportAll()
        {
            string outDir = Environment.GetEnvironmentVariable("BRUSHDUEL_PALETTE_OUT");

            if (string.IsNullOrEmpty(outDir))
            {
                outDir = Path.Combine(Directory.GetCurrentDirectory(), "palettes");
            }

            Directory.CreateDirectory(outDir);
            string[] guids = AssetDatabase.FindAssets("t:ReferenceConfig");
            Array.Sort(guids, (a, b) => string.CompareOrdinal(AssetDatabase.GUIDToAssetPath(a), AssetDatabase.GUIDToAssetPath(b)));
            StringBuilder json = new StringBuilder();
            json.Append("{\n");
            int written = 0;

            foreach (string guid in guids)
            {
                ReferenceConfig config = AssetDatabase.LoadAssetAtPath<ReferenceConfig>(AssetDatabase.GUIDToAssetPath(guid));

                if (config == null || config.Picture == null)
                {
                    continue;
                }

                Result result = Extract(AssetDatabase.GetAssetPath(config.Picture), 32, 9);

                if (written > 0)
                {
                    json.Append(",\n");
                }

                json.Append("  \"" + config.name + "\": [");

                for (int i = 0; i < result.Colors.Count; i++)
                {
                    Color32 c = result.Colors[i];
                    json.Append((i > 0 ? ", " : "") + "[\"#" + c.r.ToString("X2") + c.g.ToString("X2") + c.b.ToString("X2") + "\", " + result.Shares[i].ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + "]");
                }

                json.Append("]");
                written++;
                Debug.Log("PaletteExtractor: " + config.name + " -> " + result.Colors.Count + " colors");
            }

            json.Append("\n}\n");
            File.WriteAllText(Path.Combine(outDir, "palettes.json"), json.ToString());
            Debug.Log("PaletteExtractor: exported " + written + " palettes to " + outDir);
        }

        public static Result Extract(string picturePath, int maxColors, int showCount)
        {
            Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            tex.LoadImage(File.ReadAllBytes(picturePath));
            Color32[] px = tex.GetPixels32();
            UnityEngine.Object.DestroyImmediate(tex);

            List<Vector3> samples = new List<Vector3>();
            List<Color32> rgbs = new List<Color32>();
            int step = Mathf.Max(1, px.Length / 60000);

            for (int i = 0; i < px.Length; i += step)
            {
                if (px[i].a < 128)
                {
                    continue;
                }

                samples.Add(ToLab(px[i]));
                rgbs.Add(px[i]);
            }

            int k = maxColors;
            System.Random rnd = new System.Random(7);
            List<Vector3> cent = new List<Vector3> { samples[rnd.Next(samples.Count)] };

            while (cent.Count < k)
            {
                float best = -1f;
                Vector3 pick = cent[0];

                for (int t = 0; t < SeedCandidates; t++)
                {
                    Vector3 s = samples[rnd.Next(samples.Count)];
                    float dm = float.MaxValue;

                    foreach (Vector3 c in cent)
                    {
                        float d = (s - c).sqrMagnitude;
                        if (d < dm) dm = d;
                    }

                    if (dm > best)
                    {
                        best = dm;
                        pick = s;
                    }
                }

                cent.Add(pick);
            }

            Vector3[] acc = new Vector3[k];
            int[] cnt = new int[k];
            int[] assign = new int[samples.Count];

            for (int it = 0; it < KMeansIterations; it++)
            {
                Array.Clear(acc, 0, k);
                Array.Clear(cnt, 0, k);

                for (int i = 0; i < samples.Count; i++)
                {
                    int bi = 0;
                    float bd = float.MaxValue;

                    for (int j = 0; j < k; j++)
                    {
                        float d = (samples[i] - cent[j]).sqrMagnitude;
                        if (d < bd) { bd = d; bi = j; }
                    }

                    assign[i] = bi;
                    acc[bi] += samples[i];
                    cnt[bi]++;
                }

                for (int j = 0; j < k; j++)
                {
                    if (cnt[j] > 0) cent[j] = acc[j] / cnt[j];
                }
            }

            Vector3[] rgbAcc = new Vector3[k];

            for (int i = 0; i < samples.Count; i++)
            {
                rgbAcc[assign[i]] += new Vector3(rgbs[i].r, rgbs[i].g, rgbs[i].b);
            }

            List<Cluster> clusters = new List<Cluster>();

            for (int j = 0; j < k; j++)
            {
                if (cnt[j] == 0) continue;
                Vector3 v = rgbAcc[j] / cnt[j];
                clusters.Add(new Cluster { Rgb = new Color32((byte)v.x, (byte)v.y, (byte)v.z, 255), Lab = cent[j], Count = cnt[j] });
            }

            clusters.Sort((a, b) => b.Count.CompareTo(a.Count));
            List<Cluster> merged = new List<Cluster>();

            foreach (Cluster c in clusters)
            {
                bool dup = false;

                foreach (Cluster m in merged)
                {
                    if ((c.Lab - m.Lab).magnitude < MergeDistance) { dup = true; break; }
                }

                if (dup == false) merged.Add(c);
            }

            int total = samples.Count;
            merged.RemoveAll(c => 100f * c.Count / total < MinSharePercent);

            List<Cluster> show = new List<Cluster>();
            List<Cluster> pool = new List<Cluster>(merged);

            while (show.Count < showCount - 2 && pool.Count > 0)
            {
                int bestIdx = -1;
                float bestScore = -1f;

                for (int i = 0; i < pool.Count; i++)
                {
                    float dmin = MinDistance(pool[i], show);
                    if (show.Count > 0 && dmin < ShowMinDistance) continue;
                    float score = pool[i].Count;
                    if (score > bestScore) { bestScore = score; bestIdx = i; }
                }

                if (bestIdx < 0) break;
                show.Add(pool[bestIdx]);
                pool.RemoveAt(bestIdx);
            }

            while (show.Count < showCount && pool.Count > 0)
            {
                int bestIdx = -1;
                float bestScore = -1f;

                for (int i = 0; i < pool.Count; i++)
                {
                    float dmin = MinDistance(pool[i], show);
                    if (dmin < ShowMinDistance) continue;
                    float sat = Mathf.Sqrt(pool[i].Lab.y * pool[i].Lab.y + pool[i].Lab.z * pool[i].Lab.z);
                    float score = sat * Mathf.Min(dmin, 50f) * Mathf.Log(1f + pool[i].Count);
                    if (score > bestScore) { bestScore = score; bestIdx = i; }
                }

                if (bestIdx < 0) break;
                show.Add(pool[bestIdx]);
                pool.RemoveAt(bestIdx);
            }

            pool.Sort((a, b) => b.Count.CompareTo(a.Count));
            Result result = new Result();

            foreach (Cluster s in show) { result.Colors.Add(s.Rgb); result.Shares.Add(100f * s.Count / total); }
            foreach (Cluster p in pool) { result.Colors.Add(p.Rgb); result.Shares.Add(100f * p.Count / total); }

            return result;
        }

        private class Cluster
        {
            public Color32 Rgb;
            public Vector3 Lab;
            public int Count;
        }

        private static float MinDistance(Cluster c, List<Cluster> others)
        {
            float dmin = float.MaxValue;
            foreach (Cluster s in others) dmin = Mathf.Min(dmin, (c.Lab - s.Lab).magnitude);
            return dmin;
        }

        private static Vector3 ToLab(Color32 c)
        {
            float r = Lin(c.r / 255f), g = Lin(c.g / 255f), b = Lin(c.b / 255f);
            float x = (0.4124f * r + 0.3576f * g + 0.1805f * b) / 0.95047f;
            float y = 0.2126f * r + 0.7152f * g + 0.0722f * b;
            float z = (0.0193f * r + 0.1192f * g + 0.9505f * b) / 1.08883f;
            x = F(x); y = F(y); z = F(z);
            return new Vector3(116f * y - 16f, 500f * (x - y), 200f * (y - z));
        }

        private static float Lin(float c) { return c <= 0.04045f ? c / 12.92f : Mathf.Pow((c + 0.055f) / 1.055f, 2.4f); }
        private static float F(float t) { return t > 0.008856f ? Mathf.Pow(t, 1f / 3f) : 7.787f * t + 16f / 116f; }
    }
}
