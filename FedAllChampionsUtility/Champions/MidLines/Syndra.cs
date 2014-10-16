#region

using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

#endregion

namespace FedAllChampionsUtility
{
    class Syndra : Champion
    {
        public static Spell Q, W, E, R, EQ;
        public static List<Spell> spellList = new List<Spell>();

        public static int QEComboT;
        public static int WEComboT;

        public static SpellSlot IgniteSlot;
        public static Items.Item DFG;

        public Syndra()
        {
            LoadMenu();
            LoadSpells();
            
            Game.OnGameUpdate += Game_OnGameUpdate;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
            Drawing.OnDraw += Drawing_OnDraw;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPossibleToInterrupt;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Orbwalking.BeforeAttack += Orbwalking_BeforeAttack;

            PluginLoaded();
        }

        private void LoadSpells()
        {
            //Create the spells
            Q = new Spell(SpellSlot.Q, 790);
            W = new Spell(SpellSlot.W, 925);
            E = new Spell(SpellSlot.E, 700);
            R = new Spell(SpellSlot.R, 675);
            EQ = new Spell(SpellSlot.Q, Q.Range + 500);

            IgniteSlot = ObjectManager.Player.GetSpellSlot("SummonerDot");
            DFG = Utility.Map.GetMap()._MapType == Utility.Map.MapType.TwistedTreeline ? new Items.Item(3188, 750) : new Items.Item(3128, 750);

            Q.SetSkillshot(0.6f, 100f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            W.SetSkillshot(0.25f, 175f, 1475f, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.25f, (float)(45 * 0.5), 2500f, false, SkillshotType.SkillshotCircle);
            EQ.SetSkillshot(float.MaxValue, 55f, 2100f, false, SkillshotType.SkillshotCircle);

            // Add to spell list
            spellList.AddRange(new[] { Q, W, E, R });
        }

        private void LoadMenu()
        {
            Program.Menu.AddSubMenu(new Menu("TeamFight", "TeamFight"));
            Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
            Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
            Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
            Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("UseQECombo", "Use QE").SetValue(true));
            Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("UseRCombo", "Use R").SetValue(true));
            Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("UseIgniteCombo", "Use Ignite").SetValue(true));
            Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("ComboActive", "Combo!").SetValue<KeyBind>(new KeyBind(32, KeyBindType.Press)));

            Program.Menu.AddSubMenu(new Menu("Harass", "Harass"));
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(true));
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("UseWHarass", "Use W").SetValue(false));
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("UseEHarass", "Use E").SetValue(false));
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("UseQEHarass", "Use QE").SetValue(false));
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("HarassActiveT", "Harass (toggle)!").SetValue<KeyBind>(new KeyBind('Y', KeyBindType.Toggle)));
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("HarassActive", "Harass!").SetValue<KeyBind>(new KeyBind('C', KeyBindType.Press)));
            AddManaManager("Harass", 35);

            Program.Menu.AddSubMenu(new Menu("LaneClear", "LaneClear"));
            Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("UseQFarm", "Use Q").SetValue(new StringList(new[] { "Freeze", "LaneClear", "Both", "No" }, 2)));
            Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("UseWFarm", "Use W").SetValue(new StringList(new[] { "Freeze", "LaneClear", "Both", "No" }, 1)));
            Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("UseEFarm", "Use E").SetValue(new StringList(new[] { "Freeze", "LaneClear", "Both", "No" }, 3)));
            Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("FreezeActive", "Freeze!").SetValue<KeyBind>(new KeyBind('C', KeyBindType.Press)));
            Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("LaneClearActive", "LaneClear!").SetValue<KeyBind>(new KeyBind('V', KeyBindType.Press)));

            //JungleFarm menu:
            Program.Menu.AddSubMenu(new Menu("JungleFarm", "JungleFarm"));
            Program.Menu.SubMenu("JungleFarm").AddItem(new MenuItem("UseQJFarm", "Use Q").SetValue(true));
            Program.Menu.SubMenu("JungleFarm").AddItem(new MenuItem("UseWJFarm", "Use W").SetValue(true));
            Program.Menu.SubMenu("JungleFarm").AddItem(new MenuItem("UseEJFarm", "Use E").SetValue(true));
            Program.Menu.SubMenu("JungleFarm").AddItem(new MenuItem("JungleFarmActive", "JungleFarm!").SetValue<KeyBind>(new KeyBind('V', KeyBindType.Press)));

            Program.Menu.AddSubMenu(new Menu("Misc", "Misc"));
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("UseGap", "Use E for GapCloser").SetValue(true));
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("InterruptSpells", "Interrupt spells").SetValue(true));
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("CastQE", "QE closest to cursor").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
            Program.Menu.SubMenu("Misc").AddSubMenu(new Menu("Dont use R on", "DontUlt"));

            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != ObjectManager.Player.Team))
                Program.Menu.SubMenu("Misc").SubMenu("DontUlt").AddItem(new MenuItem("DontUlt" + enemy.BaseSkinName, enemy.BaseSkinName).SetValue(false));

            //Damage after combo:
            var dmgAfterComboItem = new MenuItem("DamageAfterCombo", "Draw damage after combo").SetValue(true);
            Utility.HpBarDamageIndicator.DamageToUnit = GetComboDamage;
            Utility.HpBarDamageIndicator.Enabled = dmgAfterComboItem.GetValue<bool>();
            dmgAfterComboItem.ValueChanged += delegate(object sender, OnValueChangeEventArgs eventArgs)
            {
                Utility.HpBarDamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
            };
            
            //Drawings menu:
            Program.Menu.AddSubMenu(new Menu("Drawings", "Drawings"));
            Program.Menu.SubMenu("Drawings")
                .AddItem(new MenuItem("QRange", "Q range").SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
            Program.Menu.SubMenu("Drawings")
                .AddItem(new MenuItem("WRange", "W range").SetValue(new Circle(true, Color.FromArgb(100, 255, 0, 255))));
            Program.Menu.SubMenu("Drawings")
                .AddItem(new MenuItem("ERange", "E range").SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
            Program.Menu.SubMenu("Drawings")
                .AddItem(new MenuItem("RRange", "R range").SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
            Program.Menu.SubMenu("Drawings")
                .AddItem(new MenuItem("QERange", "QE range").SetValue(new Circle(true, Color.FromArgb(100, 255, 0, 255))));
            Program.Menu.SubMenu("Drawings").AddItem(dmgAfterComboItem);
        }

        private void Orbwalking_BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (Program.Menu.Item("ComboActive").GetValue<KeyBind>().Active)
                args.Process = !(Q.IsReady() || W.IsReady());
        }

        private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!Program.Menu.Item("UseGap").GetValue<bool>()) return;

            if (E.IsReady() && gapcloser.Sender.IsValidTarget(E.Range))
                E.Cast(gapcloser.Sender);
        }

        private void Interrupter_OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (!Program.Menu.Item("InterruptSpells").GetValue<bool>()) return;

            if (ObjectManager.Player.Distance(unit) < E.Range && E.IsReady() && Q.IsReady())
            {
                Q.Cast(unit.ServerPosition);
                E.Cast(unit.ServerPosition);
            }
            else if (ObjectManager.Player.Distance(unit) < EQ.Range && E.IsReady() && Q.IsReady())
            {
                UseQE(unit);
            }
        }

        private void Combo()
        {
            UseSpells(Program.Menu.Item("UseQCombo").GetValue<bool>(), Program.Menu.Item("UseWCombo").GetValue<bool>(),
                Program.Menu.Item("UseECombo").GetValue<bool>(), Program.Menu.Item("UseRCombo").GetValue<bool>(),
                Program.Menu.Item("UseQECombo").GetValue<bool>(), Program.Menu.Item("UseIgniteCombo").GetValue<bool>(), false);
        }

        private void Harass()
        {
            UseSpells(Program.Menu.Item("UseQHarass").GetValue<bool>(), Program.Menu.Item("UseWHarass").GetValue<bool>(),
                Program.Menu.Item("UseEHarass").GetValue<bool>(), false, Program.Menu.Item("UseQEHarass").GetValue<bool>(), false, true);
        }

        private void UseE(Obj_AI_Base enemy)
        {
            foreach (var orb in OrbManager.GetOrbs(true))
                if (ObjectManager.Player.Distance(orb) < E.Range + 100 && E.IsReady())
                {
                    var startPoint = orb.To2D().Extend(ObjectManager.Player.ServerPosition.To2D(), 100);
                    var endPoint = ObjectManager.Player.ServerPosition.To2D()
                        .Extend(orb.To2D(), ObjectManager.Player.Distance(orb) > 200 ? 1300 : 1000);
                    EQ.Delay = E.Delay + ObjectManager.Player.Distance(orb) / E.Speed;
                    EQ.From = orb;
                    var enemyPred = EQ.GetPrediction(enemy);
                    if (enemyPred.Hitchance >= HitChance.High &&
                        enemyPred.UnitPosition.To2D().Distance(startPoint, endPoint, false) <
                        EQ.Width + enemy.BoundingRadius)
                    {
                        E.Cast(orb, Packets());
                        E.LastCastAttemptT = Environment.TickCount;
                        return;
                    }
                }
        }

        private void UseQE(Obj_AI_Base enemy)
        {
            EQ.Delay = Q.Range / E.Speed;
            EQ.From = ObjectManager.Player.ServerPosition.To2D().Extend(enemy.ServerPosition.To2D(), Q.Range).To3D();

            var prediction = EQ.GetPrediction(enemy);
            if (prediction.Hitchance >= HitChance.High && Q.IsReady())
            {
                Q.Cast(ObjectManager.Player.ServerPosition.To2D().Extend(prediction.CastPosition.To2D(), Q.Range - 100));
                QEComboT = Environment.TickCount;
                W.LastCastAttemptT = Environment.TickCount;
            }
        }

        private Vector3 GetGrabableObjectPos(bool onlyOrbs)
        {
            if (!onlyOrbs)
                foreach (var minion in ObjectManager.Get<Obj_AI_Minion>().Where(minion => minion.IsValidTarget(W.Range))
                    )
                    return minion.ServerPosition;

            return OrbManager.GetOrbToGrab((int)W.Range);
        }

        private static float GetComboDamage(Obj_AI_Base enemy)
        {
            var damage = 0d;

            if (Q.IsReady(420))
                damage += ObjectManager.Player.GetSpellDamage(enemy, SpellSlot.Q) - 10;

            if (DFG.IsReady())
                damage += ObjectManager.Player.GetItemDamage(enemy, Damage.DamageItems.Dfg) / 1.2;

            if (W.IsReady())
                damage += ObjectManager.Player.GetSpellDamage(enemy, SpellSlot.W);

            if (E.IsReady())
                damage += ObjectManager.Player.GetSpellDamage(enemy, SpellSlot.E);

            if (R.IsReady())
                damage += Math.Min(7, ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).Ammo) * ObjectManager.Player.GetSpellDamage(enemy, SpellSlot.R, 1) - 20;

            return (float)damage * (DFG.IsReady() ? 1.2f : 1);
        }

        public static float getUltDmg(Obj_AI_Base enemy)
        {
            var damage = 0d;

            if (DFG.IsReady())
                damage += ObjectManager.Player.GetItemDamage(enemy, Damage.DamageItems.Dfg) / 1.2;

            if (R.IsReady())
                damage += Math.Min(7, ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).Ammo) * ObjectManager.Player.GetSpellDamage(enemy, SpellSlot.R, 1) - 20;

            return (float)damage * (DFG.IsReady() ? 1.2f : 1);
        }

        private void UseSpells(bool useQ, bool useW, bool useE, bool useR, bool useQE, bool useIgnite, bool isHarass)
        {
            var qTarget = SimpleTs.GetTarget(Q.Range + (isHarass ? Q.Width / 3 : Q.Width), SimpleTs.DamageType.Magical);
            var wTarget = SimpleTs.GetTarget(W.Range + W.Width, SimpleTs.DamageType.Magical);
            var rTarget = SimpleTs.GetTarget(R.Range, SimpleTs.DamageType.Magical);
            var qeTarget = SimpleTs.GetTarget(EQ.Range, SimpleTs.DamageType.Magical);
            var comboDamage = rTarget != null ? GetComboDamage(rTarget) : 0;

            //Q
            if (qTarget != null && useQ && Q.GetPrediction(qTarget).Hitchance >= HitChance.High && Q.IsReady())
                Q.Cast(qTarget, Packets(), true);

            //E
            if (Environment.TickCount - W.LastCastAttemptT > Game.Ping + 150 && E.IsReady() && useE)
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>())
                {
                    if (enemy.IsValidTarget(EQ.Range))
                        UseE(enemy);
                }

            //W
            if (useW && W.IsReady())
                if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).ToggleState == 1 && qeTarget != null)
                {
                    //WObject
                    var gObjectPos = GetGrabableObjectPos(wTarget == null);

                    if (gObjectPos.To2D().IsValid() && Environment.TickCount - W.LastCastAttemptT > Game.Ping + 150 && Environment.TickCount - E.LastCastAttemptT > Game.Ping + 150)
                    {
                        W.Cast(gObjectPos);
                        W.LastCastAttemptT = Environment.TickCount;
                    }
                }
                else if (wTarget != null && ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).ToggleState != 1 &&
                         Environment.TickCount - W.LastCastAttemptT > Game.Ping + 100)
                {
                    if (W.InRange(wTarget.Position) && W.GetPrediction(wTarget).Hitchance >= HitChance.High)
                    {
                        W.Cast(wTarget, Packets(), true);
                    }
                }

            if (rTarget != null)
                useR = (Program.Menu.Item("DontUlt" + rTarget.BaseSkinName) != null &&
                        Program.Menu.Item("DontUlt" + rTarget.BaseSkinName).GetValue<bool>() == false) && useR;

            //DFG (and ult if ready)
            if (rTarget != null && useR && getUltDmg(rTarget) > rTarget.Health && DFG.IsReady())
            {
                DFG.Cast(rTarget);
                if (R.IsReady())
                {
                    R.Cast(rTarget);
                }
            }

            //R
            if (rTarget != null && useR && R.IsReady() && !Q.IsReady() && !DFG.IsReady())
            {
                if (getUltDmg(rTarget) > rTarget.Health)
                {
                    R.Cast(rTarget);
                }
            }

            //Ignite
            if (rTarget != null && useIgnite && IgniteSlot != SpellSlot.Unknown &&
                ObjectManager.Player.SummonerSpellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
            {
                if (comboDamage > rTarget.Health)
                {
                    ObjectManager.Player.SummonerSpellbook.CastSpell(IgniteSlot, rTarget);
                }
            }

            //QE
            if (wTarget == null && qeTarget != null && Q.IsReady() && E.IsReady() && useQE)
                UseQE(qeTarget);

            //WE
            if (wTarget == null && qeTarget != null && E.IsReady() && useE && OrbManager.WObject(true) != null && W.IsReady())
            {
                EQ.Delay = E.Delay + Q.Range / W.Speed;
                EQ.From = ObjectManager.Player.ServerPosition.To2D().Extend(qeTarget.ServerPosition.To2D(), Q.Range).To3D();
                var prediction = EQ.GetPrediction(qeTarget);
                if (prediction.Hitchance >= HitChance.High)
                {
                    W.Cast(ObjectManager.Player.ServerPosition.To2D().Extend(prediction.CastPosition.To2D(), Q.Range - 100));
                    WEComboT = Environment.TickCount;
                }
            }
        }

        private void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && Environment.TickCount - QEComboT < 500 && E.IsReady() &&
                (args.SData.Name == "SyndraQ"))
            {
                W.LastCastAttemptT = Environment.TickCount + 400;
                E.Cast(args.End, Packets());
            }

            if (sender.IsMe && Environment.TickCount - WEComboT < 500 &&
                (args.SData.Name == "SyndraW" || args.SData.Name == "syndrawcast") && E.IsReady())
            {
                W.LastCastAttemptT = Environment.TickCount + 400;
                E.Cast(args.End, Packets());
            }
        }

        private void Farm(bool laneClear)
        {
            if (!Orbwalking.CanMove(40)) return;

            var rangedMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range + Q.Width + 30,
                MinionTypes.Ranged);
            var allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range + Q.Width + 30,
                MinionTypes.All);
            var rangedMinionsW = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, W.Range + W.Width + 30,
                MinionTypes.Ranged);
            var allMinionsW = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, W.Range + W.Width + 30,
                MinionTypes.All);

            var useQi = Program.Menu.Item("UseQFarm").GetValue<StringList>().SelectedIndex;
            var useWi = Program.Menu.Item("UseWFarm").GetValue<StringList>().SelectedIndex;
            var useEi = Program.Menu.Item("UseEFarm").GetValue<StringList>().SelectedIndex;
            var useQ = (laneClear && (useQi == 1 || useQi == 2)) || (!laneClear && (useQi == 0 || useQi == 2));
            var useW = (laneClear && (useWi == 1 || useWi == 2)) || (!laneClear && (useWi == 0 || useWi == 2));
            var useE = (laneClear && (useEi == 1 || useEi == 2)) || (!laneClear && (useEi == 0 || useEi == 2));

            if (useQ && Q.IsReady())
                if (laneClear)
                {
                    var fl1 = Q.GetCircularFarmLocation(rangedMinionsQ, Q.Width);
                    var fl2 = Q.GetCircularFarmLocation(allMinionsQ, Q.Width);

                    if (fl1.MinionsHit >= 3)
                    {
                        Q.Cast(fl1.Position);
                    }

                    else if (fl2.MinionsHit >= 2 || allMinionsQ.Count == 1)
                    {
                        Q.Cast(fl2.Position);
                    }
                }
                else
                    foreach (var minion in allMinionsQ)
                        if (!Orbwalking.InAutoAttackRange(minion) &&
                            minion.Health < 0.75 * ObjectManager.Player.GetSpellDamage(minion, SpellSlot.Q))
                            Q.Cast(minion);

            if (useW && W.IsReady() && allMinionsW.Count > 3)
            {
                if (laneClear)
                {
                    if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).ToggleState == 1)
                    {
                        //WObject
                        var gObjectPos = GetGrabableObjectPos(false);

                        if (gObjectPos.To2D().IsValid() && Environment.TickCount - W.LastCastAttemptT > Game.Ping + 150)
                        {
                            W.Cast(gObjectPos);
                        }
                    }
                    else if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).ToggleState != 1)
                    {
                        var fl1 = Q.GetCircularFarmLocation(rangedMinionsW, W.Width);
                        var fl2 = Q.GetCircularFarmLocation(allMinionsW, W.Width);

                        if (fl1.MinionsHit >= 3 && W.InRange(fl1.Position.To3D()))
                        {
                            W.Cast(fl1.Position);
                        }

                        else if (fl2.MinionsHit >= 1 && W.InRange(fl2.Position.To3D()) && fl1.MinionsHit <= 2)
                        {
                            W.Cast(fl2.Position);
                        }
                    }
                }
            }
        }

        private void JungleFarm()
        {
            var useQ = Program.Menu.Item("UseQJFarm").GetValue<bool>();
            var useW = Program.Menu.Item("UseWJFarm").GetValue<bool>();
            var useE = Program.Menu.Item("UseEJFarm").GetValue<bool>();

            var mobs = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, W.Range, MinionTypes.All,
                MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (mobs.Count > 0)
            {
                var mob = mobs[0];

                if (Q.IsReady() && useQ)
                {
                    Q.Cast(mob);
                }

                if (W.IsReady() && useW && Environment.TickCount - Q.LastCastAttemptT > 800)
                {
                    W.Cast(mob);
                }

                if (useE && E.IsReady())
                {
                    E.Cast(mob);
                }
            }
        }


        private void Game_OnGameUpdate(EventArgs args)
        {
            if (ObjectManager.Player.IsDead) return;

            //Update the R range
            R.Range = R.Level == 3 ? 750 : 675;

            if (Program.Menu.Item("CastQE").GetValue<KeyBind>().Active && E.IsReady() && Q.IsReady())
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>())
                    if (enemy.IsValidTarget(EQ.Range) && Game.CursorPos.Distance(enemy.ServerPosition) < 300)
                        UseQE(enemy);

            if (Program.Menu.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                Combo();
            }
            else
            {
                if (Program.Menu.Item("HarassActive").GetValue<KeyBind>().Active ||
                    Program.Menu.Item("HarassActiveT").GetValue<KeyBind>().Active)
                    Harass();

                var lc = Program.Menu.Item("LaneClearActive").GetValue<KeyBind>().Active;
                if (lc || Program.Menu.Item("FreezeActive").GetValue<KeyBind>().Active)
                    Farm(lc);

                if (Program.Menu.Item("JungleFarmActive").GetValue<KeyBind>().Active)
                    JungleFarm();
            }
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            //Draw the ranges of the spells.
            var menuItem = Program.Menu.Item("QERange").GetValue<Circle>();
            if (menuItem.Active) Utility.DrawCircle(ObjectManager.Player.Position, EQ.Range, menuItem.Color);

            foreach (var spell in spellList)
            {
                menuItem = Program.Menu.Item(spell.Slot + "Range").GetValue<Circle>();
                if (menuItem.Active)
                    Utility.DrawCircle(ObjectManager.Player.Position, spell.Range, menuItem.Color);
            }
        }
    }
}
