using DG.Tweening;
using TMPro;
using UnityEngine;

namespace _Project.Presentation.UI
{
    [RequireComponent(typeof(TMP_Text))]
    public class LoadingDotsAnimation : MonoBehaviour
    {
        private const float QuadCenterFactor = 0.5f;

        [SerializeField] private float _dotInterval = 0.22f;
        [SerializeField] private float _appearDuration = 0.3f;
        [SerializeField] private float _holdDuration = 0.7f;
        [SerializeField] private Ease _appearEase = Ease.OutBack;
        [SerializeField] private float _appearOvershoot = 2f;

        private TMP_Text _text;
        private float _time;

        private void Awake()
        {
            _text = GetComponent<TMP_Text>();
        }

        private void OnEnable()
        {
            _time = 0f;
        }

        private void Update()
        {
            _text.ForceMeshUpdate();
            TMP_TextInfo textInfo = _text.textInfo;
            int visibleCount = CountVisibleCharacters(textInfo);

            if (visibleCount == 0)
            {
                return;
            }

            float cycleDuration = visibleCount * _dotInterval + _appearDuration + _holdDuration;
            _time += Time.deltaTime;

            if (_time > cycleDuration)
            {
                _time -= cycleDuration;
            }

            int visibleIndex = 0;

            for (int i = 0; i < textInfo.characterCount; i++)
            {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

                if (charInfo.isVisible == false)
                {
                    continue;
                }

                float appearStart = visibleIndex * _dotInterval;
                float progress = Mathf.Clamp01((_time - appearStart) / _appearDuration);
                float easedScale = DOVirtual.EasedValue(0f, 1f, progress, _appearEase, _appearOvershoot, 0f);
                ApplyCharacter(textInfo, charInfo, easedScale, progress);
                visibleIndex++;
            }

            _text.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
        }

        private int CountVisibleCharacters(TMP_TextInfo textInfo)
        {
            int count = 0;

            for (int i = 0; i < textInfo.characterCount; i++)
            {
                if (textInfo.characterInfo[i].isVisible)
                {
                    count++;
                }
            }

            return count;
        }

        private void ApplyCharacter(TMP_TextInfo textInfo, TMP_CharacterInfo charInfo, float scale, float alpha)
        {
            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;
            Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;
            Color32[] colors = textInfo.meshInfo[materialIndex].colors32;
            Vector3 center = (vertices[vertexIndex] + vertices[vertexIndex + 2]) * QuadCenterFactor;

            for (int i = 0; i < 4; i++)
            {
                vertices[vertexIndex + i] = center + (vertices[vertexIndex + i] - center) * scale;
                Color32 color = colors[vertexIndex + i];
                color.a = (byte)(color.a * alpha);
                colors[vertexIndex + i] = color;
            }
        }
    }
}
