using System;
using _Project.Application.Configs;
using _Project.Application.Drawing;
using _Project.Core.Drawing;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VContainer;

namespace _Project.Presentation.Match
{
    [RequireComponent(typeof(RawImage))]
    public class ReferencePickerView : MonoBehaviour, IPointerDownHandler
    {
        private const int ColorMapChannels = 3;

        private DrawingSessionService _session;
        private ReferenceConfig _reference;

        [Inject]
        public void Construct(DrawingSessionService session, ReferenceConfig reference)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _reference = reference ?? throw new ArgumentNullException(nameof(reference));
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_session.ActiveToolMode != ToolMode.ColorPicker || _session.IsDrawingLocked)
            {
                return;
            }

            PickColor(eventData);
        }

        private void PickColor(PointerEventData eventData)
        {
            ReferenceBakedData baked = _reference.BakedData;

            if (baked.ColorMap == null || baked.ColorMap.Length == 0)
            {
                return;
            }

            RectTransform rectTransform = (RectTransform)transform;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out Vector2 local);
            Rect rect = rectTransform.rect;
            float u = Mathf.Clamp01((local.x - rect.xMin) / rect.width);
            float v = Mathf.Clamp01((local.y - rect.yMin) / rect.height);

            int x = Mathf.Clamp(Mathf.FloorToInt(u * baked.ColorMapWidth), 0, baked.ColorMapWidth - 1);
            int y = Mathf.Clamp(Mathf.FloorToInt(v * baked.ColorMapHeight), 0, baked.ColorMapHeight - 1);
            int offset = (y * baked.ColorMapWidth + x) * ColorMapChannels;

            _session.SelectPickedColor(new PaletteColor(baked.ColorMap[offset], baked.ColorMap[offset + 1], baked.ColorMap[offset + 2]));
        }
    }
}
