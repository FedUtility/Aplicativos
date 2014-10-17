#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using LX_Orbwalker;
using Color = System.Drawing.Color;

#endregion

namespace FedAllChampionsUtility
{
    class Jinx : Champion
    {
        public Spell Q, W, E, R; 
        
        public Jinx()
        {
            LoadSpell();
            LoadMenu();

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnGameUpdate += Game_OnGameUpdate;
            LXOrbwalker.AfterAttack += Orbwalking_AfterAttack;

            PluginLoaded();
            
        }

        private void LoadSpell()
        {
            Q = new Spell(SpellSlot.Q, float.MaxValue);
            W = new Spell(SpellSlot.W, 1500f);
            E = new Spell(SpellSlot.E, 900f);
            R = new Spell(SpellSlot.R, 25000f);

            W.SetSkillshot(0.6f, 60f, 3000f, true, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.7f, 120f, 1750f, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(0.6f, 140f, 1700f, false, SkillshotType.SkillshotLine);
        }

        private void LoadMenu()
        {
            Program.Menu.AddSubMenu(new Menu("TeamFight", "TeamFight"));
            Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("UseQC", "Use Q").SetValue(true));
            Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("UseWC", "Use W").SetValue(true));
            Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("UseRC", "Use R").SetValue(true));            
            Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("ComboActive", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));

            Program.Menu.AddSubMenu(new Menu("Harass", "Harass"));
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("UseQH", "Use Q").SetValue(true));
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("UseWH", "Use W").SetValue(false));
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("HarassActive", "Harass!").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));

            Program.Menu.AddSubMenu(new Menu("LaneClear", "LaneClear"));
            Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("SwapQ", "Always swap to Minigun").SetValue(true));
            Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("LaneClearActive", "LaneClear!").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));

            Program.Menu.AddSubMenu(new Menu("Misc", "Misc"));
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("SwapDistance", "Swap Q for distance").SetValue(true));
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("SwapAOE", "Swap Q for AOE").SetValue(true));
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("MinWRange", "Min W range").SetValue(new Slider(525 + 65 * 2, 0, 1200 )));
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("AutoEI", "Auto-E on immobile").SetValue(true));
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("AutoES", "Auto-E on slowed").SetValue(true));
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("AutoED", "Auto-E on dashing").SetValue(true));
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("CastR", "Cast R (2000 Range)").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("ROverKill", "Check R Overkill").SetValue(true));
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("MinRRange", "Min R range").SetValue(new Slider(500, 0, 1500)));
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("MaxRRange", "Max R range").SetValue(new Slider(2000, 0, 4000)));

            Program.Menu.AddSubMenu(new Menu("Drawing", "Drawing"));
            Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_Disabled", "Disable All").SetValue(false));            
            Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("DrawW", "Draw W").SetValue(true));
            Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("DrawE", "Draw E").SetValue(true));
            Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("DrawR", "Draw R").SetValue(true)); 

        }        

        private void Drawing_OnDraw(EventArgs args)
        {
            if (Program.Menu.Item("Draw_Disabled").GetValue<bool>())
                return;

            if (Program.Menu.Item("DrawW").GetValue<bool>())
                if (W.Level > 0)
                    Utility.DrawCircle(ObjectManager.Player.Position, W.Range, W.IsReady() ? Color.Green : Color.Red);

            if (Program.Menu.Item("DrawE").GetValue<bool>())
                if (E.Level > 0)
                    Utility.DrawCircle(ObjectManager.Player.Position, E.Range, E.IsReady() ? Color.Green : Color.Red);

            if (Program.Menu.Item("DrawR").GetValue<bool>())
                if (R.Level > 0)
                    Utility.DrawCircle(ObjectManager.Player.Position, R.Range, R.IsReady() ? Color.Green : Color.Red); 
            
        }

        public void Game_OnGameUpdate(EventArgs args)
        {
            var autoEi = Program.Menu.Item("AutoEI").GetValue<bool>();
            var autoEs = Program.Menu.Item("AutoES").GetValue<bool>();
            var autoEd = Program.Menu.Item("AutoED").GetValue<bool>();

            if ((autoEs || autoEi || autoEd) && E.IsReady())
            {
                AutoE();
            }

            if (Program.Menu.Item("CastR").GetValue<KeyBind>().Active && R.IsReady())
            {
                CastRT();  
            }

            if (Program.Menu.Item("SwapQ").GetValue<bool>() && FishBoneActive &&
                (Program.Menu.Item("LaneClearActive").GetValue<KeyBind>().Active ||
                 (Program.Menu.Item("HarassActive").GetValue<KeyBind>().Active && SimpleTs.GetTarget(675f + QAddRange, SimpleTs.DamageType.Physical) == null)))
            {
                Q.Cast();
            }

            if ((!Program.Menu.Item("ComboActive").GetValue<KeyBind>().Active && !Program.Menu.Item("HarassActive").GetValue<KeyBind>().Active) || !Orbwalking.CanMove(100))
            {
                return;
            }

            var useQC = Program.Menu.Item("UseQC").GetValue<bool>();
            var useWC = Program.Menu.Item("UseWC").GetValue<bool>();
            var useQH = Program.Menu.Item("UseQH").GetValue<bool>();
            var useWH = Program.Menu.Item("UseWH").GetValue<bool>();
            var useR = Program.Menu.Item("UseRC").GetValue<bool>();

            if ((useWC || useWH) && W.IsReady())
            {
                CastW();
            }

            if (useQC || useQH)
            {
                CastQ();                
            }

            if (useR && R.IsReady())
            {
                AutoCastR();
            }
        }

        private void Orbwalking_AfterAttack(Obj_AI_Base unit, Obj_AI_Base target)
        {
            if ((Program.Menu.Item("ComboActive").GetValue<KeyBind>().Active || Program.Menu.Item("HarassActive").GetValue<KeyBind>().Active) && unit.IsMe && (target is Obj_AI_Hero))
            {
                var useQC = Program.Menu.Item("UseQC").GetValue<bool>();
                var useQH = Program.Menu.Item("UseQH").GetValue<bool>();
                var useWC = Program.Menu.Item("UseWC").GetValue<bool>();
                var useWH = Program.Menu.Item("UseWH").GetValue<bool>();

                if ((useWC || useWH) && W.IsReady())
                {
                    var t = SimpleTs.GetTarget(W.Range, SimpleTs.DamageType.Physical);
                    var minW = Program.Menu.Item("MinWRange").GetValue<Slider>().Value;

                    if (t.IsValidTarget() && GetRealDistance(t) >= minW)
                    {
                        if (W.Cast(t) == Spell.CastStates.SuccessfullyCasted)
                        {
                            return;
                        }
                    }
                }

                if (useQC || useQH)
                {
                    foreach (var t in
                        ObjectManager.Get<Obj_AI_Hero>()
                            .Where(t => t.IsValidTarget(GetRealPowPowRange(t) + QAddRange + 20f)))
                    {
                        var swapDistance = Program.Menu.Item("SwapDistance").GetValue<bool>();
                        var swapAoe = Program.Menu.Item("SwapAOE").GetValue<bool>();
                        var distance = GetRealDistance(t);
                        var powPowRange = GetRealPowPowRange(t);

                        if (swapDistance && Q.IsReady())
                        {
                            if (distance > powPowRange && !FishBoneActive)
                            {
                                if (Q.Cast())
                                {
                                    return;
                                }
                            }
                            else if (distance < powPowRange && FishBoneActive)
                            {
                                if (Q.Cast())
                                {
                                    return;
                                }
                            }
                        }

                        if (swapAoe && Q.IsReady())
                        {
                            if (distance > powPowRange && PowPowStacks > 2 && !FishBoneActive &&
                                CountEnemies(t, 150) > 1)
                            {
                                if (Q.Cast())
                                {
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void CastQ()
        {
            foreach (var t in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(t => t.IsValidTarget(GetRealPowPowRange(t) + QAddRange + 20f)))
            {
                var swapDistance = Program.Menu.Item("SwapDistance").GetValue<bool>();
                var swapAoe = Program.Menu.Item("SwapAOE").GetValue<bool>();
                var distance = GetRealDistance(t);
                var powPowRange = GetRealPowPowRange(t);

                if (swapDistance && Q.IsReady())
                {
                    if (distance > powPowRange && !FishBoneActive)
                    {
                        if (Q.Cast())
                        {
                            return;
                        }
                    }
                    else if (distance < powPowRange && FishBoneActive)
                    {
                        if (Q.Cast())
                        {
                            return;
                        }
                    }
                }

                if (swapAoe && Q.IsReady())
                {
                    if (distance > powPowRange && PowPowStacks > 2 && !FishBoneActive && CountEnemies(t, 150) > 1)
                    {
                        if (Q.Cast())
                        {
                            return;
                        }
                    }
                }
            }
        }

        private void CastW()
        {
            var t = SimpleTs.GetTarget(W.Range, SimpleTs.DamageType.Physical);
            var minW = Program.Menu.Item("MinWRange").GetValue<Slider>().Value;

            if (t.IsValidTarget() && GetRealDistance(t) >= minW)
            {
                if (W.Cast(t) == Spell.CastStates.SuccessfullyCasted)
                {
                    return;
                }
            }
        }

        private void AutoE()
        {
            var autoEi = Program.Menu.Item("AutoEI").GetValue<bool>();
            var autoEs = Program.Menu.Item("AutoES").GetValue<bool>();
            var autoEd = Program.Menu.Item("AutoED").GetValue<bool>();
            
            foreach (
                    var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsValidTarget(E.Range - 150)))
            {
                if (autoEs && E.IsReady() && enemy.HasBuffOfType(BuffType.Slow))
                {
                    var castPosition =
                        Prediction.GetPrediction(
                            new PredictionInput
                            {
                                Unit = enemy,
                                Delay = 0.7f,
                                Radius = 120f,
                                Speed = 1750f,
                                Range = 900f,
                                Type = SkillshotType.SkillshotCircle,
                            }).CastPosition;


                    if (GetSlowEndTime(enemy) >= (Game.Time + E.Delay + 0.5f))
                    {
                        E.Cast(castPosition);
                    }
                }

                if (autoEi && E.IsReady() &&
                    (enemy.HasBuffOfType(BuffType.Stun) || enemy.HasBuffOfType(BuffType.Snare) ||
                     enemy.HasBuffOfType(BuffType.Charm) || enemy.HasBuffOfType(BuffType.Fear) ||
                     enemy.HasBuffOfType(BuffType.Taunt)))
                {
                    E.CastIfHitchanceEquals(enemy, HitChance.High);
                }

                if (autoEd && E.IsReady() && enemy.IsDashing())
                {
                    E.CastIfHitchanceEquals(enemy, HitChance.Dashing);
                }
            }
        }

        private void CastRT()
        {
            var maxR = Program.Menu.Item("MaxRRange").GetValue<Slider>().Value;
            var target = SimpleTs.GetTarget(maxR, SimpleTs.DamageType.Physical);

            if (target.IsValidTarget())
            {
                if (ObjectManager.Player.GetSpellDamage(target, SpellSlot.R) > target.Health)
                {
                    R.Cast(target, false, true);
                }
            }
        }

        private void AutoCastR()
        {
            var checkRok = Program.Menu.Item("ROverKill").GetValue<bool>();
            var minR = Program.Menu.Item("MinRRange").GetValue<Slider>().Value;
            var maxR = Program.Menu.Item("MaxRRange").GetValue<Slider>().Value;
            var t = SimpleTs.GetTarget(maxR, SimpleTs.DamageType.Physical);

            if (t.IsValidTarget())
            {
                var distance = GetRealDistance(t);

                if (!checkRok)
                {
                    if (ObjectManager.Player.GetSpellDamage(t, SpellSlot.R) > t.Health)
                    {
                        if (R.Cast(t, false, true) == Spell.CastStates.SuccessfullyCasted) { }
                    }
                }
                else if (checkRok && distance > minR)
                {
                    var aDamage = ObjectManager.Player.GetAutoAttackDamage(t);
                    var wDamage = ObjectManager.Player.GetSpellDamage(t, SpellSlot.W);
                    var rDamage = ObjectManager.Player.GetSpellDamage(t, SpellSlot.R);
                    var powPowRange = GetRealPowPowRange(t);

                    if (distance < (powPowRange + QAddRange) && !(aDamage * 3.5 > t.Health))
                    {
                        if (!W.IsReady() || !(wDamage > t.Health) || W.GetPrediction(t).CollisionObjects.Count > 0)
                        {
                            if (CountAlliesNearTarget(t, 500) <= 3)
                            {
                                if (rDamage > t.Health && !ObjectManager.Player.IsAutoAttacking &&
                                    !ObjectManager.Player.IsChanneling)
                                {
                                    if (R.Cast(t, false, true) == Spell.CastStates.SuccessfullyCasted) { }
                                }
                            }
                        }
                    }
                    else if (distance > (powPowRange + QAddRange))
                    {
                        if (!W.IsReady() || !(wDamage > t.Health) || distance > W.Range ||
                            W.GetPrediction(t).CollisionObjects.Count > 0)
                        {
                            if (CountAlliesNearTarget(t, 500) <= 3)
                            {
                                if (rDamage > t.Health && !ObjectManager.Player.IsAutoAttacking &&
                                    !ObjectManager.Player.IsChanneling)
                                {
                                    if (R.Cast(t, false, true) == Spell.CastStates.SuccessfullyCasted) { }
                                }
                            }
                        }
                    }
                }
            }
        }
        
        private float QAddRange
        {
            get { return 50 + 25 * ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).Level; }
        }

        private bool FishBoneActive
        {
            get { return Math.Abs(ObjectManager.Player.AttackRange - 525f) > float.Epsilon; }
        }

        private int PowPowStacks
        {
            get
            {
                return
                    ObjectManager.Player.Buffs.Where(buff => buff.DisplayName.ToLower() == "jinxqramp")
                        .Select(buff => buff.Count)
                        .FirstOrDefault();
            }
        }

        private int CountEnemies(Obj_AI_Base target, float range)
        {
            return
                ObjectManager.Get<Obj_AI_Hero>()
                    .Count(
                        hero =>
                            hero.IsValidTarget() && hero.Team != ObjectManager.Player.Team &&
                            hero.ServerPosition.Distance(target.ServerPosition) <= range);
        }

        private int CountAlliesNearTarget(Obj_AI_Base target, float range)
        {
            return
                ObjectManager.Get<Obj_AI_Hero>()
                    .Count(
                        hero =>
                            hero.Team == ObjectManager.Player.Team &&
                            hero.ServerPosition.Distance(target.ServerPosition) <= range);
        }

        private float GetRealPowPowRange(GameObject target)
        {
            return 525f + ObjectManager.Player.BoundingRadius + target.BoundingRadius;
        }

        private float GetRealDistance(GameObject target)
        {
            return ObjectManager.Player.Position.Distance(target.Position) + ObjectManager.Player.BoundingRadius +
                   target.BoundingRadius;
        }

        private float GetSlowEndTime(Obj_AI_Base target)
        {
            return
                target.Buffs.OrderByDescending(buff => buff.EndTime - Game.Time)
                    .Where(buff => buff.Type == BuffType.Slow)
                    .Select(buff => buff.EndTime)
                    .FirstOrDefault();
        }
    }
}
