using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Server;

public class CTeam
{
    protected List<ServerPlayer> m_players;
    protected int m_score;

    public CTeam()
    {
        m_players = new List<ServerPlayer>();
        m_score = 0;
    }

    public void AddPlayer(ServerPlayer player)
    {
        m_players.Add(player);
    }

    public void RemovePlayer(ServerPlayer player)
    {
        m_players.Remove(player);
    }

    public List<ServerPlayer> GetPlayers()
    {
        return m_players;
    }

    public void AddScore(int score)
    {
        m_score += score;
    }
}
