﻿#region License

/*
 Copyright 2014 - 2014 Nikita Bernthaler
 Activators.cs is part of FedUtility.
 
 Utility is free software: you can redistribute it and/or modify
 it under the terms of the GNU General Public License as published by
 the Free Software Foundation, either version 3 of the License, or
 (at your option) any later version.
 
 Utility is distributed in the hope that it will be useful,
 but WITHOUT ANY WARRANTY; without even the implied warranty of
 MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 GNU General Public License for more details.
 
 You should have received a copy of the GNU General Public License
 along with FedUtility. If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

namespace FedUtility.Feature
{
    #region

    using System;
    using Class;
    using IoCContainer;
    using LeagueSharp.Common;

    #endregion

    internal class Activators : Base
    {
        #region Constructors

        public Activators(IContainer container)
            : base(container)
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }

        #endregion

        #region Properties

        public override bool Enabled
        {
            get { return Menu != null && Menu.Item(Name + "Enabled").GetValue<bool>(); }
        }

        public override string Name
        {
            get { return "Activators"; }
        }

        #endregion

        #region Methods

        private void OnGameLoad(EventArgs args)
        {
            try
            {
                Logger.Prefix = string.Format("{0} - {1}", BaseName, Name);

                Menu = new Menu(Name, Name);
                if (IoC.IsRegistered<Mediator>())
                {
                    IoC.Resolve<Mediator>().NotifyColleagues(Name + "_initialized", this);
                }
                Menu.AddItem(new MenuItem(Name + "Enabled", "Enabled").SetValue(true));
                BaseMenu.AddSubMenu(Menu);

                Initialized = true;
            }
            catch (Exception ex)
            {
                Logger.WriteBlock(ex.Message, ex.ToString());
            }
        }

        #endregion
    }
}