#region

using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

#endregion

namespace FedAllChampionsUtility
{
    class Nocturne : Champion
    {
        public static Spell Q, W, E, R;
        public static Vector2 PingLocation;
        public static int LastPingT = 0;

        public Nocturne()
        {
            LoadMenu();
            LoadSells();

            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Drawing.OnEndScene += Drawing_OnEndScene;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPossibleToInterrupt;            

            PluginLoaded();
        }

        private void LoadSells()
        {
            Q = new Spell(SpellSlot.Q, 1200f);
            W = new Spell(SpellSlot.W, 0f);
            E = new Spell(SpellSlot.E, 425f);
            R = new Spell(SpellSlot.R, 25000f);

            Q.SetSkillshot(0.25f, 60f, 1600f, false, SkillshotType.SkillshotLine);
            E.SetTargetted(0.5f, 1700f);
            R.SetTargetted(0.75f, 2500f);
        }

        private void LoadMenu()
        {
            Program.Menu.AddSubMenu(new Menu("TeamFight", "TeamFight"));
            Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
            Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
            Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
            Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("UseRCombo", "Use R").SetValue(true));
            Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("UseRHP", "% HP to R Combo: ").SetValue<Slider>(new Slider(50, 100, 10)));
            Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("ComboActive", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));

            Program.Menu.AddSubMenu(new Menu("Harass", "Harass"));
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(true));            
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("UseEHarass", "Use E").SetValue(false));
            AddManaManager("Harass", 40);
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("HarassActive", "Harass!").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));

            Program.Menu.AddSubMenu(new Menu("LaneClear", "LaneClear"));
            Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("UseQFarm", "Use Q").SetValue(true));            
            AddManaManager("LaneClear", 30);            
            Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("LaneClearActive", "LaneClear!").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));

            Program.Menu.AddSubMenu(new Menu("JungleFarm", "JungleFarm"));
            Program.Menu.SubMenu("JungleFarm").AddItem(new MenuItem("UseQJFarm", "Use Q").SetValue(true));
            Program.Menu.SubMenu("JungleFarm").AddItem(new MenuItem("UseWJFarm", "Use W").SetValue(true));
            Program.Menu.SubMenu("JungleFarm").AddItem(new MenuItem("UseEJFarm", "Use E").SetValue(true));
            Program.Menu.SubMenu("JungleFarm").AddItem(new MenuItem("JungleFarmActive", "JungleFarm!").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));

            Program.Menu.AddSubMenu(new Menu("Misc", "Misc"));
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("InterruptSpells", "Interrupt spells").SetValue(true));
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("AutoRHP", "Auto R LowHP").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("HPR", "% Low HP: ").SetValue<Slider>(new Slider(30, 100, 10)));
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("useR_Killableping", "Ping if is Low HP").SetValue(true));
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("AutoRStrong", "R Most AD/AP in Rage").SetValue(new KeyBind("G".ToCharArray()[0], KeyBindType.Press)));
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("MostR", "R Most: ").SetValue(new StringList(new[] { "AD", "AP", "Easy" }, 0))); 

            Program.Menu.AddSubMenu(new Menu("Drawing", "Drawing"));
            Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_Disabled", "Disable All").SetValue(false));
            Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_Q", "Draw Q").SetValue(true));            
            Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_E", "Draw E").SetValue(true));
            Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_R", "Draw R").SetValue(true));
            Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("DrawRRangeM", "Draw R Range (Minimap)").SetValue(new Circle(false, Color.FromArgb(150, Color.DodgerBlue))));
        }

        private void Game_OnGameUpdate(EventArgs args)
        {

            if (ObjectManager.Player.IsDead) return;

            if (Program.Menu.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                Combo();
            }
            else
            {

                if (Program.Menu.Item("HarassActive").GetValue<KeyBind>().Active)
                {
                    Harass();
                }

                if (Program.Menu.Item("LaneClearActive").GetValue<KeyBind>().Active)
                {
                    LaneClear();
                }

                if (Program.Menu.Item("JungleFarmActive").GetValue<KeyBind>().Active)
                {
                    JungleFarm();
                }
            }

            if (Program.Menu.Item("AutoRHP").GetValue<KeyBind>().Active)
            {
                AutoRLowHP();
            }

            if (Program.Menu.Item("AutoRStrong").GetValue<KeyBind>().Active)
            {
                AutoRMode();
            }

            if (R.IsReady() && Program.Menu.Item("useR_Killableping").GetValue<bool>())
            {
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsValidTarget(GetRRange()) && !h.IsDead && EnemylowHP(Program.Menu.Item("HPR").GetValue<Slider>().Value, GetRRange())))
                {
                    Ping(enemy.Position.To2D());
                }
            }
        }
        private float GetRRange()
        {
            return 1250 + (750 * R.Level);
        }
        private void AutoRMode()
        {
            Obj_AI_Hero newtarget = null;
            var RMode = Program.Menu.Item("MostR").GetValue<StringList>().SelectedIndex;

            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsValidTarget() && Geometry.Distance(enemy) <= GetRRange()))
            {
                if (newtarget == null)
                {
                    newtarget = enemy;
                }
                else
                {
                    switch (RMode)
                    {
                        case 0:
                            if (enemy.BaseAttackDamage + enemy.FlatPhysicalDamageMod <
                                        newtarget.BaseAttackDamage + newtarget.FlatPhysicalDamageMod)
                            {
                                newtarget = enemy;
                            }
                            break;
                        case 1:
                            if (enemy.FlatMagicDamageMod < newtarget.FlatMagicDamageMod)
                            {
                                newtarget = enemy;
                            }
                            break;
                        case 2:
                            if ((enemy.Health - Damage.CalcDamage(ObjectManager.Player, enemy, Damage.DamageType.Magical, enemy.Health)) <
                                        (enemy.Health - Damage.CalcDamage(ObjectManager.Player, newtarget, Damage.DamageType.Magical, newtarget.Health)))
                            {
                                newtarget = enemy;
                            }
                            break;
                    }
                }
            }

            if (R.IsReady() && newtarget != null)
            {
                R.Cast();
                R.CastOnUnit(newtarget, Packets());
            }            
        }
        private void AutoRLowHP()
        {
            var rTarget = SimpleTs.GetTarget(GetRRange(), SimpleTs.DamageType.Physical);
            if (R.IsReady() && rTarget != null)
            {
                if (ObjectManager.Player.Distance(rTarget) > 1100 && EnemylowHP(Program.Menu.Item("HPR").GetValue<Slider>().Value, GetRRange()))
                {
                    R.Cast();
                    R.CastOnUnit(rTarget, Packets());
                }
            }  
        }
        private void Combo()
        {            
            var eTarget = SimpleTs.GetTarget(E.Range - 30, SimpleTs.DamageType.Physical); 
            var rTarget = SimpleTs.GetTarget(GetRRange(), SimpleTs.DamageType.Physical);

            if (R.IsReady() && Program.Menu.Item("UseRCombo").GetValue<bool>())
            {
                if (ObjectManager.Player.Distance(rTarget) > 1100 && EnemylowHP(Program.Menu.Item("UseRHP").GetValue<Slider>().Value, GetRRange()))
                {
                    R.Cast();
                    R.CastOnUnit(rTarget, Packets());
                }
            }
            
            if (Q.IsReady() && Program.Menu.Item("UseQCombo").GetValue<bool>())
            {
                Cast_BasicLineSkillshot_Enemy(Q);
            }
            
            if (E.IsReady() && Program.Menu.Item("UseECombo").GetValue<bool>())
            {
                Cast_onEnemy(E);
            }

            if (W.IsReady() && Program.Menu.Item("UseWCombo").GetValue<bool>())
            {
                if (ObjectManager.Player.Distance(eTarget) < 425)
                {
                    W.Cast();
                }
            }
        }
        private void Harass()
        {            
            if (Q.IsReady() && Program.Menu.Item("UseQHarass").GetValue<bool>())
            {
                Cast_BasicLineSkillshot_Enemy(Q);
            }
            
            if (E.IsReady() && Program.Menu.Item("UseEHarass").GetValue<bool>())
            {
                Cast_onEnemy(E);
            }
        }
        private void LaneClear()
        { 
            if (Q.IsReady() && Program.Menu.Item("UseQFarm").GetValue<bool>())
            {
                Cast_BasicLineSkillshot_AOE_Farm(Q);
            }
        }
        private void JungleFarm()
        {
            var mobs = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range,
                MinionTypes.All,
                MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            if (mobs.Count > 0)
            {
                var mob = mobs[0];

                if (Q.IsReady() && Program.Menu.Item("UseQJFarm").GetValue<bool>())
                {
                    Q.Cast(mob.Position);
                }
                if (W.IsReady() && Program.Menu.Item("UseWJFarm").GetValue<bool>())
                {
                    W.Cast();
                }
                if (E.IsReady() && Program.Menu.Item("UseEJFarm").GetValue<bool>())
                {
                    E.CastOnUnit(mob);
                }
            }
        }
        private bool EnemylowHP(int percentHP, float range)
        {
            return ObjectManager.Get<Obj_AI_Hero>().Where(enemmy => enemmy.IsEnemy && !enemmy.IsDead).Any(enemmy => Vector3.Distance(ObjectManager.Player.Position, enemmy.Position) < range && ((enemmy.Health / enemmy.MaxHealth) * 100) < percentHP);
        }
        private void Ping(Vector2 position)
        {
            if (Environment.TickCount - LastPingT < 30 * 1000) return;
            LastPingT = Environment.TickCount;
            PingLocation = position;
            SimplePing();
            Utility.DelayAction.Add(150, SimplePing);
            Utility.DelayAction.Add(300, SimplePing);
            Utility.DelayAction.Add(400, SimplePing);
            Utility.DelayAction.Add(800, SimplePing);
        }
        private void SimplePing()
        {
            Packet.S2C.Ping.Encoded(new Packet.S2C.Ping.Struct(PingLocation.X, PingLocation.Y, 0, 0, Packet.PingType.Fallback)).Process();
        }
        private void Drawing_OnEndScene(EventArgs args)
        {
            if (Program.Menu.Item("Draw_Disabled").GetValue<bool>())
                return;

            if (R.Level == 0) return;
            var menuItem = Program.Menu.Item("DrawRRangeM").GetValue<Circle>();
            if (menuItem.Active)
                Utility.DrawCircle(ObjectManager.Player.Position, GetRRange(), menuItem.Color, 2, 30, true);            
        }
        private void Drawing_OnDraw(EventArgs args)
        {
            if (Program.Menu.Item("Draw_Disabled").GetValue<bool>())
                return;

            if (Program.Menu.Item("Draw_Q").GetValue<bool>())
                if (Q.Level > 0)
                    Utility.DrawCircle(ObjectManager.Player.Position, Q.Range, Q.IsReady() ? Color.Green : Color.Red);            

            if (Program.Menu.Item("Draw_E").GetValue<bool>())
                if (E.Level > 0)
                    Utility.DrawCircle(ObjectManager.Player.Position, E.Range, E.IsReady() ? Color.Green : Color.Red);

            if (Program.Menu.Item("Draw_R").GetValue<bool>())
                if (R.Level > 0)
                    Utility.DrawCircle(ObjectManager.Player.Position, GetRRange(), R.IsReady() ? Color.Green : Color.Red);
            
        }
        private void Interrupter_OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (!Program.Menu.Item("InterruptSpells").GetValue<bool>())
                return;

            if (ObjectManager.Player.Distance(unit) < E.Range && E.IsReady() && unit.IsEnemy)
            {
                if (W.IsReady())
                {
                    W.CastOnUnit(ObjectManager.Player);
                }

                E.CastOnUnit(unit, Packets());
            }
        }
        
    }
}
