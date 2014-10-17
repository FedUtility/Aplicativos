using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;
using LX_Orbwalker;

namespace FedAllChampionsUtility
{
	class Khazix : Champion
	{

		public Spell Q;
		public Spell W;
		public Spell E;
		public Spell R;

        public Khazix()
        {
			LoadMenu();
			LoadSpells();

			Drawing.OnDraw += Drawing_OnDraw;
			Game.OnGameUpdate += Game_OnGameUpdate;
            LXOrbwalker.AfterAttack += Orbwalking_AfterAttack;
			PluginLoaded();
		}

		private void LoadMenu()
		{
			Program.Menu.AddSubMenu(new Menu("TeamFight", "TeamFight"));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useQ_TeamFight", "Use Q").SetValue(true));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useW_TeamFight", "Use W").SetValue(true));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useE_TeamFight", "Use E").SetValue(true));

			Program.Menu.AddSubMenu(new Menu("Harass", "Harass"));
			Program.Menu.SubMenu("Harass").AddItem(new MenuItem("useQ_Harass", "Use Q").SetValue(true));
			Program.Menu.SubMenu("Harass").AddItem(new MenuItem("useW_Harass", "Use W").SetValue(true));

			Program.Menu.AddSubMenu(new Menu("LaneClear", "LaneClear"));
			Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("useQ_LaneClear", "Use Q").SetValue(true));
			Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("useW_LaneClear", "Use W").SetValue(true));
			Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("useE_LaneClear", "Use E").SetValue(true));

			Program.Menu.AddSubMenu(new Menu("LastHit", "LastHit"));
			Program.Menu.SubMenu("LastHit").AddItem(new MenuItem("useQ_LastHit", "Use Q").SetValue(true));


			Program.Menu.AddSubMenu(new Menu("Drawing", "Drawing"));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_Disabled", "Disable All").SetValue(false));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_Q", "Draw Q").SetValue(true));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_W", "Draw W").SetValue(true));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_E", "Draw E").SetValue(true));

		}

		private void LoadSpells()
		{
			Q = new Spell(SpellSlot.Q, 325f);

			W = new Spell(SpellSlot.W, 1000f);
			W.SetSkillshot(0.225f, 100f, 828.5f, true, SkillshotType.SkillshotLine);

			E = new Spell(SpellSlot.E, 600f);
			E.SetSkillshot(0.250f, 100f, 1000f, false, SkillshotType.SkillshotCircle);

			R = new Spell(SpellSlot.R);
		}

		private void Drawing_OnDraw(EventArgs args)
		{
			if(Program.Menu.Item("Draw_Disabled").GetValue<bool>())
				return;

			if(Program.Menu.Item("Draw_Q").GetValue<bool>())
				if(Q.Level > 0)
					Utility.DrawCircle(ObjectManager.Player.Position, Q.Range, Q.IsReady() ? Color.Green : Color.Red);

			if(Program.Menu.Item("Draw_W").GetValue<bool>())
				if(W.Level > 0)
					Utility.DrawCircle(ObjectManager.Player.Position, W.Range, W.IsReady() ? Color.Green : Color.Red);

			if(Program.Menu.Item("Draw_E").GetValue<bool>())
				if(E.Level > 0)
					Utility.DrawCircle(ObjectManager.Player.Position, E.Range, E.IsReady() ? Color.Green : Color.Red);
		}

		private void Orbwalking_AfterAttack(Obj_AI_Base unit, Obj_AI_Base target)
		{
			switch(LXOrbwalker.CurrentMode)
			{
				case LXOrbwalker.Mode.Combo:
					if(Program.Menu.Item("useQ_TeamFight").GetValue<bool>())
						CastQEnemy();
					break;
				case LXOrbwalker.Mode.Harass:
					if(Program.Menu.Item("useQ_Harass").GetValue<bool>())
						CastQEnemy();
					break;
				case LXOrbwalker.Mode.LaneClear:
					if(Program.Menu.Item("useQ_LaneClear").GetValue<bool>())
						CastQMinion();
					break;
				case LXOrbwalker.Mode.Lasthit:
					if(Program.Menu.Item("useQ_LastHit").GetValue<bool>())
						CastQMinion();
					break;
			}
		}

		private void Game_OnGameUpdate(EventArgs args)
		{
			EvolutionCheck();

			switch(LXOrbwalker.CurrentMode)
			{
				case LXOrbwalker.Mode.Combo:
					if(Program.Menu.Item("useW_TeamFight").GetValue<bool>())
						CastWEnemy();
					if(Program.Menu.Item("useE_TeamFight").GetValue<bool>())
						CastEEnemy();
					break;
				case LXOrbwalker.Mode.Harass:
					if(Program.Menu.Item("useW_Harass").GetValue<bool>())
						CastWEnemy();
					break;
				case LXOrbwalker.Mode.LaneClear:
					if(Program.Menu.Item("useW_LaneClear").GetValue<bool>())
						CastWMinion();
					if(Program.Menu.Item("useE_LaneClear").GetValue<bool>())
						CastEMinion();
					break;
			}
		}

		private void CastEEnemy()
		{
			if(!E.IsReady())
				return;
			var target = SimpleTs.GetTarget(E.Range + (E.Width / 2), SimpleTs.DamageType.Physical);
			if(target == null)
				return;
			if(target.IsValidTarget(E.Range + (E.Width / 2)) && E.GetPrediction(target).Hitchance >= HitChance.High)
				E.Cast(E.GetPrediction(target).CastPosition, Packets());
		}

		private void CastEMinion()
		{
			if(!E.IsReady())
				return;
			var minions = MinionManager.GetMinions(ObjectManager.Player.Position, E.Range + (E.Width / 2), MinionTypes.All, MinionTeam.NotAlly);
			if(minions.Count == 0)
				return;
			var castPostion = MinionManager.GetBestCircularFarmLocation(minions.Select(minion => minion.ServerPosition.To2D()).ToList(), E.Width, E.Range);
			E.Cast(castPostion.Position, Packets());
		}

		private void CastQEnemy()
		{
			if(!Q.IsReady())
				return;
			var target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Physical);
			if(target.IsValidTarget(Q.Range))
				Q.CastOnUnit(target, Packets());

		}

		private void CastQMinion()
		{
			if(!Q.IsReady())
				return;
			var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.NotAlly);
			foreach(var minion in allMinions)
			{
				if(!minion.IsValidTarget())
					continue;
                var minionInRangeAa = LXOrbwalker.InAutoAttackRange(minion);
				var minionInRangeSpell = minion.Distance(ObjectManager.Player) <= Q.Range;
				var minionKillableAa = ObjectManager.Player.GetAutoAttackDamage(minion) >= minion.Health;
				var minionKillableSpell = ObjectManager.Player.GetSpellDamage(minion, SpellSlot.Q) >= minion.Health;
                var lastHit = LXOrbwalker.CurrentMode == LXOrbwalker.Mode.Lasthit;
				var laneClear = LXOrbwalker.CurrentMode == LXOrbwalker.Mode.LaneClear;

				if((lastHit && minionInRangeSpell && minionKillableSpell) && ((minionInRangeAa && !minionKillableAa) || !minionInRangeAa))
					Q.CastOnUnit(minion, Packets());

				else if((laneClear && minionInRangeSpell && !minionKillableSpell) && ((minionInRangeAa && !minionKillableAa) || !minionInRangeAa))
					Q.CastOnUnit(minion, Packets());

			}
		}

		private void CastWEnemy()
		{
			if(!W.IsReady())
				return;
			var target = SimpleTs.GetTarget(W.Range, SimpleTs.DamageType.Physical);
			if(target == null)
				return;
			if(target.IsValidTarget(W.Range) && W.GetPrediction(target).Hitchance >= HitChance.High)
				W.Cast(W.GetPrediction(target).CastPosition, Packets());
			else if(W.GetPrediction(target).Hitchance == HitChance.Collision)
			{
				var wCollision = W.GetPrediction(target).CollisionObjects;
				foreach(var wCollisionChar in wCollision.Where(wCollisionChar => wCollisionChar.Distance(target) <= 50))
					W.Cast(wCollisionChar.Position, Packets());
			}
		}

		private void CastWMinion()
		{
			if(!W.IsReady())
				return;
			var allMinions = MinionManager.GetMinions(ObjectManager.Player.Position, W.Range + 50, MinionTypes.All,
				MinionTeam.NotAlly);
			var minion = allMinions.FirstOrDefault(minionn => minionn.IsValidTarget(W.Range));
			if(minion != null)
				W.Cast(minion.Position, Packets());
		}

		private void EvolutionCheck()
		{
			if(ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).Name == "khazixqlong" && Q.Range < 374f)
				Q.Range = 375f;
			if(ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).Name == "khazixelong" && E.Range < 899f)
				E.Range = 900f;
		}
	}
}