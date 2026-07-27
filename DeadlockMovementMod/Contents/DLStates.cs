using DeadlockMovementAPI.EntityStates;

namespace DeadlockMovementAPI.Contents
{
    public static class DLStates
    {
        public static void Init()
        {

            Modules.Content.AddEntityState(typeof(DLMain));
            Modules.Content.AddEntityState(typeof(Dash));
            Modules.Content.AddEntityState(typeof(DashJump));
            Modules.Content.AddEntityState(typeof(Slide));



        }
    }
}
