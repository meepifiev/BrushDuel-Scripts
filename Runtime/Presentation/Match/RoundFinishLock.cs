using _Project.Application.Match;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace _Project.Presentation.Match
{
    public class RoundFinishLock : MonoBehaviour
    {
        [SerializeField] private Button[] _disableOnFinish;

        private IRoundTimeline _timeline;

        [Inject]
        public void Construct(IRoundTimeline timeline)
        {
            _timeline = timeline;
        }

        private void Start()
        {
            _timeline.Finished += OnFinished;
        }

        private void OnDestroy()
        {
            if (_timeline != null)
            {
                _timeline.Finished -= OnFinished;
            }
        }

        private void OnFinished()
        {
            for (int i = 0; i < _disableOnFinish.Length; i++)
            {
                if (_disableOnFinish[i] != null)
                {
                    _disableOnFinish[i].interactable = false;
                }
            }
        }
    }
}
