using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Communication
{
    enum PacketType
    {
        UserPacketStart = 100,
        AddNewPlayer,
        SetPlayerID,
        RequestMapInfo,
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
