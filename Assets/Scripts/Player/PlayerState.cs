// The player's COMMANDED mode — what the player is told/allowed to do.
// This is NOT locomotion: Idle vs Walk is derived from velocity (the animator
// reads PlayerMotor.Velocity), so it does not belong here. This enum holds only
// states that GATE control.
public enum PlayerState
{
    Free,      // normal player control
    UsingTool, // mid tool-swing — movement locked
    InEvent,   // cutscene / dialogue / menu — movement locked
}
