using Duck.Input.Exception;

namespace Duck.Input;

public class InputAxis
{
    public string Name => _name;
    public InputAxisBinding[] Bindings => _bindings.ToArray();
    public float Value { get; set; }
    public float RawValue { get; set; }

    private readonly string _name;
    private readonly List<InputAxisBinding> _bindings = new();

    internal InputAxis(string name, InputAxisBinding[] bindings)
    {
        _name = name;
        _bindings.AddRange(bindings);
    }
}

public class InputAxisBinding
{
    public InputName Name { get; }
    public float Scale { get; }
    public float Value { get; set; }

    public InputAxisBinding(InputName name, float scale)
    {
        Name = name;
        Scale = scale;
    }
}

public class InputAxisBuilder
{
    private string? _name;
    private readonly List<InputAxisBinding> _bindings = new();

    public InputAxisBuilder WithName(string name)
    {
        _name = name;

        return this;
    }

    public InputAxisBuilder AddBinding(InputName input, float scale)
    {
        _bindings.Add(
            new InputAxisBinding(input, scale)
        );

        return this;
    }

    public void Register(IInputSubsystem input)
    {
        if (string.IsNullOrEmpty(_name)) {
            throw new MissingNameException();
        }

        input.Register(
            new InputAxis(_name, _bindings.ToArray())
        );
    }

    public static InputAxisBuilder Create()
    {
        return new();
    }
}
