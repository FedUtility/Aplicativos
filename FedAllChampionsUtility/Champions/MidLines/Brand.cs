#region

using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using LeagueSharp.Common;

using SharpDX;

using Color = System.Drawing.Color;

#endregion



namespace FedAllChampionsUtility
{
    class Brand : Champion
    {
        public static Spell Q, W, E, R;
        public static List<Spell> spellList = new List<Spell>();
        public static int bounceRadiusR = 450;

        public static bool hasIgnite = false;
        public static SpellSlot igniteSlot;
        public static Items.Item DFG;

        public Brand()
        {
            LoadMenu();
            LoadSpells();

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPossibleToInterrupt;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGabcloser;
            PluginLoaded();
        }

        private void LoadSpells()
        {
            // Initialize spells
            Q = new Spell(SpellSlot.Q, 1100);
            W = new Spell(SpellSlot.W, 900);
            E = new Spell(SpellSlot.E, 625);
            R = new Spell(SpellSlot.R, 750);

            // Finetune spells
            Q.SetSkillshot(0.25f, 60, 1600, true, SkillshotType.SkillshotLine);
            W.SetSkillshot(1, 240, float.MaxValue, false, SkillshotType.SkillshotCircle);
            E.SetTargetted(0.25f, float.MaxValue);
            R.SetTargetted(0.25f, 1000);

            // Add to spell list
            spellList.AddRange(new[] { Q, W, E, R });

            // Ignite check
            var ignite = ObjectManager.Player.Spellbook.GetSpell(ObjectManager.Player.GetSpellSlot("SummonerDot"));
            if (ignite != null && ignite.Slot != SpellSlot.Unknown)
            {
                hasIgnite = true;
                igniteSlot = ignite.Slot;
            }

            DFG = Utility.Map.GetMap()._MapType == Utility.Map.MapType.TwistedTreeline ? new Items.Item(3188, 750) : new Items.Item(3128, 750);
           
        }

        private void LoadMenu()
        {            
            // Combo
            Program.Menu.AddSubMenu(new Menu("TeamFight", "TeamFight"));            
            Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("comboUseQ", "Use Q").SetValue(true));
            Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("comboUseW", "Use W").SetValue(true));
            Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("comboUseE", "Use E").SetValue(true));
            Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("comboUseR", "Use R").SetValue(true));
            Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("comboActive", "Combo active").SetValue<KeyBind>(new KeyBind(32, KeyBindType.Press)));

            // Harass
            Program.Menu.AddSubMenu(new Menu("Harass", "Harass"));
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("harassUseQ", "Use Q").SetValue(true));
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("harassUseW", "Use W").SetValue(true));
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("harassToggleW", "Use W (toggle)").SetValue<KeyBind>(new KeyBind('T', KeyBindType.Toggle)));
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("harassUseE", "Use E").SetValue(true));
            Program.Menu.SubMenu("Harass").AddItem(new MenuItem("harassActive", "Harass active").SetValue<KeyBind>(new KeyBind('C', KeyBindType.Press)));

            // Wave clear
            Program.Menu.AddSubMenu(new Menu("LaneClear", "LaneClear"));
            Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("waveUseQ", "Use Q").SetValue(true));
            Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("waveUseW", "Use W").SetValue(true));
            Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("waveUseE", "Use E").SetValue(true));
            Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("waveNumW", "Minions to hit with W").SetValue<Slider>(new Slider(3, 1, 10)));
            Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("waveActive", "WaveClear active").SetValue<KeyBind>(new KeyBind('V', KeyBindType.Press)));

            Program.Menu.AddSubMenu(new Menu("Passive", "Passive"));
            Program.Menu.SubMenu("Passive").AddItem(new MenuItem("useQ_Interrupt", "Use Q Interrupt").SetValue(true));
            Program.Menu.SubMenu("Passive").AddItem(new MenuItem("useQ_Antigapclose", "Use Q AntigapcloseS").SetValue(true));

            // Drawings
            Program.Menu.AddSubMenu(new Menu("Drawings", "Drawings"));
            Program.Menu.SubMenu("Drawings").AddItem(new MenuItem("Draw_Disabled", "Disable All").SetValue(false));
            Program.Menu.SubMenu("Drawings").AddItem(new MenuItem("drawRangeQ", "Q range").SetValue(new Circle(true, Color.FromArgb(150, Color.IndianRed))));
            Program.Menu.SubMenu("Drawings").AddItem(new MenuItem("drawRangeW", "W range").SetValue(new Circle(true, Color.FromArgb(150, Color.IndianRed))));
            Program.Menu.SubMenu("Drawings").AddItem(new MenuItem("drawRangeE", "E range").SetValue(new Circle(false, Color.FromArgb(150, Color.DarkRed))));
            Program.Menu.SubMenu("Drawings").AddItem(new MenuItem("drawRangeR", "R range").SetValue(new Circle(false, Color.FromArgb(150, Color.Red))));
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (Program.Menu.Item("Draw_Disabled").GetValue<bool>())
                return;

            // Spell ranges
            foreach (var spell in spellList)
            {
                var circleEntry = Program.Menu.SubMenu("Drawings").Item("drawRange" + spell.Slot.ToString()).GetValue<Circle>();
                if (circleEntry.Active)
                    Utility.DrawCircle(ObjectManager.Player.Position, spell.Range, spell.IsReady() ? Color.Green : Color.Red);
            }
            

        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            // Combo
            if (Program.Menu.SubMenu("TeamFight").Item("comboActive").GetValue<KeyBind>().Active)
                OnCombo();

            // Harass
            if (Program.Menu.SubMenu("Harass").Item("harassActive").GetValue<KeyBind>().Active)
                OnHarass();

            // Wave clear
            if (Program.Menu.SubMenu("LaneClear").Item("waveActive").GetValue<KeyBind>().Active)
                OnWaveClear();

            // Toggles
            if (Program.Menu.SubMenu("Harass").Item("harassToggleW").GetValue<KeyBind>().Active && W.IsReady())
            {
                var target = SimpleTs.GetTarget(W.Range, SimpleTs.DamageType.Magical);
                if (target != null)
                    W.CastIfHitchanceEquals(target, HitChance.High);
            }
        }

        private static float GetComboDamage(Obj_AI_Base enemy)
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

            if (igniteSlot != SpellSlot.Unknown && ObjectManager.Player.SummonerSpellbook.CanUseSpell(igniteSlot) == SpellState.Ready)
                damage += ObjectManager.Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);

            if (R.IsReady())
                damage += Math.Min(7, ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).Ammo) * ObjectManager.Player.GetSpellDamage(enemy, SpellSlot.R, 1);

            return (float)damage * (DFG.IsReady() ? 1.2f : 1);
        }

        private static bool mainComboKillable()
        {
            var target = SimpleTs.GetTarget(W.Range, SimpleTs.DamageType.Magical);
            var comboDamage = target != null ? GetComboDamage(target) : 0; 

            if (comboDamage > target.Health)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        private static void OnCombo()
        {
            // Target aquireing
            var target = SimpleTs.GetTarget(W.Range, SimpleTs.DamageType.Magical);

            // Target validation
            if (target == null)
                return;

            // Spell usage
            bool useQ = Program.Menu.SubMenu("TeamFight").Item("comboUseQ").GetValue<bool>();
            bool useW = Program.Menu.SubMenu("TeamFight").Item("comboUseW").GetValue<bool>();
            bool useE = Program.Menu.SubMenu("TeamFight").Item("comboUseE").GetValue<bool>();
            bool useR = Program.Menu.SubMenu("TeamFight").Item("comboUseR").GetValue<bool>();            

            // Killable status          
            
            bool inMinimumRange = Vector2.DistanceSquared(target.ServerPosition.To2D(), ObjectManager.Player.Position.To2D()) < E.Range * E.Range;

            foreach (var spell in spellList)
            {
                // Continue if spell not ready
                if (!spell.IsReady())
                    continue;

                // Q
                if (spell.Slot == SpellSlot.Q && useQ)
                {
                    if ((mainComboKillable() && inMinimumRange) || // Main combo killable
                        (!useW && !useE) || // Casting when not using W and E
                        (IsAblazed(target)) || // Ablazed
                        (target.Health < ObjectManager.Player.GetSpellDamage(target, SpellSlot.Q)) || // Killable
                        (useW && !useE && !W.IsReady() && W.IsReady((int)(ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).Cooldown * 1000))) || // Cooldown substraction W ready
                        ((useE && !useW || useW && useE) && !E.IsReady() && E.IsReady((int)(ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).Cooldown * 1000)))) // Cooldown substraction E ready
                    {
                        // Cast Q on high hitchance
                        Q.CastIfHitchanceEquals(target, HitChance.High);
                    }
                }
                // W
                else if (spell.Slot == SpellSlot.W && useW)
                {
                    if ((mainComboKillable() && inMinimumRange) || // Main combo killable
                        (!useE) || // Casting when not using E
                        (IsAblazed(target)) || // Ablazed
                        (target.Health < ObjectManager.Player.GetSpellDamage(target, SpellSlot.W)) || // Killable
                        (Vector2.DistanceSquared(target.ServerPosition.To2D(), ObjectManager.Player.Position.To2D()) > E.Range * E.Range) ||
                        (!E.IsReady() && E.IsReady((int)(ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Cooldown * 1000)))) // Cooldown substraction E ready
                    {
                        // Cast W on high hitchance
                        W.CastIfHitchanceEquals(target, HitChance.High);
                    }
                }
                // E
                else if (spell.Slot == SpellSlot.E && useE)
                {
                    // Distance check
                    if (Vector2.DistanceSquared(target.ServerPosition.To2D(), ObjectManager.Player.Position.To2D()) < E.Range * E.Range)
                    {
                        if ((mainComboKillable()) || // Main combo killable
                            (!useQ && !useW) || // Casting when not using Q and W
                            (E.Level >= 4) || // E level high, damage output higher
                            (useQ && (Q.IsReady() || ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).Cooldown < 5)) || // Q ready
                            (useW && W.IsReady())) // W ready
                        {
                            // Cast E on target
                            E.CastOnUnit(target);
                        }
                    }
                }
                // R
                else if (spell.Slot == SpellSlot.R && useR)
                {
                    // Distance check
                    if (Vector2.DistanceSquared(target.ServerPosition.To2D(), ObjectManager.Player.Position.To2D()) < R.Range * R.Range)
                    {
                        // Logic prechecks
                        if ((useQ && Q.IsReady() && Q.GetPrediction(target).Hitchance == HitChance.High || useW && W.IsReady()) && ObjectManager.Player.Health / ObjectManager.Player.MaxHealth > 0.4f)
                            continue;

                        // Single hit
                        if (mainComboKillable() && inMinimumRange || target.Health < ObjectManager.Player.GetSpellDamage(target, SpellSlot.R))
                            R.CastOnUnit(target);                        
                    }
                }
            }
        }

        private static void OnHarass()
        {
            // Target aquireing
            var target = SimpleTs.GetTarget(W.Range, SimpleTs.DamageType.Magical);

            // Target validation
            if (target == null)
                return;

            // Spell usage
            bool useQ = Program.Menu.SubMenu("Harass").Item("harassUseQ").GetValue<bool>();
            bool useW = Program.Menu.SubMenu("Harass").Item("harassUseW").GetValue<bool>();
            bool useE = Program.Menu.SubMenu("Harass").Item("harassUseE").GetValue<bool>();

            foreach (var spell in spellList)
            {
                // Continue if spell not ready
                if (!spell.IsReady())
                    continue;

                // Q
                if (spell.Slot == SpellSlot.Q && useQ)
                {
                    if ((IsAblazed(target)) || // Ablazed
                        (target.Health < ObjectManager.Player.GetSpellDamage(target, SpellSlot.Q)) || // Killable
                        (!useW && !useE) || // Casting when not using W and E
                        (useW && !useE && !W.IsReady() && ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).CooldownExpires - Game.Time > ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).Cooldown) || // Cooldown substraction W ready, jodus please...
                        ((useE && !useW || useW && useE) && !E.IsReady() && ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).CooldownExpires - Game.Time > ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).Cooldown)) // Cooldown substraction E ready, jodus please...
                    {
                        // Cast Q on high hitchance
                        Q.CastIfHitchanceEquals(target, HitChance.High);
                    }
                }
                // W
                else if (spell.Slot == SpellSlot.W && useW)
                {
                    if ((!useE) || // Casting when not using E
                        (IsAblazed(target)) || // Ablazed
                        (target.Health < ObjectManager.Player.GetSpellDamage(target, SpellSlot.W)) || // Killable
                        (Vector2.DistanceSquared(target.ServerPosition.To2D(), ObjectManager.Player.Position.To2D()) > E.Range * E.Range) ||
                        (!E.IsReady() && ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).CooldownExpires - Game.Time > ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Cooldown)) // Cooldown substraction E ready
                    {
                        // Cast W on high hitchance
                        W.CastIfHitchanceEquals(target, HitChance.High);
                    }
                }
                // E
                else if (spell.Slot == SpellSlot.E && useE)
                {
                    // Distance check
                    if (Vector2.DistanceSquared(target.ServerPosition.To2D(), ObjectManager.Player.Position.To2D()) < E.Range * E.Range)
                    {
                        if ((!useQ && !useW) || // Casting when not using Q and W
                            target.Health < ObjectManager.Player.GetSpellDamage(target, SpellSlot.E) || // Killable
                            (useQ && (Q.IsReady() || ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).Cooldown < 5)) || // Q ready
                            (useW && W.IsReady())) // W ready
                        {
                            // Cast E on target
                            E.CastOnUnit(target);
                        }
                    }
                }
            }
        }

        private static void OnWaveClear()
        {
            // Minions around
            var minions = MinionManager.GetMinions(ObjectManager.Player.Position, W.Range + W.Width / 2);

            // Spell usage
            bool useQ = Q.IsReady() && Program.Menu.SubMenu("LaneClear").Item("waveUseQ").GetValue<bool>();
            bool useW = W.IsReady() && Program.Menu.SubMenu("LaneClear").Item("waveUseW").GetValue<bool>();
            bool useE = E.IsReady() && Program.Menu.SubMenu("LaneClear").Item("waveUseE").GetValue<bool>();

            if (useQ)
            {
                // Loop through all minions to find a target, preferred a killable one
                Obj_AI_Base target = null;
                foreach (var minion in minions)
                {
                    var prediction = Q.GetPrediction(minion);
                    if (prediction.Hitchance == HitChance.High)
                    {
                        // Set target
                        target = minion;

                        // Break if killlable
                        if (minion.Health > ObjectManager.Player.GetAutoAttackDamage(minion) && minion.Health > ObjectManager.Player.GetSpellDamage(minion, SpellSlot.Q))
                            break;
                    }
                }

                // Cast if target found
                if (target != null)
                    Q.Cast(target);
            }

            if (useW)
            {
                // Get farm location
                var farmLocation = MinionManager.GetBestCircularFarmLocation(minions.Select(minion => minion.ServerPosition.To2D()).ToList(), W.Width, W.Range);

                // Check required hitnumber and cast
                if (farmLocation.MinionsHit >= Program.Menu.SubMenu("LaneClear").Item("waveNumW").GetValue<Slider>().Value)
                    W.Cast(farmLocation.Position);
            }

            if (useE)
            {
                // Loop through all minions to find a target
                foreach (var minion in minions)
                {
                    // Distance check
                    if (Vector2.DistanceSquared(minion.ServerPosition.To2D(), ObjectManager.Player.Position.To2D()) < E.Range * E.Range)
                    {
                        // E only on targets that are ablaze or killable
                        if (IsAblazed(minion) || minion.Health > ObjectManager.Player.GetAutoAttackDamage(minion) && minion.Health > ObjectManager.Player.GetSpellDamage(minion, SpellSlot.E))
                        {
                            E.CastOnUnit(minion);
                            break;
                        }
                    }
                }
            }
        }
                    

        private static bool IsAblazed(Obj_AI_Base target)
        {
            return target.HasBuff("brandablaze", true);
        }

        private void AntiGapcloser_OnEnemyGabcloser(ActiveGapcloser gapcloser)
        {
            if (!Q.IsReady() || !gapcloser.Sender.IsValidTarget(Q.Range) ||
                !Program.Menu.Item("useQ_Antigapclose").GetValue<bool>())
                return;
            Q.Cast(gapcloser.Sender, Packets());
        }

        private void Interrupter_OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (!Q.IsReady() || !unit.IsValidTarget(Q.Range) || unit.IsAlly ||
                !Program.Menu.Item("useQ_Interrupt").GetValue<bool>())
                return;
            Q.Cast(unit, Packets());
        }

    }
}
