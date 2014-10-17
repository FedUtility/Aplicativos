#region

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using LX_Orbwalker;
using Color = System.Drawing.Color;

#endregion

namespace FedAllChampionsUtility
{ 
    class DrMundo : Champion
    {
        public static List<Spell> SpellList = new List<Spell>();

        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;

        public static SpellSlot SmiteSlot;
        public static SpellSlot IgniteSlot;

        public static bool WActive = false;

        public static Obj_AI_Hero Player = ObjectManager.Player;

        public DrMundo()
        {
            LoadMenu();
            LoadSpells();

            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw; 

            PluginLoaded();
        }

        private void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 1000);
            W = new Spell(SpellSlot.W, Player.AttackRange + 25);
            E = new Spell(SpellSlot.E, Player.AttackRange + 25);
            R = new Spell(SpellSlot.R, Player.AttackRange + 25);

            SmiteSlot = Player.GetSpellSlot("SummonerSmite");
            IgniteSlot = Player.GetSpellSlot("SummonerDot");

            Q.SetSkillshot(0.50f, 75f, 1500f, true, SkillshotType.SkillshotLine);

            SpellList.AddRange(new[] { Q, W, E, R });
        }

        private void LoadMenu()
        {
            Program.Menu.AddSubMenu(new Menu("Combo", "Combo"));
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("UseItensCombo", "Use Items in Combo").SetValue(true));
            Program.Menu.SubMenu("Combo").AddItem(new MenuItem("ComboActive", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));

            Program.Menu.AddSubMenu(new Menu("Harass", "Harass"));
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("LifeHarass", "Dont Harass if HP < %").SetValue(new Slider(30, 100, 0)));
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("harassToggle", "Use Harass (toggle)").SetValue<KeyBind>(new KeyBind('T', KeyBindType.Toggle)));
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("HarassActive", "Harass!").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));

            Program.Menu.AddSubMenu(new Menu("Farm", "Farm"));
            Program.Menu.SubMenu("Farm").AddItem(new MenuItem("UseQFarm", "Use Q Farm").SetValue(true));
            Program.Menu.SubMenu("Farm").AddItem(new MenuItem("UseWFarm", "Use W Farm").SetValue(true));
            Program.Menu.SubMenu("Farm").AddItem(new MenuItem("UseEFarm", "Use E Farm").SetValue(true));
            Program.Menu.SubMenu("Farm").AddItem(new MenuItem("FreezeActive", "Freeze!").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
            Program.Menu.SubMenu("Farm").AddItem(new MenuItem("LaneClearActive", "LaneClear!").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));

            Program.Menu.AddSubMenu(new Menu("JungleFarm", "JungleFarm"));
            Program.Menu.SubMenu("JungleFarm").AddItem(new MenuItem("UseQJFarm", "Use Q").SetValue(true));
            Program.Menu.SubMenu("JungleFarm").AddItem(new MenuItem("UseWJFarm", "Use W").SetValue(true));
            Program.Menu.SubMenu("JungleFarm").AddItem(new MenuItem("UseEJFarm", "Use E").SetValue(true));
            Program.Menu.SubMenu("JungleFarm").AddItem(new MenuItem("AutoSmite", "Auto Smite!").SetValue<KeyBind>(new KeyBind('J', KeyBindType.Toggle)));
            Program.Menu.SubMenu("JungleFarm").AddItem(new MenuItem("JungleFarmActive", "JungleFarm!").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));

            Program.Menu.AddSubMenu(new Menu("Misc", "Misc"));            
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("KS", "Killsteal using Q").SetValue(false));            
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("RangeQ", "Q Range Slider").SetValue(new Slider(980, 1000, 0)));
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("lifesave", "Life saving Ultimate").SetValue(true));
            Program.Menu.SubMenu("Misc").AddItem(new MenuItem("percenthp", "Life Saving Ult %").SetValue(new Slider(30, 100, 0)));

            var dmgAfterComboItem = new MenuItem("DamageAfterCombo", "Draw damage after a rotation").SetValue(true);
            Utility.HpBarDamageIndicator.DamageToUnit = GetComboDamage;
            Utility.HpBarDamageIndicator.Enabled = dmgAfterComboItem.GetValue<bool>();
            dmgAfterComboItem.ValueChanged += delegate(object sender, OnValueChangeEventArgs eventArgs)
            {
                Utility.HpBarDamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
            };

            Program.Menu.AddSubMenu(new Menu("Drawing", "Drawing"));
            Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_Disabled", "Disable All").SetValue(false));
            Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_Q", "Draw Q").SetValue(true));
            Program.Menu.SubMenu("Drawing").AddItem(dmgAfterComboItem);
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (Program.Menu.Item("Draw_Disabled").GetValue<bool>())
                return;

            if (Program.Menu.Item("Draw_Q").GetValue<bool>())
                if (Q.Level > 0)
                    Utility.DrawCircle(ObjectManager.Player.Position, Q.Range, Q.IsReady() ? Color.Green : Color.Red);
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead) return;

            if (Player.HasBuff("BurningAgony"))
            {
                WActive = true;
            }
            else
            {
                WActive = false;
            }

            if (WActive && W.IsReady())
            {
                int inimigos = Utility.CountEnemysInRange(600);

                if (!Program.Menu.Item("LaneClearActive").GetValue<KeyBind>().Active && !Program.Menu.Item("JungleFarmActive").GetValue<KeyBind>().Active && inimigos == 0)
                {
                    W.Cast();
                }
            }

            if (Program.Menu.Item("harassToggle").GetValue<KeyBind>().Active)
            {
                ToggleHarass();
            }

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

                if (Program.Menu.Item("FreezeActive").GetValue<KeyBind>().Active)
                {
                    FreezeFarm();
                }

                if (Program.Menu.Item("JungleFarmActive").GetValue<KeyBind>().Active)
                {
                    JungleFarm();
                }

                if (Program.Menu.Item("LaneClearActive").GetValue<KeyBind>().Active)
                {
                    LaneClear();
                }                
            }

            if (Program.Menu.Item("lifesave").GetValue<bool>())
                LifeSave();

            if (Program.Menu.Item("KS").GetValue<bool>())
                Killsteal();

            if (Program.Menu.Item("AutoSmite").GetValue<KeyBind>().Active)
                AutoSmite();            
        }

        private float GetComboDamage(Obj_AI_Base enemy)
        {
            var damage = 0d;

            if (Q.IsReady())
                damage += ObjectManager.Player.GetSpellDamage(enemy, SpellSlot.Q);            

            if (W.IsReady())
                damage += ObjectManager.Player.GetSpellDamage(enemy, SpellSlot.W);            

            if (IgniteSlot != SpellSlot.Unknown && ObjectManager.Player.SummonerSpellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
                damage += ObjectManager.Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);            

            return (float)damage * 1;
        }        

        private void LifeSave()
        {
            int inimigos = Utility.CountEnemysInRange(900);

            if (Player.Health < (Player.MaxHealth * Program.Menu.Item("percenthp").GetValue<Slider>().Value * 0.01) && R.IsReady() && inimigos >= 1)
            {
                R.Cast();
            }
        }

        private void Killsteal()
        {
            int qRange = Program.Menu.Item("RangeQ").GetValue<Slider>().Value;
            var qTarget = SimpleTs.GetTarget(Q.Range + Q.Width, SimpleTs.DamageType.Magical);
            var Qdamage = ObjectManager.Player.GetSpellDamage(qTarget, SpellSlot.Q) * 0.95;

            if (qTarget != null && Program.Menu.Item("UseQCombo").GetValue<bool>() && Q.IsReady() && Player.Distance(qTarget) <= qRange && qTarget.Health < Qdamage)
            {
                PredictionOutput qPred = Q.GetPrediction(qTarget);
                if (qPred.Hitchance >= HitChance.High)
                    Q.Cast(qPred.CastPosition);
            }
        } 

        private void AutoSmite()
        {
            if (Program.Menu.Item("AutoSmite").GetValue<KeyBind>().Active)
            {
                float[] SmiteDmg = { 20 * Player.Level + 370, 30 * Player.Level + 330, 40 * Player.Level + 240, 50 * Player.Level + 100 };
                string[] MonsterNames = { "LizardElder", "AncientGolem", "Worm", "Dragon" };
                var vMinions = MinionManager.GetMinions(Player.ServerPosition, Player.SummonerSpellbook.Spells.FirstOrDefault(
                    spell => spell.Name.Contains("smite")).SData.CastRange[0], MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.Health);
                foreach (var vMinion in vMinions)
                {
                    if (vMinion != null
                        && !vMinion.IsDead
                        && !Player.IsDead
                        && !Player.IsStunned
                        && SmiteSlot != SpellSlot.Unknown
                        && Player.SummonerSpellbook.CanUseSpell(SmiteSlot) == SpellState.Ready)
                    {
                        if ((vMinion.Health < SmiteDmg.Max()) && (MonsterNames.Any(name => vMinion.BaseSkinName.StartsWith(name))))
                        {
                            Player.SummonerSpellbook.CastSpell(SmiteSlot, vMinion); 
                        }
                    }
                }
            }
        }

        private void Combo()
        {
            int qRange = Program.Menu.Item("RangeQ").GetValue<Slider>().Value;

            var qTarget = SimpleTs.GetTarget(Q.Range + Q.Width, SimpleTs.DamageType.Magical);
            
            if (qTarget != null && Program.Menu.Item("UseQCombo").GetValue<bool>() && Q.IsReady() && Player.Distance(qTarget) <= qRange)
            {
                PredictionOutput qPred = Q.GetPrediction(qTarget);
                if (qPred.Hitchance >= HitChance.High)
                    Q.Cast(qPred.CastPosition, Packets());
            }
            if (!WActive && qTarget != null && Program.Menu.Item("UseWCombo").GetValue<bool>() && W.IsReady() && Player.Distance(qTarget) <= 300)
            {
                W.Cast();
            }
            if (qTarget != null && Program.Menu.Item("UseECombo").GetValue<bool>() && E.IsReady() && Player.Distance(qTarget) <= qRange)
            {
                E.Cast();
            }
        }

        private void Harass()
        {
            var qTarget = SimpleTs.GetTarget(Q.Range + Q.Width, SimpleTs.DamageType.Magical);

            int qRange = Program.Menu.Item("RangeQ").GetValue<Slider>().Value;

            var RLife = Program.Menu.Item("LifeHarass").GetValue<Slider>().Value;
            var LPercentR = Player.Health * 100 / Player.MaxHealth;

            if (qTarget != null && Q.IsReady() && LPercentR >= RLife && Player.Distance(qTarget) <= qRange)
            {
                PredictionOutput qPred = Q.GetPrediction(qTarget);
                if (qPred.Hitchance >= HitChance.High)
                    Q.Cast(qPred.CastPosition, Packets());
            }
        }

        private void ToggleHarass()
        {
            var qTarget = SimpleTs.GetTarget(Q.Range + Q.Width, SimpleTs.DamageType.Magical);

            int qRange = Program.Menu.Item("RangeQ").GetValue<Slider>().Value;

            var RLife = Program.Menu.Item("LifeHarass").GetValue<Slider>().Value;
            var LPercentR = Player.Health * 100 / Player.MaxHealth;

            if (qTarget != null && Q.IsReady() && LPercentR >= RLife && Player.Distance(qTarget) <= qRange)
            {
                PredictionOutput qPred = Q.GetPrediction(qTarget);
                if (qPred.Hitchance >= HitChance.High)
                    Q.Cast(qPred.CastPosition, Packets());
            }
        }

        private void FreezeFarm()
        {
            var allMinionsQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range + Q.Width + 30, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.Health);

            foreach (var vMinion in allMinionsQ)
            {
                var Qdamage = ObjectManager.Player.GetSpellDamage(vMinion, SpellSlot.Q) * 0.85;

                if (Program.Menu.Item("UseQFarm").GetValue<bool>() && Q.IsReady() && Qdamage >= Q.GetHealthPrediction(vMinion))
                {
                    Q.Cast(vMinion.Position);
                }
            }
        }

        private void LaneClear()
        {
            var allMinionsQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range + Q.Width + 30, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.Health);
            var allMinionsW = MinionManager.GetMinions(Player.ServerPosition, 350, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.Health);

            foreach (var vMinion in allMinionsQ)
            {
                var Qdamage = ObjectManager.Player.GetSpellDamage(vMinion, SpellSlot.Q) * 0.85;

                if (Program.Menu.Item("UseQFarm").GetValue<bool>() && Q.IsReady() && Qdamage >= Q.GetHealthPrediction(vMinion))
                {
                    Q.Cast(vMinion.Position);
                }

                if (Program.Menu.Item("UseWFarm").GetValue<bool>() && W.IsReady() && !WActive && allMinionsW.Count > 2)
                {
                    W.Cast();
                }

                if (Program.Menu.Item("UseEFarm").GetValue<bool>() && E.IsReady() && allMinionsW.Count > 2)
                {
                    E.Cast();
                }
            }
        }

        private void JungleFarm()
        {
            var mobs = MinionManager.GetMinions(Player.ServerPosition, Q.Range,
                MinionTypes.All,
                MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (mobs.Count > 0)
            {
                if (Program.Menu.Item("UseQJFarm").GetValue<bool>() && Q.IsReady())
                {                    
                    Q.Cast(mobs[0].Position);
                }
                if (Program.Menu.Item("UseWJFarm").GetValue<bool>() && !WActive && W.IsReady())
                {                    
                    W.Cast();
                }
                if (Program.Menu.Item("UseEJFarm").GetValue<bool>() && E.IsReady())
                {                    
                    E.Cast();
                }
            }
        }
    }
}
