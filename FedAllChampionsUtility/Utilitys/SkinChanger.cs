using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using System.Reflection;

namespace FedAllChampionsUtility
{
    class SkinChanger
    {
        private static int currSkinId = 0;
        private static Dictionary<string, int> numSkins = new Dictionary<string, int>();
        private static Menu _menu;

        public SkinChanger()
        {

#region Skins Dic...

            numSkins.Add("Aatrox", 2);
            numSkins.Add("Ahri", 4);
            numSkins.Add("Akali", 6);
            numSkins.Add("Alistar", 7);
            numSkins.Add("Amumu", 7);
            numSkins.Add("Anivia", 5);
            numSkins.Add("Annie", 8);
            numSkins.Add("Ashe", 6);
            numSkins.Add("Azir", 1);
            numSkins.Add("Blitzcrank", 7);
            numSkins.Add("Brand", 4);
            numSkins.Add("Braum", 1);
            numSkins.Add("Caitlyn", 6);
            numSkins.Add("Cassiopeia", 4);
            numSkins.Add("Chogath", 5);
            numSkins.Add("Corki", 6);
            numSkins.Add("Darius", 3);
            numSkins.Add("Diana", 2);
            numSkins.Add("Draven", 5);
            numSkins.Add("DrMundo", 7);
            numSkins.Add("Elise", 2);
            numSkins.Add("Evelynn", 3);
            numSkins.Add("Ezreal", 7);
            numSkins.Add("Fiddlesticks", 8);
            numSkins.Add("Fiora", 3);
            numSkins.Add("Fizz", 4);
            numSkins.Add("Galio", 4);
            numSkins.Add("Gangplank", 6);
            numSkins.Add("Garen", 6);
            numSkins.Add("Gnar", 1);
            numSkins.Add("Gragas", 7);
            numSkins.Add("Graves", 5);
            numSkins.Add("Hecarim", 5);
            numSkins.Add("Heimerdinger", 7);
            numSkins.Add("Irelia", 4);
            numSkins.Add("Janna", 5);
            numSkins.Add("JarvanIV", 5);
            numSkins.Add("Jax", 8);
            numSkins.Add("Jayce", 2);
            numSkins.Add("Jinx", 1);
            numSkins.Add("Karma", 3);
            numSkins.Add("Karthus", 4);
            numSkins.Add("Kassadin", 4);
            numSkins.Add("Katarina", 7);
            numSkins.Add("Kayle", 6);
            numSkins.Add("Kennen", 5);
            numSkins.Add("Khazix", 2);
            numSkins.Add("KogMaw", 7);
            numSkins.Add("Leblanc", 3);
            numSkins.Add("LeeSin", 6);
            numSkins.Add("Leona", 4);
            numSkins.Add("Lissandra", 2);
            numSkins.Add("Lucian", 2);
            numSkins.Add("Lulu", 4);
            numSkins.Add("Lux", 5);
            numSkins.Add("Malphite", 6);
            numSkins.Add("Malzahar", 4);
            numSkins.Add("Maokai", 5);
            numSkins.Add("Masteryi", 5);
            numSkins.Add("MasterYi", 5);
            numSkins.Add("MissFortune", 7);
            numSkins.Add("MonkeyKing", 3);
            numSkins.Add("Mordekaiser", 4);
            numSkins.Add("Morgana", 5);
            numSkins.Add("Nami", 2);
            numSkins.Add("Nasus", 5);
            numSkins.Add("Nautilus", 3);
            numSkins.Add("Nidalee", 6);
            numSkins.Add("Nocturne", 5);
            numSkins.Add("Nunu", 6);
            numSkins.Add("Olaf", 4);
            numSkins.Add("Orianna", 4);
            numSkins.Add("Pantheon", 6);
            numSkins.Add("Poppy", 6);
            numSkins.Add("Quinn", 2);
            numSkins.Add("Rammus", 6);
            numSkins.Add("Renekton", 6);
            numSkins.Add("Rengar", 2);
            numSkins.Add("Riven", 5);
            numSkins.Add("Rumble", 3);
            numSkins.Add("Ryze", 8);
            numSkins.Add("Sejuani", 4);
            numSkins.Add("Shaco", 6);
            numSkins.Add("Shen", 6);
            numSkins.Add("Shyvana", 4);
            numSkins.Add("Singed", 6);
            numSkins.Add("Sion", 4);
            numSkins.Add("Sivir", 6);
            numSkins.Add("Skarner", 2);
            numSkins.Add("Sona", 5);
            numSkins.Add("Soraka", 3);
            numSkins.Add("Swain", 3);
            numSkins.Add("Syndra", 2);
            numSkins.Add("Talon", 3);
            numSkins.Add("Taric", 3);
            numSkins.Add("Teemo", 7);
            numSkins.Add("Thresh", 2);
            numSkins.Add("Tristana", 6);
            numSkins.Add("Trundle", 3);
            numSkins.Add("Tryndamere", 6);
            numSkins.Add("TwistedFate", 8);
            numSkins.Add("Twitch", 5);
            numSkins.Add("Udyr", 3);
            numSkins.Add("Urgot", 3);
            numSkins.Add("Varus", 3);
            numSkins.Add("Vayne", 5);
            numSkins.Add("Veigar", 8);
            numSkins.Add("Velkoz", 1);
            numSkins.Add("Viktor", 3);
            numSkins.Add("Vi", 3);
            numSkins.Add("Vladimir", 6);
            numSkins.Add("Volibear", 3);
            numSkins.Add("Warwick", 7);
            numSkins.Add("Xerath", 3);
            numSkins.Add("XinZhao", 5);
            numSkins.Add("Yasuo", 2);
            numSkins.Add("Yorick", 2);
            numSkins.Add("Zac", 1);
            numSkins.Add("Zed", 3);
            numSkins.Add("Ziggs", 4);
            numSkins.Add("Zilean", 4);
            numSkins.Add("Zyra", 3);

#endregion

            _menu = Program.Menu.AddSubMenu(new Menu("Skins Changer", "SkingChanger"));
            _menu.AddItem(new MenuItem("SkingChanger", "Change Skin!").SetValue(new KeyBind(106, KeyBindType.Toggle)));

            var ChangeSkin = _menu.AddItem(new MenuItem("SkingChanger", "Change Skin!").SetValue(new KeyBind(106, KeyBindType.Toggle)));

            ChangeSkin.ValueChanged += delegate(object sender, OnValueChangeEventArgs EventArgs)
            {
                if (numSkins[ObjectManager.Player.ChampionName] > currSkinId)
                    currSkinId++;
                else
                    currSkinId = 0;

                GenerateSkinPacket(ObjectManager.Player.ChampionName, currSkinId);
            };
        }

        public static void GenerateSkinPacket(string currentChampion, int skinNumber)
        {
            int netID = ObjectManager.Player.NetworkId;
            GamePacket model = Packet.S2C.UpdateModel.Encoded(new Packet.S2C.UpdateModel.Struct(ObjectManager.Player.NetworkId, skinNumber, currentChampion));
            model.Process(PacketChannel.S2C);
        }
    }
}
