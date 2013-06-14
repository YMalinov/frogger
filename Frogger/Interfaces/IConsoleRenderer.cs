namespace Frogger
{
    public interface IConsoleRenderer
    {
        void EnqueueForRendering(GameObject obj);
        
        void RenderAll(int lives = 0, int score = 0);

        void ClearQueue();
    }
}
