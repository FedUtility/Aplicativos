#region
using System;
using System.Collections.Generic;
using Color = System.Drawing.Color;
using System.Linq;
using SharpDX;
using LeagueSharp;
using LeagueSharp.Common;
using LX_Orbwalker;
#endregion

namespace FedAllChampionsUtility
{
    class Ryze : Champion
    {        
        private static string LastCast;
        private static float LastFlashTime;
        private static Obj_AI_Hero target;        
        private static bool UseShield;

        private static Spell Q, W, E, R;        
        private static SpellSlot IgniteSlot;

        public Ryze()
        {
            LoadSpells();
            LoadMenu();

            Game.OnGameUpdate += Game_OnGameUpdate;
            LXOrbwalker.BeforeAttack += OrbwalkingOnBeforeAttack;
            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;	

            PluginLoaded();
        }

        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 625);
            W = new Spell(SpellSlot.W, 600);
            E = new Spell(SpellSlot.E, 600);
            R = new Spell(SpellSlot.R);

            IgniteSlot = ObjectManager.Player.GetSpellSlot("SummonerDot");
        }

        private void LoadMenu()
        {
            Program.Menu.AddSubMenu(new Menu("Combo", "Combo"));                        
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("TypeCombo", "Mode: ").SetValue(new StringList(new[] { "Mixed ", "Burst ", "Long " }, 0)));
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("UseR", "Use R").SetValue(true));
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("UseIgnite", "Use Ignite").SetValue(true));

            Program.Menu.AddSubMenu(new Menu("Harass", "Harass"));            
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("HQ", "Use Q").SetValue(true));
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("HW", "Use W").SetValue(true));
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("HE", "Use E").SetValue(true));

            Program.Menu.AddSubMenu(new Menu("Farm", "Farm"));            
            Program.Menu.SubMenu("Farm").AddItem(new MenuItem("FQ", "Use Q").SetValue(true));
            Program.Menu.SubMenu("Farm").AddItem(new MenuItem("FW", "Use W").SetValue(true));
            Program.Menu.SubMenu("Farm").AddItem(new MenuItem("FE", "Use E").SetValue(true));

            Program.Menu.AddSubMenu(new Menu("JungleFarm", "JungleFarm"));            
            Program.Menu.SubMenu("JungleFarm").AddItem(new MenuItem("JQ", "Use Q").SetValue(true));
            Program.Menu.SubMenu("JungleFarm").AddItem(new MenuItem("JW", "Use W").SetValue(true));
            Program.Menu.SubMenu("JungleFarm").AddItem(new MenuItem("JE", "Use E").SetValue(true));

            Program.Menu.AddSubMenu(new Menu("KillSteal", "KillSteal"));
            Program.Menu.SubMenu("KillSteal").AddItem(new MenuItem("KillSteal", "Use Kill Steal").SetValue(true));
            Program.Menu.SubMenu("KillSteal").AddItem(new MenuItem("AutoIgnite", "Use Ignite").SetValue(true));

            Program.Menu.AddSubMenu(new Menu("Extra", "Extra"));
            Program.Menu.SubMenu("Extra").AddItem(new MenuItem("UseSera", "Use Seraphs Embrace").SetValue(true));
            Program.Menu.SubMenu("Extra").AddItem(new MenuItem("HP", "When % HP").SetValue(new Slider(20, 100, 0)));
            Program.Menu.SubMenu("Extra").AddItem(new MenuItem("UseWGap", "Use W GapCloser").SetValue(true));            

            Program.Menu.AddSubMenu(new Menu("Drawings", "Drawings"));
            Program.Menu.SubMenu("Drawings").AddItem(new MenuItem("QRange", "Q range").SetValue(new Circle(true, Color.FromArgb(255, 255, 255, 255))));
            Program.Menu.SubMenu("Drawings").AddItem(new MenuItem("WERange", "W+E range").SetValue(new Circle(false, Color.FromArgb(255, 255, 255, 255))));
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            target = SimpleTs.GetTarget(Q.Range + 25, SimpleTs.DamageType.Magical);

            if (LXOrbwalker.CurrentMode == LXOrbwalker.Mode.Combo)
            {
                if (Program.Menu.Item("TypeCombo").GetValue<StringList>().SelectedIndex == 0) ComboMixed();
                else if (Program.Menu.Item("TypeCombo").GetValue<StringList>().SelectedIndex == 1) ComboBurst();
                else if (Program.Menu.Item("TypeCombo").GetValue<StringList>().SelectedIndex == 2) ComboLong();
            }

            if (LXOrbwalker.CurrentMode == LXOrbwalker.Mode.Harass)
            {
                Harass();
            }

            var lc = LXOrbwalker.CurrentMode == LXOrbwalker.Mode.LaneClear;
            if (lc || LXOrbwalker.CurrentMode == LXOrbwalker.Mode.Harass)
                Farm(lc);

            if (LXOrbwalker.CurrentMode == LXOrbwalker.Mode.LaneClear)
            {
                JungleFarm();
            }

            if (Program.Menu.Item("UseSera").GetValue<bool>())
            {
                UseItems();
            }

            if (Program.Menu.Item("KillSteal").GetValue<bool>())
            {
                KillSteal();
            }
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            var drawQ = Program.Menu.Item("QRange").GetValue<Circle>();
            if (drawQ.Active && !ObjectManager.Player.IsDead)
            {
                Utility.DrawCircle(ObjectManager.Player.Position, Q.Range, drawQ.Color);
            }

            var drawWE = Program.Menu.Item("WERange").GetValue<Circle>();
            if (drawWE.Active && !ObjectManager.Player.IsDead)
            {
                Utility.DrawCircle(ObjectManager.Player.Position, W.Range, drawWE.Color);
            }
        }

        private static void OrbwalkingOnBeforeAttack(LXOrbwalker.BeforeAttackEventArgs args)
        {
            if (LXOrbwalker.CurrentMode == LXOrbwalker.Mode.Combo)
                args.Process = !(Q.IsReady() || W.IsReady() || E.IsReady() ||ObjectManager.Player.Distance(args.Target) >= 600);
        }

        private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            var UseW = Program.Menu.Item("UseWGap").GetValue<bool>();
            
            if (ObjectManager.Player.HasBuff("Recall") || ObjectManager.Player.IsWindingUp) return;
            if (UseW && W.IsReady()) W.CastOnUnit(gapcloser.Sender, Packets());
        }

        private void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                if (args.SData.Name.ToLower() == "overload")
                {
                    LastCast = "Q";
                }
                else if (args.SData.Name.ToLower() == "runeprison")
                {
                    LastCast = "W";
                }
                else if (args.SData.Name.ToLower() == "spellflux")
                {
                    LastCast = "E";
                }
                else if (args.SData.Name.ToLower() == "desperatepower")
                {
                    LastCast = "R";
                }
                else if (args.SData.Name.ToLower() == "summonerflash")
                {
                    LastFlashTime = Environment.TickCount;
                }
            }
            if (sender.IsEnemy && (sender.Type == GameObjectType.obj_AI_Hero || sender.Type == GameObjectType.obj_AI_Turret))
            {
                if (SpellData.SpellName.Any(Each => Each.Contains(args.SData.Name)) || (args.Target == ObjectManager.Player && ObjectManager.Player.Distance(sender) <= 700))
                    UseShield = true;
            }
        }

        private bool IsFacing(Obj_AI_Base enemy)
        {
            if (enemy.Path.Count() > 0 && enemy.Path[0].Distance(ObjectManager.Player.ServerPosition) > ObjectManager.Player.Distance(enemy))
                return false;
            else return true;
        }

        public bool IgniteKillable(Obj_AI_Hero source, Obj_AI_Base target)
        {
            return Damage.GetSummonerSpellDamage(ObjectManager.Player, target, Damage.SummonerSpell.Ignite) > target.Health;
        }

        public bool IsKillable(Obj_AI_Hero source, Obj_AI_Base target, IEnumerable<SpellSlot> spellCombo)
        {
            return Damage.GetComboDamage(source, target, spellCombo) > target.Health;
        }

        public float GetDistanceSqr(Obj_AI_Hero source, Obj_AI_Base target)
        {
            return Vector2.DistanceSquared(source.Position.To2D(), target.ServerPosition.To2D());
        }

        private void UseItems()
        {
            var myHP = ObjectManager.Player.Health / ObjectManager.Player.MaxHealth * 100;
            var ConfigHP = Program.Menu.Item("HP").GetValue<Slider>().Value;
            if (myHP <= ConfigHP && Items.HasItem(3040) && Items.CanUseItem(3040) && UseShield)
            {
                Items.UseItem(3040);
                UseShield = false;
            }
        }

        private void ComboMixed()
        {
            var UseR = Program.Menu.Item("UseR").GetValue<bool>();
            var UseIgnite = Program.Menu.Item("UseIgnite").GetValue<bool>();
            
            if (target == null)
            {
                return;
            }
            if (UseIgnite && IgniteKillable(ObjectManager.Player, target))
            {
                if (IgniteSlot != SpellSlot.Unknown && ObjectManager.Player.SummonerSpellbook.CanUseSpell(IgniteSlot) == SpellState.Ready && ObjectManager.Player.Distance(target) <= 600)
                    ObjectManager.Player.SummonerSpellbook.CastSpell(IgniteSlot, target);
            }
            if (Environment.TickCount - LastFlashTime < 1 && W.IsReady()) W.CastOnUnit(target, Packets());
            else
            {
                if (IsKillable(ObjectManager.Player, target, new[] { SpellSlot.Q }) && Q.IsReady())
                {
                    Q.CastOnUnit(target, Packets());
                }
                else if (IsKillable(ObjectManager.Player, target, new[] { SpellSlot.E }) && E.IsReady())
                {
                    E.CastOnUnit(target, Packets());
                }
                else if (IsKillable(ObjectManager.Player, target, new[] { SpellSlot.W }) && W.IsReady())
                {
                    W.CastOnUnit(target, Packets());
                }
                else if (ObjectManager.Player.Distance(target) >= 575 && !IsFacing(target) && W.IsReady())
                {
                    W.CastOnUnit(target, Packets());
                }
                else
                {
                    if (Q.IsReady() && W.IsReady() && E.IsReady() && IsKillable(ObjectManager.Player, target, new[] { SpellSlot.Q, SpellSlot.W, SpellSlot.E }))
                    {
                        if (Q.IsReady()) Q.CastOnUnit(target, Packets());
                        if (R.IsReady() && UseR) CastR();
                        if (W.IsReady()) W.CastOnUnit(target, Packets());
                        if (E.IsReady()) E.CastOnUnit(target, Packets());
                    }
                    else if (Math.Abs(ObjectManager.Player.PercentCooldownMod) >= 0.2)
                    {
                        if (CountEnemyInRange(target, 300) > 1)
                        {
                            if (LastCast == "Q")
                            {
                                if (Q.IsReady()) Q.CastOnUnit(target, Packets());
                                if (R.IsReady() && UseR) CastR();
                                if (!R.IsReady()) W.CastOnUnit(target, Packets());
                                if (!R.IsReady() && !W.IsReady()) E.CastOnUnit(target, Packets());
                            }
                            else Q.CastOnUnit(target, Packets());
                        }
                        else
                        {
                            if (LastCast == "Q")
                            {
                                if (Q.IsReady()) Q.CastOnUnit(target, Packets());
                                if (W.IsReady()) W.CastOnUnit(target, Packets());
                                if (!W.IsReady()) E.CastOnUnit(target, Packets());
                                if (!W.IsReady() && !E.IsReady() && UseR) CastR();
                            }
                            else
                            {
                                if (Q.IsReady()) Q.CastOnUnit(target, Packets());
                            }
                        }
                    }
                    else
                    {
                        if (Q.IsReady()) Q.CastOnUnit(target, Packets());
                        else if (R.IsReady() && UseR) CastR();
                        else if (E.IsReady()) E.CastOnUnit(target, Packets());
                        else if (W.IsReady()) W.CastOnUnit(target, Packets());
                    }
                }
            }
        }

        private void ComboBurst()
        {
            var UseR = Program.Menu.Item("UseR").GetValue<bool>();
            var UseIgnite = Program.Menu.Item("UseIgnite").GetValue<bool>();
            
            if (target == null) return;
            if (UseIgnite && IgniteKillable(ObjectManager.Player, target))
            {
                if (IgniteSlot != SpellSlot.Unknown && ObjectManager.Player.SummonerSpellbook.CanUseSpell(IgniteSlot) == SpellState.Ready && ObjectManager.Player.Distance(target) <= 600)
                    ObjectManager.Player.SummonerSpellbook.CastSpell(IgniteSlot, target);
            }
            if (Environment.TickCount - LastFlashTime < 1 && W.IsReady()) W.CastOnUnit(target, Packets());
            else
            {
                if (IsKillable(ObjectManager.Player, target, new[] { SpellSlot.Q }) && Q.IsReady())
                {
                    Q.CastOnUnit(target, Packets());
                }
                else if (IsKillable(ObjectManager.Player, target, new[] { SpellSlot.E }) && E.IsReady())
                {
                    E.CastOnUnit(target, Packets());
                }
                else if (IsKillable(ObjectManager.Player, target, new[] { SpellSlot.W }) && W.IsReady())
                {
                    W.CastOnUnit(target, Packets());
                }
                else if (ObjectManager.Player.Distance(target) >= 575 && !IsFacing(target) && W.IsReady())
                    W.CastOnUnit(target, Packets());
                else
                {
                    if (Q.IsReady()) Q.CastOnUnit(target, Packets());
                    else if (R.IsReady() && UseR) CastR();
                    else if (E.IsReady()) E.CastOnUnit(target, Packets());
                    else if (W.IsReady()) W.CastOnUnit(target, Packets());
                }
            }
        }

        private void ComboLong()
        {
            var UseR = Program.Menu.Item("UseR").GetValue<bool>();
            var UseIgnite = Program.Menu.Item("UseIgnite").GetValue<bool>();
            
            if (target == null) return;
            if (UseIgnite && IgniteKillable(ObjectManager.Player, target))
            {
                if (IgniteSlot != SpellSlot.Unknown && ObjectManager.Player.SummonerSpellbook.CanUseSpell(IgniteSlot) == SpellState.Ready && ObjectManager.Player.Distance(target) <= 600)
                    ObjectManager.Player.SummonerSpellbook.CastSpell(IgniteSlot, target);
            }
            if (Environment.TickCount - LastFlashTime < 1 && W.IsReady()) W.CastOnUnit(target, Packets());
            else
            {
                if (IsKillable(ObjectManager.Player, target, new[] { SpellSlot.Q }) && Q.IsReady())
                {
                    Q.CastOnUnit(target, Packets());
                }
                else if (IsKillable(ObjectManager.Player, target, new[] { SpellSlot.E }) && E.IsReady())
                {
                    E.CastOnUnit(target, Packets());
                }
                else if (IsKillable(ObjectManager.Player, target, new[] { SpellSlot.W }) && W.IsReady())
                {
                    W.CastOnUnit(target, Packets());
                }
                else if (ObjectManager.Player.Distance(target) >= 575 && !IsFacing(target) && W.IsReady())
                    W.CastOnUnit(target, Packets());
                else
                {
                    if (CountEnemyInRange(target, 300) > 1)
                    {
                        if (LastCast == "Q")
                        {
                            if (Q.IsReady()) Q.CastOnUnit(target, Packets());
                            if (R.IsReady() && UseR) CastR();
                            if (!R.IsReady()) W.CastOnUnit(target, Packets());
                            if (!R.IsReady() && !W.IsReady()) E.CastOnUnit(target, Packets());
                        }
                        else Q.CastOnUnit(target, Packets());
                    }
                    else
                    {
                        if (LastCast == "Q")
                        {
                            if (Q.IsReady()) Q.CastOnUnit(target, Packets());
                            if (W.IsReady()) W.CastOnUnit(target, Packets());
                            if (!W.IsReady()) E.CastOnUnit(target, Packets());
                            if (!W.IsReady() && !E.IsReady() && R.IsReady() && UseR) CastR();
                        }
                        else
                        {
                            if (Q.IsReady()) Q.CastOnUnit(target, Packets());
                        }
                    }
                }
            }
        }

        private void Harass()
        {
            var UseQ = Program.Menu.Item("HQ").GetValue<bool>();
            var UseW = Program.Menu.Item("HW").GetValue<bool>();
            var UseE = Program.Menu.Item("HE").GetValue<bool>();
            
            if (ObjectManager.Player.Distance(target) <= 625)
            {
                if (UseQ && Q.IsReady()) Q.CastOnUnit(target, Packets());
                if (UseW && W.IsReady()) W.CastOnUnit(target, Packets());
                if (UseE && E.IsReady()) E.CastOnUnit(target, Packets());
            }
        }

        private void Farm(bool laneClear)
        {
            var UseQ = Program.Menu.Item("FQ").GetValue<bool>();
            var UseW = Program.Menu.Item("FW").GetValue<bool>();
            var UseE = Program.Menu.Item("FE").GetValue<bool>();
            
            var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.All, MinionOrderTypes.MaxHealth);

            if (!laneClear)
            {
                if (UseQ && Q.IsReady())
                {
                    foreach (var minion in allMinions)
                    {
                        if (minion.IsValidTarget() && HealthPrediction.GetHealthPrediction(minion, (int)(ObjectManager.Player.Distance(minion) * 1000 / 1400)) <=
                            Damage.GetComboDamage(ObjectManager.Player, minion, new[] { SpellSlot.Q }))
                        {
                            Q.CastOnUnit(minion, Packets());
                            return;
                        }
                    }
                }
                else if (UseW && W.IsReady())
                {
                    foreach (var minion in allMinions)
                    {
                        if (minion.IsValidTarget(W.Range) && minion.Health < Damage.GetComboDamage(ObjectManager.Player, minion, new[] { SpellSlot.W }))
                        {
                            W.CastOnUnit(minion, Packets());
                            return;
                        }
                    }
                }
                else if (UseE && E.IsReady())
                {
                    foreach (var minion in allMinions)
                    {
                        if (minion.IsValidTarget(E.Range) && HealthPrediction.GetHealthPrediction(minion, (int)(ObjectManager.Player.Distance(minion) * 1000 / 1000)) <=
                                                                    Damage.GetComboDamage(ObjectManager.Player, minion, new[] { SpellSlot.E }))
                        {
                            E.CastOnUnit(minion, Packets());
                            return;
                        }
                    }
                }
            }
            else
            {
                foreach (var minion in allMinions)
                {
                    if (UseQ && Q.IsReady()) Q.CastOnUnit(minion, Packets());
                    if (UseW && W.IsReady()) W.CastOnUnit(minion, Packets());
                    if (UseE && E.IsReady()) E.CastOnUnit(minion, Packets());
                }
            }
        }

        private void JungleFarm()
        {
            var UseQ = Program.Menu.Item("JQ").GetValue<bool>();
            var UseW = Program.Menu.Item("JW").GetValue<bool>();
            var UseE = Program.Menu.Item("JE").GetValue<bool>();
            
            var jungminions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            if (jungminions.Count > 0)
            {
                var minion = jungminions[0];
                if (UseQ && Q.IsReady()) Q.CastOnUnit(minion, Packets());
                if (UseW && W.IsReady()) W.CastOnUnit(minion, Packets());
                if (UseE && E.IsReady()) E.CastOnUnit(minion, Packets());
            }
        }

        private void KillSteal()
        {
            var AutoIgnite = Program.Menu.Item("AutoIgnite").GetValue<bool>();
            var KillSteal = Program.Menu.Item("KillSteal").GetValue<bool>();
            
            if (AutoIgnite && IgniteSlot != SpellSlot.Unknown && ObjectManager.Player.SummonerSpellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
            {
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => ObjectManager.Player.Distance(enemy) <= 600 && enemy.IsEnemy && enemy.IsVisible && !enemy.IsDead))
                {
                    if (IgniteKillable(ObjectManager.Player, enemy))
                        ObjectManager.Player.SummonerSpellbook.CastSpell(IgniteSlot, enemy);
                }
            }
            if (KillSteal & (Q.IsReady() || W.IsReady() || E.IsReady()))
            {
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => ObjectManager.Player.Distance(enemy) <= Q.Range && enemy.IsEnemy && enemy.IsVisible && !enemy.IsDead))
                {
                    if (Q.IsReady() && IsKillable(ObjectManager.Player, enemy, new[] { SpellSlot.Q })) Q.CastOnUnit(enemy, Packets());
                    if (W.IsReady() && IsKillable(ObjectManager.Player, enemy, new[] { SpellSlot.W })) W.CastOnUnit(enemy, Packets());
                    if (E.IsReady() && IsKillable(ObjectManager.Player, enemy, new[] { SpellSlot.E })) E.CastOnUnit(enemy, Packets());
                }

            }
        }

        private void CastR()
        {            
            if (Packets()) Packet.C2S.Cast.Encoded(new Packet.C2S.Cast.Struct(0, SpellSlot.R)).Send();
            else R.Cast();
        }

        private int CountEnemyInRange(Obj_AI_Hero target, float range)
        {
            int count = 0;
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => ObjectManager.Player.Distance3D(enemy, true) <= range * range && enemy.IsEnemy && enemy.IsVisible && !enemy.IsDead))
            {
                count = count + 1;
            }
            return count;
        }        
    }
    
}
