namespace RoleplayersToolbox.Tools {
    internal abstract class BaseTool : ITool {
        public abstract string Name { get; }

        public abstract void DrawSettings(ref bool anyChanged);

        public virtual void DrawAlways() {
            // do nothing
        }
    }
}
