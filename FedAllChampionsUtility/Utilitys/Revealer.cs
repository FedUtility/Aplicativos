using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace FedAllChampionsUtility
{

    internal class Revealer
    {
        public static List<GameObject> wardList = new List<GameObject>();
        public static List<GameObject> akaliShroud = new List<GameObject>();
        public static Spell E;

        public Revealer()
        {            
            Program.Menu.AddSubMenu(new Menu("Revealer", "Revealer"));
            Program.Menu.SubMenu("Revealer").AddItem(new MenuItem("tb_sep0", "====== Settings"));
            Program.Menu.SubMenu("Revealer").AddItem(new MenuItem("activemode", "= Active Mode: ").SetValue(new StringList(new[] { "Active", "Key" }, 0)));
            Program.Menu.SubMenu("Revealer").AddItem(new MenuItem("active1", "= Enabled").SetValue(true));
            Program.Menu.SubMenu("Revealer").AddItem(new MenuItem("active2", "= Enabled Key").SetValue(new KeyBind(32, KeyBindType.Press)));            

            if (ObjectManager.Player.BaseSkinName == "LeeSin")
            {
                Program.Menu.SubMenu("Revealer").AddItem(new MenuItem("leeE", "Lee Sin: Use E").SetValue(true));
                E = new Spell(SpellSlot.E, 350f);
            }
            Program.Menu.SubMenu("Revealer").AddItem(new MenuItem("tb_sep", "Revealer invisibles with a pink!"));
            Program.Menu.SubMenu("Revealer").AddItem(new MenuItem("tb_sep1", "========="));
                        
            Game.OnGameUpdate += Game_OnGameUpdate;
            LeagueSharp.GameObject.OnCreate += GameObject_OnCreate;
        }        

        private void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            if (sender.Name == "akali_smoke_bomb_tar_team_red.troy")
            {
                akaliShroud.Add(sender);
            }
            if (sender.Name == "VisionWard")
            {
                wardList.Add(sender);
            }
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            var Usemode = Program.Menu.Item("activemode").GetValue<StringList>().SelectedIndex;

            if ((Usemode == 0 && Program.Menu.Item("active1").GetValue<bool>()) || (Usemode == 1 && Program.Menu.Item("active2").GetValue<KeyBind>().Active))
            {
                foreach (var player in getEnemies())
                {
                    if (player.HasBuffOfType(BuffType.Invisibility) && player.BaseSkinName != "Evelynn")
                    {
                        if (ObjectManager.Player.BaseSkinName == "LeeSin" && E.IsReady() && ObjectManager.Player.Distance(player) <= E.Range && Program.Menu.Item("leeE").GetValue<bool>())
                        {
                            E.Cast();
                        }

                        else if (Items.HasItem(3364) && Items.CanUseItem(3364))
                        {
                            if (ObjectManager.Player.Distance(player) < 900)
                            {
                                if (ObjectManager.Player.Distance(player) < 600)
                                {
                                    Items.UseItem(3364, ObjectManager.Player.Position);

                                }
                                else
                                {
                                    Items.UseItem(3364, Vector3.Lerp(ObjectManager.Player.Position, player.Position, 600 / ObjectManager.Player.Distance(player)));
                                }
                            }
                        }
                        else if (Items.HasItem(2043))
                        {
                            var castward = true;
                            foreach (var ward in wardList)
                            {
                                if (ObjectManager.Player.Distance(ward.Position) < 600)
                                {
                                    castward = false;
                                }
                            }
                            if (ObjectManager.Player.Distance(player) < 600 && castward)
                            {
                                Items.UseItem(2043, player.Position);
                            }
                        }
                    }
                }
                if (akaliShroud.Count > 0)
                {
                    foreach (var shroud in akaliShroud)
                    {
                        if (Items.HasItem(3364) && Items.CanUseItem(3364))
                        {
                            if (ObjectManager.Player.Distance(shroud.Position) < 900)
                            {
                                if (ObjectManager.Player.Distance(shroud.Position) < 600)
                                {
                                    Items.UseItem(3364, ObjectManager.Player.Position);
                                    akaliShroud.Remove(shroud);

                                }
                                else
                                {
                                    Items.UseItem(3364, Vector3.Lerp(ObjectManager.Player.Position, shroud.Position, 600 / ObjectManager.Player.Distance(shroud.Position)));
                                    akaliShroud.Remove(shroud);
                                }
                            }
                        }
                        else if (Items.HasItem(2043))
                        {
                            var castward = true;
                            foreach (var ward in wardList)
                            {
                                if (ObjectManager.Player.Distance(ward.Position) < 600)
                                {
                                    castward = false;
                                }
                            }
                            if (ObjectManager.Player.Distance(shroud.Position) < 600 && castward)
                            {
                                Items.UseItem(2043, shroud.Position);
                                akaliShroud.Remove(shroud);
                            }
                        }
                    }
                }

            }
        }

        private IEnumerable<Obj_AI_Hero> getEnemies()
        {
            var enemies = from enemy in ObjectManager.Get<Obj_AI_Hero>()
                          where !enemy.IsAlly && ObjectManager.Player.Distance(enemy) < 2000
                          select enemy;
            return enemies;
        }        
    }
}