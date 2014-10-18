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
    class Rengar : Champion
    {
        private static Obj_AI_Hero Player = ObjectManager.Player;
        private static Items.Item YGB, TMT, HYD, BCL, BRK, DFG;
        
        private static Spell Q, W, E, R;        

        private static float LastETick;
        public Rengar()
        {
            LoadSpells();
            LoadMenu();

            Game.OnGameUpdate += OnGameUpdate;
            Obj_AI_Hero.OnProcessSpellCast += OnProcessSpell;
            Drawing.OnDraw += OnDraw;
            Drawing.OnEndScene += OnDraw_EndScene;            

            PluginLoaded();
        }

        private void LoadMenu()
        {
            // Keys
            var KeyBindings = new Menu("Key Bindings", "KB");            
            KeyBindings.AddItem(new MenuItem("KeysE", "Cast E").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
            var FeroSwitcher = KeyBindings.AddItem(new MenuItem("KeysFS", "Switch Ferocity spell").SetValue(new KeyBind("N".ToCharArray()[0], KeyBindType.Press)));

            // Combo
            var Combo = new Menu("Combo", "Combo");
            Combo.AddItem(new MenuItem("FeroSpellC", "Ferocity").SetValue(new StringList(new[] { "Q", "W", "E" }, 2)));
            Combo.AddItem(new MenuItem("ForceWC", "Force W %HP").SetValue(new Slider(30)));
            Combo.AddItem(new MenuItem("ForceEC", "Force E").SetValue(false));

            // Harass
            var Harass = new Menu("Harass", "Harass");
            Harass.AddItem(new MenuItem("HarassW", "W").SetValue(true));
            Harass.AddItem(new MenuItem("HarassE", "E").SetValue(true));
            Harass.AddItem(new MenuItem("FeroSpellH", "Ferocity").SetValue(new StringList(new[] { "OFF", "W", "E" })));

            // Lane Clear
            var LaneClear = new Menu("Lane/Jungle Clear", "LJC");
            LaneClear.AddItem(new MenuItem("FeroSaveRRdy", "Save 5 Ferocity").SetValue(true));
            LaneClear.AddItem(new MenuItem("FeroSpellF", "Ferocity").SetValue(new StringList(new[] { "Q", "W", "E" }, 1)));
            LaneClear.AddItem(new MenuItem("ForceWF", "Force W %HP").SetValue(new Slider(70)));

            // LastHit
            var LastHit = new Menu("Last Hit", "LH");
            LastHit.AddItem(new MenuItem("LastHitW", "W").SetValue(true));
            LastHit.AddItem(new MenuItem("LastHitE", "E").SetValue(true));
            LastHit.AddItem(new MenuItem("FeroSpellLH", "Ferocity").SetValue(new StringList(new[] { "OFF", "W", "E" })));

            // Drawings
            var Drawings = new Menu("Drawings", "Drawings");
            Drawings.AddItem(new MenuItem("DrawW", "W Range").SetValue(true));
            Drawings.AddItem(new MenuItem("DrawE", "E Range").SetValue(true));
            Drawings.AddItem(new MenuItem("DrawES", "E: Search").SetValue(true));
            Drawings.AddItem(new MenuItem("DrawUR", "R").SetValue(new StringList(new[] { "Off", "Normal", "Minimap", "Both" }, 2)));
            Drawings.AddItem(new MenuItem("DrawFS", "Ferocity Spell").SetValue(true));
            
            Program.Menu.AddSubMenu(KeyBindings);
            Program.Menu.AddSubMenu(Combo);
            Program.Menu.AddSubMenu(Harass);
            Program.Menu.AddSubMenu(LaneClear);
            Program.Menu.AddSubMenu(LastHit);
            Program.Menu.AddSubMenu(Drawings);

            FeroSwitcher.ValueChanged += delegate(object sender, OnValueChangeEventArgs vcArgs)
            {
                if (vcArgs.GetOldValue<KeyBind>().Active) return;

                var FeroSpell = Program.Menu.Item("FeroSpellC");
                var OldValues = FeroSpell.GetValue<StringList>();
                var NewValue = OldValues.SelectedIndex + 1 >= OldValues.SList.Count() ? 0 : OldValues.SelectedIndex + 1;
                FeroSpell.SetValue(new StringList(OldValues.SList, NewValue));
            };
        }

        private void LoadSpells()
        {
                    YGB = new Items.Item(3142, 0f);     // Ghostblade
                    TMT = new Items.Item(3077, 400f);   // Tiamat
                    HYD = new Items.Item(3074, 400f);   // Hydra
                    BCL = new Items.Item(3144, 450f);   // Cutlass
                    BRK = new Items.Item(3153, 450f);   // Blade of the Ruined King
                    DFG = new Items.Item(GetDFG(), 750f); // Deathfire Grasp

                    Q = new Spell(SpellSlot.Q);
                    W = new Spell(SpellSlot.W, 500f);
                    E = new Spell(SpellSlot.E, 1000f);
                    R = new Spell(SpellSlot.R);

                    E.SetSkillshot(.5f, 70f, 1500f, true, SkillshotType.SkillshotLine);
        }

        private static void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;

            var drawW = Program.Menu.Item("DrawW").GetValue<bool>();
            var drawE = Program.Menu.Item("DrawE").GetValue<bool>();
            var drawES = Program.Menu.Item("DrawES").GetValue<bool>();
            var drawFS = Program.Menu.Item("DrawFS").GetValue<bool>();
            var drawUR = Program.Menu.Item("DrawUR").GetValue<StringList>();

            // W Range
            if (drawW)
                Utility.DrawCircle(Player.Position, W.Range, W.IsReady() ? Color.Green : Color.Red);

            // E Range
            if (drawE)
                Utility.DrawCircle(Player.Position, E.Range, E.IsReady() ? Color.Green : Color.Red);

            // E Search Position
            if (drawES && Program.Menu.Item("KeysE").GetValue<KeyBind>().Active)
            {
                Vector3 SearchPosition;

                if (Player.Distance(Game.CursorPos) < E.Range - 200f)
                    SearchPosition = Game.CursorPos;
                else
                    SearchPosition = Player.Position + Vector3.Normalize(Game.CursorPos - Player.Position) * (E.Range - 200f);

                Utility.DrawCircle(SearchPosition, 200f, E.IsReady() ? Color.Green : Color.Red);
            }

            // Ultimate Range
            if (R.Level > 0 && (drawUR.SelectedIndex == 1 || drawUR.SelectedIndex == 3))
                Utility.DrawCircle(Player.Position, 1000f + 1000f * R.Level, Color.Green, 10);

            // Ferocity Spell
            if (drawFS)
            {
                var FeroSpell = Program.Menu.Item("FeroSpellC").GetValue<StringList>();

                int posX = 0;
                if (Drawing.WorldToMinimap(new Vector3()).X < Drawing.Width / 2)
                    posX = Drawing.Width - 140;
                else
                    posX = 10;

                Drawing.DrawText(posX, (Drawing.Height * 0.85f), Color.YellowGreen, "Ferocity Spell: {0}", FeroSpell.SList[FeroSpell.SelectedIndex]);
            }
        }

        private static void OnDraw_EndScene(EventArgs args)
        {
            var drawUR = Program.Menu.Item("DrawUR").GetValue<StringList>();

            if (drawUR.SelectedIndex > 1 && R.Level > 0)
                Utility.DrawCircle(Player.Position, 1000f + 1000f * R.Level, Color.Green, 1, 30, true);
        }

        private static void OnProcessSpell(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && args.SData.Name == "RengarE")
                LastETick = Environment.TickCount;
        }

        private static void OnGameUpdate(EventArgs args)
        {
            var useE = Program.Menu.Item("KeysE").GetValue<KeyBind>();
            if (useE.Active && E.IsReady())
            {
                var ForceE = Program.Menu.Item("ForceEC").GetValue<bool>();
                Vector3 SearchPosition;

                if (Player.Distance(Game.CursorPos) < E.Range - 200f)
                    SearchPosition = Game.CursorPos;
                else
                    SearchPosition = Player.Position + Vector3.Normalize(Game.CursorPos - Player.Position) * (E.Range - 200f);

                var Target = ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget(E.Range) && hero.Distance(SearchPosition) < 200f).OrderByDescending(hero => SimpleTs.GetPriority(hero)).First();
                if (Target.IsValid && (!Target.HasBuff("RengarEFinalMAX", true) && !Target.HasBuff("rengareslow") && LastETick + 1500 < Environment.TickCount || ForceE))
                    E.Cast(Target);
            }

            switch (LXOrbwalker.CurrentMode)
            {
                case LXOrbwalker.Mode.Combo:
                    doCombo();
                    break;
                case LXOrbwalker.Mode.LaneClear:
                    doFarm();
                    break;
                case LXOrbwalker.Mode.Harass:
                    doHarass();
                    break;
                case LXOrbwalker.Mode.Lasthit:
                    doLastHit();
                    break;
            }
        }

        private static void doCombo()
        {
            var FeroSpell = Program.Menu.Item("FeroSpellC").GetValue<StringList>();
            var ForceW = Program.Menu.Item("ForceWC").GetValue<Slider>();
            var ForceE = Program.Menu.Item("ForceEC").GetValue<bool>();

            var Target = SimpleTs.GetTarget(1600f, SimpleTs.DamageType.Physical);

            if (Player.HasBuff("RengarR", true))
                LXOrbwalker.ForcedTarget = Target;


            // Use Tiamat / Hydra
            if (Target.IsValidTarget(TMT.Range))
                if (TMT.IsReady()) TMT.Cast();
                else if (HYD.IsReady()) HYD.Cast();

            // Use Yommus Ghostblade
            if (YGB.IsReady() && Target.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player)))
                YGB.Cast();

            // Cutlass
            if (BCL.IsReady() && Target.IsValidTarget(BCL.Range))
                BCL.Cast(Target);

            // BORK
            if (BRK.IsReady() && Target.IsValidTarget(BRK.Range))
                BRK.Cast(Target);

            // DFG
            if (W.IsReady() && DFG.IsReady() && Target.IsValidTarget(DFG.Range))
                DFG.Cast(Target);

            // Ferocity Spell
            if (Player.Mana == 5)
            {
                if (Player.Health / Player.MaxHealth < ForceW.Value / 100f && Target.IsValidTarget(W.Range))
                {
                    W.Cast();
                    return;
                }

                switch (FeroSpell.SelectedIndex)
                {
                    case 0:
                        if (!Target.IsValidTarget(LXOrbwalker.GetAutoAttackRange(Player)))
                            return;
                        Q.Cast();
                        break;
                    case 1:
                        if (!Target.IsValidTarget(W.Range))
                            return;
                        W.Cast();
                        break;
                    case 2:
                        if (!Target.IsValidTarget(E.Range) || Player.HasBuff("RengarR", true))
                            return;
                        E.Cast(Target);
                        break;
                }
                return;
            }

            // Normal Spells
            if (Q.IsReady() && Target.IsValidTarget(LXOrbwalker.GetAutoAttackRange(Player)))
                Q.Cast();

            // Don't cast W or E while ultimate is active (force leap)
            if (Player.HasBuff("RengarR", true))
                return;

            if (E.IsReady() && Target.IsValidTarget(E.Range) && (!Target.HasBuff("RengarEFinalMAX", true) && !Target.HasBuff("rengareslow") && LastETick + 1500 < Environment.TickCount || ForceE))
                E.Cast(Target);

            if (W.IsReady() && Target.IsValidTarget(W.Range))
                W.Cast();
        }

        private static void doFarm()
        {
            var SaveFero = Program.Menu.Item("FeroSaveRRdy").GetValue<bool>();
            var FeroSpell = Program.Menu.Item("FeroSpellF").GetValue<StringList>();
            var ForceW = Program.Menu.Item("ForceWF").GetValue<Slider>();
            var Target = LXOrbwalker.GetPossibleTarget();

            // Save Ferocity
            if (SaveFero && R.IsReady() && Player.Mana == 5) return;

            // Ferocity Spells
            if (Player.Mana == 5)
            {
                if (Target.IsValidTarget(W.Range) && (Player.Health / Player.MaxHealth <= ForceW.Value / 100f || FeroSpell.SelectedIndex == 1))
                    W.Cast();

                if (Target.IsValidTarget(LXOrbwalker.GetAutoAttackRange(Player)) && FeroSpell.SelectedIndex == 0)
                    Q.Cast();

                if (Target.IsValidTarget(E.Range) && FeroSpell.SelectedIndex == 2)
                    E.Cast();

                return;
            }

            // Normal Spells
            if (Q.IsReady() && Target.IsValidTarget(LXOrbwalker.GetAutoAttackRange(Player))) Q.Cast();
            if (W.IsReady() && Target.IsValidTarget(W.Range)) W.Cast();
            if (E.IsReady() && Target.IsValidTarget(E.Range)) E.Cast(Target);
        }

        private static void doHarass()
        {
            var Target = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Physical);
            var useW = Program.Menu.Item("HarassW").GetValue<bool>();
            var useE = Program.Menu.Item("HarassE").GetValue<bool>();
            var FeroSpell = Program.Menu.Item("FeroSpellH").GetValue<StringList>();

            if (Player.Mana == 5)
            {
                if (FeroSpell.SelectedIndex == 1 && Target.IsValidTarget(W.Range))
                    W.Cast();
                if (FeroSpell.SelectedIndex == 2 && Target.IsValidTarget(E.Range))
                    E.Cast(Target);

                return;
            }

            if (useW && W.IsReady() && Target.IsValidTarget(W.Range))
                W.Cast();

            if (useE && E.IsReady() && Target.IsValidTarget(E.Range))
                E.Cast(Target);
        }

        private static void doLastHit()
        {
            var useW = Program.Menu.Item("LastHitW").GetValue<bool>();
            var useE = Program.Menu.Item("LastHitE").GetValue<bool>();
            var FeroSpell = Program.Menu.Item("FeroSpellLH").GetValue<StringList>();

            if (Player.Mana == 5 && FeroSpell.SelectedIndex == 0) return;

            foreach (var minion in MinionManager.GetMinions(Player.Position, E.Range, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.Health))
            {
                if (useW && W.IsReady() && minion.IsValidTarget(W.Range) && minion.Health < W.GetDamage(minion) && (Player.Mana == 5 ? FeroSpell.SelectedIndex == 1 : true))
                {
                    W.Cast();
                    return;
                }

                if (useE && E.IsReady() && minion.IsValidTarget(E.Range) && minion.Health < E.GetDamage(minion) && (Player.Mana == 5 ? FeroSpell.SelectedIndex == 1 : true))
                {
                    E.Cast(minion);
                    return;
                }
            }
        }  

        private static int GetDFG()
        {
            var map = Utility.Map.GetMap()._MapType;
            if (map == Utility.Map.MapType.TwistedTreeline || map == Utility.Map.MapType.CrystalScar)
                return 3128;
            else
                return 3188;
        }
    }
}
