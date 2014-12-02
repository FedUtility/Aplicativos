#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
using LX_Orbwalker;


#endregion

namespace FedAllChampionsUtility
{
    class Xerath : Champion
    {
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        public static int UltTick;

        public static Vector2 PingLocation;
        public static int LastPingT = 0;

        public static SpellSlot IgniteSlot;
        public static Items.Item DFG;

        public Xerath()
        {
            LoadMenu();
            LoadSpells();

            Drawing.OnDraw += Drawing_OnDraw;
            Drawing.OnEndScene += Drawing_OnEndScene;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Game.OnGameSendPacket += Game_OnGameSendPacket2;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPossibleToInterrupt;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            PluginLoaded();
        }

        private void LoadMenu()
        {
            Program.Menu.AddSubMenu(new Menu("TeamFight", "TeamFight"));
            Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useQ_TeamFight", "Use Q").SetValue(true));
            Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useW_TeamFight", "Use W").SetValue(true));
            Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useE_TeamFight", "Use E").SetValue(true));
            Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useR_TeamFight", "Use R").SetValue(true));

            Program.Menu.AddSubMenu(new Menu("Harass", "Harass"));
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("useQ_Harass", "Use Q").SetValue(true));
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("useW_Harass", "Use W").SetValue(true));
            AddManaManager("Harass", 40);

            Program.Menu.AddSubMenu(new Menu("LaneClear", "LaneClear"));
            Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("useQ_LaneClear", "Use Q").SetValue(true));
            Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("useW_LaneClear", "Use W").SetValue(true));
            AddManaManager("LaneClear", 20);

            Program.Menu.AddSubMenu(new Menu("Passive", "Passive"));
            Program.Menu.SubMenu("Passive").AddItem(new MenuItem("useE_Interupt", "Use E Interrupt").SetValue(true));
            Program.Menu.SubMenu("Passive").AddItem(new MenuItem("AutoEGC", "Use E Gapcloser").SetValue(true));
            Program.Menu.SubMenu("Passive").AddItem(new MenuItem("OnlyNearMouse", "Focus only targets near mouse").SetValue(false));
            Program.Menu.SubMenu("Passive").AddItem(new MenuItem("MRadius", "Radius").SetValue(new Slider(700, 1500, 300)));
            Program.Menu.SubMenu("Passive").AddItem(new MenuItem("AutoRKS", "R Automatico").SetValue(false));
            Program.Menu.SubMenu("Passive").AddItem(new MenuItem("useR_KS", "Use R for KS").SetValue((new KeyBind("T".ToCharArray()[0], KeyBindType.Press))));
            Program.Menu.SubMenu("Passive").AddItem(new MenuItem("useR_safe", "Saferange").SetValue(new Slider(700, 2000, 0)));
            Program.Menu.SubMenu("Passive").AddItem(new MenuItem("useR_Killabletext", "Write if is killable").SetValue(true));
            Program.Menu.SubMenu("Passive").AddItem(new MenuItem("useR_Killableping", "Ping if is killable").SetValue(true));
            Program.Menu.SubMenu("Passive").AddItem(new MenuItem("useR_warning", "warn if active").SetValue(false));
            Program.Menu.SubMenu("Passive").AddSubMenu(new Menu("Dont use R on", "DontUlt"));

            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != ObjectManager.Player.Team))
                Program.Menu.SubMenu("Passive").SubMenu("DontUlt").AddItem(new MenuItem("DontUlt" + enemy.BaseSkinName, enemy.BaseSkinName).SetValue(false));

            //Damage after combo:
            var dmgAfterComboItem = new MenuItem("DamageAfterCombo", "Draw damage after combo").SetValue(true);
            Utility.HpBarDamageIndicator.DamageToUnit = GetComboDamage;
            Utility.HpBarDamageIndicator.Enabled = dmgAfterComboItem.GetValue<bool>();
            dmgAfterComboItem.ValueChanged += delegate(object sender, OnValueChangeEventArgs eventArgs)
            {
                Utility.HpBarDamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
            };

            Program.Menu.AddSubMenu(new Menu("Drawing", "Drawing"));
            Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_Disabled", "Disable All").SetValue(false));
            Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_Q", "Draw Q").SetValue(true));
            Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_W", "Draw W").SetValue(true));
            Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_E", "Draw E").SetValue(true));
            Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_R", "Draw R").SetValue(true));
            Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("RRangeM", "R range (minimap)").SetValue(new Circle(false, Color.FromArgb(150, Color.DodgerBlue))));
            Program.Menu.SubMenu("Drawing").AddItem(dmgAfterComboItem);
        }

        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 1550);
            Q.SetSkillshot(0.6f, 100, float.MaxValue, false, SkillshotType.SkillshotLine);
            Q.SetCharged("XerathArcanopulseChargeUp", "XerathArcanopulseChargeUp", 750, 1550, 1.5f);

            W = new Spell(SpellSlot.W, 1000);
            W.SetSkillshot(0.7f, 125, float.MaxValue, false, SkillshotType.SkillshotCircle);

            E = new Spell(SpellSlot.E, 1150);
            E.SetSkillshot(0.2f, 60, 1400, true, SkillshotType.SkillshotLine);

            R = new Spell(SpellSlot.R, 675);
            R.SetSkillshot(0.7f, 120, float.MaxValue, false, SkillshotType.SkillshotCircle);

            IgniteSlot = ObjectManager.Player.GetSpellSlot("SummonerDot");
            DFG = Utility.Map.GetMap()._MapType == Utility.Map.MapType.TwistedTreeline ? new Items.Item(3188, 750) : new Items.Item(3128, 750);
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            R_Check();

            switch (LXOrbwalker.CurrentMode)
            {
                case LXOrbwalker.Mode.Combo:
                    if (Program.Menu.Item("useQ_TeamFight").GetValue<bool>())
                        QEnemy();
                    if (Program.Menu.Item("useW_TeamFight").GetValue<bool>())
                        Cast_BasicCircleSkillshot_Enemy(W, SimpleTs.DamageType.Magical);
                    if (Program.Menu.Item("useE_TeamFight").GetValue<bool>())
                        Cast_BasicLineSkillshot_Enemy(E, SimpleTs.DamageType.Magical);
                    break;
                case LXOrbwalker.Mode.Harass:
                    if (Program.Menu.Item("useQ_Harass").GetValue<bool>() && (ManaManagerAllowCast(Q) || Q.IsCharging))
                        QEnemy();
                    if (Program.Menu.Item("useW_Harass").GetValue<bool>() && ManaManagerAllowCast(W))
                        Cast_BasicCircleSkillshot_Enemy(W, SimpleTs.DamageType.Magical);
                    break;
                case LXOrbwalker.Mode.LaneClear:
                    if (Program.Menu.Item("useQ_LaneClear").GetValue<bool>() && (ManaManagerAllowCast(Q) || Q.IsCharging))
                        QFarm();
                    if (Program.Menu.Item("useW_LaneClear").GetValue<bool>() && ManaManagerAllowCast(Q))
                        Cast_BasicCircleSkillshot_AOE_Farm(W);
                    break;
            }

            if (R.IsReady() && Program.Menu.Item("useR_Killableping").GetValue<bool>())
            {
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsValidTarget(GetRRange()) && (float)ObjectManager.Player.GetSpellDamage(h, SpellSlot.R) * 3 > h.Health))
                {
                    if (Program.Menu.Item("DontUlt" + enemy.BaseSkinName) != null && Program.Menu.Item("DontUlt" + enemy.BaseSkinName).GetValue<bool>() == false)
                    Ping(enemy.Position.To2D());
                }
            }
        }

        private float GetComboDamage(Obj_AI_Base enemy)
        {
            var damage = 0d;

            if (Q.IsReady())
                damage += ObjectManager.Player.GetSpellDamage(enemy, SpellSlot.Q);

            if (DFG.IsReady())
                damage += ObjectManager.Player.GetItemDamage(enemy, Damage.DamageItems.Dfg) / 1.2;

            if (W.IsReady())
                damage += ObjectManager.Player.GetSpellDamage(enemy, SpellSlot.W);

            if (E.IsReady())
                damage += ObjectManager.Player.GetSpellDamage(enemy, SpellSlot.E);

            if (IgniteSlot != SpellSlot.Unknown && ObjectManager.Player.SummonerSpellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
                damage += ObjectManager.Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);

            if (R.IsReady())
                damage += Math.Min(7, ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).Ammo) * ObjectManager.Player.GetSpellDamage(enemy, SpellSlot.R, 1);

            return (float)damage * (DFG.IsReady() ? 1.2f : 1);
        }

        private void R_Check()
        {
            if (!Program.Menu.Item("useR_KS").GetValue<KeyBind>().Active && !Program.Menu.Item("AutoRKS").GetValue<bool>())
                return;

            if (!R.IsReady() && !IsShooting())
                return;
            if (Utility.CountEnemysInRange(Program.Menu.Item("useR_safe").GetValue<Slider>().Value) >= 1 && !IsShooting())
                return;            

            R.Range = GetRRange();
            Obj_AI_Hero[] lowesttarget = { null };
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget(R.Range)).Where(enemy => lowesttarget[0] == null || lowesttarget[0].Health > enemy.Health))
            {
                lowesttarget[0] = enemy;
            }            

            if (lowesttarget[0] != null &&
                lowesttarget[0].Health < ((ObjectManager.Player.GetSpellDamage(lowesttarget[0], SpellSlot.R) * 3) * 0.9) && Environment.TickCount - UltTick >= 700)
            {
                R.Cast(lowesttarget[0], Packets());
                UltTick = Environment.TickCount;
                return;
            }
            if (IsShooting())
            {
                var rTarget = Program.Menu.Item("OnlyNearMouse").GetValue<bool>() ? GetTargetNearMouse(Program.Menu.Item("MRadius").GetValue<Slider>().Value) : SimpleTs.GetTarget(R.Range, SimpleTs.DamageType.Magical);
                R.Cast(rTarget, Packets());
            }
        }

        private float GetRRange()
        {
            return 2000 + (1100 * R.Level);
        }

        private void QEnemy()
        {
            if (!Q.IsReady())
                return;
            var target = SimpleTs.GetTarget(Q.ChargedMaxRange, SimpleTs.DamageType.Physical);
            if (!target.IsValidTarget(Q.ChargedMaxRange))
                return;
            if (Q.IsCharging)
            {
                if (Q.GetPrediction(target).Hitchance >= HitChance.High)
                    Q.Cast(target, Packets());
                return;
            }
            Q.StartCharging();

        }

        public void QFarm()
        {
            if (!Q.IsReady())
                return;
            var minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.ChargedMaxRange, MinionTypes.All, MinionTeam.NotAlly);
            if (minions.Count <= 0)
                return;
            if (Q.IsCharging)
            {
                var locQ = Q.GetLineFarmLocation(minions);
                if (minions.Count == minions.Count(m => ObjectManager.Player.Distance(m) < Q.Range) && locQ.MinionsHit > 0 && locQ.Position.IsValid())
                    Q.Cast(locQ.Position);
            }
            else if (minions.Count > 0)
                Q.StartCharging();
        }

        private void Interrupter_OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (!Program.Menu.Item("useE_Interupt").GetValue<bool>())
                return;
            if (ObjectManager.Player.Distance(unit) < E.Range && E.IsReady() && unit.IsEnemy)
                E.Cast(unit, Packets());
        }

        private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!Program.Menu.Item("AutoEGC").GetValue<bool>()) return;

            if (ObjectManager.Player.Distance(gapcloser.Sender) < E.Range)
            {
                E.Cast(gapcloser.Sender);
            }
        }

        private void Game_OnGameSendPacket2(GamePacketEventArgs args)
        {
            if (args.PacketData[0] == Packet.C2S.Move.Header && IsShooting())
                args.Process = false;
        }

        private bool IsShooting()
        {
            return Environment.TickCount - UltTick < 250 || ObjectManager.Player.HasBuff("XerathLocusOfPower2");
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

        private static void SimplePing()
        {
            Packet.S2C.Ping.Encoded(new Packet.S2C.Ping.Struct(PingLocation.X, PingLocation.Y, 0, 0, Packet.PingType.Fallback)).Process();
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (Program.Menu.Item("Draw_Disabled").GetValue<bool>())
                return;

            if (Program.Menu.Item("Draw_Q").GetValue<bool>())
                if (Q.Level > 0)
                    Utility.DrawCircle(ObjectManager.Player.Position, Q.Range, Q.IsReady() ? Color.Green : Color.Red);

            if (Program.Menu.Item("Draw_W").GetValue<bool>())
                if (W.Level > 0)
                    Utility.DrawCircle(ObjectManager.Player.Position, W.Range, W.IsReady() ? Color.Green : Color.Red);

            if (Program.Menu.Item("Draw_E").GetValue<bool>())
                if (E.Level > 0)
                    Utility.DrawCircle(ObjectManager.Player.Position, E.Range, E.IsReady() ? Color.Green : Color.Red);

            if (Program.Menu.Item("Draw_R").GetValue<bool>())
                if (R.Level > 0)
                    Utility.DrawCircle(ObjectManager.Player.Position, R.Range, R.IsReady() ? Color.Green : Color.Red);

            var victims = "";

            foreach (var target in Program.Helper.EnemyInfo.Where(x =>
             x.Player.IsVisible && x.Player.IsValidTarget(GetRRange()) && (((float)ObjectManager.Player.GetSpellDamage(x.Player, SpellSlot.R) * 3) * 0.9) >= x.Player.Health))
            {
                if (Program.Menu.Item("DontUlt" + target.Player.BaseSkinName) != null && Program.Menu.Item("DontUlt" + target.Player.BaseSkinName).GetValue<bool>() == false)
                victims += target.Player.ChampionName + " ";                
            }
            if (victims != "" && Program.Menu.Item("useR_Killabletext").GetValue<bool>())
                if (!Program.Menu.Item("useR_KS").GetValue<KeyBind>().Active && R.IsReady())
                {
                    Drawing.DrawLine(Drawing.Width * 0.44f - 50, Drawing.Height * 0.7f - 2, Drawing.Width * 0.44f + 350, Drawing.Height * 0.7f - 2, 25, Color.Black);
                    Drawing.DrawText(Drawing.Width * 0.44f, Drawing.Height * 0.7f, Color.GreenYellow, "[Press " + Convert.ToChar(Program.Menu.Item("useR_KS").GetValue<KeyBind>().Key) + "] Ult can kill: " + victims);
                }
            if (victims == "")
                if (Program.Menu.Item("useR_warning").GetValue<bool>() && Program.Menu.Item("useR_KS").GetValue<KeyBind>().Active)
                {
                    Drawing.DrawLine(Drawing.Width * 0.44f - 50, Drawing.Height * 0.7f - 2, Drawing.Width * 0.44f + 350, Drawing.Height * 0.7f - 2, 25, Color.Black);
                    Drawing.DrawText(Drawing.Width * 0.44f, Drawing.Height * 0.7f, Color.OrangeRed, "Warning: [Press " + Convert.ToChar(Program.Menu.Item("useR_KS").GetValue<KeyBind>().Key) + "] to disable KS ");
                }

            if (IsShooting())
            {
                if (Program.Menu.Item("OnlyNearMouse").GetValue<bool>())
                {
                    Utility.DrawCircle(Game.CursorPos, Program.Menu.Item("MRadius").GetValue<Slider>().Value, Color.White);
                }
            }
        }

         private static Obj_AI_Hero GetTargetNearMouse(float distance)
        {
            Obj_AI_Hero bestTarget = null;
            var bestRatio = 0f;

            if (SimpleTs.SelectedTarget.IsValidTarget() && !SimpleTs.IsInvulnerable(SimpleTs.SelectedTarget) &&
                (Game.CursorPos.Distance(SimpleTs.SelectedTarget.ServerPosition) < distance && ObjectManager.Player.Distance(SimpleTs.SelectedTarget) < R.Range))
            {
                return SimpleTs.SelectedTarget;
            }

            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (!hero.IsValidTarget(R.Range) || SimpleTs.IsInvulnerable(hero) || Game.CursorPos.Distance(hero.ServerPosition) > distance)
                {
                    continue;
                }

                var damage = (float)ObjectManager.Player.CalcDamage(hero, Damage.DamageType.Magical, 100);
                var ratio = damage / (1 + hero.Health) * SimpleTs.GetPriority(hero);

                if (ratio > bestRatio)
                {
                    bestRatio = ratio;
                    bestTarget = hero;
                }
            }

            return bestTarget;
        }

        private void Drawing_OnEndScene(EventArgs args)
        {
            if (R.Level == 0) return;
            var menuItem = Program.Menu.Item("RRangeM").GetValue<Circle>();
            if (menuItem.Active)
                Utility.DrawCircle(ObjectManager.Player.Position, R.Range, menuItem.Color, 2, 30, true);
        }
    }
}
