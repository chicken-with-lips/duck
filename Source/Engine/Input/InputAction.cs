using Duck.Input.Exception;

namespace Duck.Input;

public class InputAction
{
    public string Name { get; }

    public InputActionBinding[] Bindings => _bindings.ToArray();
    public bool IsActivated { get; set; }
    public float Value => IsActivated ? 1.0f : 0.0f;

    private readonly List<InputActionBinding> _bindings = new();

    internal InputAction(string name, InputActionBinding[] bindings)
    {
        Name = name;
        _bindings.AddRange(bindings);
    }
}

public class InputActionBinding
{
    public InputName Name { get; }

    public InputActionBinding(InputName name)
    {
        Name = name;
    }
}

public class InputActionBuilder
{
    private string _name;
    private readonly List<InputActionBinding> _bindings = new();

    public InputActionBuilder WithName(string name)
    {
        _name = name;

        return this;
    }

    public InputActionBuilder AddBinding(InputName input)
    {
        _bindings.Add(
            new InputActionBinding(input)
        );

        return this;
    }

    public void Build(IInputModule input)
    {
        if (string.IsNullOrEmpty(_name)) {
            throw new MissingNameException();
        }

        input.Register(
            new InputAction(_name, _bindings.ToArray())
        );
    }

    public static InputActionBuilder Create()
    {
        return new();
    }
}
