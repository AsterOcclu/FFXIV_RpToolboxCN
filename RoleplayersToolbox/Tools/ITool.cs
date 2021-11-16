namespace RoleplayersToolbox.Tools {
    internal interface ITool {
        string Name { get; }
        void DrawSettings(ref bool anyChanged);
        void DrawAlways();
    }
}
