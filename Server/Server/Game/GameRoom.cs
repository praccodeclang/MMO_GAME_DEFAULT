﻿using Google.Protobuf;
using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public class GameRoom
    {
        object _lock = new object();
        public int RoomID { get; set; }

        List<Player> _players = new List<Player>();
        
        public void EnterGame(Player newPlayer)
        {
            if (newPlayer == null)
                return;

            lock (_lock)
            {
                _players.Add(newPlayer);
                newPlayer.Room = this;

                //본인한테 정보 전송
                {
                    S_EnterGame enterPacket = new S_EnterGame();
                    enterPacket.Player = newPlayer.Info;
                    newPlayer.Session.Send(enterPacket);

                    S_Spawn spawnPacket = new S_Spawn();
                    foreach (Player p in _players)
                    {
                        if (newPlayer != p)
                            spawnPacket.Players.Add(p.Info);
                    }
                    newPlayer.Session.Send(spawnPacket);
                }

                //타인한테 정보 전송
                {
                    S_Spawn spawnPacket = new S_Spawn();
                    spawnPacket.Players.Add(newPlayer.Info);
                    foreach (Player p in _players)
                    {
                        if (newPlayer != p)
                            p.Session.Send(spawnPacket);
                    }
                }
            }
        }

        public void LeaveGame(int playerID)
        {
            lock (_lock)
            {
                Player player = _players.Find(p => p.Info.PlayerID == playerID);
                if (player == null)
                    return;
                _players.Remove(player);
                player.Room = null;

                //본인한테 정보전송
                {
                    S_LeaveGame leavePacket = new S_LeaveGame();
                    player.Session.Send(leavePacket);
                }

                //타인한테 정보전송
                {
                    S_Despawn despawnPacket = new S_Despawn();
                    despawnPacket.PlayerIDs.Add(player.Info.PlayerID);
                    foreach (Player p in _players)
                    {
                        if(player != p)
                            p.Session.Send(despawnPacket);
                    }
                    
                }
            }
        }

        public void HandleMove(Player player, C_Move movePacket)
        {
            if (player == null)
                return;

            lock (_lock)
            {
                // TODO : 검증

                // 서버에서 먼저 좌표이동.
                PlayerInfo info = player.Info;
                info.PosInfo = movePacket.PosInfo;

                //다른 플레이어에게 알려줌.
                S_Move resMovePacket = new S_Move();
                resMovePacket.PlayerID = player.Info.PlayerID;
                resMovePacket.PosInfo = movePacket.PosInfo;

                BroadCast(resMovePacket);
            }
            
        }

        public void HandleSkill(Player player, C_Skill skillPacket)
        {
            if (player == null)
                return;
            lock (_lock)
            {
                // TODO : 검증

                PlayerInfo info = player.Info;
                if (info.PosInfo.State != CreatureState.Idle)
                    return;

                
                //TODO : 스킬사용가능여부 체크


                // 통과
                info.PosInfo.State = CreatureState.Skill;


                S_Skill skill = new S_Skill() { Info = new SkilInfo() };
                skill.Info.SkillID = 1;
                BroadCast(skill);

                Console.WriteLine($"펀치~!{skill.Info.SkillID}");
                //TODO : 데미지 판정

            }
        }
        public void BroadCast(IMessage packet)
        {
            lock (_lock)
            {
                foreach (Player p in _players)
                {
                    p.Session.Send(packet);
                }
            }
        }
    }
}
