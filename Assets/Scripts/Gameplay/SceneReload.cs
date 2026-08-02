using UnityEngine;
using UnityEngine.SceneManagement;

namespace Alien.Gameplay
{
    public static class SceneReload
    {
        private static bool isReloading;

        public static void ReloadCurrentScene()
        {
            if (isReloading) return;

            isReloading = true;
            Time.timeScale = 1f;
            SceneManager.sceneLoaded += ResetReloading;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private static void ResetReloading(Scene _, LoadSceneMode __)
        {
            SceneManager.sceneLoaded -= ResetReloading;
            isReloading = false;
        }
    }
}
