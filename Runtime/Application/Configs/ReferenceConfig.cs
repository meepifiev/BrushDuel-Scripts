using _Project.Core.Drawing;
using _Project.Core.Match;
using UnityEngine;

namespace _Project.Application.Configs
{
    [CreateAssetMenu(fileName = "ReferenceConfig", menuName = "BrushDuel/Reference Config", order = 0)]
    public class ReferenceConfig : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private string _nameRussian;
        [SerializeField] private string _nameEnglish;
        [SerializeField] private Texture2D _picture;
        [SerializeField] private ReferenceBakedData _bakedData;
        [SerializeField] private float _roundDurationSeconds = 300f;
        [SerializeField] private Color32 _canvasColor = new Color32(255, 255, 255, 255);
        [SerializeField] private Color32[] _paletteColors;
        [SerializeField] private PaintingCategory _category = PaintingCategory.Famous;

        public PaintingCategory Category => _category;
        public string Id => _id;
        public string NameRussian => _nameRussian;
        public string NameEnglish => _nameEnglish;
        public Texture2D Picture => _picture;
        public ReferenceBakedData BakedData => _bakedData;
        public float RoundDurationSeconds => _roundDurationSeconds;

        public PaletteColor[] BuildPalette()
        {
            PaletteColor[] palette = new PaletteColor[_paletteColors.Length + 1];
            int paintColor = 0;

            for (int i = 0; i < palette.Length; i++)
            {
                if (i == CanvasSpec.BackgroundColorIndex)
                {
                    palette[i] = new PaletteColor(_canvasColor.r, _canvasColor.g, _canvasColor.b);
                    continue;
                }

                Color32 color = _paletteColors[paintColor];
                palette[i] = new PaletteColor(color.r, color.g, color.b);
                paintColor++;
            }

            return palette;
        }

        public ReferenceData ToData()
        {
            RgbColor[] palette = new RgbColor[_paletteColors.Length];

            for (int i = 0; i < palette.Length; i++)
            {
                palette[i] = new RgbColor { R = _paletteColors[i].r, G = _paletteColors[i].g, B = _paletteColors[i].b };
            }

            return new ReferenceData
            {
                Id = _id,
                RoundDurationSeconds = _roundDurationSeconds,
                SourceWidth = _bakedData.SourceWidth,
                SourceHeight = _bakedData.SourceHeight,
                GridIndices = _bakedData.GridIndices,
                CanvasIndices = _bakedData.CanvasIndices,
                CanvasColor = new RgbColor { R = _canvasColor.r, G = _canvasColor.g, B = _canvasColor.b },
                PaletteColors = palette,
            };
        }

        public static ReferenceConfig FromData(ReferenceData data)
        {
            ReferenceConfig config = CreateInstance<ReferenceConfig>();
            config._id = data.Id;
            config._roundDurationSeconds = data.RoundDurationSeconds;
            config._canvasColor = new Color32(data.CanvasColor.R, data.CanvasColor.G, data.CanvasColor.B, 255);

            Color32[] palette = new Color32[data.PaletteColors.Length];

            for (int i = 0; i < palette.Length; i++)
            {
                palette[i] = new Color32(data.PaletteColors[i].R, data.PaletteColors[i].G, data.PaletteColors[i].B, 255);
            }

            config._paletteColors = palette;

            ReferenceBakedData baked = CreateInstance<ReferenceBakedData>();
            baked.Setup(data.SourceWidth, data.SourceHeight, data.GridIndices, data.CanvasIndices);
            config._bakedData = baked;

            return config;
        }
    }
}
