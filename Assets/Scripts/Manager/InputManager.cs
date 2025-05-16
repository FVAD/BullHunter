using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : StaticManagerBase<InputManager>
{
    public InputActions Actions { get; private set; }

    public InputManager()
    {
        Actions = new InputActions();
        Actions.InGame.Enable();
    }
}
