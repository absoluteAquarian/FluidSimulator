namespace AbsoluteCommons.Input {
	public readonly struct InputAxis : IInputControl {
		public readonly string Name { get; }

		public InputAxis(string name) => Name = name;

		void IInputControl.Register(InputMap map) => map.DefineAxis(Name);
	}
}
