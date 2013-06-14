using System;

namespace Frogger
{
    public interface IUserInterface
    {
        void ProcessInput();

        event EventHandler OnLeftPressed;

        event EventHandler OnRightPressed;

        event EventHandler OnUpPressed;

        event EventHandler OnDownPressed;

        event EventHandler OnPausePressed;

        event EventHandler OnExitPressed;
    }
}
