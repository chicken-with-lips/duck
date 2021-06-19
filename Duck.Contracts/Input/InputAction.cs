using System.Collections.Generic;

namespace Duck.Contracts.Input
{
    public class InputAction
    {
        public string Name => _name;
        public InputActionBinding[] Bindings => _bindings.ToArray();
        public bool IsActivated { get; set; }
        public float Value => IsActivated ? 1.0f : 0.0f;

        private readonly string _name;
        private readonly List<InputActionBinding> _bindings = new();

        internal InputAction(string name, InputActionBinding[] bindings)
        {
            _name = name;
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

        public void Register(IInputSubsystem input)
        {
            ValidateAndThrow();

            input.Register(
                new InputAction(_name, _bindings.ToArray())
            );
        }

        private void ValidateAndThrow()
        {
            if (string.IsNullOrEmpty(_name)) {
                throw new MissingNameException();
            }
        }

        public static InputActionBuilder Create()
        {
            return new();
        }
    }
}
