using ChickenWithLips.RmlUi;
using Silk.NET.Maths;

namespace Duck.Ui.RmlUi;

public class RmlContext
{
    #region Properties

    public bool ShouldReceiveInput {
        get => _shouldReceiveInput;
        set {
            if (_shouldReceiveInput && !value) {
                _wasReceivingInput = true;
            }

            _shouldReceiveInput = value;
        }
    }

    internal Context Context => _context;

    #endregion

    #region Members

    private readonly Context _context;


    private bool _shouldReceiveInput = false;
    private bool _wasReceivingInput = false;

    private bool _wasLeftButtonPressed = false;
    private bool _wasRightButtonPressed = false;

    #endregion

    public RmlContext(Context context)
    {
        _context = context;
    }

    public void Tick()
    {
        if (_wasReceivingInput && !_shouldReceiveInput) {
            _context.ProcessMouseLeave();
            _wasReceivingInput = false;
        }

        _context.Update();
    }

    public void Render()
    {
        _context.Render();
    }

    public void InjectMouseInput(Vector2D<int> position, bool leftButtonPressed, bool rightButtonPressed)
    {
        // TODO: pass in mouse state
        // TODO: track previous state in input system

        _context.ProcessMouseMove(position.X, position.Y, 0);

        if (_wasLeftButtonPressed && !leftButtonPressed) {
            _context.ProcessMouseButtonUp(0, 0);
            _wasLeftButtonPressed = false;
        }

        if (!_wasLeftButtonPressed && leftButtonPressed) {
            _context.ProcessMouseButtonDown(0, 0);
            _wasLeftButtonPressed = true;
        }

        if (_wasRightButtonPressed && !rightButtonPressed) {
            _context.ProcessMouseButtonUp(1, 0);
            _wasRightButtonPressed = false;
        }

        if (!_wasRightButtonPressed && leftButtonPressed) {
            _context.ProcessMouseButtonDown(1, 0);
            _wasRightButtonPressed = true;
        }
    }
}
