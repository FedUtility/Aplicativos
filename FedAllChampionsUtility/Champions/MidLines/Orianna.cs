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
    class Orianna : Champion
    {
        //Spells
        public static List<Spell> SpellList = new List<Spell>();

        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;

        public static SpellSlot IgniteSlot;

        public static Obj_AI_Hero SelectedTarget = null;

        //ball manager
        public static bool IsBallMoving = false;
        public static Vector3 CurrentBallPosition;
        public static Vector3 allyDraw;
        public static int ballStatus = 0; 

        public Orianna()
        {
            LoadSpells();
            LoadMenu();

            //Events
            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPosibleToInterrupt;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Game.OnGameSendPacket += Game_OnSendPacket;            

            PluginLoaded();
        }

        private void LoadMenu()
        {
            //Keys
            Program.Menu.AddSubMenu(new Menu("Keys", "Keys"));
            Program.Menu.SubMenu("Keys")
                .AddItem(
                    new MenuItem("ComboActive", "Combo!").SetValue(
                        new KeyBind(Program.Menu.Item("Combo_Key").GetValue<KeyBind>().Key, KeyBindType.Press)));
            Program.Menu.SubMenu("Keys")
                .AddItem(
                    new MenuItem("HarassActive", "Harass!").SetValue(
                        new KeyBind(Program.Menu.Item("Harass_Key").GetValue<KeyBind>().Key, KeyBindType.Press)));
            Program.Menu.SubMenu("Keys")
                .AddItem(
                    new MenuItem("HarassActiveT", "Harass (toggle)!").SetValue(new KeyBind("T".ToCharArray()[0],
                        KeyBindType.Toggle)));
            Program.Menu.SubMenu("Keys")
                .AddItem(
                    new MenuItem("LastHitQQ", "Last hit with Q").SetValue(new KeyBind("A".ToCharArray()[0],
                        KeyBindType.Press)));
            Program.Menu.SubMenu("Keys")
                .AddItem(
                    new MenuItem("LaneClearActive", "Farm!").SetValue(
                        new KeyBind(Program.Menu.Item("LaneClear_Key").GetValue<KeyBind>().Key, KeyBindType.Press)));
            Program.Menu.SubMenu("Keys")
                .AddItem(
                    new MenuItem("escape", "RUN FOR YOUR LIFE!").SetValue(new KeyBind("Z".ToCharArray()[0],
                        KeyBindType.Press)));
            //Spell Menu
            Program.Menu.AddSubMenu(new Menu("Spell", "Spell"));
            //Q Menu
            Program.Menu.SubMenu("Spell").AddSubMenu(new Menu("QSpell", "QSpell"));
            Program.Menu.SubMenu("Spell").SubMenu("QSpell").AddItem(new MenuItem("qHit", "Q HitChance Combo").SetValue(new Slider(3, 1, 3)));
            Program.Menu.SubMenu("Spell").SubMenu("QSpell").AddItem(new MenuItem("qHit2", "Q HitChance Harass").SetValue(new Slider(3, 1, 4)));
            //W
            Program.Menu.SubMenu("Spell").AddSubMenu(new Menu("WSpell", "WSpell"));
            Program.Menu.SubMenu("Spell").SubMenu("WSpell").AddItem(new MenuItem("autoW", "Use W if hit").SetValue(new Slider(2, 1, 5)));
            //E
            Program.Menu.SubMenu("Spell").AddSubMenu(new Menu("ESpell", "ESpell"));
            Program.Menu.SubMenu("Spell").SubMenu("ESpell").AddItem(new MenuItem("UseEDmg", "Use E to Dmg").SetValue(true));
            Program.Menu.SubMenu("Spell").SubMenu("ESpell").AddSubMenu(new Menu("E Ally Inc Spell", "shield"));
            Program.Menu.SubMenu("Spell").SubMenu("ESpell").SubMenu("shield").AddItem(new MenuItem("eAllyIfHP", "If HP < %").SetValue(new Slider(40, 0, 100)));

            foreach (Obj_AI_Hero ally in ObjectManager.Get<Obj_AI_Hero>().Where(ally => ally.IsAlly))
                Program.Menu.SubMenu("Spell").SubMenu("ESpell")
                    .SubMenu("shield")
                    .AddItem(new MenuItem("shield" + ally.BaseSkinName, ally.BaseSkinName).SetValue(false));
            //R
            Program.Menu.SubMenu("Spell").AddSubMenu(new Menu("RSpell", "RSpell"));
            Program.Menu.SubMenu("Spell").SubMenu("RSpell").AddItem(new MenuItem("autoR", "Use R if hit").SetValue(new Slider(3, 1, 5)));
            Program.Menu.SubMenu("Spell").SubMenu("RSpell").AddItem(new MenuItem("blockR", "Block R if no enemy").SetValue(true));
            Program.Menu.SubMenu("Spell").SubMenu("RSpell").AddItem(new MenuItem("overK", "OverKill Check").SetValue(true));
            Program.Menu.SubMenu("Spell").SubMenu("RSpell").AddItem(
                    new MenuItem("killR", "R Multi Only Toggle").SetValue(new KeyBind("U".ToCharArray()[0],
                        KeyBindType.Toggle)));

            //Combo menu:
            Program.Menu.AddSubMenu(new Menu("Combo", "Combo"));
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("selected", "Focus Selected Target").SetValue(true));
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "Use R").SetValue(true));
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("autoRCombo", "Use R if hit").SetValue(new Slider(2, 1, 5)));
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("ignite", "Use Ignite").SetValue(true));

            //Harass menu:
            Program.Menu.AddSubMenu(new Menu("Harass", "Harass"));
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(true));
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("UseWHarass", "Use W").SetValue(true));
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("UseEHarass", "Use E").SetValue(false));

            //Farming menu:
            Program.Menu.AddSubMenu(new Menu("Farm", "Farm"));
            Program.Menu.SubMenu("Farm").AddItem(new MenuItem("UseQFarm", "Use Q").SetValue(false));
            Program.Menu.SubMenu("Farm").AddItem(new MenuItem("UseWFarm", "Use W").SetValue(false));
            Program.Menu.SubMenu("Farm").AddItem(new MenuItem("qFarm", "Only Q/W if > minion").SetValue(new Slider(3, 0, 5)));

            //intiator list:
            Program.Menu.AddSubMenu((new Menu("Initiator", "Initiator")));

            foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsAlly))
            {
                foreach (Initiator intiator in Initiator.InitatorList)
                {
                    if (intiator.HeroName == hero.BaseSkinName)
                    {
                        Program.Menu.SubMenu("Initiator")
                            .AddItem(new MenuItem(intiator.spellName, intiator.spellName))
                            .SetValue(false);
                    }
                }
            }

            //Misc Menu:
            Program.Menu.AddSubMenu(new Menu("Misc", "Misc"));
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("UseInt", "Use R to Interrupt").SetValue(true));
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("packet", "Use Packets").SetValue(true));

            Program.Menu.SubMenu("Misc").AddSubMenu(new Menu("Auto use R on", "intR"));

            foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != ObjectManager.Player.Team))
                Program.Menu.SubMenu("Misc")
                    .SubMenu("intR")
                    .AddItem(new MenuItem("intR" + enemy.BaseSkinName, enemy.BaseSkinName).SetValue(false));

            //Damage after combo:
            MenuItem dmgAfterComboItem = new MenuItem("DamageAfterCombo", "Draw damage after combo").SetValue(true);
            Utility.HpBarDamageIndicator.DamageToUnit = GetComboDamage;
            Utility.HpBarDamageIndicator.Enabled = dmgAfterComboItem.GetValue<bool>();
            dmgAfterComboItem.ValueChanged +=
                delegate(object sender, OnValueChangeEventArgs eventArgs)
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
                .AddItem(
                    new MenuItem("rModeDraw", "R mode").SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
            Program.Menu.SubMenu("Drawings")
                .AddItem(dmgAfterComboItem);            
        }

        private void LoadSpells()
        {
            //intalize spell
            Q = new Spell(SpellSlot.Q, 825);
            W = new Spell(SpellSlot.W, 250);
            E = new Spell(SpellSlot.E, 1095);
            R = new Spell(SpellSlot.R, 370);

            Q.SetSkillshot(0.25f, 80, 1300, false, SkillshotType.SkillshotLine);
            W.SetSkillshot(0f, 250, float.MaxValue, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.25f, 145, 1700, false, SkillshotType.SkillshotLine);
            R.SetSkillshot(0.60f, 370, float.MaxValue, false, SkillshotType.SkillshotCircle);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            IgniteSlot = ObjectManager.Player.GetSpellSlot("SummonerDot");
        }

        public static PredictionOutput GetP(Vector3 pos, Spell spell, Obj_AI_Base target, bool aoe)
        {
            return Prediction.GetPrediction(new PredictionInput
            {
                Unit = target,
                Delay = spell.Delay,
                Radius = spell.Width,
                Speed = spell.Speed,
                From = pos,
                Range = spell.Range,
                Collision = spell.Collision,
                Type = spell.Type,
                RangeCheckFrom = ObjectManager.Player.ServerPosition,
                Aoe = aoe,
            });
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

        private static float GetComboDamage(Obj_AI_Base enemy)
        {
            double damage = 0d;

            //if (Q.IsReady())
            damage += ObjectManager.Player.GetSpellDamage(enemy, SpellSlot.Q) * 1.5;

            if (W.IsReady())
                damage += ObjectManager.Player.GetSpellDamage(enemy, SpellSlot.W);

            if (E.IsReady())
                damage += ObjectManager.Player.GetSpellDamage(enemy, SpellSlot.E);

            if (IgniteSlot != SpellSlot.Unknown && ObjectManager.Player.SummonerSpellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
                damage += ObjectManager.Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);

            if (R.IsReady())
                damage += ObjectManager.Player.GetSpellDamage(enemy, SpellSlot.R) - 25;

            return (float)damage;
        }

        private static void Combo()
        {
            //Orbwalker.SetAttacks(!(Q.IsReady()));
            UseSpells(Program.Menu.Item("UseQCombo").GetValue<bool>(), Program.Menu.Item("UseWCombo").GetValue<bool>(),
                Program.Menu.Item("UseECombo").GetValue<bool>(), Program.Menu.Item("UseRCombo").GetValue<bool>(), "Combo");
        }

        private static void UseSpells(bool useQ, bool useW, bool useE, bool useR, String source)
        {
            var focusSelected = Program.Menu.Item("selected").GetValue<bool>();
            var range = E.IsReady() ? E.Range : Q.Range;
            Obj_AI_Hero target = SimpleTs.GetTarget(range, SimpleTs.DamageType.Magical);
            if (SimpleTs.GetSelectedTarget() != null)
                if (focusSelected && SimpleTs.GetSelectedTarget().Distance(ObjectManager.Player.ServerPosition) < range)
                    target = SimpleTs.GetSelectedTarget();

            if (useQ && Q.IsReady())
            {
                castQ(target, source);
            }

            if (IsBallMoving)
                return;

            if (useW && target != null && W.IsReady())
            {
                castW(target);
            }

            //Ignite
            if (target != null && Program.Menu.Item("ignite").GetValue<bool>() && IgniteSlot != SpellSlot.Unknown &&
                ObjectManager.Player.SummonerSpellbook.CanUseSpell(IgniteSlot) == SpellState.Ready && source == "Combo")
            {
                if (GetComboDamage(target) > target.Health)
                {
                    ObjectManager.Player.SummonerSpellbook.CastSpell(IgniteSlot, target);
                }
            }

            if (useE && target != null && E.IsReady())
            {
                castE(target);
            }

            if (useR && target != null && R.IsReady())
            {
                if (Program.Menu.Item("intR" + target.BaseSkinName) != null)
                {
                    foreach (
                        Obj_AI_Hero enemy in
                            ObjectManager.Get<Obj_AI_Hero>()
                                .Where(x => ObjectManager.Player.Distance(x) < 1500 && x.IsValidTarget() && x.IsEnemy && !x.IsDead))
                    {
                        if (enemy != null && !enemy.IsDead && Program.Menu.Item("intR" + enemy.BaseSkinName).GetValue<bool>())
                        {
                            castR(enemy);
                            return;
                        }
                    }
                }

                if (!(Program.Menu.Item("killR").GetValue<KeyBind>().Active)) //check if multi
                {
                    if (Program.Menu.Item("overK").GetValue<bool>() &&
                        (ObjectManager.Player.GetSpellDamage(target, SpellSlot.Q) * 2) >= target.Health)
                    {
                    }
                    if (GetComboDamage(target) >= target.Health - 100)
                        castR(target);
                }
            }
        }

        public static bool packets()
        {
            return Program.Menu.Item("packet").GetValue<bool>();
        }

        public static void castW(Obj_AI_Base target)
        {
            if (IsBallMoving) return;

            PredictionOutput prediction = GetPCircle(CurrentBallPosition, W, target, true);

            if (W.IsReady() && prediction.UnitPosition.Distance(CurrentBallPosition) < W.Width)
            {
                W.Cast();
            }

        }

        public static void castR(Obj_AI_Base target)
        {
            if (IsBallMoving) return;

            PredictionOutput prediction = GetPCircle(CurrentBallPosition, R, target, true);

            if (R.IsReady() && prediction.UnitPosition.Distance(CurrentBallPosition) <= R.Width)
            {
                R.Cast();
            }
        }

        public static void castE(Obj_AI_Base target)
        {
            if (IsBallMoving) return;

            Obj_AI_Hero etarget = ObjectManager.Player;

            switch (ballStatus)
            {
                case 0:
                    if (target != null)
                    {
                        float TravelTime = target.Distance(ObjectManager.Player.ServerPosition) / Q.Speed;
                        float MinTravelTime = 10000f;

                        foreach (
                            Obj_AI_Hero ally in
                                ObjectManager.Get<Obj_AI_Hero>()
                                    .Where(x => x.IsAlly && ObjectManager.Player.Distance(x.ServerPosition) <= E.Range && !x.IsMe))
                        {
                            if (ally != null)
                            {
                                //dmg enemy with E
                                if (Program.Menu.Item("UseEDmg").GetValue<bool>())
                                {
                                    PredictionOutput prediction3 = GetP(ObjectManager.Player.ServerPosition, E, target, true);
                                    Object[] obj = VectorPointProjectionOnLineSegment(ObjectManager.Player.ServerPosition.To2D(),
                                        ally.ServerPosition.To2D(), prediction3.UnitPosition.To2D());
                                    var isOnseg = (bool)obj[2];
                                    var PointLine = (Vector2)obj[1];

                                    if (E.IsReady() && isOnseg &&
                                        prediction3.UnitPosition.Distance(PointLine.To3D()) < E.Width)
                                    {
                                        //Game.PrintChat("Dmg 1");
                                        E.CastOnUnit(ally, packets());
                                        return;
                                    }
                                }

                                float allyRange = target.Distance(ally.ServerPosition) / Q.Speed +
                                                  ally.Distance(ObjectManager.Player.ServerPosition) / E.Speed;
                                if (allyRange < MinTravelTime)
                                {
                                    etarget = ally;
                                    MinTravelTime = allyRange;
                                }
                            }
                        }

                        if (MinTravelTime < TravelTime && ObjectManager.Player.Distance(etarget.ServerPosition) <= E.Range &&
                            E.IsReady())
                        {
                            E.CastOnUnit(etarget, packets());
                        }
                    }
                    break;
                case 1:
                    //dmg enemy with E
                    if (Program.Menu.Item("UseEDmg").GetValue<bool>())
                    {
                        PredictionOutput prediction = GetP(CurrentBallPosition, E, target, true);
                        Object[] obj = VectorPointProjectionOnLineSegment(CurrentBallPosition.To2D(),
                            ObjectManager.Player.ServerPosition.To2D(), prediction.UnitPosition.To2D());
                        var isOnseg = (bool)obj[2];
                        var PointLine = (Vector2)obj[1];

                        if (E.IsReady() && isOnseg && prediction.UnitPosition.Distance(PointLine.To3D()) < E.Width)
                        {
                            //Game.PrintChat("Dmg 2");
                            E.CastOnUnit(ObjectManager.Player, packets());
                            return;
                        }
                    }

                    float TravelTime2 = target.Distance(CurrentBallPosition) / Q.Speed;
                    float MinTravelTime2 = target.Distance(ObjectManager.Player.ServerPosition) / Q.Speed +
                                            ObjectManager.Player.Distance(CurrentBallPosition) / E.Speed;

                    if (MinTravelTime2 < TravelTime2 && target.Distance(ObjectManager.Player.ServerPosition) <= Q.Range + Q.Width &&
                        E.IsReady())
                    {
                        E.CastOnUnit(ObjectManager.Player, packets());
                    }

                    break;
                case 2:
                    float TravelTime3 = target.Distance(CurrentBallPosition) / Q.Speed;
                    float MinTravelTime3 = 10000f;

                    foreach (
                        Obj_AI_Hero ally in
                            ObjectManager.Get<Obj_AI_Hero>()
                                .Where(x => x.IsAlly && ObjectManager.Player.Distance(x.ServerPosition) <= E.Range && !x.IsMe))
                    {
                        if (ally != null)
                        {
                            //dmg enemy with E
                            if (Program.Menu.Item("UseEDmg").GetValue<bool>())
                            {
                                PredictionOutput prediction2 = GetP(CurrentBallPosition, E, target, true);
                                Object[] obj = VectorPointProjectionOnLineSegment(CurrentBallPosition.To2D(),
                                    ally.ServerPosition.To2D(), prediction2.UnitPosition.To2D());
                                var isOnseg = (bool)obj[2];
                                var PointLine = (Vector2)obj[1];

                                if (E.IsReady() && isOnseg &&
                                    prediction2.UnitPosition.Distance(PointLine.To3D()) < E.Width)
                                {
                                    //Game.PrintChat("Dmg 3");
                                    E.CastOnUnit(ally, packets());
                                    return;
                                }
                            }

                            float allyRange2 = target.Distance(ally.ServerPosition) / Q.Speed +
                                                ally.Distance(CurrentBallPosition) / E.Speed;

                            if (allyRange2 < MinTravelTime3)
                            {
                                etarget = ally;
                                MinTravelTime3 = allyRange2;
                            }
                        }
                    }

                    if (MinTravelTime3 < TravelTime3 && ObjectManager.Player.Distance(etarget.ServerPosition) <= E.Range &&
                        E.IsReady())
                    {
                        E.CastOnUnit(etarget, packets());
                    }

                    break;
            }
        }

        public static void castQ(Obj_AI_Base target, String Source)
        {
            if (IsBallMoving) return;

            var hitC = HitChance.High;
            int qHit = Program.Menu.Item("qHit").GetValue<Slider>().Value;
            int harassQHit = Program.Menu.Item("qHit2").GetValue<Slider>().Value;

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

            PredictionOutput prediction = GetP(CurrentBallPosition, Q, target, true);

            if (Q.IsReady() && prediction.Hitchance >= hitC && ObjectManager.Player.Distance(target) <= Q.Range + Q.Width)
            {
                Q.Cast(prediction.CastPosition, packets());
            }
        }

        public static void checkWMec()
        {
            if (!W.IsReady() || IsBallMoving)
                return;

            int hit = 0;
            int minHit = Program.Menu.Item("autoW").GetValue<Slider>().Value;

            foreach (
                Obj_AI_Hero enemy in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(x => ObjectManager.Player.Distance(x) < 1500 && x.IsValidTarget() && x.IsEnemy && !x.IsDead))
            {
                if (enemy != null)
                {
                    PredictionOutput prediction = GetPCircle(CurrentBallPosition, W, enemy, true);

                    if (W.IsReady() && prediction.UnitPosition.Distance(CurrentBallPosition) < W.Width)
                    {
                        hit++;
                    }
                }
            }

            if (hit >= minHit && W.IsReady())
                W.Cast();
        }

        public static void checkRMec()
        {
            if (!R.IsReady() || IsBallMoving)
                return;

            int hit = 0;
            int minHit = Program.Menu.Item("autoRCombo").GetValue<Slider>().Value;

            foreach (
                Obj_AI_Hero enemy in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(x => ObjectManager.Player.Distance(x) < 1500 && x.IsValidTarget() && x.IsEnemy && !x.IsDead))
            {
                if (enemy != null)
                {
                    PredictionOutput prediction = GetPCircle(CurrentBallPosition, R, enemy, true);

                    if (R.IsReady() && prediction.UnitPosition.Distance(CurrentBallPosition) <= R.Width)
                    {
                        hit++;
                    }
                }
            }

            if (hit >= minHit && R.IsReady())
                R.Cast();
        }

        public static void checkRMecGlobal()
        {
            if (!R.IsReady() || IsBallMoving)
                return;

            int hit = 0;
            int minHit = Program.Menu.Item("autoR").GetValue<Slider>().Value;

            foreach (
                Obj_AI_Hero enemy in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(x => ObjectManager.Player.Distance(x) < 1500 && x.IsValidTarget() && x.IsEnemy && !x.IsDead))
            {
                if (enemy != null)
                {
                    PredictionOutput prediction = GetPCircle(CurrentBallPosition, R, enemy, true);

                    if (R.IsReady() && prediction.UnitPosition.Distance(CurrentBallPosition) <= R.Width)
                    {
                        hit++;
                    }
                }
            }

            if (hit >= minHit && R.IsReady())
                R.Cast();
        }

        //credit to dien
        public static Object[] VectorPointProjectionOnLineSegment(Vector2 v1, Vector2 v2, Vector2 v3)
        {
            float cx = v3.X;
            float cy = v3.Y;
            float ax = v1.X;
            float ay = v1.Y;
            float bx = v2.X;
            float by = v2.Y;
            float rL = ((cx - ax) * (bx - ax) + (cy - ay) * (by - ay)) /
                       ((float)Math.Pow(bx - ax, 2) + (float)Math.Pow(by - ay, 2));
            var pointLine = new Vector2(ax + rL * (bx - ax), ay + rL * (by - ay));
            float rS;
            if (rL < 0)
            {
                rS = 0;
            }
            else if (rL > 1)
            {
                rS = 1;
            }
            else
            {
                rS = rL;
            }
            bool isOnSegment;
            if (rS.CompareTo(rL) == 0)
            {
                isOnSegment = true;
            }
            else
            {
                isOnSegment = false;
            }
            var pointSegment = new Vector2();
            if (isOnSegment)
            {
                pointSegment = pointLine;
            }
            else
            {
                pointSegment = new Vector2(ax + rS * (bx - ax), ay + rS * (by - ay));
            }
            return new object[3] { pointSegment, pointLine, isOnSegment };
        }

        public static int countR()
        {
            if (!R.IsReady())
                return 0;

            int hit = 0;
            foreach (
                Obj_AI_Hero enemy in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(x => ObjectManager.Player.Distance(x) < 1500 && x.IsValidTarget() && x.IsEnemy && !x.IsDead))
            {
                if (enemy != null)
                {
                    PredictionOutput prediction = GetPCircle(CurrentBallPosition, R, enemy, true);

                    if (R.IsReady() && prediction.UnitPosition.Distance(CurrentBallPosition) <= R.Width)
                    {
                        hit++;
                    }
                }
            }

            return hit;
        }

        public static void lastHit()
        {
            if (!Orbwalking.CanMove(40)) return;

            List<Obj_AI_Base> allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range);

            if (Q.IsReady())
            {
                foreach (Obj_AI_Base minion in allMinions)
                {
                    if (minion.IsValidTarget() &&
                        HealthPrediction.GetHealthPrediction(minion, (int)(ObjectManager.Player.Distance(minion) * 1000 / 1400)) <
                        ObjectManager.Player.GetSpellDamage(minion, SpellSlot.Q) - 10)
                    {
                        PredictionOutput prediction = GetP(CurrentBallPosition, Q, minion, true);

                        if (prediction.Hitchance >= HitChance.High && Q.IsReady())
                            Q.Cast(prediction.CastPosition, packets());
                    }
                }
            }
        }

        private static void Farm()
        {
            if (!Orbwalking.CanMove(40)) return;

            List<Obj_AI_Base> allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition,
                Q.Range + Q.Width, MinionTypes.All);
            List<Obj_AI_Base> allMinionsW = MinionManager.GetMinions(ObjectManager.Player.ServerPosition,
                Q.Range + Q.Width, MinionTypes.All);

            var useQ = Program.Menu.Item("UseQFarm").GetValue<bool>();
            var useW = Program.Menu.Item("UseWFarm").GetValue<bool>();
            int min = Program.Menu.Item("qFarm").GetValue<Slider>().Value;

            int hit = 0;

            if (useQ && Q.IsReady())
            {
                foreach (Obj_AI_Base enemy in allMinionsW)
                {
                    Q.From = CurrentBallPosition;

                    MinionManager.FarmLocation pred = Q.GetCircularFarmLocation(allMinionsQ, Q.Width + 15);

                    if (pred.MinionsHit >= min)
                        Q.Cast(pred.Position, packets());
                }
            }

            hit = 0;
            if (useW && W.IsReady())
            {
                foreach (Obj_AI_Base enemy in allMinionsW)
                {
                    if (enemy.Distance(CurrentBallPosition) < W.Range)
                        hit++;
                }

                if (hit >= min && W.IsReady())
                    W.Cast();
            }
        }

        private static void Harass()
        {
            UseSpells(Program.Menu.Item("UseQHarass").GetValue<bool>(), Program.Menu.Item("UseWHarass").GetValue<bool>(),
                Program.Menu.Item("UseEHarass").GetValue<bool>(), false, "Harass");
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            foreach (Spell spell in SpellList)
            {
                var menuItem = Program.Menu.Item(spell.Slot + "Range").GetValue<Circle>();
                if ((spell.Slot == SpellSlot.R && menuItem.Active) || (spell.Slot == SpellSlot.W && menuItem.Active))
                {
                    if (ballStatus == 0)
                        Utility.DrawCircle(ObjectManager.Player.Position, spell.Range, spell.IsReady() ? Color.Aqua : Color.Red);
                    else if (ballStatus == 2)
                        Utility.DrawCircle(allyDraw, spell.Range, spell.IsReady() ? Color.Aqua : Color.Red);
                    else
                        Utility.DrawCircle(CurrentBallPosition, spell.Range, spell.IsReady() ? Color.Aqua : Color.Red);
                }
                else if (menuItem.Active)
                    Utility.DrawCircle(ObjectManager.Player.Position, spell.Range, spell.IsReady() ? Color.Aqua : Color.Red);
            }
            if (Program.Menu.Item("rModeDraw").GetValue<Circle>().Active)
            {
                if (Program.Menu.Item("killR").GetValue<KeyBind>().Active)
                {
                    Vector2 wts = Drawing.WorldToScreen(ObjectManager.Player.Position);
                    Drawing.DrawText(wts[0], wts[1], Color.White, "R Multi On");
                }
                else
                {
                    Vector2 wts = Drawing.WorldToScreen(ObjectManager.Player.Position);
                    Drawing.DrawText(wts[0], wts[1], Color.Red, "R Multi Off");
                }
            }
        }

        public static void onGainBuff()
        {
            if (ObjectManager.Player.HasBuff("OrianaGhostSelf"))
            {
                ballStatus = 0;
                CurrentBallPosition = ObjectManager.Player.ServerPosition;
                IsBallMoving = false;
                return;
            }

            foreach (Obj_AI_Hero ally in
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(ally => ally.IsAlly && !ally.IsDead && ally.HasBuff("orianaghost", true)))
            {
                ballStatus = 2;
                CurrentBallPosition = ally.ServerPosition;
                allyDraw = ally.Position;
                IsBallMoving = false;
                return;
            }

            ballStatus = 1;
        }

        public static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs args)
        {
            //Shield Ally
            if (unit.IsEnemy && unit.Type == GameObjectType.obj_AI_Hero && E.IsReady())
            {
                foreach (
                    Obj_AI_Hero ally in
                        ObjectManager.Get<Obj_AI_Hero>()
                            .Where(x => ObjectManager.Player.Distance(x) < E.Range && ObjectManager.Player.Distance(unit) < 1500 && x.IsAlly && !x.IsDead).OrderBy(x => x.Distance(args.End)))
                {
                    if (Program.Menu.Item("shield" + ally.BaseSkinName) != null)
                    {
                        if (ally != null && Program.Menu.Item("shield" + ally.BaseSkinName).GetValue<bool>())
                        {
                            int hp = Program.Menu.Item("eAllyIfHP").GetValue<Slider>().Value;
                            float hpPercent = ally.Health / ally.MaxHealth * 100;

                            if (ally.Distance(args.End) < 500 && hpPercent <= hp)
                            {
                                //Game.PrintChat("shielding");
                                E.CastOnUnit(ally, packets());
                                IsBallMoving = true;
                                return;
                            }
                        }
                    }
                }
            }

            //intiator
            if (unit.IsAlly)
            {
                foreach (Initiator spell in Initiator.InitatorList)
                {
                    if (args.SData.Name == spell.SDataName)
                    {
                        if (Program.Menu.Item(spell.spellName).GetValue<bool>())
                        {
                            if (E.IsReady() && ObjectManager.Player.Distance(unit) < E.Range)
                            {
                                E.CastOnUnit(unit, packets());
                                IsBallMoving = true;
                                return;
                            }
                        }
                    }
                }
            }

            if (!unit.IsMe) return;

            SpellSlot castedSlot = ObjectManager.Player.GetSpellSlot(args.SData.Name, false);

            if (castedSlot == SpellSlot.Q)
            {
                IsBallMoving = true;
                Utility.DelayAction.Add(
                    (int)Math.Max(1, 1000 * (args.End.Distance(CurrentBallPosition) - Game.Ping - 0.1) / Q.Speed), () =>
                    {
                        CurrentBallPosition = args.End;
                        ballStatus = 1;
                        IsBallMoving = false;
                        //Game.PrintChat("Stopped");
                    });
            }
        }

        private static void Interrupter_OnPosibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (!Program.Menu.Item("UseInt").GetValue<bool>()) return;

            if (ObjectManager.Player.Distance(unit) < R.Range && unit != null)
            {
                castR(unit);
            }
            else
            {
                castQ(unit, "Combo");
            }
        }

        private static void Game_OnSendPacket(GamePacketEventArgs args)
        {
            if (args.PacketData[0] == Packet.C2S.Cast.Header)
            {
                Packet.C2S.Cast.Struct decodedPacket = Packet.C2S.Cast.Decoded(args.PacketData);
                if (decodedPacket.Slot == SpellSlot.R)
                {
                    if (countR() == 0 && Program.Menu.Item("blockR").GetValue<bool>())
                    {
                        //Block packet if enemies hit is 0
                        args.Process = false;
                    }
                }
            }
        }

        public static void escape()
        {
            if (ballStatus == 0 && W.IsReady())
                W.Cast();
            else if (E.IsReady() && ballStatus != 0)
                E.CastOnUnit(ObjectManager.Player, packets());
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            //check if player is dead
            if (ObjectManager.Player.IsDead) return;

            onGainBuff();

            checkRMecGlobal();

            if (Program.Menu.Item("escape").GetValue<KeyBind>().Active)
            {
                escape();
            }
            else if (Program.Menu.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                checkRMec();
                Combo();
            }
            else
            {
                if (Program.Menu.Item("HarassActive").GetValue<KeyBind>().Active ||
                    Program.Menu.Item("HarassActiveT").GetValue<KeyBind>().Active)
                    Harass();

                if (Program.Menu.Item("LaneClearActive").GetValue<KeyBind>().Active)
                {
                    Farm();
                }

                if (Program.Menu.Item("LastHitQQ").GetValue<KeyBind>().Active)
                {
                    lastHit();
                }
            }

            checkWMec();
        }
    }
}
