using UnityEngine;
using UnityEngine.Playables;

namespace Alien.Gameplay
{
    [RequireComponent(typeof(PlayableDirector))]
    public sealed class ReloadSceneOnDirectorStopped : MonoBehaviour
    {
        private PlayableDirector playableDirector;

        private void Awake()
        {
            playableDirector = GetComponent<PlayableDirector>();
        }

        private void OnEnable()
        {
            playableDirector.stopped += ReloadScene;
        }

        private void OnDisable()
        {
            playableDirector.stopped -= ReloadScene;
        }

        private void ReloadScene(PlayableDirector _)
        {
            playableDirector.stopped -= ReloadScene;
            SceneReload.ReloadCurrentScene();
        }
    }
}
