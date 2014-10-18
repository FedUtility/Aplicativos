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
    class Jayce : Champion
    {
        //Spells
        public static List<Spell> SpellList = new List<Spell>();

        public static Spell Q;
        public static Spell QCharge;
        public static Spell Q2;
        public static Spell W;
        public static Spell W2;
        public static Spell E;
        public static Spell E2;
        public static Spell R;

        //status
        public static bool HammerTime = false;
        public static SpellDataInst qdata;
        public static Vector3 ePos;
        public static bool firstE = false;

        //CoolDowns
        private static float[] CannonQcd = { 8, 8, 8, 8, 8 };
        private static float[] CannonWcd = { 14, 12, 10, 8, 6 };
        private static float[] CannonEcd = { 16, 16, 16, 16, 16 };

        private static float[] HammerQcd = { 16, 14, 12, 10, 8 };
        private static float[] HammerWcd = { 10, 10, 10, 10, 10 };
        private static float[] HammerEcd = { 14, 13, 12, 11, 10 };

        public Jayce()
        {
            LoadSpells();
            LoadMenu();

            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPosibleToInterrupt;
            GameObject.OnCreate += OnCreate;
            GameObject.OnDelete += OnDelete;
            LXOrbwalker.AfterAttack += Orbwalking_AfterAttack;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;

            PluginLoaded();
        }

        private void LoadMenu()
        {
            //Keys
            Program.Menu.AddSubMenu(new Menu("Keys", "Keys"));
            Program.Menu.SubMenu("Keys").AddItem(new MenuItem("ComboActive", "Combo!").SetValue(new KeyBind(Program.Menu.Item("Orbwalk").GetValue<KeyBind>().Key, KeyBindType.Press)));
            Program.Menu.SubMenu("Keys").AddItem(new MenuItem("HarassActive", "Harass!").SetValue(new KeyBind(Program.Menu.Item("Farm").GetValue<KeyBind>().Key, KeyBindType.Press)));
            Program.Menu.SubMenu("Keys").AddItem(new MenuItem("HarassActiveT", "Harass (toggle)!").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Toggle)));
            Program.Menu.SubMenu("Keys").AddItem(new MenuItem("shootMouse", "Shoot QE Mouse").SetValue(new KeyBind("G".ToCharArray()[0], KeyBindType.Press)));

            //Combo menu:
            Program.Menu.AddSubMenu(new Menu("Combo", "Combo"));
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "Use Cannon Q").SetValue(true));
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("qSpeed", "Charge Q Proj Speed").SetValue(new Slider(1600, 400, 2500)));
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "Use Cannon W").SetValue(true));
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "Use Cannon E").SetValue(true));
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("UseQComboHam", "Use Hammer Q").SetValue(true));
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("UseWComboHam", "Use Hammer W").SetValue(true));
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("UseEComboHam", "Use Hammer E").SetValue(true));
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "Use R to Switch").SetValue(true));

            //Harass menu:
            Program.Menu.AddSubMenu(new Menu("Harass", "Harass"));
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(true));
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("UseWHarass", "Use W").SetValue(true));
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("UseEHarass", "Use E").SetValue(true));
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("UseQHarassHam", "Use Q Hammer").SetValue(true));
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("UseWHarassHam", "Use W Hammer").SetValue(true));
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("UseEHarassHam", "Use E Hammer").SetValue(true));
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("UseRHarass", "Use R to switch").SetValue(true));
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("manaH", "Mana > %").SetValue(new Slider(40, 0, 100)));

            //Misc Menu:
            Program.Menu.AddSubMenu(new Menu("Misc", "Misc"));
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("UseInt", "Use E to Interrupt").SetValue(true));
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("UseGap", "Use E for GapCloser").SetValue(true));
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("packet", "Use Packets").SetValue(true));
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("forceGate", "Force Gate After Q").SetValue(false));
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("gatePlace", "Gate Distance").SetValue(new Slider(300, 250, 600)));
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("UseQAlways", "Use Q When E onCD").SetValue(true));
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("autoE", "EPushInCombo HP < %").SetValue(new Slider(40, 0, 100)));
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("smartKS", "Smart KS").SetValue(true));
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("lagMode", "Bad PC MODE(EQCombo)").SetValue(false));

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
                .AddItem(new MenuItem("drawcds", "Draw Cooldowns")).SetValue(true);
            Program.Menu.SubMenu("Drawings")
                .AddItem(dmgAfterComboItem);
        }

        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 1050);
            QCharge = new Spell(SpellSlot.Q, 1650);
            Q2 = new Spell(SpellSlot.Q, 600);
            W = new Spell(SpellSlot.W, float.MaxValue);
            W2 = new Spell(SpellSlot.W, 350);
            E = new Spell(SpellSlot.E, 650);
            E2 = new Spell(SpellSlot.E, 240);
            R = new Spell(SpellSlot.R, float.MaxValue);

            Q.SetSkillshot(0.15f, 60, 1200, true, SkillshotType.SkillshotLine);
            QCharge.SetSkillshot(0.25f, 60, 1600, true, SkillshotType.SkillshotLine);
            Q2.SetTargetted(0.25f, float.MaxValue);
            E.SetSkillshot(0.1f, 120, float.MaxValue, false, SkillshotType.SkillshotCircle);
            E2.SetTargetted(.25f, float.MaxValue);

            SpellList.Add(Q);
            SpellList.Add(QCharge);
            SpellList.Add(Q2);
            SpellList.Add(W);
            SpellList.Add(W2);
            SpellList.Add(E);
            SpellList.Add(E2);
            SpellList.Add(R);

            qdata = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q);
        }

        private static float GetComboDamage(Obj_AI_Base enemy)
        {
            var damage = 0d;

            if (canQcd == 0 && canEcd == 0 && Q.Level > 0 && E.Level > 0)
                damage += ObjectManager.Player.GetSpellDamage(enemy, SpellSlot.Q) * 1.4;
            else if (canQcd == 0 && Q.Level > 0)
                damage += ObjectManager.Player.GetSpellDamage(enemy, SpellSlot.Q);

            if (hamQcd == 0 && Q.Level > 0)
                damage += ObjectManager.Player.GetSpellDamage(enemy, SpellSlot.Q, 1);

            if (hamWcd == 0 && W.Level > 0)
                damage += ObjectManager.Player.GetSpellDamage(enemy, SpellSlot.W);

            if (hamEcd == 0 && E.Level > 0)
                damage += ObjectManager.Player.GetSpellDamage(enemy, SpellSlot.E);

            return (float)damage;
        }

        private static void Combo()
        {
            UseSpells(Program.Menu.Item("UseQCombo").GetValue<bool>(), Program.Menu.Item("UseWCombo").GetValue<bool>(),
                Program.Menu.Item("UseECombo").GetValue<bool>(), Program.Menu.Item("UseQComboHam").GetValue<bool>(), Program.Menu.Item("UseWComboHam").GetValue<bool>(),
                Program.Menu.Item("UseEComboHam").GetValue<bool>(), Program.Menu.Item("UseRCombo").GetValue<bool>(), "Combo");

        }
        private static void Harass()
        {
            UseSpells(Program.Menu.Item("UseQHarass").GetValue<bool>(), Program.Menu.Item("UseWHarass").GetValue<bool>(),
                Program.Menu.Item("UseEHarass").GetValue<bool>(), Program.Menu.Item("UseQHarassHam").GetValue<bool>(), Program.Menu.Item("UseWHarassHam").GetValue<bool>(),
                Program.Menu.Item("UseEHarassHam").GetValue<bool>(), Program.Menu.Item("UseRHarass").GetValue<bool>(), "Harass");
        }

        private static void UseSpells(bool useQ, bool useW, bool useE, bool useQ2, bool useW2, bool useE2, bool useR, String source)
        {
            var qTarget = SimpleTs.GetTarget(QCharge.Range, SimpleTs.DamageType.Physical);
            var q2Target = SimpleTs.GetTarget(Q2.Range, SimpleTs.DamageType.Physical);

            var wTarget = SimpleTs.GetTarget(W.Range, SimpleTs.DamageType.Physical);
            var w2Target = SimpleTs.GetTarget(W2.Range, SimpleTs.DamageType.Magical);

            var eTarget = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Physical);
            var e2Target = SimpleTs.GetTarget(E2.Range, SimpleTs.DamageType.Physical);

            //mana manager for harass
            var mana = Program.Menu.Item("manaH").GetValue<Slider>().Value;
            var manaPercent = ObjectManager.Player.Mana / ObjectManager.Player.MaxMana * 100;

            //Main Combo
            if (source == "Combo")
            {

                if (useQ && canQcd == 0 && ObjectManager.Player.Distance(qTarget) <= QCharge.Range && qTarget != null && !HammerTime)
                {
                    //Game.PrintChat("Yay");
                    castQCannon(qTarget, useE);
                    return;
                }

                if (HammerTime)
                {
                    if (useW2 && ObjectManager.Player.Distance(q2Target) <= 300 && W.IsReady())
                        W.Cast();

                    if (useQ2 && ObjectManager.Player.Distance(q2Target) <= Q2.Range + q2Target.BoundingRadius && Q2.IsReady())
                        Q2.Cast(q2Target, Program.Menu.Item("packet").GetValue<bool>());

                    if (useE2 && eCheck(e2Target, useQ, useW) && ObjectManager.Player.Distance(e2Target) <= E2.Range + q2Target.BoundingRadius && E2.IsReady())
                        E2.Cast(q2Target, Program.Menu.Item("packet").GetValue<bool>());
                }

                //form switch check
                if (useR)
                    switchFormCheck(q2Target, useQ, useW, useE, useQ2, useW2, useE2);
            }
            else if (source == "Harass" && manaPercent > mana)
            {

                if (useQ && canQcd == 0 && ObjectManager.Player.Distance(qTarget) <= QCharge.Range && qTarget != null && !HammerTime)
                {
                    castQCannon(qTarget, useE);
                    return;
                }

                if (HammerTime)
                {
                    if (useW2 && ObjectManager.Player.Distance(q2Target) <= 300 && W.IsReady())
                        W.Cast();

                    if (useQ2 && ObjectManager.Player.Distance(q2Target) <= Q2.Range + q2Target.BoundingRadius && Q2.IsReady())
                        Q2.Cast(q2Target, Program.Menu.Item("packet").GetValue<bool>());

                    if (useE2 && ObjectManager.Player.Distance(q2Target) <= E2.Range + q2Target.BoundingRadius && E2.IsReady())
                        E2.Cast(q2Target, Program.Menu.Item("packet").GetValue<bool>());
                }

                //form switch check
                if (useR)
                    switchFormCheck(q2Target, useQ, useW, useE, useQ2, useW2, useE2);
            }

        }

        public static bool eCheck(Obj_AI_Hero target, bool useQ, bool useW)
        {
            if (ObjectManager.Player.GetSpellDamage(target, SpellSlot.E) >= target.Health)
            {
                //Game.PrintChat("Hammer KS");
                return true;
            }
            if (((canQcd == 0 && useQ) || (canWcd == 0 && useW)) && hamQcd != 0 && hamWcd != 0)
            {
                //Game.PrintChat("Hammer Range mode");
                return true;
            }
            if (wallStun(target))
            {
                //Game.PrintChat("Walled");
                return true;
            }

            var hp = Program.Menu.Item("autoE").GetValue<Slider>().Value;
            var hpPercent = ObjectManager.Player.Health / ObjectManager.Player.MaxHealth * 100;

            if (hpPercent <= hp)
            {
                //Game.PrintChat("Hammer Shield");
                return true;
            }

            return false;
        }

        public static bool wallStun(Obj_AI_Hero target)
        {
            var pred = E2.GetPrediction(target);

            var PushedPos = pred.CastPosition + Vector3.Normalize(pred.CastPosition - ObjectManager.Player.ServerPosition) * 350;

            if (IsWall(PushedPos))
                return true;

            return false;
        }

        public static bool IsWall(Vector3 position)
        {
            var cFlags = NavMesh.GetCollisionFlags(position);
            return (cFlags == CollisionFlags.Wall || cFlags == CollisionFlags.Building || cFlags == CollisionFlags.Prop);
        }

        public static void KSCheck()
        {
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (enemy != null && !enemy.IsDead && enemy.IsEnemy && ObjectManager.Player.Distance(enemy.ServerPosition) <= QCharge.Range && enemy.IsValidTarget(QCharge.Range))
                {
                    //Q
                    if ((ObjectManager.Player.GetSpellDamage(enemy, SpellSlot.Q) - 20) > enemy.Health && canQcd == 0 && Q.GetPrediction(enemy).Hitchance >= HitChance.High && ObjectManager.Player.Distance(enemy.ServerPosition) <= Q.Range)
                    {
                        if (HammerTime && R.IsReady())
                            R.Cast();

                        if (!HammerTime && Q.IsReady())
                            Q.Cast(enemy, Program.Menu.Item("packet").GetValue<bool>());
                    }

                    //QE
                    if ((ObjectManager.Player.GetSpellDamage(enemy, SpellSlot.Q) * 1.4 - 20) > enemy.Health && canQcd == 0 && canEcd == 0 && ObjectManager.Player.Distance(enemy.ServerPosition) <= QCharge.Range)
                    {
                        if (HammerTime && R.IsReady())
                            R.Cast();

                        if (!HammerTime)
                            castQCannon(enemy, true);
                    }

                    //Hammer QE
                    if ((ObjectManager.Player.GetSpellDamage(enemy, SpellSlot.E) + ObjectManager.Player.GetSpellDamage(enemy, SpellSlot.Q, 1) - 20) > enemy.Health
                        && hamEcd == 0 && hamQcd == 0 && ObjectManager.Player.Distance(enemy.ServerPosition) <= Q2.Range + enemy.BoundingRadius)
                    {
                        if (!HammerTime && R.IsReady())
                            R.Cast();

                        if (HammerTime && Q2.IsReady() && E2.IsReady())
                        {
                            Q2.Cast(enemy, Program.Menu.Item("packet").GetValue<bool>());
                            E2.Cast(enemy, Program.Menu.Item("packet").GetValue<bool>());
                            return;
                        }
                    }

                    //Hammer Q
                    if ((ObjectManager.Player.GetSpellDamage(enemy, SpellSlot.Q, 1) - 20) > enemy.Health && hamQcd == 0 && ObjectManager.Player.Distance(enemy.ServerPosition) <= Q2.Range + enemy.BoundingRadius)
                    {
                        if (!HammerTime && R.IsReady())
                            R.Cast();

                        if (HammerTime && Q2.IsReady())
                        {
                            Q2.Cast(enemy, Program.Menu.Item("packet").GetValue<bool>());
                            return;
                        }
                    }

                    //Hammer E
                    if ((ObjectManager.Player.GetSpellDamage(enemy, SpellSlot.E) - 20) > enemy.Health && hamEcd == 0 && ObjectManager.Player.Distance(enemy.ServerPosition) <= E2.Range + enemy.BoundingRadius)
                    {
                        if (!HammerTime && R.IsReady() && enemy.Health > 80)
                            R.Cast();

                        if (HammerTime && E2.IsReady())
                        {
                            E2.Cast(enemy, Program.Menu.Item("packet").GetValue<bool>());
                            return;
                        }
                    }
                }
            }
        }

        public static void switchFormCheck(Obj_AI_Hero target, bool useQ, bool useW, bool useE, bool useQ2, bool useW2, bool useE2)
        {
            if (target.Health > 80)
            {
                //switch to hammer
                if ((canQcd != 0 || !useQ) &&
                    (canWcd != 0 && !hyperCharged() || !useW) && R.IsReady() &&
                     hammerAllReady() && !HammerTime && ObjectManager.Player.Distance(target.ServerPosition) < 650 &&
                     (useQ2 || useW2 || useE2))
                {
                    //Game.PrintChat("Hammer Time");
                    R.Cast();
                    return;
                }
            }

            //switch to cannon
            if (((canQcd == 0 && useQ) || (canWcd == 0 && useW) && R.IsReady())
                && HammerTime)
            {
                //Game.PrintChat("Cannon Time");
                R.Cast();
                return;
            }

            if (hamQcd != 0 && hamWcd != 0 && hamEcd != 0 && HammerTime && R.IsReady())
            {
                R.Cast();
                return;
            }
        }

        public static bool hyperCharged()
        {
            foreach (var buffs in ObjectManager.Player.Buffs)
            {
                if (buffs.Name == "jaycehypercharge")
                    return true;
            }
            return false;
        }
        public static bool hammerAllReady()
        {
            if (hamQcd == 0 && hamWcd == 0 && hamEcd == 0)
            {
                return true;
            }
            return false;
        }

        public static bool cannonAllReady()
        {
            if (canQcd == 0 && canWcd == 0 && canEcd == 0)
            {
                return true;
            }
            return false;
        }

        public static PredictionOutput GetP(Vector3 pos, Spell spell, Obj_AI_Base target, float delay, float speed, bool aoe)
        {

            return Prediction.GetPrediction(new PredictionInput
            {
                Unit = target,
                Delay = delay,
                Radius = spell.Width,
                Speed = speed,
                From = pos,
                Range = spell.Range,
                Collision = spell.Collision,
                Type = spell.Type,
                RangeCheckFrom = ObjectManager.Player.ServerPosition,
                Aoe = aoe,
            });
        }

        public static void castQCannon(Obj_AI_Hero target, bool useE)
        {
            var gateDis = Program.Menu.Item("gatePlace").GetValue<Slider>().Value;
            var qSpeed = Program.Menu.Item("qSpeed").GetValue<Slider>().Value;
            var lagFree = Program.Menu.Item("lagMode").GetValue<bool>();

            QCharge.Speed = qSpeed;

            var tarPred = QCharge.GetPrediction(target);

            //dieno :> <3
            if (tarPred.Hitchance >= HitChance.High && canQcd == 0 && canEcd == 0 && useE && !firstE)
            {
                var GateVector = ObjectManager.Player.Position + Vector3.Normalize(target.ServerPosition - ObjectManager.Player.Position) * gateDis;
                var vecClose = ObjectManager.Player.ServerPosition - Vector3.Normalize(ObjectManager.Player.ServerPosition - target.ServerPosition) * 50;

                if (ObjectManager.Player.Distance(tarPred.CastPosition) < QCharge.Range + 100)
                {
                    if (!lagFree)
                    {
                        if (ObjectManager.Player.Distance(target) < 250 && E.IsReady() && QCharge.IsReady())
                        {
                            E.Cast(vecClose, Program.Menu.Item("packet").GetValue<bool>());
                            QCharge.Cast(tarPred.CastPosition, Program.Menu.Item("packet").GetValue<bool>());
                            firstE = true;
                            return;
                        }
                        else if (QCharge.IsReady())
                        {
                            ePos = GateVector;
                            QCharge.Cast(tarPred.CastPosition, Program.Menu.Item("packet").GetValue<bool>());
                            firstE = true;
                            return;
                        }
                    }
                    else if (E.IsReady() && QCharge.IsReady())
                    {
                        E.Cast(GateVector, Program.Menu.Item("packet").GetValue<bool>());
                        QCharge.Cast(tarPred.CastPosition, Program.Menu.Item("packet").GetValue<bool>());
                        return;
                    }
                }

                Q.Cast(tarPred.CastPosition, Program.Menu.Item("packet").GetValue<bool>());
            }

            if ((Program.Menu.Item("UseQAlways").GetValue<bool>() || !useE) && canQcd == 0 && Q.GetPrediction(target).Hitchance >= HitChance.High && ObjectManager.Player.Distance(target.ServerPosition) <= Q.Range && Q.IsReady())
            {
                Q.Cast(target, Program.Menu.Item("packet").GetValue<bool>());
                return;
            }

        }

        public static void castQCannonMouse()
        {
            LXOrbwalker.Orbwalk(Game.CursorPos, null);
            
            if (HammerTime && !R.IsReady())
                return;

            if (HammerTime && R.IsReady())
            {
                R.Cast();
                return;
            }

            var lagFree = Program.Menu.Item("lagMode").GetValue<bool>();

            if (canEcd == 0 && canQcd == 0 && !HammerTime && !firstE)
            {
                var gateDis = Program.Menu.Item("gatePlace").GetValue<Slider>().Value;
                var GateVector = ObjectManager.Player.ServerPosition + Vector3.Normalize(Game.CursorPos - ObjectManager.Player.ServerPosition) * gateDis;

                if (!lagFree && Q.IsReady())
                {
                    ePos = GateVector;
                    Q.Cast(Game.CursorPos, Program.Menu.Item("packet").GetValue<bool>());
                    firstE = true;
                    return;
                }
                else if (E.IsReady() && Q.IsReady())
                {
                    E.Cast(GateVector, Program.Menu.Item("packet").GetValue<bool>());
                    Q.Cast(Game.CursorPos, Program.Menu.Item("packet").GetValue<bool>());
                    return;
                }
            }


        }

        private static float canQcd = 0, canWcd = 0, canEcd = 0;
        private static float hamQcd = 0, hamWcd = 0, hamEcd = 0;
        private static float canQcdRem = 0, canWcdRem = 0, canEcdRem = 0;
        private static float hamQcdRem = 0, hamWcdRem = 0, hamEcdRem = 0;

        private static void ProcessCooldowns()
        {
            canQcd = ((canQcdRem - Game.Time) > 0) ? (canQcdRem - Game.Time) : 0;
            canWcd = ((canWcdRem - Game.Time) > 0) ? (canWcdRem - Game.Time) : 0;
            canEcd = ((canEcdRem - Game.Time) > 0) ? (canEcdRem - Game.Time) : 0;
            hamQcd = ((hamQcdRem - Game.Time) > 0) ? (hamQcdRem - Game.Time) : 0;
            hamWcd = ((hamWcdRem - Game.Time) > 0) ? (hamWcdRem - Game.Time) : 0;
            hamEcd = ((hamEcdRem - Game.Time) > 0) ? (hamEcdRem - Game.Time) : 0;
        }

        private static float CalculateCd(float time)
        {
            return time + (time * ObjectManager.Player.PercentCooldownMod);
        }

        private static void GetCooldowns(GameObjectProcessSpellCastEventArgs spell)
        {
            if (HammerTime)
            {
                if (spell.SData.Name == "JayceToTheSkies")
                    hamQcdRem = Game.Time + CalculateCd(HammerQcd[Q.Level - 1]);
                if (spell.SData.Name == "JayceStaticField")
                    hamWcdRem = Game.Time + CalculateCd(HammerWcd[W.Level - 1]);
                if (spell.SData.Name == "JayceThunderingBlow")
                {
                    hamEcdRem = Game.Time + CalculateCd(HammerEcd[E.Level - 1]);
                    firstE = false;
                }
            }
            else
            {

                if (spell.SData.Name == "jayceshockblast")
                    canQcdRem = Game.Time + CalculateCd(CannonQcd[Q.Level - 1]);
                if (spell.SData.Name == "jaycehypercharge")
                    canWcdRem = Game.Time + CalculateCd(CannonWcd[W.Level - 1]);
                if (spell.SData.Name == "jayceaccelerationgate")
                {
                    canEcdRem = Game.Time + CalculateCd(CannonEcd[E.Level - 1]);
                    firstE = false;
                }
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            //check if player is dead
            if (ObjectManager.Player.IsDead) return;

            LXOrbwalker.SetAttack(true);

            //cd check
            ProcessCooldowns();

            //Check form
            HammerTime = !qdata.Name.Contains("jayceshockblast");

            //ks check
            if (Program.Menu.Item("smartKS").GetValue<bool>())
                KSCheck();

            if (Program.Menu.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                Combo();
            }
            else
            {
                if (Program.Menu.Item("shootMouse").GetValue<KeyBind>().Active)
                    castQCannonMouse();

                if (Program.Menu.Item("HarassActive").GetValue<KeyBind>().Active)
                    Harass();

                if (Program.Menu.Item("HarassActiveT").GetValue<KeyBind>().Active)
                    Harass();
            }
        }

        public static void Orbwalking_AfterAttack(Obj_AI_Base unit, Obj_AI_Base target)
        {
            var useWCombo = Program.Menu.Item("UseWCombo").GetValue<bool>();
            var useWHarass = Program.Menu.Item("UseWHarass").GetValue<bool>();

            if (unit.IsMe && !HammerTime)
            {
                if (Program.Menu.Item("ComboActive").GetValue<KeyBind>().Active)
                {
                    if (canWcd == 0 && ObjectManager.Player.Distance(target) < 600 && !HammerTime && W.Level > 0 && W.IsReady())
                        if (useWCombo)
                        {
                            LXOrbwalker.ResetAutoAttackTimer();
                            W.Cast();
                        }
                }

                if (Program.Menu.Item("HarassActive").GetValue<KeyBind>().Active || Program.Menu.Item("HarassActiveT").GetValue<KeyBind>().Active)
                {
                    if (canWcd == 0 && ObjectManager.Player.Distance(target) < 600 && !HammerTime && W.Level > 0 && W.IsReady())
                        if (useWHarass)
                        {
                            LXOrbwalker.ResetAutoAttackTimer();
                            W.Cast();
                        }
                }
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            foreach (var spell in SpellList)
            {
                var menuItem = Program.Menu.Item(spell.Slot + "Range").GetValue<Circle>();
                if (menuItem.Active)
                    Utility.DrawCircle(ObjectManager.Player.Position, spell.Range, menuItem.Color);
            }

            //kurisu credit
            if (Program.Menu.Item("drawcds").GetValue<bool>())
            {
                var wts = Drawing.WorldToScreen(ObjectManager.Player.Position);
                if (HammerTime) // lets show cooldown timers for the opposite form :)
                {

                    if (canQcd == 0)
                        Drawing.DrawText(wts[0] - 80, wts[1], Color.White, "Q Ready");
                    else
                        Drawing.DrawText(wts[0] - 80, wts[1], Color.Orange, "Q: " + canQcd.ToString("0.0"));
                    if (canWcd == 0)
                        Drawing.DrawText(wts[0] - 30, wts[1] + 30, Color.White, "W Ready");
                    else
                        Drawing.DrawText(wts[0] - 30, wts[1] + 30, Color.Orange, "W: " + canWcd.ToString("0.0"));
                    if (canEcd == 0)
                        Drawing.DrawText(wts[0], wts[1], Color.White, "E Ready");
                    else
                        Drawing.DrawText(wts[0], wts[1], Color.Orange, "E: " + canEcd.ToString("0.0"));

                }
                else
                {
                    if (hamQcd == 0)
                        Drawing.DrawText(wts[0] - 80, wts[1], Color.White, "Q Ready");
                    else
                        Drawing.DrawText(wts[0] - 80, wts[1], Color.Orange, "Q: " + hamQcd.ToString("0.0"));
                    if (hamWcd == 0)
                        Drawing.DrawText(wts[0] - 30, wts[1] + 30, Color.White, "W Ready");
                    else
                        Drawing.DrawText(wts[0] - 30, wts[1] + 30, Color.Orange, "W: " + hamWcd.ToString("0.0"));
                    if (hamEcd == 0)
                        Drawing.DrawText(wts[0], wts[1], Color.White, "E Ready");
                    else
                        Drawing.DrawText(wts[0], wts[1], Color.Orange, "E: " + hamEcd.ToString("0.0"));

                }
            }

        }
        private static void OnCreate(GameObject sender, EventArgs args)
        {
            var spell = (Obj_SpellMissile)sender;
            var unit = spell.SpellCaster.Name;
            var name = spell.SData.Name;

            if (unit == ObjectManager.Player.Name && name == "JayceShockBlastMis")
            {
                if (ePos != Vector3.Zero && canEcd == 0 && E.IsReady())
                {
                    E.Cast(ePos, Program.Menu.Item("packet").GetValue<bool>());
                    firstE = false;
                }
                else if (Program.Menu.Item("forceGate").GetValue<bool>() && canEcd == 0 && ObjectManager.Player.Distance(spell.Position) < 250 && E.IsReady())
                {
                    E.Cast(spell.EndPosition, Program.Menu.Item("packet").GetValue<bool>());
                    firstE = false;
                }
            }
        }

        private static void OnDelete(GameObject sender, EventArgs args)
        {
            var spell = (Obj_SpellMissile)sender;
            var unit = spell.SpellCaster.Name;
            var name = spell.SData.Name;

            if (unit == ObjectManager.Player.Name && name == "JayceShockBlastMis")
            {
                firstE = false;
                ePos = Vector3.Zero;
            }
        }
        public static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs attack)
        {
            if (unit.IsMe)
                GetCooldowns(attack);
        }

        public static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!Program.Menu.Item("UseGap").GetValue<bool>()) return;

            if (hamEcd == 0 && gapcloser.Sender.IsValidTarget(E2.Range + gapcloser.Sender.BoundingRadius))
            {
                if (!HammerTime && R.IsReady())
                    R.Cast();

                if (E2.IsReady())
                    E2.Cast(gapcloser.Sender, Program.Menu.Item("packet").GetValue<bool>());
            }
        }

        private static void Interrupter_OnPosibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (!Program.Menu.Item("UseInt").GetValue<bool>()) return;

            if (unit != null && ObjectManager.Player.Distance(unit) < Q2.Range + unit.BoundingRadius && hamQcd == 0 && hamEcd == 0)
            {
                if (!HammerTime && R.IsReady())
                    R.Cast();

                if (Q2.IsReady())
                    Q2.Cast(unit, Program.Menu.Item("packet").GetValue<bool>());
            }

            if (ObjectManager.Player.Distance(unit) < E2.Range + unit.BoundingRadius && unit != null && hamEcd == 0)
            {
                if (!HammerTime && R.IsReady())
                    R.Cast();

                if (E2.IsReady())
                E2.Cast(unit, Program.Menu.Item("packet").GetValue<bool>());
            }
        }

    }
}
