namespace TirexGame.Utils.LoadingScene
{
    /// <summary>
    /// Factory class để tạo ra scene loading steps.
    /// Đơn giản hóa việc tạo các SceneLoadingStep.
    /// </summary>
    public static class LoadingStepFactory
    {
        /// <summary>
        /// Tạo scene loading step
        /// </summary>
        /// <param name="sceneName">Tên scene</param>
        /// <param name="weight">Trọng số</param>
        /// <returns>SceneLoadingStep instance</returns>
        public static SceneLoadingStep CreateSceneLoad(string sceneName, float weight = 2f)
        {
            return SceneLoadingStep.LoadScene(sceneName, weight);
        }
        
        /// <summary>
        /// Tạo scene loading step với build index
        /// </summary>
        /// <param name="sceneIndex">Build index của scene</param>
        /// <param name="weight">Trọng số</param>
        /// <returns>SceneLoadingStep instance</returns>
        public static SceneLoadingStep CreateSceneLoad(int sceneIndex, float weight = 2f)
        {
            return SceneLoadingStep.LoadScene(sceneIndex, weight);
        }
    }
}