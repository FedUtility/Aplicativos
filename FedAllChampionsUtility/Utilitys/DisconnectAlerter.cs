using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace FedAllChampionsUtility
{
    internal class DisconnectAlerter
    {
        public DisconnectAlerter()
        {
            LoadMenu();

            Game.OnGameProcessPacket += Game_OnGameProcessPacket;
        }

        private void LoadMenu()
        {
            Program.Menu.AddSubMenu(new Menu("Alertar DCs", "DC Alerter"));
            Program.Menu.SubMenu("DC Alerter").AddItem(new MenuItem("Alerter", "Ativar Alerta?").SetValue(true));
            
        }

        private static void Game_OnGameProcessPacket(GamePacketEventArgs args)
        {
            if (args.PacketData[0] == Packet.S2C.PlayerDisconnect.Header && Program.Menu.Item("Alerter").GetValue<bool>())
            {

                Game.PrintChat("<b><font color='#FF0000'>" + Packet.S2C.PlayerDisconnect.Decoded(args.PacketData).Player.ChampionName +
                    "</font></b><font color='#FFFFFF'> foi desconectado!</font></b>");
            }           
        }
    }
}
