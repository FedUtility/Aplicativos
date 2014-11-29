using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
using LX_Orbwalker;

namespace FedAllChampionsUtility
{
    class Nidalee : Champion
    {
        public static Spell Q, W, E, R, QC, WC, EC;

        public static List<Spell> SpellList = new List<Spell>();

        private static Items.Item _tiamat, _hydra, _blade, _bilge, _rand, _lotis, _zhonya, _dfg;

        public static SpellSlot IgniteSlot;

        public static bool IsHuman;

        public static bool IsCougar;

        public static bool Recall;

        private static readonly float[] HumanQcd = { 6, 6, 6, 6, 6 };

        private static readonly float[] HumanWcd = { 12, 12, 12, 12, 12 };

        private static readonly float[] HumanEcd = { 13, 12, 11, 10, 9 };

        private static readonly float[] CougarQcd, CougarWcd, CougarEcd = { 5, 5, 5, 5, 5 };

        private static float _humQcd = 0, _humWcd = 0, _humEcd = 0;

        private static float _spidQcd = 0, _spidWcd = 0, _spidEcd = 0;

        private static float _humaQcd = 0, _humaWcd = 0, _humaEcd = 0;

        private static float _spideQcd = 0, _spideWcd = 0, _spideEcd = 0;
        public Nidalee()
        {
            LoadMenu();
            LoadSpells();

            Game.OnGameUpdate += Game_OnGameUpdate;
            Obj_AI_Hero.OnCreate += OnCreateObj;
            Obj_AI_Hero.OnDelete += OnDeleteObj;
            //Game_OnGameEnd += Game_OnGameEnd;
            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;

            PluginLoaded();
        }

        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 1500f);
            W = new Spell(SpellSlot.W, 900f);
            E = new Spell(SpellSlot.E, 600f);
            WC = new Spell(SpellSlot.W, 750f);
            EC = new Spell(SpellSlot.E, 300f);
            R = new Spell(SpellSlot.R, 0);

            Q.SetSkillshot(0.125f, 70f, 1300, true, SkillshotType.SkillshotLine);
            W.SetSkillshot(0.500f, 80f, 1450, false, SkillshotType.SkillshotCircle);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
            SpellList.Add(QC);
            SpellList.Add(WC);
            SpellList.Add(EC);

            _bilge = new Items.Item(3144, 475f);
            _blade = new Items.Item(3153, 425f);
            _hydra = new Items.Item(3074, 250f);
            _tiamat = new Items.Item(3077, 250f);
            _rand = new Items.Item(3143, 490f);
            _lotis = new Items.Item(3190, 590f);
            _dfg = new Items.Item(3128, 750f);
            _zhonya = new Items.Item(3157, 10);


            IgniteSlot = ObjectManager.Player.GetSpellSlot("SummonerDot");
        }

        private void LoadMenu()
        {
            //Combo
            Program.Menu.AddSubMenu(new Menu("Combo", "Combo"));
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "Use Q")).SetValue(true);
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "Use W")).SetValue(true);
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "Use R")).SetValue(true);
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("UseQComboCougar", "Use Q Cougar")).SetValue(true);
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("UseWComboCougar", "Use W Cougar")).SetValue(true);
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("UseEComboCougar", "Use E Cougar")).SetValue(true);
            Program.Menu.SubMenu("Combo")
                .AddItem(
                    new MenuItem("QHitCombo", "Q HitChange").SetValue(
                        new StringList(new[] { "Low", "Medium", "High", "Very High" })));

            Program.Menu.SubMenu("Combo")
                .AddItem(new MenuItem("ActiveCombo", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));

            //Extra
            Program.Menu.AddSubMenu(new Menu("Heal", "Heal"));
            Program.Menu.SubMenu("Heal").AddItem(new MenuItem("UseAutoE", "Use auto E")).SetValue(true);
            Program.Menu.SubMenu("Heal").AddItem(new MenuItem("HPercent", "Health percent")).SetValue(new Slider(40, 1, 100));
            Program.Menu.SubMenu("Heal").AddItem(new MenuItem("AllyUseAutoE", "Ally Use auto E")).SetValue(true);
            Program.Menu.SubMenu("Heal")
                .AddItem(new MenuItem("AllyHPercent", "Health percent"))
                .SetValue(new Slider(20, 1, 100));

            Program.Menu.AddSubMenu(new Menu("items", "items"));
            Program.Menu.SubMenu("items").AddSubMenu(new Menu("Offensive", "Offensive"));
            Program.Menu.SubMenu("items").SubMenu("Offensive").AddItem(new MenuItem("UseItemsdfg", "Use DFG")).SetValue(true);
            Program.Menu.SubMenu("items")
                .SubMenu("Offensive")
                .AddItem(new MenuItem("UseItemsignite", "Use Ignite"))
                .SetValue(true);
            Program.Menu.SubMenu("items").SubMenu("Offensive").AddItem(new MenuItem("Tiamat", "Use Tiamat")).SetValue(true);
            Program.Menu.SubMenu("items").SubMenu("Offensive").AddItem(new MenuItem("Hydra", "Use Hydra")).SetValue(true);
            Program.Menu.SubMenu("items").SubMenu("Offensive").AddItem(new MenuItem("Bilge", "Use Bilge")).SetValue(true);
            Program.Menu.SubMenu("items")
                .SubMenu("Offensive")
                .AddItem(new MenuItem("BilgeEnemyhp", "If Enemy Hp <").SetValue(new Slider(85, 1, 100)));
            Program.Menu.SubMenu("items")
                .SubMenu("Offensive")
                .AddItem(new MenuItem("Bilgemyhp", "Or your Hp < ").SetValue(new Slider(85, 1, 100)));
            Program.Menu.SubMenu("items").SubMenu("Offensive").AddItem(new MenuItem("Blade", "Use Blade")).SetValue(true);
            Program.Menu.SubMenu("items")
                .SubMenu("Offensive")
                .AddItem(new MenuItem("BladeEnemyhp", "If Enemy Hp <").SetValue(new Slider(85, 1, 100)));
            Program.Menu.SubMenu("items")
                .SubMenu("Offensive")
                .AddItem(new MenuItem("Blademyhp", "Or Your  Hp <").SetValue(new Slider(85, 1, 100)));
            Program.Menu.SubMenu("items").AddSubMenu(new Menu("Deffensive", "Deffensive"));
            Program.Menu.SubMenu("items")
                .SubMenu("Deffensive")
                .AddItem(new MenuItem("Omen", "Use Randuin Omen"))
                .SetValue(true);
            Program.Menu.SubMenu("items")
                .SubMenu("Deffensive")
                .AddItem(new MenuItem("Omenenemys", "Randuin if enemys>").SetValue(new Slider(2, 1, 5)));
            Program.Menu.SubMenu("items")
                .SubMenu("Deffensive")
                .AddItem(new MenuItem("lotis", "Use Iron Solari"))
                .SetValue(true);
            Program.Menu.SubMenu("items")
                .SubMenu("Deffensive")
                .AddItem(new MenuItem("lotisminhp", "Solari if Ally Hp<").SetValue(new Slider(35, 1, 100)));
            Program.Menu.SubMenu("items")
                .SubMenu("Deffensive")
                .AddItem(new MenuItem("Zhonyas", "Use Zhonya's"))
                .SetValue(true);
            Program.Menu.SubMenu("items")
                .SubMenu("Deffensive")
                .AddItem(new MenuItem("Zhonyashp", "Use Zhonya's if HP%<").SetValue(new Slider(20, 1, 100)));

            //Harass
            Program.Menu.AddSubMenu(new Menu("Harass", "Harass"));
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "Use Q")).SetValue(true);
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("UseWHarass", "Use W")).SetValue(false);
            Program.Menu.SubMenu("Harass")
                .AddItem(
                    new MenuItem("QHitharass", "Q HitChange").SetValue(
                        new StringList(new[] { "Low", "Medium", "High", "Very High" })));
            Program.Menu.SubMenu("Harass")
                .AddItem(
                    new MenuItem("harasstoggle", "AutoHarass (toggle)").SetValue(new KeyBind("G".ToCharArray()[0],
                        KeyBindType.Toggle)));
            Program.Menu.SubMenu("Harass")
                .AddItem(new MenuItem("Harrasmana", "Minimum Mana").SetValue(new Slider(60, 1, 100)));
            Program.Menu.SubMenu("Harass")
                .AddItem(
                    new MenuItem("ActiveHarass", "Harass key").SetValue(new KeyBind("X".ToCharArray()[0],
                        KeyBindType.Press)));


            Program.Menu.AddSubMenu(new Menu("Farm", "Farm"));
            Program.Menu.SubMenu("Farm").AddSubMenu(new Menu("LastHit", "LastHit"));
            Program.Menu.SubMenu("Farm").SubMenu("LastHit").AddItem(new MenuItem("UseQLH", "Use Q (Human)")).SetValue(true);
            Program.Menu.SubMenu("Farm")
                .SubMenu("LastHit")
                .AddItem(new MenuItem("lastmana", "Minimum Mana% >").SetValue(new Slider(35, 1, 100)));
            Program.Menu.SubMenu("Farm")
                .SubMenu("LastHit")
                .AddItem(
                    new MenuItem("ActiveLast", "LastHit!").SetValue(new KeyBind("X".ToCharArray()[0], KeyBindType.Press)));

            Program.Menu.SubMenu("Farm").AddSubMenu(new Menu("Lane/Jungle", "Lane"));
            Program.Menu.SubMenu("Farm").SubMenu("Lane").AddItem(new MenuItem("farm_E1", "Use E (Human)")).SetValue(true);
            Program.Menu.SubMenu("Farm").SubMenu("Lane").AddItem(new MenuItem("UseQLane", "Use Q (Cougar)")).SetValue(true);
            Program.Menu.SubMenu("Farm").SubMenu("Lane").AddItem(new MenuItem("UseWLane", "Use W (Cougar)")).SetValue(true);
            Program.Menu.SubMenu("Farm").SubMenu("Lane").AddItem(new MenuItem("UseELane", "Use E (Cougar)")).SetValue(true);
            Program.Menu.SubMenu("Farm")
                .SubMenu("Lane")
                .AddItem(
                    new MenuItem("LaneClear", "Clear key").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
            Program.Menu.SubMenu("Farm")
                .SubMenu("Lane")
                .AddItem(
                    new MenuItem("farm_R", "Auto Switch Forms(toggle)").SetValue(new KeyBind("G".ToCharArray()[0],
                        KeyBindType.Toggle)));
            Program.Menu.SubMenu("Farm")
                .SubMenu("Lane")
                .AddItem(new MenuItem("Lane", "Minimum Mana").SetValue(new Slider(60, 1, 100)));


            //Kill Steal
            Program.Menu.AddSubMenu(new Menu("Misc", "Misc"));
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("ActiveKs", "Use KillSteal")).SetValue(true);
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("UseQKs", "Use Q")).SetValue(true);
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("UseIgnite", "Use Ignite")).SetValue(true);
            Program.Menu.SubMenu("Misc")
                .AddItem(new MenuItem("escapeterino", "Escape!!!"))
                .SetValue(new KeyBind("Z".ToCharArray()[0], KeyBindType.Press));

            //Damage after combo:
            MenuItem dmgAfterComboItem = new MenuItem("DamageAfterCombo", "Draw damage after combo").SetValue(true);
            Utility.HpBarDamageIndicator.DamageToUnit = ComboDamage;
            Utility.HpBarDamageIndicator.Enabled = dmgAfterComboItem.GetValue<bool>();
            dmgAfterComboItem.ValueChanged +=
                delegate(object sender, OnValueChangeEventArgs eventArgs)
                {
                    Utility.HpBarDamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
                };

            //Drawings
            Program.Menu.AddSubMenu(new Menu("Drawings", "Drawings"));
            Program.Menu.SubMenu("Drawings").AddItem(new MenuItem("DrawQ", "Draw Q")).SetValue(true);
            Program.Menu.SubMenu("Drawings").AddItem(new MenuItem("DrawW", "Draw W")).SetValue(true);
            Program.Menu.SubMenu("Drawings").AddItem(new MenuItem("DrawE", "Draw E")).SetValue(true);
            Program.Menu.SubMenu("Drawings").AddItem(dmgAfterComboItem);
            Program.Menu.SubMenu("Drawings").AddItem(new MenuItem("CircleLag", "Lag Free Circles").SetValue(true));
            Program.Menu.SubMenu("Drawings").AddItem(new MenuItem("DrawCooldown", "Draw Cooldown")).SetValue(true);
            Program.Menu.SubMenu("Drawings")
                .AddItem(new MenuItem("CircleQuality", "Circles Quality").SetValue(new Slider(100, 100, 10)));
            Program.Menu.SubMenu("Drawings")
                .AddItem(new MenuItem("CircleThickness", "Circles Thickness").SetValue(new Slider(1, 10, 1)));
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (Program.Menu.Item("UseAutoE").GetValue<bool>())
            {
                AutoE();
            }
            if (Program.Menu.Item("escapeterino").GetValue<KeyBind>().Active)
            {
                Escapeterino();
            }
            AllyAutoE();
            Cooldowns();            
            QC = new Spell(SpellSlot.Q, ObjectManager.Player.AttackRange + 50);
            LXOrbwalker.SetAttack(true);

            CheckSpells();
            if (Program.Menu.Item("ActiveLast").GetValue<KeyBind>().Active &&
                (100 * (ObjectManager.Player.Mana / ObjectManager.Player.MaxMana)) > Program.Menu.Item("lastmana").GetValue<Slider>().Value)
            {
                LastHit();
            }
            if (Program.Menu.Item("ActiveCombo").GetValue<KeyBind>().Active)
            {
                Combo();
            }
            if ((Program.Menu.Item("ActiveHarass").GetValue<KeyBind>().Active || Program.Menu.Item("harasstoggle").GetValue<KeyBind>().Active) &&
                (100 * (ObjectManager.Player.Mana / ObjectManager.Player.MaxMana)) > Program.Menu.Item("Harrasmana").GetValue<Slider>().Value)
            {
                Harass();
            }
            if (Program.Menu.Item("LaneClear").GetValue<KeyBind>().Active)
            {
                Farm();
            }
            if (Program.Menu.Item("ActiveKs").GetValue<bool>())
            {
                KillSteal();
            }
        }

        private static void Escapeterino()
        {
            ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            if (IsHuman)
            {
                if (R.IsReady())
                {
                    R.Cast();
                }
                if (WC.IsReady())
                {
                    WC.Cast(Game.CursorPos);
                }
            }
            else if (IsCougar && WC.IsReady())
            {
                WC.Cast(Game.CursorPos);
            }
        }

        private static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
                //Game.PrintChat("Spell name: " + args.SData.Name.ToString());
                GetCDs(args);
        }

        private static float CalculateCd(float time)
        {
            return time + (time * ObjectManager.Player.PercentCooldownMod);
        }

        private static void Cooldowns()
        {
            _humaQcd = ((_humQcd - Game.Time) > 0) ? (_humQcd - Game.Time) : 0;
            _humaWcd = ((_humWcd - Game.Time) > 0) ? (_humWcd - Game.Time) : 0;
            _humaEcd = ((_humEcd - Game.Time) > 0) ? (_humEcd - Game.Time) : 0;
            _spideQcd = ((_spidQcd - Game.Time) > 0) ? (_spidQcd - Game.Time) : 0;
            _spideWcd = ((_spidWcd - Game.Time) > 0) ? (_spidWcd - Game.Time) : 0;
            _spideEcd = ((_spidEcd - Game.Time) > 0) ? (_spidEcd - Game.Time) : 0;
        }

        private static void GetCDs(GameObjectProcessSpellCastEventArgs spell)
        {
            if (IsHuman)
            {
                if (spell.SData.Name == "JavelinToss")
                    _humQcd = Game.Time + CalculateCd(HumanQcd[Q.Level]);
                if (spell.SData.Name == "Bushwhack")
                    _humWcd = Game.Time + CalculateCd(HumanWcd[W.Level]);
                if (spell.SData.Name == "PrimalSurge")
                    _humEcd = Game.Time + CalculateCd(HumanEcd[E.Level]);
            }
            else
            {
                if (spell.SData.Name == "Takedown")
                    _spidQcd = Game.Time + CalculateCd(CougarQcd[QC.Level]);
                if (spell.SData.Name == "Pounce")
                    _spidWcd = Game.Time + CalculateCd(CougarWcd[WC.Level]);
                if (spell.SData.Name == "Swipe")
                    _spidEcd = Game.Time + CalculateCd(CougarEcd[EC.Level]);
            }
        }

        private static HitChance QHitChanceCombo()
        {
            switch (Program.Menu.Item("QHitCombo").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    return HitChance.Low;
                case 1:
                    return HitChance.Medium;
                case 2:
                    return HitChance.High;
                case 3:
                    return HitChance.VeryHigh;
                default:
                    return HitChance.Medium;
            }
        }

        private static HitChance QHitChanceHarass()
        {
            switch (Program.Menu.Item("QHitharass").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    return HitChance.Low;
                case 1:
                    return HitChance.Medium;
                case 2:
                    return HitChance.High;
                case 3:
                    return HitChance.VeryHigh;
                default:
                    return HitChance.High;
            }
        }

        private static void Combo()
        {
            var target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
           LXOrbwalker.SetAttack((!Q.IsReady() || W.IsReady()));
            var itemsDfg = Program.Menu.Item("UseItemsdfg").GetValue<bool>();
            var itemsIgnite = Program.Menu.Item("UseItemsignite").GetValue<bool>();
            if (target != null)
            {
                if (_dfg.IsReady() && target.HasBuff("nidaleepassivehunted", true) && ObjectManager.Player.Distance(target) <= _dfg.Range && itemsDfg &&
                     target.Health <= ComboDamage(target))
                {
                    _dfg.Cast(target);
                }
                if (itemsIgnite && IgniteSlot != SpellSlot.Unknown &&
                    ObjectManager.Player.SummonerSpellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
                {
                    if (ComboDamage(target) > target.Health)
                    {
                        ObjectManager.Player.SummonerSpellbook.CastSpell(IgniteSlot, target);
                    }
                }
                if (Q.IsReady() && IsHuman && ObjectManager.Player.Distance(target) <= Q.Range && Program.Menu.Item("UseQCombo").GetValue<bool>())
                {
                    var prediction = Q.GetPrediction(target);
                    if (prediction.Hitchance >= QHitChanceCombo())
                        Q.Cast(prediction.CastPosition);

                }
                if (W.IsReady() && IsHuman && ObjectManager.Player.Distance(target) <= W.Range && Program.Menu.Item("UseWCombo").GetValue<bool>())
                {
                    W.Cast(target);
                }

                if (R.IsReady() && IsHuman && Program.Menu.Item("UseRCombo").GetValue<bool>() && ObjectManager.Player.Distance(target) <= 625)
                {
                    if (IsHuman)
                    {
                        R.Cast();
                    }

                    if (IsCougar)
                    {
                        if (WC.IsReady() && Program.Menu.Item("UseWComboCougar").GetValue<bool>() && ObjectManager.Player.Distance(target) <= WC.Range)
                        {
                            WC.Cast(target);
                        }
                        if (EC.IsReady() && Program.Menu.Item("UseEComboCougar").GetValue<bool>() && ObjectManager.Player.Distance(target) <= EC.Range)
                        {
                            EC.Cast(target);
                        }
                        if (QC.IsReady() && Program.Menu.Item("UseQComboCougar").GetValue<bool>() && ObjectManager.Player.Distance(target) <= QC.Range)
                        {
                          LXOrbwalker.SetAttack(true);
                            QC.Cast();
                        }

                    }

                }

                if (IsCougar && ObjectManager.Player.Distance(target) < 625)
                {
                    if (IsHuman && R.IsReady())
                    {
                        R.Cast();
                    }

                    if (IsCougar)
                    {
                        if (WC.IsReady() && Program.Menu.Item("UseWComboCougar").GetValue<bool>() && ObjectManager.Player.Distance(target) <= WC.Range)
                        {
                            WC.Cast(target);
                        }
                        if (EC.IsReady() && Program.Menu.Item("UseEComboCougar").GetValue<bool>() && ObjectManager.Player.Distance(target) <= EC.Range)
                        {
                            EC.Cast(target);
                        }
                        if (QC.IsReady() && Program.Menu.Item("UseQComboCougar").GetValue<bool>() && ObjectManager.Player.Distance(target) <= QC.Range)
                        {
                            LXOrbwalker.SetAttack(true);
                            QC.Cast();
                        }

                    }
                }

                if (R.IsReady() && IsCougar && Program.Menu.Item("UseRCombo").GetValue<bool>() && ObjectManager.Player.Distance(target) > WC.Range)
                {
                    R.Cast();
                }
                if (R.IsReady() && IsCougar && ObjectManager.Player.Distance(target) > EC.Range && Program.Menu.Item("UseRCombo").GetValue<bool>())
                {
                    R.Cast();
                }
            }

            UseItemes(target);
        }

        private static float ComboDamage(Obj_AI_Base hero)
        {
            var dmg = 0d;
            if (ObjectManager.Player.GetSpellSlot("SummonerIgnite") != SpellSlot.Unknown)
            {
                dmg += ObjectManager.Player.GetSummonerSpellDamage(hero, Damage.SummonerSpell.Ignite);
            }
            if (Items.HasItem(3128) && Items.CanUseItem(3128))
                dmg += ObjectManager.Player.GetItemDamage(hero, Damage.DamageItems.Dfg);
            if (Items.HasItem(3153) && Items.CanUseItem(3153))
                dmg += ObjectManager.Player.GetItemDamage(hero, Damage.DamageItems.Botrk);
            if (Items.HasItem(3144) && Items.CanUseItem(3144))
                dmg += ObjectManager.Player.GetItemDamage(hero, Damage.DamageItems.Bilgewater);
            if (QC.IsReady())
                dmg += ObjectManager.Player.GetSpellDamage(hero, SpellSlot.Q);
            if (EC.IsReady())
                dmg += ObjectManager.Player.GetSpellDamage(hero, SpellSlot.E);
            if (WC.IsReady())
                dmg += ObjectManager.Player.GetSpellDamage(hero, SpellSlot.W);
            if (Q.IsReady() && !IsCougar)
                dmg += ObjectManager.Player.GetSpellDamage(hero, SpellSlot.Q);
            return (float)dmg;
        }

        private static void UseItemes(Obj_AI_Hero target)
        {
            var iBilge = Program.Menu.Item("Bilge").GetValue<bool>();
            var iBilgeEnemyhp = target.Health <=
                                (target.MaxHealth * (Program.Menu.Item("BilgeEnemyhp").GetValue<Slider>().Value) / 100);
            var iBilgemyhp = ObjectManager.Player.Health <=
                             (ObjectManager.Player.MaxHealth * (Program.Menu.Item("Bilgemyhp").GetValue<Slider>().Value) / 100);
            var iBlade = Program.Menu.Item("Blade").GetValue<bool>();
            var iBladeEnemyhp = target.Health <=
                                (target.MaxHealth * (Program.Menu.Item("BladeEnemyhp").GetValue<Slider>().Value) / 100);
            var iBlademyhp = ObjectManager.Player.Health <=
                             (ObjectManager.Player.MaxHealth * (Program.Menu.Item("Blademyhp").GetValue<Slider>().Value) / 100);
            var iOmen = Program.Menu.Item("Omen").GetValue<bool>();
            var iOmenenemys = ObjectManager.Get<Obj_AI_Hero>().Count(hero => hero.IsValidTarget(450)) >=
                              Program.Menu.Item("Omenenemys").GetValue<Slider>().Value;
            var iTiamat = Program.Menu.Item("Tiamat").GetValue<bool>();
            var iHydra = Program.Menu.Item("Hydra").GetValue<bool>();
            var ilotis = Program.Menu.Item("lotis").GetValue<bool>();
            var iZhonyas = Program.Menu.Item("Zhonyas").GetValue<bool>();
            var iZhonyashp = ObjectManager.Player.Health <=
                             (ObjectManager.Player.MaxHealth * (Program.Menu.Item("Zhonyashp").GetValue<Slider>().Value) / 100);
            //var ihp = _Program.Menu.Item("Hppotion").GetValue<bool>();
            // var ihpuse = _ObjectManager.Player.Health <= (_ObjectManager.Player.MaxHealth * (_Program.Menu.Item("Hppotionuse").GetValue<Slider>().Value) / 100);
            //var imp = _Program.Menu.Item("Mppotion").GetValue<bool>();
            //var impuse = _ObjectManager.Player.Health <= (_ObjectManager.Player.MaxHealth * (_Program.Menu.Item("Mppotionuse").GetValue<Slider>().Value) / 100);

            if (ObjectManager.Player.Distance(target) <= 450 && iBilge && (iBilgeEnemyhp || iBilgemyhp) && _bilge.IsReady())
            {
                _bilge.Cast(target);

            }
            if (ObjectManager.Player.Distance(target) <= 450 && iBlade && (iBladeEnemyhp || iBlademyhp) && _blade.IsReady())
            {
                _blade.Cast(target);

            }
            if (iTiamat && _tiamat.IsReady() && target.IsValidTarget(_tiamat.Range))
            {
                _tiamat.Cast();

            }
            if (iHydra && _hydra.IsReady() && target.IsValidTarget(_hydra.Range))
            {
                _hydra.Cast();

            }
            if (iOmenenemys && iOmen && _rand.IsReady())
            {
                _rand.Cast();

            }
            if (ilotis)
            {
                foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsAlly || hero.IsMe))
                {
                    if (hero.Health <= (hero.MaxHealth * (Program.Menu.Item("lotisminhp").GetValue<Slider>().Value) / 100) &&
                        hero.Distance(ObjectManager.Player.ServerPosition) <= _lotis.Range && _lotis.IsReady())
                        _lotis.Cast();
                }
            }
            if (iZhonyas && iZhonyashp && Utility.CountEnemysInRange(1000) >= 1)
            {
                _zhonya.Cast(ObjectManager.Player);

            }
        }

        private static void Harass()
        {
            var target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
            if (target != null)
            {
                if (Q.IsReady() && IsHuman && ObjectManager.Player.Distance(target) <= Q.Range && Program.Menu.Item("UseQHarass").GetValue<bool>())
                {
                    var prediction = Q.GetPrediction(target);
                    if (prediction.Hitchance >= QHitChanceHarass())
                        Q.Cast(prediction.CastPosition);

                }

                if (W.IsReady() && IsHuman && ObjectManager.Player.Distance(target) <= W.Range && Program.Menu.Item("UseWHarass").GetValue<bool>())
                {
                    W.Cast(target);
                }
            }
        }


        private static void Farm()
        {
            foreach (
                Obj_AI_Minion Minion in
                    ObjectManager.Get<Obj_AI_Minion>()
                        .Where(
                            minion =>
                                minion.Team != ObjectManager.Player.Team && !minion.IsDead &&
                                Vector2.Distance(minion.ServerPosition.To2D(), ObjectManager.Player.ServerPosition.To2D()) < 600f)
                        .OrderBy(minion => Vector2.Distance(minion.Position.To2D(), ObjectManager.Player.Position.To2D())))
            {
                if (IsCougar)
                {
                    if (QC.IsReady() && Program.Menu.Item("UseQLane").GetValue<bool>() && ObjectManager.Player.Distance(Minion) < QC.Range)
                        QC.Cast();
                    else if (WC.IsReady() && Program.Menu.Item("UseWLane").GetValue<bool>() && ObjectManager.Player.Distance(Minion) > 200f)
                        WC.Cast(Minion);
                    else if (EC.IsReady() && Program.Menu.Item("UseELane").GetValue<bool>() &&
                             ObjectManager.Player.Distance(Minion) < EC.Range)
                        EC.Cast(Minion);
                }

                else if (R.IsReady() && Program.Menu.Item("farm_R").GetValue<KeyBind>().Active)
                    R.Cast();
                else if (E.IsReady() && !Program.Menu.Item("farm_R").GetValue<KeyBind>().Active &&
                         Program.Menu.Item("farm_E1").GetValue<bool>() &&
                         (100 * (ObjectManager.Player.Mana / ObjectManager.Player.MaxMana)) > Program.Menu.Item("Lane").GetValue<Slider>().Value)
                    E.CastOnUnit(ObjectManager.Player);
                return;
            }
        }

        private static void AutoE()
        {
            if (ObjectManager.Player.Spellbook.CanUseSpell(SpellSlot.E) == SpellState.Ready && ObjectManager.Player.IsMe)
            {

                if (ObjectManager.Player.HasBuff("Recall")) return;

                if (E.IsReady() && ObjectManager.Player.Health <= (ObjectManager.Player.MaxHealth * (Program.Menu.Item("HPercent").GetValue<Slider>().Value) / 100))
                {
                    ObjectManager.Player.Spellbook.CastSpell(SpellSlot.E, ObjectManager.Player);
                }

            }


        }

        private static void LastHit()
        {
            var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.All);
            var useQ = Program.Menu.Item("UseQLH").GetValue<bool>();
            foreach (var minion in allMinions)
            {
                if (Q.IsReady() && IsHuman && useQ && ObjectManager.Player.Distance(minion) < Q.Range &&
                    minion.Health <= 0.95 * ObjectManager.Player.GetSpellDamage(minion, SpellSlot.Q))
                {
                    Q.Cast(minion);
                }
            }
        }

        private static void AllyAutoE()
        {
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsAlly && !hero.IsMe))
            {
                if (ObjectManager.Player.HasBuff("Recall") || hero.HasBuff("Recall")) return;
                if (E.IsReady() && Program.Menu.Item("AllyUseAutoE").GetValue<bool>() &&
                    (hero.Health / hero.MaxHealth) * 100 <= Program.Menu.Item("AllyHPercent").GetValue<Slider>().Value &&
                    Utility.CountEnemysInRange(1200) > 0 &&
                    hero.Distance(ObjectManager.Player.ServerPosition) <= E.Range)
                {
                    E.Cast(hero);
                }
            }
        }

        private static void KillSteal()
        {
            var target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
            var igniteDmg = ObjectManager.Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
            var QHDmg = ObjectManager.Player.GetSpellDamage(target, SpellSlot.Q);

            if (target != null && Program.Menu.Item("UseIgnite").GetValue<bool>() && IgniteSlot != SpellSlot.Unknown &&
                ObjectManager.Player.SummonerSpellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
            {
                if (igniteDmg > target.Health)
                {
                    ObjectManager.Player.SummonerSpellbook.CastSpell(IgniteSlot, target);
                }
            }

            if (Q.IsReady() && ObjectManager.Player.Distance(target) <= Q.Range && target != null &&
                Program.Menu.Item("UseQKs").GetValue<bool>())
            {
                if (target.Health <= QHDmg)
                {
                    Q.Cast(target);
                }
            }
        }

        private static void CheckSpells()
        {
            if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).Name == "JavelinToss" ||
                ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Name == "Bushwhack" ||
                ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).Name == "PrimalSurge")
            {
                IsHuman = true;
                IsCougar = false;
            }
            if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).Name == "Takedown" ||
                ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Name == "Pounce" ||
                ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).Name == "Swipe")
            {
                IsHuman = false;
                IsCougar = true;
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            var cat = Drawing.WorldToScreen(ObjectManager.Player.Position);
            if (Program.Menu.Item("CircleLag").GetValue<bool>())
            {
                if (Program.Menu.Item("DrawQ").GetValue<bool>() && IsHuman)
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, Q.Range, System.Drawing.Color.White,
                        Program.Menu.Item("CircleThickness").GetValue<Slider>().Value,
                        Program.Menu.Item("CircleQuality").GetValue<Slider>().Value);
                }
                if (Program.Menu.Item("DrawW").GetValue<bool>() && IsHuman)
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, W.Range, System.Drawing.Color.White,
                        Program.Menu.Item("CircleThickness").GetValue<Slider>().Value,
                        Program.Menu.Item("CircleQuality").GetValue<Slider>().Value);
                }
                if (Program.Menu.Item("DrawE").GetValue<bool>() && IsHuman)
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, E.Range, System.Drawing.Color.White,
                        Program.Menu.Item("CircleThickness").GetValue<Slider>().Value,
                        Program.Menu.Item("CircleQuality").GetValue<Slider>().Value);
                }
            }
            else
            {
                if (Program.Menu.Item("DrawQ").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, Q.Range, System.Drawing.Color.White);
                }
                if (Program.Menu.Item("DrawW").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, W.Range, System.Drawing.Color.White);
                }
                if (Program.Menu.Item("DrawE").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, E.Range, System.Drawing.Color.White);
                }
            }
            if (Program.Menu.Item("DrawCooldown").GetValue<bool>())
            {
                if (!IsCougar)
                {
                    if (_spideQcd == 0)
                        Drawing.DrawText(cat[0] - 60, cat[1], Color.White, "CQ Rdy");
                    else
                        Drawing.DrawText(cat[0] - 60, cat[1], Color.Orange, "CQ: " + _spideQcd.ToString("0.0"));
                    if (_spideWcd == 0)
                        Drawing.DrawText(cat[0] - 20, cat[1] + 30, Color.White, "CW Rdy");
                    else
                        Drawing.DrawText(cat[0] - 20, cat[1] + 30, Color.Orange, "CW: " + _spideWcd.ToString("0.0"));
                    if (_spideEcd == 0)
                        Drawing.DrawText(cat[0], cat[1], Color.White, "CE Rdy");
                    else
                        Drawing.DrawText(cat[0], cat[1], Color.Orange, "CE: " + _spideEcd.ToString("0.0"));
                }
                else
                {
                    if (_humaQcd == 0)
                        Drawing.DrawText(cat[0] - 60, cat[1], Color.White, "HQ Rdy");
                    else
                        Drawing.DrawText(cat[0] - 60, cat[1], Color.Orange, "HQ: " + _humaQcd.ToString("0.0"));
                    if (_humaWcd == 0)
                        Drawing.DrawText(cat[0] - 20, cat[1] + 30, Color.White, "HW Rdy");
                    else
                        Drawing.DrawText(cat[0] - 20, cat[1] + 30, Color.Orange, "HW: " + _humaWcd.ToString("0.0"));
                    if (_humaEcd == 0)
                        Drawing.DrawText(cat[0], cat[1], Color.White, "HE Rdy");
                    else
                        Drawing.DrawText(cat[0], cat[1], Color.Orange, "HE: " + _humaEcd.ToString("0.0"));
                }
            }
        }


        private static void OnCreateObj(GameObject sender, EventArgs args)
        {
            //Recall
            if (!(sender is Obj_GeneralParticleEmmiter)) return;
            var obj = (Obj_GeneralParticleEmmiter)sender;
            if (obj != null && obj.IsMe && obj.Name == "TeleportHome")
            {
                Recall = true;
            }

        }

        private static void OnDeleteObj(GameObject sender, EventArgs args)
        {
            //Recall
            if (!(sender is Obj_GeneralParticleEmmiter)) return;
            var obj = (Obj_GeneralParticleEmmiter)sender;
            if (obj != null && obj.IsMe && obj.Name == "TeleportHome")
            {
                Recall = false;
            }
        }
    }
}
