using System;

namespace Distance.CustomCar
{
    [Flags]
    public enum MessageButtons
    {
        Ok = MessagePanelLogic.ButtonType.Ok,
        OkCancel = MessagePanelLogic.ButtonType.OkCancel,
        YesNo = MessagePanelLogic.ButtonType.YesNo
    }
}
