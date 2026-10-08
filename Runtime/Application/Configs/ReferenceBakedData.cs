using UnityEngine;

namespace _Project.Application.Configs
{
    public class ReferenceBakedData : ScriptableObject
    {
        [SerializeField] private int _sourceWidth;
        [SerializeField] private int _sourceHeight;
        [SerializeField] private byte[] _gridIndices;
        [SerializeField] private byte[] _canvasIndices;
        [SerializeField] private int _colorMapWidth;
        [SerializeField] private int _colorMapHeight;
        [SerializeField] private byte[] _colorMap;

        public int SourceWidth => _sourceWidth;
        public int SourceHeight => _sourceHeight;
        public byte[] GridIndices => _gridIndices;
        public byte[] CanvasIndices => _canvasIndices;
        public int ColorMapWidth => _colorMapWidth;
        public int ColorMapHeight => _colorMapHeight;
        public byte[] ColorMap => _colorMap;

        public void Setup(int sourceWidth, int sourceHeight, byte[] gridIndices, byte[] canvasIndices)
        {
            _sourceWidth = sourceWidth;
            _sourceHeight = sourceHeight;
            _gridIndices = gridIndices;
            _canvasIndices = canvasIndices;
        }

        public void SetupColorMap(int width, int height, byte[] colorMap)
        {
            _colorMapWidth = width;
            _colorMapHeight = height;
            _colorMap = colorMap;
        }
    }
}
