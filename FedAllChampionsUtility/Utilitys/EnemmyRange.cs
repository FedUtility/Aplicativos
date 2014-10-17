using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using LX_Orbwalker;

using Color = System.Drawing.Color;

namespace FedAllChampionsUtility
{
    class EnemmyRange
    {
        private static Menu _menu;

        public EnemmyRange()
        {
            _menu = Program.Menu.AddSubMenu(new Menu("Enemmy Range", "EnemmyRange"));
            _menu.AddItem(new MenuItem("EnemmyRange", "Ativar Enemmy Range").SetValue(true));
            
            Drawing.OnDraw += Drawing_OnDraw;

        }        

        private void Drawing_OnDraw(EventArgs args)
        {
            if (!_menu.Item("EnemmyRange").GetValue<bool>())
                return;

            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy && hero.IsVisible && !hero.IsDead))
            {
                Utility.DrawCircle(hero.Position, LXOrbwalker.GetAutoAttackRange(hero), ObjectManager.Player.Distance(hero) < LXOrbwalker.GetAutoAttackRange(hero) ? Color.Red : Color.Yellow, 4, 30, false);
            }

        }        
    }
}
                
                
           
    


