using System;
using _Project.Application.Drawing;
using _Project.Core.Drawing;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace _Project.Presentation.Drawing.Toolbar
{
    [RequireComponent(typeof(Slider))]
    public class BrushSizeSlider : MonoBehaviour
    {
        private Slider _slider;
        private DrawingSessionService _session;

        [Inject]
        public void Construct(DrawingSessionService session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        private void Awake()
        {
            _slider = GetComponent<Slider>();
            _slider.wholeNumbers = true;
            _slider.minValue = 0;
            _slider.maxValue = BrushSpec.SizeSteps - 1;
        }

        private void OnEnable()
        {
            _slider.onValueChanged.AddListener(OnValueChanged);
            _session.ToolStateChanged += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            _slider.onValueChanged.RemoveListener(OnValueChanged);
            _session.ToolStateChanged -= Refresh;
        }

        private void OnValueChanged(float value)
        {
            int index = Mathf.Clamp(Mathf.RoundToInt(value), 0, BrushSpec.SizeSteps - 1);

            if (index != _session.ActiveSizeIndex)
            {
                _session.SelectSize(index);
            }
        }

        private void Refresh()
        {
            _slider.SetValueWithoutNotify(_session.ActiveSizeIndex);
        }
    }
}
