using System;
using System.Collections.Generic;
using System.Text;

namespace IPioneerReceiverControl.Rx.Model.Enum
{
    public enum ListeningMode
    {
        Stereo_Cyclic = 0001,
        Standard = 0010,
        Stereo_Direct = 0009,
        Ch2Source = 0011,
        ProLogic2Movie = 0013,
        ProLogic2xMovie = 0018,
        ProLogic2Music = 0014,
        ProLogic2xMusic = 0019,
        ProLogic2Game = 0015,
        ProLogic2xGame = 0020,
        ProLogic2zHeight = 0031,
        WideSurroundMovie = 0032,
        WideSurroundMusic = 0033,
        ProLogic = 0012,
        Neo6Cinema = 0016,
        Neo6Music = 0017,
        XMHDSurrond = 0029,

        Unknown = 9999,


    }
}
