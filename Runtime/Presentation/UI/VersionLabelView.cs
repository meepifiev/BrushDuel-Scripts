using _Project.Application.Configs;
using TMPro;
using UnityEngine;
using VContainer;

namespace _Project.Presentation.UI
{
    public class VersionLabelView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _label;

        [Inject]
        public void Construct(VersionConfig config)
        {
            _label.text = "v" + config.Version;
        }
    }
}
