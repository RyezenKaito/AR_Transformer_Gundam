using UnityEngine;
using UnityEngine.InputSystem;

namespace UnicornDemo
{
    static class UnicornInputBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void ConfigureMobileInput()
        {
            if(!Application.isMobilePlatform)return;
            // App UI enables Enhanced Touch during SubsystemRegistration, before
            // Input System can reset its touch state. Reapply settings after that
            // stage so its registered callbacks restore a valid update mask before
            // the first native touchscreen is discovered. No package files are edited.
            InputSystem.settings.updateMode=InputSettings.UpdateMode.ProcessEventsManually;
            InputSystem.settings.updateMode=InputSettings.UpdateMode.ProcessEventsInDynamicUpdate;
        }
    }
}
