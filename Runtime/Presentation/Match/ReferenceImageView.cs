using System;
using _Project.Application.Configs;
using _Project.Presentation.UI;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace _Project.Presentation.Match
{
    [RequireComponent(typeof(RawImage))]
    public class ReferenceImageView : MonoBehaviour
    {
        private RawImage _image;
        private ReferenceConfig _config;

        [Inject]
        public void Construct(ReferenceConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _config = config;
        }

        private void Awake()
        {
            _image = GetComponent<RawImage>();
        }

        private void Start()
        {
            _image.texture = _config.Picture;
            float aspect = (float)_config.Picture.width / _config.Picture.height;
            new RectAspectFitter().FitInside((RectTransform)transform, aspect);
        }
    }
}
