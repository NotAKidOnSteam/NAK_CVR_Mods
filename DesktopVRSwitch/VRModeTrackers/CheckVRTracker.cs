﻿using ABI_RC.Core.Savior;

namespace NAK.DesktopVRSwitch.VRModeTrackers;

public class CheckVRTracker : VRModeTracker
{
    public override void TrackerInit()
    {
        VRModeSwitchManager.OnPostVRModeSwitch += OnPostSwitch;
    }

    public override void TrackerDestroy()
    {
        VRModeSwitchManager.OnPostVRModeSwitch -= OnPostSwitch;
    }

    private void OnPostSwitch(bool intoVR)
    {
        CheckVR.Instance.hasVrDeviceLoaded = intoVR;
    }
}