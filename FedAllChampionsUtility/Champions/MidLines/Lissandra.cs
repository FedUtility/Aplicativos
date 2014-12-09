using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using LX_Orbwalker;
using Color = System.Drawing.Color;

namespace FedAllChampionsUtility
{
    class Lissandra : Champion
    {
        //Spells
        public static List<Spell> SpellList = new List<Spell>();

        public static Spell Q;
        public static Spell Q2;
        public static Spell W;
        public static Spell E;
        public static Spell R;

        //item and summoner
        public static Items.Item DFG;
        public static SpellSlot IgniteSlot;        

        //spell settings
        public static Obj_SpellMissile eMissle;
        public static bool eCreated = false;
        
        public Lissandra()
        {
            LoadMenu();
            Loadspells();

            //Events
            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPosibleToInterrupt;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            GameObject.OnCreate += OnCreate;
            GameObject.OnDelete += OnDelete;

            PluginLoaded();
        }

        private void LoadMenu()
        {
            //Keys
            Program.Menu.AddSubMenu(new Menu("Keys", "Keys"));
            Program.Menu.SubMenu("Keys").AddItem(new MenuItem("ComboActive", "Combo!").SetValue(new KeyBind(Program.Menu.Item("Combo_Key").GetValue<KeyBind>().Key, KeyBindType.Press)));
            Program.Menu.SubMenu("Keys").AddItem(new MenuItem("HarassActive", "Harass!").SetValue(new KeyBind(Program.Menu.Item("LaneClear_Key").GetValue<KeyBind>().Key, KeyBindType.Press)));
            Program.Menu.SubMenu("Keys").AddItem(new MenuItem("HarassActiveT", "Harass (toggle)!").SetValue(new KeyBind("Y".ToCharArray()[0], KeyBindType.Toggle)));
            Program.Menu.SubMenu("Keys").AddItem(new MenuItem("stunMelles", "Stun Enemy Melle Range").SetValue(new KeyBind("M".ToCharArray()[0], KeyBindType.Toggle)));
            Program.Menu.SubMenu("Keys").AddItem(new MenuItem("stunTowers", "Stun Enemy under Tower").SetValue(new KeyBind("J".ToCharArray()[0], KeyBindType.Toggle)));
            Program.Menu.SubMenu("Keys").AddItem(new MenuItem("LastHitQQ", "Last hit with Q").SetValue(new KeyBind("A".ToCharArray()[0], KeyBindType.Press)));
            Program.Menu.SubMenu("Keys").AddItem(new MenuItem("LaneClearActive", "Farm!").SetValue(new KeyBind(Program.Menu.Item("LaneClear_Key").GetValue<KeyBind>().Key, KeyBindType.Press)));

            //Combo menu:
            Program.Menu.AddSubMenu(new Menu("Combo", "Combo"));
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("qHit", "Q HitChance").SetValue(new Slider(3, 1, 4)));
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "Use R").SetValue(true));
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("rHp", "R if HP <").SetValue(new Slider(20, 0, 100)));
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("defR", "R Self if > enemy").SetValue(new Slider(3, 0, 5)));
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("dfg", "Use DFG").SetValue(true));
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("ignite", "Use Ignite").SetValue(true));
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("igniteMode", "Mode").SetValue(new StringList(new[] { "Combo", "KS" }, 0)));

            //Harass menu:
            Program.Menu.AddSubMenu(new Menu("Harass", "Harass"));
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(true));
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("qHit2", "Q HitChance").SetValue(new Slider(3, 1, 4)));
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("UseWHarass", "Use W").SetValue(false));
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("UseEHarass", "Use E").SetValue(true));

            //Farming menu:
            Program.Menu.AddSubMenu(new Menu("Farm", "Farm"));
            Program.Menu.SubMenu("Farm").AddItem(new MenuItem("UseQFarm", "Use Q").SetValue(false));
            Program.Menu.SubMenu("Farm").AddItem(new MenuItem("UseWFarm", "Use W").SetValue(false));
            Program.Menu.SubMenu("Farm").AddItem(new MenuItem("UseEFarm", "Use E").SetValue(false));

            //Misc Menu:
            Program.Menu.AddSubMenu(new Menu("Misc", "Misc"));
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("UseInt", "Use R to Interrupt").SetValue(true));
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("UseGap", "Use W for GapCloser").SetValue(true));
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("packet", "Use Packets").SetValue(true));
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("smartKS", "Use Smart KS System").SetValue(true));
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("UseHAM", "Always use E").SetValue(false));
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("UseEGap", "Use E to Gap Close").SetValue(true));
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("gapD", "Min Distance").SetValue(new Slider(600, 300, 1050)));

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
                .AddItem(new MenuItem("qExtend", "Extended Q range").SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
            Program.Menu.SubMenu("Drawings")
                .AddItem(new MenuItem("WRange", "W range").SetValue(new Circle(true, Color.FromArgb(100, 255, 0, 255))));
            Program.Menu.SubMenu("Drawings")
                .AddItem(new MenuItem("ERange", "E range").SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
            Program.Menu.SubMenu("Drawings")
                .AddItem(new MenuItem("RRange", "R range").SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
            Program.Menu.SubMenu("Drawings")
                .AddItem(dmgAfterComboItem);
        }

        private void Loadspells()
        {
            //intalize spell
            Q = new Spell(SpellSlot.Q, 725);
            Q2 = new Spell(SpellSlot.Q, 850);
            W = new Spell(SpellSlot.W, 450);
            E = new Spell(SpellSlot.E, 1050);
            R = new Spell(SpellSlot.R, 700);

            Q.SetSkillshot(0.50f, 100, 1300, false, SkillshotType.SkillshotLine);
            Q2.SetSkillshot(0.50f, 150, 1300, true, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.50f, 110, 850, false, SkillshotType.SkillshotLine);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            IgniteSlot = ObjectManager.Player.GetSpellSlot("SummonerDot");

            DFG = Utility.Map.GetMap()._MapType == Utility.Map.MapType.TwistedTreeline || Utility.Map.GetMap()._MapType == Utility.Map.MapType.CrystalScar ? new Items.Item(3188, 750) : new Items.Item(3128, 750);
        }

        private static float GetComboDamage(Obj_AI_Base enemy)
        {
            var damage = 0d;

            if (DFG.IsReady())
                damage += ObjectManager.Player.GetItemDamage(enemy, Damage.DamageItems.Dfg) / 1.2;

            if (Q.IsReady())
                damage += ObjectManager.Player.GetSpellDamage(enemy, SpellSlot.Q);

            if (W.IsReady())
                damage += ObjectManager.Player.GetSpellDamage(enemy, SpellSlot.W);

            if (E.IsReady())
                damage += ObjectManager.Player.GetSpellDamage(enemy, SpellSlot.E);

            if (R.IsReady())
                damage += ObjectManager.Player.GetSpellDamage(enemy, SpellSlot.R);

            if (DFG.IsReady())
                damage = damage * 1.2;

            damage = damage - 15;

            if (IgniteSlot != SpellSlot.Unknown && ObjectManager.Player.SummonerSpellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
                damage += ObjectManager.Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);

            return (float)damage;
        }

        private static void Combo()
        {
            UseSpells(Program.Menu.Item("UseQCombo").GetValue<bool>(), Program.Menu.Item("UseWCombo").GetValue<bool>(),
                Program.Menu.Item("UseECombo").GetValue<bool>(), Program.Menu.Item("UseRCombo").GetValue<bool>(), "Combo");
        }

        private static void Harass()
        {
            UseSpells(Program.Menu.Item("UseQHarass").GetValue<bool>(), Program.Menu.Item("UseWHarass").GetValue<bool>(),
                Program.Menu.Item("UseEHarass").GetValue<bool>(), false, "Harass");
        }

        private static void UseSpells(bool useQ, bool useW, bool useE, bool useR, string Source)
        {
            var qTarget = SimpleTs.GetTarget(Q2.Range, SimpleTs.DamageType.Magical);
            var eTarget = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Magical);

            var IgniteMode = Program.Menu.Item("igniteMode").GetValue<StringList>().SelectedIndex;

            //E
            if (useE && eTarget != null && E.IsReady() && ObjectManager.Player.Distance(eTarget) < E.Range && shouldE(eTarget))
            {
                E.Cast(eTarget, packets());
            }

            //R
            if (useR && qTarget != null && R.IsReady() && ObjectManager.Player.Distance(qTarget) < R.Range)
            {
                castR(qTarget);
            }

            //Ignite
            if (qTarget != null && Program.Menu.Item("ignite").GetValue<bool>() && IgniteSlot != SpellSlot.Unknown &&
                ObjectManager.Player.SummonerSpellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
            {
                if (IgniteMode == 0 && GetComboDamage(qTarget) > qTarget.Health)
                {
                    ObjectManager.Player.SummonerSpellbook.CastSpell(IgniteSlot, qTarget);
                }
            }

            //W
            if (useW && qTarget != null && W.IsReady())
            {
                var wPred = GetPCircle(ObjectManager.Player.ServerPosition, W, qTarget, true);
                if (wPred.Hitchance > HitChance.High && ObjectManager.Player.Distance(qTarget) <= W.Range)
                    W.Cast();
            }

            //Q
            if (useQ && Q.IsReady() && qTarget != null)
            {
                var qPred = Q2.GetPrediction(qTarget);
                var collision = qPred.CollisionObjects;

                if (collision.Count > 0 && ObjectManager.Player.Distance(qTarget) <= Q2.Range)
                {
                    Q.Cast(qPred.CastPosition, packets());
                    return;
                }
                if (Q.GetPrediction(qTarget).Hitchance >= getHit(Source) && ObjectManager.Player.Distance(qTarget) < Q.Range)
                {
                    Q.Cast(qTarget, packets());
                    return;
                }
            }

        }

        public static bool shouldE(Obj_AI_Hero target)
        {
            if (eCreated)
                return false;

            if (GetComboDamage(target) >= target.Health + 20)
                return true;

            if (Program.Menu.Item("UseHAM").GetValue<bool>())
                return true;

            return false;
        }

        public static void castR(Obj_AI_Hero target)
        {
            if (GetComboDamage(target) > target.Health + 20)
            {
                if (target != null && DFG.IsReady() && Program.Menu.Item("dfg").GetValue<bool>())
                {
                    Items.UseItem(DFG.Id, target);
                }

                R.Cast(target, packets());
                return;
            }

            if ((ObjectManager.Player.GetItemDamage(target, Damage.DamageItems.Dfg) + (ObjectManager.Player.GetSpellDamage(target, SpellSlot.R) * 1.2)) > target.Health + 20)
            {
                if (target != null && DFG.IsReady() && Program.Menu.Item("dfg").GetValue<bool>())
                {
                    Items.UseItem(DFG.Id, target);
                }

                R.Cast(target, packets());
                return;
            }

            //Defensive R
            var rHp = Program.Menu.Item("rHp").GetValue<Slider>().Value;
            var hpPercent = ObjectManager.Player.Health / ObjectManager.Player.MaxHealth * 100;

            if (hpPercent < rHp)
            {
                R.CastOnUnit(ObjectManager.Player, packets());
                return;
            }

            var rDef = Program.Menu.Item("defR").GetValue<Slider>().Value;

            if (Utility.CountEnemysInRange(300) >= rDef)
            {
                R.CastOnUnit(ObjectManager.Player, packets());
                return;
            }
        }

        public static void smartKS()
        {
            if (!Program.Menu.Item("smartKS").GetValue<bool>())
                return;

            var nearChamps = (from champ in ObjectManager.Get<Obj_AI_Hero>() where ObjectManager.Player.Distance(champ.ServerPosition) <= 1375 && champ.IsEnemy select champ).ToList();
            nearChamps.OrderBy(x => x.Health);

            foreach (var target in nearChamps)
            {
                //ignite
                if (target != null && Program.Menu.Item("ignite").GetValue<bool>() && IgniteSlot != SpellSlot.Unknown &&
                                ObjectManager.Player.SummonerSpellbook.CanUseSpell(IgniteSlot) == SpellState.Ready && ObjectManager.Player.Distance(target.ServerPosition) <= 600)
                {
                    var IgniteMode = Program.Menu.Item("igniteMode").GetValue<StringList>().SelectedIndex;
                    if (IgniteMode == 1 && ObjectManager.Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite) > target.Health)
                    {
                        ObjectManager.Player.SummonerSpellbook.CastSpell(IgniteSlot, target);
                    }
                }

                //dfg
                if (DFG.IsReady() && ObjectManager.Player.GetItemDamage(target, Damage.DamageItems.Dfg) > target.Health + 20 && ObjectManager.Player.Distance(target.ServerPosition) <= 750)
                {
                    Items.UseItem(DFG.Id, target);
                    return;
                }

                //Q
                if (ObjectManager.Player.Distance(target.ServerPosition) <= Q.Range && (ObjectManager.Player.GetSpellDamage(target, SpellSlot.Q)) > target.Health + 20)
                {
                    if (Q.IsReady())
                    {
                        Q.Cast(target, packets());
                        return;
                    }
                }

                //E
                if (ObjectManager.Player.Distance(target.ServerPosition) <= E.Range && (ObjectManager.Player.GetSpellDamage(target, SpellSlot.E)) > target.Health + 20)
                {
                    if (E.IsReady() && E.GetPrediction(target).Hitchance >= HitChance.High)
                    {
                        E.Cast(target, packets());
                        return;
                    }
                }

                //W
                if (ObjectManager.Player.Distance(target.ServerPosition) <= W.Range && (ObjectManager.Player.GetSpellDamage(target, SpellSlot.W)) > target.Health + 20)
                {
                    if (W.IsReady())
                    {
                        W.Cast();
                        return;
                    }
                }
            }
        }

        public static HitChance getHit(string Source)
        {
            var hitC = HitChance.High;
            var qHit = Program.Menu.Item("qHit").GetValue<Slider>().Value;
            var harassQHit = Program.Menu.Item("qHit2").GetValue<Slider>().Value;

            // HitChance.Low = 3, Medium , High .... etc..
            if (Source == "Combo")
            {
                switch (qHit)
                {
                    case 1:
                        hitC = HitChance.Low;
                        break;
                    case 2:
                        hitC = HitChance.Medium;
                        break;
                    case 3:
                        hitC = HitChance.High;
                        break;
                    case 4:
                        hitC = HitChance.VeryHigh;
                        break;
                }
            }
            else if (Source == "Harass")
            {
                switch (harassQHit)
                {
                    case 1:
                        hitC = HitChance.Low;
                        break;
                    case 2:
                        hitC = HitChance.Medium;
                        break;
                    case 3:
                        hitC = HitChance.High;
                        break;
                    case 4:
                        hitC = HitChance.VeryHigh;
                        break;
                }
            }

            return hitC;
        }

        public static PredictionOutput GetPCircle(Vector3 pos, Spell spell, Obj_AI_Base target, bool aoe)
        {

            return Prediction.GetPrediction(new PredictionInput
            {
                Unit = target,
                Delay = spell.Delay,
                Radius = 1,
                Speed = float.MaxValue,
                From = pos,
                Range = float.MaxValue,
                Collision = spell.Collision,
                Type = spell.Type,
                RangeCheckFrom = ObjectManager.Player.ServerPosition,
                Aoe = aoe,
            });
        }

        public static void detonateE()
        {
            var enemy = SimpleTs.GetTarget(2000, SimpleTs.DamageType.Magical);

            if (eMissle != null && enemy.ServerPosition.Distance(eMissle.Position) < 110 && enemy != null && eCreated && Program.Menu.Item("ComboActive").GetValue<KeyBind>().Active && E.IsReady())
            {
                E.Cast();
                return;
            }
            else if (eMissle != null && enemy != null && eCreated && Program.Menu.Item("ComboActive").GetValue<KeyBind>().Active && Program.Menu.Item("UseEGap").GetValue<bool>()
                && ObjectManager.Player.Distance(enemy) > enemy.Distance(eMissle.Position) && E.IsReady())
            {
                if (eMissle.EndPosition.Distance(eMissle.Position) < 400 && enemy.Distance(eMissle.Position) < enemy.Distance(eMissle.EndPosition))
                    E.Cast();
                else if (eMissle.Position == eMissle.EndPosition)
                    E.Cast();
            }

        }

        public static void gapClose()
        {
            var Target = SimpleTs.GetTarget(1500, SimpleTs.DamageType.Magical);
            var distance = Program.Menu.Item("gapD").GetValue<Slider>().Value;

            if (ObjectManager.Player.Distance(Target.ServerPosition) >= distance && Target.IsValidTarget(E.Range) && !eCreated && E.GetPrediction(Target).Hitchance >= HitChance.Medium && E.IsReady())
            {
                E.Cast(Target, packets());
            }
        }

        public static void lastHit()
        {
            if (!Orbwalking.CanMove(40)) return;

            var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range);

            if (Q.IsReady())
            {
                foreach (var minion in allMinions)
                {
                    if (minion.IsValidTarget() && HealthPrediction.GetHealthPrediction(minion, (int)(ObjectManager.Player.Distance(minion) * 1000 / 1400)) < Damage.GetSpellDamage(ObjectManager.Player, minion, SpellSlot.Q) - 10)
                    {
                        if (Q.IsReady())
                        {
                            Q.Cast(minion, packets());
                            return;
                        }
                    }
                }
            }
        }

        private static void Farm()
        {
            var allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range + Q.Width, MinionTypes.All);
            var allMinionsW = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, W.Range, MinionTypes.All);
            var allMinionsE = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, E.Range + E.Width, MinionTypes.All, MinionTeam.NotAlly);

            var useQ = Program.Menu.Item("UseQFarm").GetValue<bool>();
            var useW = Program.Menu.Item("UseWFarm").GetValue<bool>();
            var useE = Program.Menu.Item("UseEFarm").GetValue<bool>();

            if (useE && E.IsReady() && !eCreated)
            {
                var ePos = E.GetLineFarmLocation(allMinionsE);
                if (ePos.MinionsHit >= 3)
                    E.Cast(ePos.Position, packets());
            }

            if (useQ && Q.IsReady())
            {
                var qPos = Q.GetLineFarmLocation(allMinionsQ);
                if (qPos.MinionsHit >= 2)
                    Q.Cast(qPos.Position, packets());
            }

            if (useW && W.IsReady())
            {
                var wPos = W.GetLineFarmLocation(allMinionsW);
                if (wPos.MinionsHit >= 2)
                    W.Cast();
            }
        }
        public static void checkUnderTower()
        {
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (enemy.IsEnemy && ObjectManager.Player.Distance(enemy.ServerPosition) <= R.Range && enemy != null)
                {
                    foreach (var turret in ObjectManager.Get<Obj_AI_Turret>())
                    {
                        if (turret != null && turret.IsValid && turret.IsAlly && turret.Health > 0)
                        {
                            if (Vector2.Distance(enemy.Position.To2D(), turret.Position.To2D()) < 750 && R.IsReady())
                            {
                                R.Cast(enemy, packets());
                                return;
                            }
                        }
                    }
                }
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            //check if player is dead
            if (ObjectManager.Player.IsDead) return;

            detonateE();

            smartKS();

            if (Program.Menu.Item("stunMelles").GetValue<KeyBind>().Active)
            {
                var nearChamps = (from champ in ObjectManager.Get<Obj_AI_Hero>() where ObjectManager.Player.Distance(champ.ServerPosition) <= 200 && champ.IsEnemy select champ).ToList();
                nearChamps.OrderBy(x => x.Health);

                if (nearChamps.FirstOrDefault() != null && R.IsReady())
                {
                    R.Cast(nearChamps.FirstOrDefault());
                    return;
                }
            }

            if (Program.Menu.Item("stunTowers").GetValue<KeyBind>().Active)
            {
                checkUnderTower();
            }

            if (Program.Menu.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                if (Program.Menu.Item("UseEGap").GetValue<bool>())
                    gapClose();

                Combo();
            }
            else
            {
                if (Program.Menu.Item("LaneClearActive").GetValue<KeyBind>().Active)
                {
                    Farm();
                }

                if (Program.Menu.Item("LastHitQQ").GetValue<KeyBind>().Active)
                {
                    lastHit();
                }

                if (Program.Menu.Item("HarassActive").GetValue<KeyBind>().Active)
                    Harass();

                if (Program.Menu.Item("HarassActiveT").GetValue<KeyBind>().Active)
                    Harass();
            }
        }

        public static bool packets()
        {
            return Program.Menu.Item("packet").GetValue<bool>();
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            foreach (var spell in SpellList)
            {
                var menuItem = Program.Menu.Item(spell.Slot + "Range").GetValue<Circle>();
                if (menuItem.Active)
                    Utility.DrawCircle(ObjectManager.Player.Position, spell.Range, menuItem.Color);
            }

            if (Program.Menu.Item("qExtend").GetValue<Circle>().Active)
            {
                Utility.DrawCircle(ObjectManager.Player.Position, Q2.Range, Color.Aquamarine);
            }

        }

        public static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!Program.Menu.Item("UseGap").GetValue<bool>()) return;

            if (W.IsReady() && gapcloser.Sender.IsValidTarget(W.Range))
                W.Cast();
        }

        private static void Interrupter_OnPosibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (!Program.Menu.Item("UseInt").GetValue<bool>()) return;

            if (ObjectManager.Player.Distance(unit) < R.Range && unit != null && R.IsReady())
            {
                R.Cast(unit, packets());
            }
        }

        private static void OnCreate(GameObject sender, EventArgs args)
        {
            var spell = (Obj_SpellMissile)sender;
            var unit = spell.SpellCaster.Name;
            var name = spell.SData.Name;

            if (unit == ObjectManager.Player.Name && name == "LissandraEMissile")
            {
                eMissle = spell;
                eCreated = true;
                return;
            }
        }

        private static void OnDelete(GameObject sender, EventArgs args)
        {
            var spell = (Obj_SpellMissile)sender;
            var unit = spell.SpellCaster.Name;
            var name = spell.SData.Name;

            if (unit == ObjectManager.Player.Name && name == "LissandraEMissile")
            {
                eMissle = null;
                eCreated = false;
                return;
            }
        }
    }
}
