using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Communication
{
    enum PacketType
    {
        UserPacketStart = 100,
        SetPlayerID,
        SendMapInfo,
        UpdatePlayerName,
        BroadcastPlayerName,
        PlayerNotReady,
        UpdatePlayerTeam,
        BroadcastPlayerTeam,
        PlayerPrepared,
        CharacterStatus
    }
}
