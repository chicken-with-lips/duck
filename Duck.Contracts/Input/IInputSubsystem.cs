namespace Duck.Contracts.Input
{
    public interface IInputSubsystem : IApplicationSubsystem
    {
        public void Register(InputAction action);
        public void Register(InputAxis axis);

        public InputAxis GetAxis(string name);
        public InputAction GetAction(string name);

    }
}
