using UnityEngine;
using WordSpinAlpha.Core;

namespace WordSpinAlpha.Presentation
{
    public class GameSceneNavigationButtons : MonoBehaviour
    {
        public void OpenMainMenu()
        {
            SceneNavigator.Instance?.OpenMainMenu();
        }

        public void OpenStore()
        {
            SceneNavigator.Instance?.OpenStore();
        }
    }
}
