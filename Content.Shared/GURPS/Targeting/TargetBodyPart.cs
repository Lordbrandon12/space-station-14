namespace Content.Shared.GURPS.Targeting
{
    [Flags]
    public enum TargetBodyPart
    {
        Head = 1,
        LeftEye = 1 << 1,
        RightEye = 1 << 2,
        Mouth = 1 << 3,
        Neck = 1 << 4,
        Torso = 1 << 5,
        RightArm = 1 << 6,
        RightHand = 1 << 7,
        LeftArm = 1 << 8,
        LeftHand = 1 << 9,
        Groin = 1 << 10,
        RightLeg = 1 << 11,
        RightFoot = 1 << 12,
        LeftLeg = 1 << 13,
        LeftFoot = 1 << 14
    }
}
